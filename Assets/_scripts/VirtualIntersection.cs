using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * An intersection is the (virtual world) crossing of several paths.
 * Each intersection has one jointpoint as a real world counterpart.
 * */
[System.Serializable]
public class VirtualIntersection : ScriptableObject
{

    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private JointPoint jointPoint;
    [SerializeField]
    private VirtualPath[] paths;
    [SerializeField]
    private Vector3[] walkingStartPositions;
    [SerializeField]
    private string label;

    public void init(Vector3 position, JointPoint jointPoint, string label)
    {
        this.position = position;
        this.jointPoint = jointPoint;
        this.label = label;
        this.paths = new VirtualPath[4];
        this.walkingStartPositions = new Vector3[4];
    }

    public Vector3 getPosition()
    {
        return position;
    }

    public string getLabel()
    {
        return this.label + " (" + jointPoint.getLabel() + ")";
    }

    public JointPoint getJoint()
    {
        return jointPoint;
    }

    public void addPath(VirtualPath path, int direction)
    {
        if (paths[direction] == null)
            paths[direction] = path;
    }

    public VirtualPath getPath(int direction)
    {
        return paths[direction];
    }

    public void setWalkingStartPosition(int curveIndex, Vector3 position)
    {
        this.walkingStartPositions[curveIndex] = position;
    }

    public Vector3 getWalkingStartPosition(int curveIndex)
    {
        return this.walkingStartPositions[curveIndex];
    }
}
