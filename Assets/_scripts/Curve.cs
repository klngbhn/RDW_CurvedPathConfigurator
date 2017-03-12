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

    public void init(Vector3 circleCenter, float radius, List<JointPoint> endPoints)
    {
        this.circleCenter = circleCenter;
        this.radius = radius;
        this.endPoints = endPoints;
        this.angle = calculateAngle();
    }

    private float calculateAngle()
    {
        Vector3 directionVector1 = endPoints[0].getPosition() - circleCenter;
        Vector3 directionVector2 = endPoints[1].getPosition() - circleCenter;

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
