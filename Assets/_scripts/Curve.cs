using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A curve is the (real world) connection between two jointpoints.
 * There are only six curves in total.
 * */
[System.Serializable]
public class Curve : ScriptableObject
{

    [SerializeField]
    private Vector3 circleCenter;
    [SerializeField]
    private float angle;
    [SerializeField]
    private float radius;
    [SerializeField]
    private List<JointPoint> endPoints;

    public void init(Vector3 circleCenter, float radius, List<JointPoint> endPoints, bool isSmallCurve)
    {
        this.circleCenter = circleCenter;
        this.radius = radius;
        this.endPoints = endPoints;
		this.angle = calculateAngle(isSmallCurve);
    }

	private float calculateAngle(bool isSmallCurve)
    {
        Vector3[] walkingStartPositions = new Vector3[2];

        // Calculate angle for walking zone
        float angle = Mathf.Rad2Deg * (endPoints[0].getWalkingZoneRadius() / radius);

        // Calculate direction vectors to walking start positions
        Vector3 directionToStartJoint = (endPoints[0].getPosition() - circleCenter).normalized;
        Vector3 rotatedDirectionToStartJoint = Quaternion.AngleAxis(-angle, Vector3.up) * directionToStartJoint;
        Vector3 directionToEndJoint = (endPoints[1].getPosition() - circleCenter).normalized;
        Vector3 rotatedDirectionToEndJoint = Quaternion.AngleAxis(angle, Vector3.up) * directionToEndJoint;

        // Set walking start positions
        walkingStartPositions[0] = circleCenter + rotatedDirectionToStartJoint * radius;
        walkingStartPositions[1] = circleCenter + rotatedDirectionToEndJoint * radius;

		if (isSmallCurve) {
            endPoints[0].setWalkingStartPosition(1, walkingStartPositions[0]);
            endPoints[1].setWalkingStartPosition(2, walkingStartPositions[1]);
		}
        else
        {
            endPoints[0].setWalkingStartPosition(0, walkingStartPositions[0]);
            endPoints[1].setWalkingStartPosition(3, walkingStartPositions[1]);
        }

        // Finally, calculate angle of curve
        Vector3 directionVector1 = walkingStartPositions[0] - circleCenter;
        Vector3 directionVector2 = walkingStartPositions[1] - circleCenter;

        return Vector3.Angle(directionVector1, directionVector2);
    }

    public Vector3 getCircleCenter()
    {
        return circleCenter;
    }

    public float getRadius()
    {
        return radius;
    }

    public float getAngle()
    {
        return angle;
    }

    public List<JointPoint> getEndPoints()
    {
        return endPoints;
    }

    public JointPoint getOtherJointPoint(JointPoint joint)
    {
        if (joint.Equals(endPoints[0]))
            return endPoints[1];

        return endPoints[0];
    }
}
