using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointPoint {

    private Vector3 position;
    private List<Curve> curves;

    public JointPoint(Vector3 position)
    {
        this.position = position;
        curves = new List<Curve>();
    }

    public Vector3 getPosition()
    {
        return position;
    }

    public void addCurve(Curve curve)
    {
        curves.Add(curve);
    }
}
