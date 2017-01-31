using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualPath {

    private Vector3 circleCenter;
    //private float angle;
    private float radius;
    private VirtualIntersection endPointA;
    private VirtualIntersection endPointB;
    private Curve curve;

    public VirtualPath(Vector3 circleCenter, float radius, Curve curve, VirtualIntersection endPointA, VirtualIntersection endPointB)
    {
        this.circleCenter = circleCenter;
        this.radius = radius;
        this.curve = curve;
        this.endPointA = endPointA;
        this.endPointB = endPointB;
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
