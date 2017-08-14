using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A jointpoint is the (real world) crossing of several curves.
 * There are only three jointpoints in total.
 * */
[System.Serializable]
public class JointPoint : ScriptableObject
{

    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private Vector3[] walkingStartPositions;
    [SerializeField]
    private string label;
	[SerializeField]
	private float walkingZoneRadius;

	public void init(Vector3 position, string label, float walkingZoneRadius)
    {
        this.position = position;
        this.label = label;
		this.walkingZoneRadius = walkingZoneRadius;
        this.walkingStartPositions = new Vector3[4];
    }

    public Vector3 getPosition()
    {
        return position;
    }

    public string getLabel()
    {
        return label;
    }

	public float getWalkingZoneRadius()
	{
		return walkingZoneRadius;
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
