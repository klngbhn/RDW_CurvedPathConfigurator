using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Redirection : MonoBehaviour {

    public Transform orientationOffset;
    public Transform positionOffset;

    public bool showGizmos = false;

    private JointPoint jointPointA;
    private JointPoint jointPointB;
    private JointPoint jointPointC;

    private Curve curveABsmallRadius;
    private Curve curveACsmallRadius;
    private Curve curveBCsmallRadius;
    private Curve curveABlargeRadius;
    private Curve curveAClargeRadius;
    private Curve curveBClargeRadius;

    private JointPoint currentJoint;
    private VirtualIntersection currentIntersection;
    private Curve currentCurve;
    private VirtualPath currentPath;

    private bool redirectionStarted = false;
    private bool redirectLeft = false;

    // Use this for initialization
    void Start () {
        /*
            Tracking Space -> specify jointpoints -> specify curves
        */

        // Set three joint points (real world)
        // TODO: Later this will be done by using the tracking space size
        jointPointA = new JointPoint(new Vector3(0.5f, 0f, -1.25f));
        jointPointB = new JointPoint(new Vector3(0.5f, 0f, 1.25f));
        jointPointC = new JointPoint(new Vector3(-1.6f, 0, 0));

        // Specify curves with small radii (real world)
        curveABsmallRadius = createSmallCurve(jointPointA, jointPointB);
        curveACsmallRadius = createSmallCurve(jointPointA, jointPointC);
        curveBCsmallRadius = createSmallCurve(jointPointB, jointPointC);

        // Specify curves with large radii (real world)
        curveABlargeRadius = createLargeCurve(jointPointA, jointPointB, jointPointC);
        curveAClargeRadius = createLargeCurve(jointPointA, jointPointC, jointPointB);
        curveBClargeRadius = createLargeCurve(jointPointB, jointPointC, jointPointA);

        //TODO: Think about this. Do the jointpoints need to know their curves?
        jointPointA.addCurve(curveABsmallRadius);

        // TODO: This will be loaded from the path configuration tool in the future
        // Specify intersections and paths (virtual world)
        VirtualIntersection intersection = new VirtualIntersection(jointPointA.getPosition(), jointPointA);
        VirtualIntersection intersection2 = new VirtualIntersection(jointPointA.getPosition(), jointPointB); // TODO: This is not the correct position!!!
        VirtualPath path = new VirtualPath(new Vector3(0.5f, 0, 2.5f), 3.75f, curveABsmallRadius, intersection, intersection2);

        // TODO: How to set the starting conditions?
        currentJoint = jointPointA;
        currentIntersection = intersection;
        currentCurve = curveABsmallRadius;
        currentPath = path;
    }

    private Curve createSmallCurve(JointPoint joint1, JointPoint joint2)
    {
        Vector3 circleCenter = Vector3.Lerp(joint1.getPosition(), joint2.getPosition(), 0.5f);
        float radius = 0.5f * Vector3.Magnitude(joint1.getPosition() - joint2.getPosition());
        List<JointPoint> endPoints = new List<JointPoint>();
        endPoints.Add(joint1);
        endPoints.Add(joint2);
        Curve curve = new Curve(circleCenter, radius, endPoints);

        Debug.Log(circleCenter);
        Debug.Log(radius);

        return curve;
    }

    private Curve createLargeCurve(JointPoint joint1, JointPoint joint2, JointPoint joint3)
    {
        Vector3 circleCenter = joint3.getPosition();
        float radius = Vector3.Magnitude(joint1.getPosition() - joint2.getPosition());
        List<JointPoint> endPoints = new List<JointPoint>();
        endPoints.Add(joint1);
        endPoints.Add(joint2);
        Curve curve = new Curve(circleCenter, radius, endPoints);

        Debug.Log(circleCenter);
        Debug.Log(radius);

        return curve;
    }

    // Update is called once per frame
    void Update () {
        /*
            "Start" button -> startRedirection -> stopRedirection
        */

        if (Input.GetKeyDown(KeyCode.Space) || SteamVR_Controller.Input(SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.FarthestRight)).GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            startRedirection();
        }

        if (redirectionStarted)
            doRedirection();
    }

    private void startRedirection()
    {
        redirectionStarted = true;
        redirectLeft = true;

        // TODO: Implement this:

        // If from A to B: redirect left
        // If from B to C: redirect left
        // If from C to A: redirect left

        // If from A to C: redirect right
        // If from C to B: redirect right
        // If from B to A: redirect right
    }

    private void doRedirection()
    {
        // Set offset transforms (virtual position/orientation is set to zero)
        orientationOffset.localRotation = Quaternion.Inverse(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head));
        positionOffset.localPosition = -UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head);

        // Get current position and rotation (data from tracking system)
        Vector2 realWorldCurrentPosition = switchDimensions(Camera.main.transform.localPosition);
        Quaternion realWorldCurrentRotation = Camera.main.transform.localRotation;
        //Debug.Log("Current: " + realWorldCurrentPosition);

        // Calculate distance between real world current position and real world circle center point
        float distance = Vector2.Distance(realWorldCurrentPosition, currentCurve.getCircleCenter());
        //Debug.Log("Distance: " + distance);

        // Calculate angle between radius and distance
        float degreesWalkedOnRealCircle = Mathf.Acos((Vector2.Dot((switchDimensions(currentCurve.getCircleCenter()) - switchDimensions(currentJoint.getPosition())), (switchDimensions(currentCurve.getCircleCenter()) - realWorldCurrentPosition))) / ((switchDimensions(currentCurve.getCircleCenter()) - switchDimensions(currentJoint.getPosition())).magnitude * (switchDimensions(currentCurve.getCircleCenter()) - realWorldCurrentPosition).magnitude));
        //Debug.Log("degreesWalkedOnRealCircle: " + degreesWalkedOnRealCircle);

        // Multiply angle and radius = walked distance
        float walkedDistance = degreesWalkedOnRealCircle * currentCurve.getRadius();
        //Debug.Log("Walked: " + walkedDistance);

        // Calculate side drift: d-r
        float sideDrift = distance - currentCurve.getRadius();
        //Debug.Log("Side: " + sideDrift);

        // Calculate angle on virtual circle
        float degreesWalkedOnVirtualCircle = walkedDistance / currentPath.getRadius();
        //Debug.Log("degreesWalkedOnVirtualCircle: " + degreesWalkedOnVirtualCircle);

        if (redirectLeft)
        {
            Vector2 v1 = switchDimensions(currentIntersection.getPosition() - currentPath.getCircleCenter());
            v1 = v1 / v1.magnitude;
            //Debug.Log("V1: " + v1);

            Vector3 v2 = Quaternion.AngleAxis(-degreesWalkedOnVirtualCircle * Mathf.Rad2Deg, Vector3.up) * new Vector3(v1.x, 0, v1.y);
            v2 = v2 * (currentPath.getRadius() + sideDrift);
            Vector2 v3 = switchDimensions(v2);
            // Debug.Log("V3: " + v3);


            // Calculate point on virtual circle
            //Vector2 pointOnVirtualCircle = virtualWorldCircleCenter - new Vector2((rotationOfCurvePoint * new Vector3(-Mathf.Sin(degreesWalkedOnVirtualCircle), 0, Mathf.Cos(degreesWalkedOnVirtualCircle))).x, (rotationOfCurvePoint * new Vector3(-Mathf.Sin(degreesWalkedOnVirtualCircle), 0, Mathf.Cos(degreesWalkedOnVirtualCircle))).z) * virtualRadius;
            //Debug.Log("point: " + point);
            //Debug.Log("degreesWalkedOnRealCircle " + degreesWalkedOnRealCircle+"; );

            // Calculate and set new position (add sideDrift to point on virtual circle) and rotation
            this.transform.position = new Vector3(currentPath.getCircleCenter().x + v3.x, Camera.main.transform.localPosition.y, currentPath.getCircleCenter().y + v3.y); //new Vector3((pointOnVirtualCircle + ((pointOnVirtualCircle - virtualWorldCircleCenter) / (pointOnVirtualCircle - virtualWorldCircleCenter).magnitude) * sideDrift).x, Camera.main.transform.localPosition.y, (pointOnVirtualCircle + ((pointOnVirtualCircle - virtualWorldCircleCenter) / (pointOnVirtualCircle - virtualWorldCircleCenter).magnitude) * sideDrift).y);
            this.transform.rotation = /*globalStartRotation * */ Quaternion.Euler(Vector3.up * Mathf.Rad2Deg * (degreesWalkedOnRealCircle - degreesWalkedOnVirtualCircle)) * realWorldCurrentRotation;
        }
        else
        {
            Vector2 v1 = currentIntersection.getPosition() - currentPath.getCircleCenter();
            v1 = v1 / v1.magnitude;
            //Debug.Log("V1_: " + v1);

            Vector3 v2 = Quaternion.AngleAxis(degreesWalkedOnVirtualCircle * Mathf.Rad2Deg, Vector3.up) * new Vector3(v1.x, 0, v1.y);
            v2 = v2 * (currentPath.getRadius() + sideDrift);
            Vector2 v3 = switchDimensions(v2);
            //Debug.Log("V3_: " + v3);

            // Calculate point on virtual circle
            // Vector2 point = virtualWorldCircleCenter + new Vector2((rotationOfCurvePoint * new Vector3(Mathf.Sin(degreesWalkedOnVirtualCircle), 0, Mathf.Cos(degreesWalkedOnVirtualCircle))).x, (rotationOfCurvePoint * new Vector3(Mathf.Sin(degreesWalkedOnVirtualCircle), 0, Mathf.Cos(degreesWalkedOnVirtualCircle))).z) * virtualRadius;
            //Debug.Log("point: " + point);

            // Calculate and set new position and rotation
            this.transform.position = new Vector3(currentPath.getCircleCenter().x + v3.x, Camera.main.transform.localPosition.y, currentPath.getCircleCenter().y + v3.y);  //new Vector3((point + ((point - virtualWorldCircleCenter) / (point - virtualWorldCircleCenter).magnitude) * sideDrift).x, Camera.main.transform.localPosition.y, (point + ((point - virtualWorldCircleCenter) / (point - virtualWorldCircleCenter).magnitude) * sideDrift).y);
            this.transform.rotation = /*globalStartRotation * */ Quaternion.Euler(Vector3.up * Mathf.Rad2Deg * -(degreesWalkedOnRealCircle - degreesWalkedOnVirtualCircle)) * realWorldCurrentRotation;
        }
    }

    private void stopRedirection()
    {

    }

    Vector2 switchDimensions(Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            // Real world circles
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(jointPointA.getPosition(), 1f);
            Gizmos.DrawWireSphere(curveABsmallRadius.getCircleCenter(), curveABsmallRadius.getRadius());
            Gizmos.DrawWireSphere(curveABlargeRadius.getCircleCenter(), curveABlargeRadius.getRadius());
            Gizmos.DrawSphere(jointPointB.getPosition(), 1f);
            Gizmos.DrawWireSphere(curveBCsmallRadius.getCircleCenter(), curveBCsmallRadius.getRadius());
            Gizmos.DrawWireSphere(curveBClargeRadius.getCircleCenter(), curveBClargeRadius.getRadius());
            Gizmos.DrawSphere(jointPointC.getPosition(), 1f);
            Gizmos.DrawWireSphere(curveACsmallRadius.getCircleCenter(), curveACsmallRadius.getRadius());
            Gizmos.DrawWireSphere(curveAClargeRadius.getCircleCenter(), curveAClargeRadius.getRadius());

            // Virtual world circles
            //Gizmos.color = Color.green;
            //Gizmos.DrawSphere(pos1, 1f);
            //Gizmos.DrawWireSphere(circleCenter1, 3.75f);
        }
    }
}
