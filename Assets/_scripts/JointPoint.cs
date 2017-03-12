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
    private string label;

    public void init(Vector3 position, string label)
    {
        this.position = position;
        this.label = label;
    }

    public Vector3 getPosition()
    {
        return position;
    }

    public string getLabel()
    {
        return label;
    }
}
