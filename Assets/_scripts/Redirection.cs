using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class handles the redirection.
 * It chooses the current path and curve itself according to the position of the user.
 * */
public class Redirection : MonoBehaviour {

    public Transform orientationOffset;
    public Transform positionOffset;

    public RedirectionDataStructure data;

    private string pathLayoutAsset = "data";

    private JointPoint currentJoint;
    private VirtualIntersection currentIntersection;
    private Curve currentCurve;
    private VirtualPath currentPath;

    public bool ready = false;
    private bool redirectionStarted = false;
    private int redirectionDirection = 0;

	private float angleWalkedOnRealCircle = 0;
	private float angleWalkedOnVirtualCircle = 0;
    private float oldRotation = 0;

    void Start() {
        Debug.Log("Application started");
		if (data == null)
			this.loadPathLayout (pathLayoutAsset);
		else {
			currentIntersection = data.intersections [0];
			currentJoint = currentIntersection.getJoint ();
		}
    }

	/*
	 * Loads path layout asset with the specified name from a resources folder.
	 * */
    public void loadPathLayout(string layoutName)
    {
        pathLayoutAsset = layoutName;

        // Load data structure object (created by tool) from disk
        data = (RedirectionDataStructure)Resources.LoadAll(pathLayoutAsset)[0] as RedirectionDataStructure;
        if (data != null)
        {
            //Debug.Log("Label: " + data.jointPointA.getLabel());
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

        if (redirectionStarted)
        {
            doRedirection();

            // if user reaches one of the endpoints of current path
            //      stop redirection

            // Is user stopping at end intersection?
            if (isPathLeft(currentPath.getOtherIntersection(currentIntersection)))
                stopRedirection(currentPath.getOtherIntersection(currentIntersection), true);

            // Is user stopping at start intersection?
            else if (isPathLeft(currentIntersection))
                stopRedirection(currentIntersection, false);
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
     * Checks if user reached an intersection.
     * */
	private bool isPathLeft(VirtualIntersection intersection)
    {
        int direction = getRedirectionDirection(intersection.getJoint(), currentPath.getCurve());
        int pathIndex = data.getPathIndex(intersection.getJoint(), currentPath.getCurve());

		Vector3 directionToPathCircleCenter = (currentPath.getCurve().getCircleCenter() - intersection.getJoint().getWalkingStartPosition(pathIndex)).normalized;
		Vector3 rotatedDirectionVector = Quaternion.AngleAxis(-direction * 90f, Vector3.up) * directionToPathCircleCenter;
		Plane plane = new Plane(rotatedDirectionVector.normalized, intersection.getJoint().getWalkingStartPosition(pathIndex));
		if (plane.GetSide(Camera.main.transform.localPosition))
            return false;
        return true;
    }

    /*
     * Checks if user choosed to walk on the specified path.
     * */
    private bool isPathChosen(VirtualPath path)
    {
        int direction = getRedirectionDirection(currentJoint, path.getCurve());
        int pathIndex = data.getPathIndex(currentJoint, path.getCurve());

		Vector3 directionToCurveCircleCenter = (path.getCurve().getCircleCenter() - currentJoint.getWalkingStartPosition(pathIndex)).normalized;
		Vector3 rotatedDirectionVector = Quaternion.AngleAxis(-direction * 90f, Vector3.up) * directionToCurveCircleCenter;
		Plane plane = new Plane(rotatedDirectionVector.normalized, currentJoint.getWalkingStartPosition(pathIndex));
		if (plane.GetSide(Camera.main.transform.localPosition))
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

        int pathIndex = data.getPathIndex(currentJoint, currentCurve);

        // Calculate distance between real world current position and real world circle center point
        float distance = Vector2.Distance(realWorldCurrentPosition, switchDimensions(currentCurve.getCircleCenter()));
        //Debug.Log("Distance: " + distance);

        // Calculate angle between start and current position
        //float angleWalkedOnRealCircle = Vector2.Angle(switchDimensions(switchDimensions(currentJoint.getPosition() + redirectionDirection*data.calculateOffset(currentJoint.getPosition(), currentCurve.getCircleCenter()) - currentCurve.getCircleCenter())), realWorldCurrentPosition - switchDimensions(currentCurve.getCircleCenter()));
        Vector3 realWalkingStartPosition = currentJoint.getWalkingStartPosition(pathIndex);
		angleWalkedOnRealCircle = Mathf.Acos((Vector2.Dot((switchDimensions(currentCurve.getCircleCenter()) - switchDimensions(realWalkingStartPosition)), (switchDimensions(currentCurve.getCircleCenter()) - realWorldCurrentPosition))) / ((switchDimensions(currentCurve.getCircleCenter()) - switchDimensions(realWalkingStartPosition)).magnitude * (switchDimensions(currentCurve.getCircleCenter()) - realWorldCurrentPosition).magnitude));
        //Debug.Log("angleWalkedOnRealCircle: " + angleWalkedOnRealCircle);

        // Multiply angle and radius = walked distance
		float walkedDistance = angleWalkedOnRealCircle * currentCurve.getRadius();
        //Debug.Log("Walked: " + walkedDistance);

        // Calculate side drift: d-r
        float sideDrift = distance - currentCurve.getRadius();
        //Debug.Log("Side: " + sideDrift);

        // Calculate angle on virtual circle
        angleWalkedOnVirtualCircle = walkedDistance / currentPath.getRadius();
         //Debug.Log("angleWalkedOnVirtualCircle: " + angleWalkedOnVirtualCircle);

		// Calculate direction from virtual circle center to intersection // - redirectionDirection*data.calculateOffset(currentIntersection.getPosition(), currentPath.getCircleCenter())
		Vector3 virtualWalkingStartPosition = currentIntersection.getWalkingStartPosition(pathIndex);
        Vector2 directionToIntersection = switchDimensions(virtualWalkingStartPosition - currentPath.getCircleCenter()).normalized;
        //Debug.Log("directionToIntersection: " + directionToIntersection);

        // Calculate direction to virtual position by rotating direction by angle walked
        Vector3 directionToVirtualPosition = Quaternion.AngleAxis(redirectionDirection * angleWalkedOnVirtualCircle * Mathf.Rad2Deg, Vector3.up) * new Vector3(directionToIntersection.x, 0, directionToIntersection.y);
        // Multiply direction by radius + side drift
		directionToVirtualPosition = directionToVirtualPosition * (currentPath.getRadius() + sideDrift);
        //Debug.Log("virtualPosition: " + virtualPosition);

        // Calculate and set virtual position
        this.transform.position = new Vector3(currentPath.getCircleCenter().x + directionToVirtualPosition.x, Camera.main.transform.localPosition.y, currentPath.getCircleCenter().z + directionToVirtualPosition.z); 
        //Debug.Log(this.transform.position);

        // Calculate and set virtual rotation: redirection + current camera rotation + old rotation
        float redirection = Mathf.Rad2Deg * -redirectionDirection * (angleWalkedOnRealCircle - angleWalkedOnVirtualCircle);
        float y = redirection + realWorldCurrentRotation.eulerAngles.y + oldRotation;
        Quaternion virtualRotation = Quaternion.Euler(realWorldCurrentRotation.eulerAngles.x, y, realWorldCurrentRotation.eulerAngles.z);
        this.transform.rotation = virtualRotation;
    }

    /*
     * Stops the redirection at the given intersection.
     * Sets current intersection and current joint to the next point.
     * If this intersection is not the same intersection the user started, the rotation angle is added (bool addAngle).
     * */
    private void stopRedirection(VirtualIntersection intersection, bool addAngle)
    {
        currentIntersection = intersection; 
        currentJoint = currentIntersection.getJoint();
		if (addAngle)
			oldRotation += Mathf.Rad2Deg * -redirectionDirection * (angleWalkedOnRealCircle - angleWalkedOnVirtualCircle);//(Mathf.Abs(currentCurve.getAngle()) - Mathf.Abs(currentPath.getAngle())) * -redirectionDirection;
        redirectionStarted = false;
        Debug.Log("Redirection stopped at joint: " + currentJoint.getLabel() + ", intersection: " + currentIntersection.getLabel() + " (old rotation: " + oldRotation + ")");
    }

    /*
     * Switches a 3-dimensional vector to a 2-dimensional vector by removing the y coordinate.
     * */ 
    private Vector2 switchDimensions(Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }
}
