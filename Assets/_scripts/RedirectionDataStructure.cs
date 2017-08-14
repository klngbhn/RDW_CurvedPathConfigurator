using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/*
 * This class keeps all the data that is necessary for the redirection.
 * Hence, it holds all jointpoints, curves, intersections, and paths.
 * Offers also methods to create and generate these data.
 * */
[CreateAssetMenu(fileName = "RedirectionPathLayout", menuName = "Redirection/PathLayout", order = 1)]
[System.Serializable]
public class RedirectionDataStructure : ScriptableObject
{
    public string objectName = "RedirectionPathLayout";

    public JointPoint jointPointA;
    public JointPoint jointPointB;
    public JointPoint jointPointC;

    public JointPoint startJoint;

    public Curve curveABsmallRadius;
    public Curve curveACsmallRadius;
    public Curve curveBCsmallRadius;
    public Curve curveABlargeRadius;
    public Curve curveAClargeRadius;
    public Curve curveBClargeRadius;

    public List<VirtualIntersection> intersections;
    public List<VirtualPath> paths;

    public void OnEnable()
    {
        if (intersections == null)
            intersections = new List<VirtualIntersection>();

        if (paths == null)
            paths = new List<VirtualPath>();

        if (jointPointA == null)
        {
            intersections = new List<VirtualIntersection>();
            paths = new List<VirtualPath>();
        }
    }

    /*
     * Initializes jointpoints and curves by using the defined positions. In the future
     * it might be comfortable to automatically generate the positions by using the size of the 
     * tracking space.
     * */
	public void initJointsAndCurves(Vector3 jointAPosition, Vector3 jointBPosition, Vector3 jointCPosition, float walkingZoneRadius)
    {
        // Set three joint points (real world)
        jointPointA = CreateInstance<JointPoint>();
		jointPointA.init(jointAPosition, "A", walkingZoneRadius);
        jointPointB = CreateInstance<JointPoint>();
		jointPointB.init(jointBPosition, "B", walkingZoneRadius);
        jointPointC = CreateInstance<JointPoint>();
		jointPointC.init(jointCPosition, "C", walkingZoneRadius);
#if UNITY_EDITOR
        AssetDatabase.AddObjectToAsset(jointPointA, this);
        AssetDatabase.AddObjectToAsset(jointPointB, this);
        AssetDatabase.AddObjectToAsset(jointPointC, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif

        // Specify curves with small radii (real world)
        curveABsmallRadius = createSmallCurve(jointPointA, jointPointB);
        curveACsmallRadius = createSmallCurve(jointPointC, jointPointA);
        curveBCsmallRadius = createSmallCurve(jointPointB, jointPointC);

        // Specify curves with large radii (real world)
        curveABlargeRadius = createLargeCurve(jointPointA, jointPointB, jointPointC);
        curveAClargeRadius = createLargeCurve(jointPointC, jointPointA, jointPointB);
        curveBClargeRadius = createLargeCurve(jointPointB, jointPointC, jointPointA);

#if UNITY_EDITOR
        SceneView.RepaintAll();
#endif
        Debug.Log("Initialized joints and curves");
    }

    /*
     * Creates a curve from joint1 to joint2 with the center between both points (plus offset for real walking area at joint point).
     * */
    private Curve createSmallCurve(JointPoint joint1, JointPoint joint2)
    {
		Vector3 circleCenter = Vector3.Lerp(joint1.getPosition(), joint2.getPosition(), 0.5f);

        float radius = 0.5f * Vector3.Magnitude(joint1.getPosition() - joint2.getPosition());
        List<JointPoint> endPoints = new List<JointPoint>();
        endPoints.Add(joint1);
        endPoints.Add(joint2);
        Curve curve = CreateInstance<Curve>();
        curve.init(circleCenter, radius, endPoints, true);

#if UNITY_EDITOR
        AssetDatabase.AddObjectToAsset(curve, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif

        return curve;
    }

    /*
     * Creates a curve from joint1 to joint2 with the center at joint3.
     * */
    private Curve createLargeCurve(JointPoint joint1, JointPoint joint2, JointPoint joint3)
    {
		Vector3 circleCenter = joint3.getPosition();
        
        float radius = Vector3.Magnitude(joint1.getPosition() - joint2.getPosition());
        
        List<JointPoint> endPoints = new List<JointPoint>();
        endPoints.Add(joint1);
        endPoints.Add(joint2);
        Curve curve = CreateInstance<Curve>();
        curve.init(circleCenter, radius, endPoints, false);

#if UNITY_EDITOR
        AssetDatabase.AddObjectToAsset(curve, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif

        return curve;
    }

    /*
     * Creates a new path and the corresponding intersection at the end.
     * */
    public void createPathAndIntersection(VirtualIntersection startIntersection, Curve curve, float gain)
    {
        // Calculate radius of virtual path
        float radius = curve.getRadius() * gain;
        // Calculate length of curve
        float length = Mathf.Deg2Rad * curve.getAngle() * curve.getRadius();
        // Calculate angle of virtual path
        float angle = Mathf.Rad2Deg * (length / radius);

        int newPathIndex = this.getPathIndex(startIntersection.getJoint(), curve);

        // Calculate end joint point
        JointPoint endJointPoint = getCorrespondingEndJointPoint(startIntersection, curve);

        // Calculate the circle center of the new path
        Vector3 circleCenter = calculateCircleCenterOfPath(startIntersection, curve, radius);

        // Calculate (nomralized) direction vector from circle center to start intersection
		Vector3 directionToStartPath = (startIntersection.getWalkingStartPosition(newPathIndex) - circleCenter).normalized;

        // Calculate direction vector from circle center to end intersection by: 
        // 1. rotating the direction vector to start intersection (around angle of path and angle of walking zone)
        // 2. extending the vector by the virtual radius
        float angleOfWalkingZone = Mathf.Rad2Deg * startIntersection.getJoint().getWalkingZoneRadius() / radius;
        Vector3 directionToEndPath = Quaternion.AngleAxis(this.getSignOfCurve(startIntersection.getJoint(), endJointPoint) * angle, Vector3.up) * directionToStartPath;
        directionToEndPath = directionToEndPath * radius;

        Vector3 directionToEndIntersection = Quaternion.AngleAxis(this.getSignOfCurve(startIntersection.getJoint(), endJointPoint) * angleOfWalkingZone, Vector3.up) * directionToEndPath;

        // Calculate position of new end intersection
        Vector3 positionEndIntersection = circleCenter + directionToEndIntersection; 

        VirtualIntersection endIntersection = CreateInstance<VirtualIntersection>();
        endIntersection.init(positionEndIntersection, endJointPoint, ""+intersections.Count);

        List<VirtualIntersection> endPoints = new List<VirtualIntersection>();
        endPoints.Add(startIntersection);
        endPoints.Add(endIntersection);

        VirtualPath path = CreateInstance<VirtualPath>();
        path.init(circleCenter, gain, curve, endPoints, radius, angle);

        intersections.Add(endIntersection);
        paths.Add(path);

        startIntersection.addPath(path, this.getPathIndex(startIntersection.getJoint(), curve));
        endIntersection.addPath(path, this.getPathIndex(endJointPoint, curve));

        calculateWalkingStartPositions(this.getPathIndex(endJointPoint, curve), endJointPoint, endIntersection, circleCenter, directionToEndPath);

#if UNITY_EDITOR
        SceneView.RepaintAll();
        AssetDatabase.AddObjectToAsset(endIntersection, this);
        AssetDatabase.AddObjectToAsset(path, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
        Debug.Log("Created new path and intersection");
    }

    /*
     * Calculates walking start positions for new intersection
     * */
    private void calculateWalkingStartPositions(int pathIndex, JointPoint endJointPoint, VirtualIntersection endIntersection, Vector3 circleCenter, Vector3 directionToEndPath)
    {
        endIntersection.setWalkingStartPosition(pathIndex, circleCenter + directionToEndPath);

        Vector3 directionToCurve0 = endJointPoint.getWalkingStartPosition(0) - endJointPoint.getPosition();
        Vector3 directionToCurve1 = endJointPoint.getWalkingStartPosition(1) - endJointPoint.getPosition();
        Vector3 directionToCurve2 = endJointPoint.getWalkingStartPosition(2) - endJointPoint.getPosition();
        Vector3 directionToCurve3 = endJointPoint.getWalkingStartPosition(3) - endJointPoint.getPosition();

        float angleBetweenCurves01 = angle360(directionToCurve0, directionToCurve1);
        float angleBetweenCurves02 = angle360(directionToCurve0, directionToCurve2);
        float angleBetweenCurves03 = angle360(directionToCurve0, directionToCurve3);

        float angleBetweenCurves12 = angle360(directionToCurve1, directionToCurve2);
        float angleBetweenCurves13 = angle360(directionToCurve1, directionToCurve3);
        float angleBetweenCurves10 = angle360(directionToCurve1, directionToCurve0);

        float angleBetweenCurves23 = angle360(directionToCurve2, directionToCurve3);
        float angleBetweenCurves20 = angle360(directionToCurve2, directionToCurve0);
        float angleBetweenCurves21 = angle360(directionToCurve2, directionToCurve1);

        float angleBetweenCurves30 = angle360(directionToCurve3, directionToCurve0);
        float angleBetweenCurves31 = angle360(directionToCurve3, directionToCurve1);
        float angleBetweenCurves32 = angle360(directionToCurve3, directionToCurve2);

        Vector3 directionToFirstStartPosition = (circleCenter + directionToEndPath) - endIntersection.getPosition();
        Vector3 directionToOtherStartPosition = new Vector3();

        switch (pathIndex)
        {
            case 0:
                endIntersection.setWalkingStartPosition(0, circleCenter + directionToEndPath);

                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves01, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(1, endIntersection.getPosition() + directionToOtherStartPosition);
                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves02, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(2, endIntersection.getPosition() + directionToOtherStartPosition);
                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves03, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(3, endIntersection.getPosition() + directionToOtherStartPosition);
                break;
            case 1:
                endIntersection.setWalkingStartPosition(1, circleCenter + directionToEndPath);

                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves12, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(2, endIntersection.getPosition() + directionToOtherStartPosition);
                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves13, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(3, endIntersection.getPosition() + directionToOtherStartPosition);
                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves10, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(0, endIntersection.getPosition() + directionToOtherStartPosition);
                break;
            case 2:
                endIntersection.setWalkingStartPosition(2, circleCenter + directionToEndPath);

                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves23, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(3, endIntersection.getPosition() + directionToOtherStartPosition);
                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves20, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(0, endIntersection.getPosition() + directionToOtherStartPosition);
                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves21, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(1, endIntersection.getPosition() + directionToOtherStartPosition);
                break;
            case 3:
                endIntersection.setWalkingStartPosition(3, circleCenter + directionToEndPath);

                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves30, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(0, endIntersection.getPosition() + directionToOtherStartPosition);
                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves31, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(1, endIntersection.getPosition() + directionToOtherStartPosition);
                directionToOtherStartPosition = Quaternion.AngleAxis(angleBetweenCurves32, Vector3.up) * directionToFirstStartPosition;
                endIntersection.setWalkingStartPosition(2, endIntersection.getPosition() + directionToOtherStartPosition);
                break;
        }
    }

    /*
     * Returns the angle between from and to in 360 degrees clockwise.
     * */
    private float angle360(Vector3 from, Vector3 to)
    {
        Vector3 right = Vector3.Cross(Vector3.up, from);
        float angle = Vector3.Angle(from, to);
        return (Vector3.Angle(right, to) > 90f) ? 360f - angle : angle;
    }

    /*
     * Returns the circle center position of the new path that starts at given intersection and uses given curve and radius.
     * */
    private Vector3 calculateCircleCenterOfPath(VirtualIntersection startIntersection, Curve curve, float radius)
    {
        int newPathIndex = this.getPathIndex(startIntersection.getJoint(), curve);

        Vector3 directionToCurveCircleCenter = (curve.getCircleCenter() - startIntersection.getJoint().getWalkingStartPosition(newPathIndex)).normalized;
        Vector3 directionToPathCircleCenter = new Vector3();

        // Check which paths are connected to the intersection and pick one (first) for calculation.
        // New path circle center can be calculated by using the direction to the curve circle center and rotating it.
        for (int i = 0; i < 4; i++)
        {
            if (startIntersection.getPath(i) != null)
            {
                Vector3 directionToStartWalkingPosOnPath = (startIntersection.getWalkingStartPosition(i) - startIntersection.getPosition()).normalized;
                Vector3 directionToStartWalkingPosOnCurve = (startIntersection.getJoint().getWalkingStartPosition(i) - startIntersection.getJoint().getPosition()).normalized;
                float angle = -angle360(directionToStartWalkingPosOnPath, directionToStartWalkingPosOnCurve);
                directionToPathCircleCenter = Quaternion.AngleAxis(angle, Vector3.up) * directionToCurveCircleCenter;
                break;
            }
            if (i == 3)
            {
                // If no case was chosen this is the start joint. Then,
                // we can use the curve circle center for calculating the path circle center without rotating.
                directionToPathCircleCenter = directionToCurveCircleCenter;
            }
        }

        Vector3 result = startIntersection.getWalkingStartPosition(newPathIndex) + directionToPathCircleCenter.normalized * radius;

        return result;
    }

    /*
     * Returns the index at that a curve/path is connected to a joint/intersection.
     * 0 index is always the curve with large radius to the next joint (e.g. A to B, B to C).
     * Then index is increasing clockwise.
     * */
    public int getPathIndex(JointPoint joint, Curve curve)
    {
        if (joint.Equals(jointPointA))
        {
            if (curve.Equals(curveABsmallRadius))
                return 1;
            if (curve.Equals(curveABlargeRadius))
                return 0;
            if (curve.Equals(curveACsmallRadius))
                return 2;
            if (curve.Equals(curveAClargeRadius))
                return 3;
        }
        if (joint.Equals(jointPointB))
        {
            if (curve.Equals(curveABsmallRadius))
                return 2;
            if (curve.Equals(curveABlargeRadius))
                return 3;
            if (curve.Equals(curveBCsmallRadius))
                return 1;
            if (curve.Equals(curveBClargeRadius))
                return 0;
        }
        if (joint.Equals(jointPointC))
        {
            if (curve.Equals(curveBCsmallRadius))
                return 2;
            if (curve.Equals(curveBClargeRadius))
                return 3;
            if (curve.Equals(curveACsmallRadius))
                return 1;
            if (curve.Equals(curveAClargeRadius))
                return 0;
        }
        return 0;
    }

    /*
     * Returns the sign for the angle (curve to the right 1 or left -1?)
     * */
    public int getSignOfCurve(JointPoint startJointPoint, JointPoint endJointPoint)
    {
        if (startJointPoint.Equals(jointPointA))
        {
            if (endJointPoint.Equals(jointPointB))
                return -1;
            if (endJointPoint.Equals(jointPointC))
                return 1;
        }
        if (startJointPoint.Equals(jointPointB))
        {
            if (endJointPoint.Equals(jointPointA))
                return 1;
            if (endJointPoint.Equals(jointPointC))
                return -1;
        }
        if (startJointPoint.Equals(jointPointC))
        {
            if (endJointPoint.Equals(jointPointB))
                return 1;
            if (endJointPoint.Equals(jointPointA))
                return -1;
        }

        return -1;
    }

    /*
     * Returns the joint point that corresponds to the new end intersection that is connected to the given curve and 
     * starts at the given intersection.
     * */
    private JointPoint getCorrespondingEndJointPoint(VirtualIntersection intersection, Curve curve)
    {
        if (intersection.getJoint().Equals(jointPointA))
        {
            if (curve.Equals(curveABsmallRadius))
                return jointPointB;
            if (curve.Equals(curveABlargeRadius))
                return jointPointB;
            if (curve.Equals(curveACsmallRadius))
                return jointPointC;
            if (curve.Equals(curveAClargeRadius))
                return jointPointC;
        }
        if (intersection.getJoint().Equals(jointPointB))
        {
            if (curve.Equals(curveABsmallRadius))
                return jointPointA;
            if (curve.Equals(curveABlargeRadius))
                return jointPointA;
            if (curve.Equals(curveBCsmallRadius))
                return jointPointC;
            if (curve.Equals(curveBClargeRadius))
                return jointPointC;
        }
        if (intersection.getJoint().Equals(jointPointC))
        {
            if (curve.Equals(curveBCsmallRadius))
                return jointPointB;
            if (curve.Equals(curveBClargeRadius))
                return jointPointB;
            if (curve.Equals(curveACsmallRadius))
                return jointPointA;
            if (curve.Equals(curveAClargeRadius))
                return jointPointA;
        }
        return jointPointB;
    }

    /*
     * Sets a joint point as the new start position. Removes all saved virtual paths and intersections.
     * */
    public void setStartPosition(JointPoint joint)
    {
        startJoint = joint;
        intersections = new List<VirtualIntersection>();
        paths = new List<VirtualPath>();

        VirtualIntersection intersection = CreateInstance<VirtualIntersection>();
        intersection.init(joint.getPosition(), joint, "" + intersections.Count);
        intersections.Add(intersection);

        intersection.setWalkingStartPosition(0, joint.getWalkingStartPosition(0));
        intersection.setWalkingStartPosition(1, joint.getWalkingStartPosition(1));
        intersection.setWalkingStartPosition(2, joint.getWalkingStartPosition(2));
        intersection.setWalkingStartPosition(3, joint.getWalkingStartPosition(3));

#if UNITY_EDITOR
        SceneView.RepaintAll();
        AssetDatabase.AddObjectToAsset(intersection, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
        Debug.Log("Set new start position");
    }
}
