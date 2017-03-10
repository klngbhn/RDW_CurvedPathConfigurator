using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
 * This class handles the redirection.
 * It chooses the current path and curve itself according to the position of the user.
 * */
public class Redirection : MonoBehaviour {

    public Transform orientationOffset;
    public Transform positionOffset;

    public RedirectionDataStructure data;

    private JointPoint currentJoint;
    private VirtualIntersection currentIntersection;
    private Curve currentCurve;
    private VirtualPath currentPath;

    private bool ready = false;
    private bool redirectionStarted = false;
    private int redirectionDirection = 0;

    private float oldRotation = 0;

    void Start() {
        // Load data structure object (created by tool) from disk
        data = (RedirectionDataStructure)AssetDatabase.LoadAssetAtPath(@"Assets\Resources\data.asset", typeof(RedirectionDataStructure));
        if (data != null)
        {
            currentIntersection = data.intersections[0];
            currentJoint = currentIntersection.getJoint();
        }
        else
            Debug.Log("Data is null");
    }

    /*
     * Starts, stops, and executes the redirection.
     * In the beginning, "ready" button has to be pressed. Then, the user can start walking and
     * the redirection will be started and stopped automatically when crossing the intersections.
     * 
     * "Ready" button -> startRedirection -> stopRedirection
     * */
    void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
		{
            ready = true;
        }

        if (redirectionStarted)
        {
            doRedirection();

            // if user reaches one of the endpoints of current path
            //      stop redirection
            // TODO: Right now, redirection stops only at the end point, not at the start point
            Vector2 userPosition = switchDimensions(Camera.main.transform.position);
            Vector2 intersectionCenter = switchDimensions(currentPath.getOtherIntersection(currentIntersection).getPosition());
            float distance = Vector2.Distance(userPosition, intersectionCenter);
            if (distance < 0.25f)
                stopRedirection();
        }
        else if(ready)
        {
            // if user crosses boundary to one of the paths
            //      start redirection

            if (currentIntersection.getPath(0) != null)
            {
                if (isPathChosen(currentIntersection.getPath(0)))
                    startRedirection(currentIntersection.getPath(0));
            }
            if (currentIntersection.getPath(1) != null)
            {
                if (isPathChosen(currentIntersection.getPath(1)))
                    startRedirection(currentIntersection.getPath(1));
            }
            if (currentIntersection.getPath(2) != null)
            {
                if (isPathChosen(currentIntersection.getPath(2)))
                    startRedirection(currentIntersection.getPath(2));
            }
            if (currentIntersection.getPath(3) != null)
            {
                if (isPathChosen(currentIntersection.getPath(3)))
                    startRedirection(currentIntersection.getPath(3));
            }

        }
            
    }

    /*
     * Checks if user choosed to walk on the specified path.
     * */
    private bool isPathChosen(VirtualPath path)
    {
        int direction = getRedirectionDirection(currentJoint, path.getCurve());
        Vector3 directionToPathCircleCenter = (path.getCircleCenter() - currentIntersection.getPosition()).normalized;
        Vector3 rotatedDirectionVector = Quaternion.AngleAxis(-direction * 90f, Vector3.up) * directionToPathCircleCenter;
        Vector3 planePosition = currentIntersection.getPosition() + rotatedDirectionVector * 0.25f;
        Plane plane = new Plane(rotatedDirectionVector.normalized, planePosition);
        if (plane.GetSide(Camera.main.transform.position))
            return true;
        return false;
    }

    /*
     * Starts redirection on the specified path.
     * Sets current path and current curve as well as the redirection direction.
     * */
    private void startRedirection(VirtualPath path)
    {
        currentCurve = path.getCurve(); 
        currentPath = path; 
        redirectionDirection = this.getRedirectionDirection(currentJoint, currentCurve);
        redirectionStarted = true;
        Debug.Log("Redirection started with target: " + currentPath.getOtherIntersection(currentIntersection).getLabel());
    }

    /*
     * Returns the direction (left or right) to that the current path is bent.
     * -1 is left, 1 is right.
     * */
    private int getRedirectionDirection(JointPoint joint, Curve curve)
    {
        if (joint.Equals(data.jointPointA))
        {
            JointPoint endJoint = curve.getOtherJointPoint(joint);
            if (endJoint.Equals(data.jointPointB))
                return -1;
            if (endJoint.Equals(data.jointPointC))
                return 1;
        }
        if (joint.Equals(data.jointPointB))
        {
            JointPoint endJoint = curve.getOtherJointPoint(joint);
            if (endJoint.Equals(data.jointPointA))
                return 1;
            if (endJoint.Equals(data.jointPointC))
                return -1;
        }
        if (joint.Equals(data.jointPointC))
        {
            JointPoint endJoint = curve.getOtherJointPoint(joint);
            if (endJoint.Equals(data.jointPointB))
                return 1;
            if (endJoint.Equals(data.jointPointA))
                return -1;
        }

        return 0;
    }

    /*
     * Executes the redirection according to the chosen curve and path by
     * calculating a position and orientation for the virtual camera.
     * */
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
        float distance = Vector2.Distance(realWorldCurrentPosition, switchDimensions(currentCurve.getCircleCenter()));
        //Debug.Log("Distance: " + distance);

        // Calculate angle between radius and distance
        //Vector2.Angle(switchDimensions(switchDimensions(currentJoint.getPosition() - currentCurve.getCircleCenter())), realWorldCurrentPosition - switchDimensions(currentCurve.getCircleCenter()));
        float angleWalkedOnRealCircle = Mathf.Acos((Vector2.Dot((switchDimensions(currentCurve.getCircleCenter()) - switchDimensions(currentJoint.getPosition())), (switchDimensions(currentCurve.getCircleCenter()) - realWorldCurrentPosition))) / ((switchDimensions(currentCurve.getCircleCenter()) - switchDimensions(currentJoint.getPosition())).magnitude * (switchDimensions(currentCurve.getCircleCenter()) - realWorldCurrentPosition).magnitude));
        //Debug.Log("angleWalkedOnRealCircle: " + angleWalkedOnRealCircle);

        // Multiply angle and radius = walked distance
        float walkedDistance = angleWalkedOnRealCircle * currentCurve.getRadius();
        //Debug.Log("Walked: " + walkedDistance);

        // Calculate side drift: d-r
        float sideDrift = distance - currentCurve.getRadius();
        //Debug.Log("Side: " + sideDrift);

        // Calculate angle on virtual circle
        float angleWalkedOnVirtualCircle = walkedDistance / currentPath.getRadius();
         //Debug.Log("angleWalkedOnVirtualCircle: " + angleWalkedOnVirtualCircle);

        // Calculate direction from virtual circle center to intersection
        Vector2 directionToIntersection = switchDimensions(currentIntersection.getPosition() - currentPath.getCircleCenter()).normalized;
        //Debug.Log("directionToIntersection: " + directionToIntersection);

        // Calculate virtual position by rotating direction by angle walked
        Vector3 virtualPosition = Quaternion.AngleAxis(redirectionDirection * angleWalkedOnVirtualCircle * Mathf.Rad2Deg, Vector3.up) * new Vector3(directionToIntersection.x, 0, directionToIntersection.y);
        // Add side drift to virtual position
        virtualPosition = virtualPosition * (currentPath.getRadius() + sideDrift);
        //Debug.Log("virtualPosition: " + virtualPosition);

        // Calculate and set virtual position
        this.transform.position = new Vector3(currentPath.getCircleCenter().x + virtualPosition.x, Camera.main.transform.localPosition.y, currentPath.getCircleCenter().z + virtualPosition.z); 
        //Debug.Log(this.transform.position);

        // Calculate and set virtual rotation: redirection + current camera rotation + old rotation
        float redirection = Mathf.Rad2Deg * -redirectionDirection * (angleWalkedOnRealCircle - angleWalkedOnVirtualCircle);
        float y = redirection + realWorldCurrentRotation.eulerAngles.y + oldRotation;
        Quaternion virtualRotation = Quaternion.Euler(realWorldCurrentRotation.eulerAngles.x, y, realWorldCurrentRotation.eulerAngles.z);
        this.transform.rotation = virtualRotation;
    }

    /*
     * Stops the redirection.
     * Sets current intersection and current joint to the next point.
     * */
    private void stopRedirection()
    {
        currentIntersection = currentPath.getOtherIntersection(currentIntersection);
        currentJoint = currentIntersection.getJoint();
        oldRotation += currentPath.getAngle() * -redirectionDirection;
        redirectionStarted = false;
        Debug.Log("Redirection stopped at joint: " + currentJoint.getLabel() + ", intersection: " + currentIntersection.getLabel());
    }

    /*
     * Switches a 3-dimensional vector to a 2-dimensional vector by removing the y coordinate.
     * */ 
    private Vector2 switchDimensions(Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }
}
