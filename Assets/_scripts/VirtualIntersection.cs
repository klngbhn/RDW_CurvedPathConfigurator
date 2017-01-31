using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualIntersection {

    private Vector3 position;
    private JointPoint jointPoint;
    private List<VirtualPath> paths;

    public VirtualIntersection(Vector3 position, JointPoint jointPoint)
    {
        this.position = position;
        this.jointPoint = jointPoint;
    }

    public Vector3 getPosition()
    {
        return position;
    }
}
