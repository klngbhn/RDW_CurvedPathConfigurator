using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A path is the (virtual world) connection between two intersections.
 * Each path has one curve as a real world counterpart.
 * */
[System.Serializable]
public class VirtualPath : ScriptableObject
{

    [SerializeField]
    private Vector3 circleCenter;
    [SerializeField]
    private float angle;
    [SerializeField]
    private float radius;
    [SerializeField]
    private float gain;
    [SerializeField]
    private List<VirtualIntersection> endPoints;
    [SerializeField]
    private Curve curve;

    public void init (Vector3 circleCenter, float gain, Curve curve, List<VirtualIntersection> endPoints)
    {
        this.circleCenter = circleCenter;
        this.gain = gain;
        this.radius = curve.getRadius() * this.gain;
        this.curve = curve;
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

    public List<VirtualIntersection> getEndPoints()
    {
        return endPoints;
    }

    public VirtualIntersection getOtherIntersection(VirtualIntersection intersection)
    {
        if (intersection.Equals(endPoints[0]))
            return endPoints[1];

        return endPoints[0];
    }

    public Curve getCurve()
    {
        return curve;
    }
}
