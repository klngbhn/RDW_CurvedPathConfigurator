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
    public void initJointsAndCurves(Vector3 jointAPosition, Vector3 jointBPosition, Vector3 jointCPosition)
    {
        // Set three joint points (real world)
        jointPointA = CreateInstance<JointPoint>();
        jointPointA.init(jointAPosition, "A");
        jointPointB = CreateInstance<JointPoint>();
        jointPointB.init(jointBPosition, "B");
        jointPointC = CreateInstance<JointPoint>();
        jointPointC.init(jointCPosition, "C");
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
        curveACsmallRadius = createSmallCurve(jointPointA, jointPointC);
        curveBCsmallRadius = createSmallCurve(jointPointB, jointPointC);

        // Specify curves with large radii (real world)
        curveABlargeRadius = createLargeCurve(jointPointA, jointPointB, jointPointC);
        curveAClargeRadius = createLargeCurve(jointPointA, jointPointC, jointPointB);
        curveBClargeRadius = createLargeCurve(jointPointB, jointPointC, jointPointA);

#if UNITY_EDITOR
        SceneView.RepaintAll();
#endif
        Debug.Log("Initialized joints and curves");
    }

    /*
     * Creates a curve from joint1 to joint2 with the center between both points.
     * */
    private Curve createSmallCurve(JointPoint joint1, JointPoint joint2)
    {
        Vector3 circleCenter = Vector3.Lerp(joint1.getPosition(), joint2.getPosition(), 0.5f);
        float radius = 0.5f * Vector3.Magnitude(joint1.getPosition() - joint2.getPosition());
        List<JointPoint> endPoints = new List<JointPoint>();
        endPoints.Add(joint1);
        endPoints.Add(joint2);
        Curve curve = CreateInstance<Curve>();
        curve.init(circleCenter, radius, endPoints);

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
        curve.init(circleCenter, radius, endPoints);

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

        // Calculate end joint point
        JointPoint endJointPoint = getCorrespondingEndJointPoint(startIntersection, curve);

        // Calculate the circle center of the new path
        Vector3 circleCenter = calculateCircleCenterOfPath(startIntersection, curve, radius);

        // Calculate (nomralized) direction vector from circle center to start intersection
        Vector3 directionToStartIntersection = (startIntersection.getPosition() - circleCenter).normalized;

        // Calculate direction vector from circle center to end intersection by: 
        // 1. rotating the direction vector to start intersection
        // 2. extending the vector by the virtual radius
        Vector3 directionToEndIntersection = Quaternion.AngleAxis(this.getSignOfCurve(startIntersection.getJoint(), endJointPoint) * angle, Vector3.up) * directionToStartIntersection;
        directionToEndIntersection = directionToEndIntersection * radius;

        // Calculate position of new end intersection
        Vector3 position = circleCenter + directionToEndIntersection;

        VirtualIntersection endIntersection = CreateInstance<VirtualIntersection>();
        endIntersection.init(position, endJointPoint, ""+intersections.Count);

        List<VirtualIntersection> endPoints = new List<VirtualIntersection>();
        endPoints.Add(startIntersection);
        endPoints.Add(endIntersection);

        VirtualPath path = CreateInstance<VirtualPath>();
        path.init(circleCenter, gain, curve, endPoints);

        intersections.Add(endIntersection);
        paths.Add(path);

        startIntersection.addPath(path, this.getPathIndex(startIntersection.getJoint(), curve));
        endIntersection.addPath(path, this.getPathIndex(endJointPoint, curve));

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
     * Returns the circle center position of the new path that starts at given intersection and uses given curve and radius.
     * */
    private Vector3 calculateCircleCenterOfPath(VirtualIntersection startIntersection, Curve curve, float radius)
    {
        Vector3 result = new Vector3();
        int newPathIndex = this.getPathIndex(startIntersection.getJoint(), curve);

        // Check which paths are connected to the intersection and pick one (first) for calculation.
        // New path circle center can be calculated by using the direction to the old path circle center.
        // In some cases it is necessary to rotate this direction vector by 90 (left) or -90 (right) degrees.
        switch (newPathIndex)
        {
            case 0:
                if (startIntersection.getPath(1) != null)
                {
                    // Calculate direction from intersection to old path circle center and rotate it by 90 degrees
                    Vector3 directionToOldPathCircleCenter = (startIntersection.getPath(1).getCircleCenter() - startIntersection.getPosition()).normalized;
                    Vector3 rotatedDirectionVector = Quaternion.AngleAxis(-90f, Vector3.up) * directionToOldPathCircleCenter;
                    result = startIntersection.getPosition() + rotatedDirectionVector * radius;
                    break;
                }
                if (startIntersection.getPath(2) != null)
                {
                    // Opposite path: Just calculate direction from intersection to old path circle center and multiply new radius
                    Vector3 directionToNewPathCircleCenter = (startIntersection.getPath(2).getCircleCenter() - startIntersection.getPosition()).normalized * radius;
                    result = startIntersection.getPosition() + directionToNewPathCircleCenter;
                    break;
                }
                if (startIntersection.getPath(3) != null)
                {
                    // Calculate direction from intersection to old path circle center and rotate it by 90 degrees
                    Vector3 directionToOldPathCircleCenter = (startIntersection.getPath(3).getCircleCenter() - startIntersection.getPosition()).normalized;
                    Vector3 rotatedDirectionVector = Quaternion.AngleAxis(-90f, Vector3.up) * directionToOldPathCircleCenter;
                    result = startIntersection.getPosition() + rotatedDirectionVector * radius;
                    break;
                }
                goto default;
            case 1:
                if (startIntersection.getPath(0) != null)
                {
                    // Calculate direction from intersection to old path circle center and rotate it by 90 degrees
                    Vector3 directionToOldPathCircleCenter = (startIntersection.getPath(0).getCircleCenter() - startIntersection.getPosition()).normalized;
                    Vector3 rotatedDirectionVector = Quaternion.AngleAxis(90f, Vector3.up) * directionToOldPathCircleCenter;
                    result = startIntersection.getPosition() + rotatedDirectionVector * radius;
                    break;
                }
                if (startIntersection.getPath(2) != null)
                {
                    // Calculate direction from intersection to old path circle center and rotate it by 90 degrees
                    Vector3 directionToOldPathCircleCenter = (startIntersection.getPath(2).getCircleCenter() - startIntersection.getPosition()).normalized;
                    Vector3 rotatedDirectionVector = Quaternion.AngleAxis(90f, Vector3.up) * directionToOldPathCircleCenter;
                    result = startIntersection.getPosition() + rotatedDirectionVector * radius;
                    break;
                }
                if (startIntersection.getPath(3) != null)
                {
                    // Opposite path: Just calculate direction from intersection to old path circle center and multiply new radius
                    Vector3 directionToNewPathCircleCenter = (startIntersection.getPath(3).getCircleCenter() - startIntersection.getPosition()).normalized * radius;
                    result = startIntersection.getPosition() + directionToNewPathCircleCenter;
                    break;
                }
                goto default;
            case 2:
                if (startIntersection.getPath(0) != null)
                {
                    // Opposite path: Just calculate direction from intersection to old path circle center and multiply new radius
                    Vector3 directionToNewPathCircleCenter = (startIntersection.getPath(0).getCircleCenter() - startIntersection.getPosition()).normalized * radius;
                    result = startIntersection.getPosition() + directionToNewPathCircleCenter;
                    break;
                }
                if (startIntersection.getPath(1) != null)
                {
                    // Calculate direction from intersection to old path circle center and rotate it by 90 degrees
                    Vector3 directionToOldPathCircleCenter = (startIntersection.getPath(1).getCircleCenter() - startIntersection.getPosition()).normalized;
                    Vector3 rotatedDirectionVector = Quaternion.AngleAxis(-90f, Vector3.up) * directionToOldPathCircleCenter;
                    result = startIntersection.getPosition() + rotatedDirectionVector * radius;
                    break;
                }
                if (startIntersection.getPath(3) != null)
                {
                    // Calculate direction from intersection to old path circle center and rotate it by 90 degrees
                    Vector3 directionToOldPathCircleCenter = (startIntersection.getPath(3).getCircleCenter() - startIntersection.getPosition()).normalized;
                    Vector3 rotatedDirectionVector = Quaternion.AngleAxis(-90f, Vector3.up) * directionToOldPathCircleCenter;
                    result = startIntersection.getPosition() + rotatedDirectionVector * radius;
                    break;
                }
                goto default;
            case 3:
                if (startIntersection.getPath(1) != null)
                {
                    // Opposite path: Just calculate direction from intersection to old path circle center and multiply new radius
                    Vector3 directionToNewPathCircleCenter = (startIntersection.getPath(1).getCircleCenter() - startIntersection.getPosition()).normalized * radius;
                    result = startIntersection.getPosition() + directionToNewPathCircleCenter;
                    break;
                }
                if (startIntersection.getPath(2) != null)
                {
                    // Calculate direction from intersection to old path circle center and rotate it by 90 degrees
                    Vector3 directionToOldPathCircleCenter = (startIntersection.getPath(2).getCircleCenter() - startIntersection.getPosition()).normalized;
                    Vector3 rotatedDirectionVector = Quaternion.AngleAxis(90f, Vector3.up) * directionToOldPathCircleCenter;
                    result = startIntersection.getPosition() + rotatedDirectionVector * radius;
                    break;
                }
                if (startIntersection.getPath(0) != null)
                {
                    // Calculate direction from intersection to old path circle center and rotate it by 90 degrees
                    Vector3 directionToOldPathCircleCenter = (startIntersection.getPath(0).getCircleCenter() - startIntersection.getPosition()).normalized;
                    Vector3 rotatedDirectionVector = Quaternion.AngleAxis(90f, Vector3.up) * directionToOldPathCircleCenter;
                    result = startIntersection.getPosition() + rotatedDirectionVector * radius;
                    break;
                }
                goto default;
            default:
                // If no case was chosen this is the start joint. Then,
                // we can use the curve circle center for calculating the path circle center.
                // Calculate the direction vector to the circle center of the new path and then the circle center position itself
                Vector3 directionToCircleCenter = (curve.getCircleCenter() - startIntersection.getPosition()).normalized * radius;
                result = startIntersection.getPosition() + directionToCircleCenter;
                break;
        }
        

        return result;
    }

    /*
     * Returns the index at that a curve/path is connected to a joint/intersection.
     * 0 index is always the curve with large radius to the next joint (e.g. A to B, B to C).
     * Then index is increasing clockwise.
     * */
    private int getPathIndex(JointPoint joint, Curve curve)
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
