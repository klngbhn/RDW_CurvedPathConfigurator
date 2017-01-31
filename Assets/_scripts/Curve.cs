using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve {

    private Vector3 circleCenter;
    //private float angle;
    private float radius;
    private List<JointPoint> endPoints;

    public Curve(Vector3 circleCenter, float radius, List<JointPoint> endPoints)
    {
        this.circleCenter = circleCenter;
       // TODO: this.angle = angle;
        this.radius = radius;
        this.endPoints = endPoints;
    }

    public Vector3 getCircleCenter()
    {
        return circleCenter;
    }

    public float getRadius()
    {
        return radius;
    }
}
