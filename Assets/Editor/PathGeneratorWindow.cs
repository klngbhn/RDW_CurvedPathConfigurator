using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * This class draws the gui of the path generator window.
 * */
[System.Serializable]
public class PathGeneratorWindow : EditorWindow {

    private RedirectionDataStructure data;

    private float gain = 2;
    private Vector3 jointAPosition = new Vector3(0.55f, 0, -1.25f);
    private Vector3 jointBPosition = new Vector3(0.55f, 0, 1.25f);
    private Vector3 jointCPosition = new Vector3(-1.615f, 0, 0);

    private int intersectionIndex = 0;
    private string[] curveOptions = new string[] { "" };
    private int curveIndex = 0;
    private int joint = 0;

    private bool showCurves = true;
    private bool showJoints = true;
    private bool showPaths = true;
    private bool showIntersections = true;

    private bool hideJointsGui = false;
    private bool generateAutomatically = false;
    private Vector2 scrollPos;
    private float sideLength = 4;

    [MenuItem("Window/Path Generator")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PathGeneratorWindow));
    }

    /*
     * Load (or create if it does not exist) the data structure object.
     * Load all the gui specific variables saved to editorprefs.
     * */
    private void OnEnable()
    {
        data = (RedirectionDataStructure)AssetDatabase.LoadAssetAtPath(@"Assets/Resources/data.asset", typeof(RedirectionDataStructure));
        gain = EditorPrefs.GetFloat("gain", gain);
        sideLength = EditorPrefs.GetFloat("sideLength", sideLength);
        intersectionIndex = EditorPrefs.GetInt("intersectionIndex", intersectionIndex);
        curveIndex = EditorPrefs.GetInt("curveIndex", curveIndex);
        joint = EditorPrefs.GetInt("joint", joint);
        showCurves = EditorPrefs.GetBool("showCurves", showCurves);
        showJoints = EditorPrefs.GetBool("showJoints", showJoints);
        showPaths = EditorPrefs.GetBool("showPaths", showPaths);
        showIntersections = EditorPrefs.GetBool("showIntersections", showIntersections);
        hideJointsGui = EditorPrefs.GetBool("hideJointsGui", hideJointsGui);
        generateAutomatically = EditorPrefs.GetBool("generateAutomatically", generateAutomatically);

        if (data == null)
        {
            Debug.Log("Created new data resource");
            data = CreateInstance<RedirectionDataStructure>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources/"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateAsset(data, "Assets/Resources/data.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            gain = 2;
            sideLength = 4;
            intersectionIndex = 0;
            curveIndex = 0;
            joint = 0;
            showCurves = true;
            showJoints = true;
            showPaths = true;
            showIntersections = true;
            hideJointsGui = false;
            generateAutomatically = false;
        }
    }

    /*
     * Save all the gui specific variables to editorprefs.
     * */
    private void OnDisable()
    {
        EditorPrefs.SetFloat("gain", gain);
        EditorPrefs.SetFloat("sideLength", sideLength);
        EditorPrefs.SetInt("intersectionIndex", intersectionIndex);
        EditorPrefs.SetInt("curveIndex", curveIndex);
        EditorPrefs.SetInt("joint", joint);
        EditorPrefs.SetBool("showCurves", showCurves);
        EditorPrefs.SetBool("showJoints", showJoints);
        EditorPrefs.SetBool("showPaths", showPaths);
        EditorPrefs.SetBool("showIntersections", showIntersections);
        EditorPrefs.SetBool("hideJointsGui", hideJointsGui);
        EditorPrefs.SetBool("generateAutomatically", generateAutomatically);
    }

    /*
     * Draws the gui of the editor window.
     * */
    void OnGUI () {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Data Object", EditorStyles.boldLabel);
        GUI.enabled = false;
        data = (RedirectionDataStructure)EditorGUILayout.ObjectField(data, typeof(object), false);
        GUI.enabled = true;

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        bool oldShowCurves = showCurves;
        showCurves = EditorGUILayout.ToggleLeft("Show Curves", showCurves);
        if (oldShowCurves != showCurves)
            SceneView.RepaintAll();
        bool oldShowJoints = showJoints;
        showJoints = EditorGUILayout.ToggleLeft("Show JointPoints", showJoints);
        if (oldShowJoints != showJoints)
            SceneView.RepaintAll();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        bool oldShowPaths = showPaths;
        showPaths = EditorGUILayout.ToggleLeft("Show Virtual Paths", showPaths);
        if (oldShowPaths != showPaths)
            SceneView.RepaintAll();
        bool oldShowIntersections = showIntersections;
        showIntersections = EditorGUILayout.ToggleLeft("Show Virtual Intersections", showIntersections);
        if (oldShowIntersections != showIntersections)
            SceneView.RepaintAll();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Resetting means removing all joints, curves, intersections, and paths!", MessageType.Warning);

        if (GUILayout.Button("Reset all", GUILayout.Width(100)))
        {
            intersectionIndex = 0;
            curveIndex = 0;
            data.intersections = new List<VirtualIntersection>();
            data.paths = new List<VirtualPath>();
            data.startJoint = null;
            data.jointPointA = null;
            data.jointPointB = null;
            data.jointPointC = null;
            data.curveABlargeRadius = null;
            data.curveABsmallRadius = null;
            data.curveAClargeRadius = null;
            data.curveACsmallRadius = null;
            data.curveBClargeRadius = null;
            data.curveBCsmallRadius = null;
            hideJointsGui = false;
            jointAPosition = new Vector3(0.55f, 0, -1.25f);
            jointBPosition = new Vector3(0.55f, 0, 1.25f);
            jointCPosition = new Vector3(-1.615f, 0, 0);
            this.saveChanges();
            SceneView.RepaintAll();
        }

        if (hideJointsGui)
            GUI.enabled = false;

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Jointpoints", EditorStyles.boldLabel);

        generateAutomatically = EditorGUILayout.ToggleLeft("Generate jointpoints automatically according to tracking space", generateAutomatically);
        if (!generateAutomatically)
            GUI.enabled = false;
        sideLength = EditorGUILayout.FloatField("Side length", sideLength, GUILayout.Width(200));
        GUI.enabled = true;

        if (generateAutomatically)
            GUI.enabled = false;
        jointAPosition = EditorGUILayout.Vector3Field("Joint A", jointAPosition);
        jointBPosition = EditorGUILayout.Vector3Field("Joint B", jointBPosition);
        jointCPosition = EditorGUILayout.Vector3Field("Joint C", jointCPosition);
        GUI.enabled = true;

        if (hideJointsGui)
            GUI.enabled = false;
        if (GUILayout.Button("Create curves", GUILayout.Width(100)))
        {
            hideJointsGui = true;
            if (generateAutomatically)
            {
                calculateJointPositions(sideLength, 0.2f);
                data.initJointsAndCurves(jointAPosition, jointBPosition, jointCPosition);
            }
            else
                data.initJointsAndCurves(jointAPosition, jointBPosition, jointCPosition);
            this.saveChanges();
        }
        GUI.enabled = true;

        if (!hideJointsGui)
            GUI.enabled = false;

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Start Position", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Choosing a new start joint, removes all existing intersections and paths!", MessageType.Warning);

        joint = EditorGUILayout.Popup("Start Joint", joint, new string[] { "A", "B", "C" }, GUILayout.Width(300));
        if (GUILayout.Button("Set start joint", GUILayout.Width(100)))
        {
            if (joint == 0)
                data.setStartPosition(data.jointPointA);
            if (joint == 1)
                data.setStartPosition(data.jointPointB);
            if (joint == 2)
                data.setStartPosition(data.jointPointC);
            intersectionIndex = 0;
            curveIndex = 0;
            this.saveChanges();
        }

        if (intersectionIndex > data.intersections.Count)
            intersectionIndex = 0;

        // Show path creation fields only when a start point was chosen
        if (data.intersections.Count > 0 && intersectionIndex < data.intersections.Count)
            GUI.enabled = true;
        else
            GUI.enabled = false;

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Virtual Paths", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Active intersection is marked yellow in the scene view.", MessageType.Info);

        string[] intersectionOptions = new string[data.intersections.Count];
        int i = 0;
        foreach (VirtualIntersection intersection in data.intersections)
        {
            intersectionOptions[i] = intersection.getLabel();
            i++;
        }
        int oldIntersectionIndex = intersectionIndex;
        intersectionIndex = EditorGUILayout.Popup("Intersection", intersectionIndex, intersectionOptions, GUILayout.Width(400));
        if (oldIntersectionIndex != intersectionIndex)
        {
            curveIndex = 0;
            SceneView.RepaintAll();
        }

        curveIndex = EditorGUILayout.Popup("Curve", curveIndex, calculateCurveOptions(intersectionIndex), GUILayout.Width(400));
        gain = EditorGUILayout.FloatField("Gain", gain); // TODO: Limit this to detection thresholds!

        if (curveOptions.Length == 0 || curveOptions[curveIndex]  == "")
            GUI.enabled = false;
        if (GUILayout.Button("Create Path", GUILayout.Width(100)))
        {
            data.createPathAndIntersection(data.intersections[intersectionIndex], this.getCurve(curveIndex, intersectionIndex), gain);
            curveIndex = 0;
            this.saveChanges();
            this.Repaint();
        }

        GUI.enabled = true;
        EditorGUILayout.EndScrollView();
    }

    /*
     * Save changes of the data structure object to disk.
     * */
    private void saveChanges()
    {
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /*
     * Calculates joint positions according to the given side length.
     * Side length should match the size of the tracking space.
     * Overwrites joint positions from gui field.
     * */
    public void calculateJointPositions(float sideLength, float safetyDistance)
    {
        float smallRadius = (Mathf.Sqrt(3) / (Mathf.Sqrt(3) + 1)) * (sideLength / 2f);
        float largeRadius = 2 * smallRadius;
        float heightOfTriangle = (largeRadius / 2) * Mathf.Sqrt(3);

        jointAPosition = new Vector3((sideLength / 2) - smallRadius - safetyDistance, 0, -smallRadius);
        jointBPosition = new Vector3((sideLength / 2) - smallRadius - safetyDistance, 0, smallRadius);
        jointCPosition = new Vector3((sideLength / 2) - smallRadius - heightOfTriangle - safetyDistance, 0, 0);
    }

    /*
     * Calculates the available curve options based on the chosen intersection and the already existing paths.
     * */
    private string[] calculateCurveOptions(int index)
    {
        if (data.intersections.Count == 0)
        {
            curveIndex = 0;
            return new string[] { "" };
        }

        if (data.intersections[index].getJoint().Equals(data.jointPointA))
        {
            List<string> options = new List<string>();
            if (data.intersections[index].getPath(0) == null)
                options.Add("Curve AB large radius");
            if (data.intersections[index].getPath(1) == null)
                options.Add("Curve AB small radius");
            if (data.intersections[index].getPath(2) == null)
                options.Add("Curve AC small radius");
            if (data.intersections[index].getPath(3) == null)
                options.Add("Curve AC large radius");

            curveOptions = options.ToArray();
            if (data.intersections[index].getPath(0) == null && data.intersections[index].getPath(1) == null && data.intersections[index].getPath(2) == null && data.intersections[index].getPath(3) == null)
                curveOptions = new string[4] { "Curve AB small radius", "Curve AB large radius", "Curve AC small radius", "Curve AC large radius" };
        }
        if (data.intersections[index].getJoint().Equals(data.jointPointB))
        {
            List<string> options = new List<string>();
            if (data.intersections[index].getPath(0) == null)
                options.Add("Curve BC large radius");
            if (data.intersections[index].getPath(1) == null)
                options.Add("Curve BC small radius");
            if (data.intersections[index].getPath(2) == null)
                options.Add("Curve AB small radius");
            if (data.intersections[index].getPath(3) == null)
                options.Add("Curve AB large radius");

            curveOptions = options.ToArray();
            if (data.intersections[index].getPath(0) == null && data.intersections[index].getPath(1) == null && data.intersections[index].getPath(2) == null && data.intersections[index].getPath(3) == null)
                curveOptions = new string[4] { "Curve AB small radius", "Curve AB large radius", "Curve BC small radius", "Curve BC large radius" };
        }
        if (data.intersections[index].getJoint().Equals(data.jointPointC))
        {
            List<string> options = new List<string>();
            if (data.intersections[index].getPath(0) == null)
                options.Add("Curve AC large radius");
            if (data.intersections[index].getPath(1) == null)
                options.Add("Curve AC small radius");
            if (data.intersections[index].getPath(2) == null)
                options.Add("Curve BC small radius");
            if (data.intersections[index].getPath(3) == null)
                options.Add("Curve BC large radius");

            curveOptions = options.ToArray();
            if (data.intersections[index].getPath(0) == null && data.intersections[index].getPath(1) == null && data.intersections[index].getPath(2) == null && data.intersections[index].getPath(3) == null)
                curveOptions = new string[4] { "Curve AC small radius", "Curve AC large radius", "Curve BC small radius", "Curve BC large radius" };
        }

        return curveOptions;
    }

    /*
     * Returns the curve object according to the chosen index.
     * */
    private Curve getCurve(int curveIndex, int intersectionIndex)
    {
        string chosenCurveName = curveOptions[curveIndex];

        if (chosenCurveName == "Curve AB large radius")
            return data.curveABlargeRadius;
        if (chosenCurveName == "Curve AB small radius")
            return data.curveABsmallRadius;
        if (chosenCurveName == "Curve AC large radius")
            return data.curveAClargeRadius;
        if (chosenCurveName == "Curve AC small radius")
            return data.curveACsmallRadius;
        if (chosenCurveName == "Curve BC large radius")
            return data.curveBClargeRadius;
        if (chosenCurveName == "Curve BC small radius")
            return data.curveBCsmallRadius;

        return data.curveABsmallRadius;
    }

    void OnFocus()
    {
        // Remove and re-add the listener to make sure there is always one attached
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }

    void OnDestroy()
    {
        // Remove the listener to stop drawing
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    /*
     * Draws all joint points, curves, paths, and intersections to the scene view.
     * */
    void OnSceneGUI(SceneView sceneView)
    {
        if (data == null)
            return;
        if (data.jointPointA == null)
            return;
        if (data.jointPointB == null)
            return;
        if (data.jointPointC == null)
            return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;

        // Real world joint points
        if (showJoints)
        {
            Handles.color = Color.red;
            Handles.SphereCap(0, data.jointPointA.getPosition(), Quaternion.identity, 0.1f);
            Handles.Label(data.jointPointA.getPosition(), data.jointPointA.getLabel(), style);
            Handles.SphereCap(0, data.jointPointB.getPosition(), Quaternion.identity, 0.1f);
            Handles.Label(data.jointPointB.getPosition(), data.jointPointB.getLabel(), style);
            Handles.SphereCap(0, data.jointPointC.getPosition(), Quaternion.identity, 0.1f);
            Handles.Label(data.jointPointC.getPosition(), data.jointPointC.getLabel(), style);
        }

        // Real world curves
        if (showCurves)
        {
            Handles.color = Color.red;
            Vector3 directionVector = data.jointPointB.getPosition() - data.curveABsmallRadius.getCircleCenter();
            Handles.DrawWireArc(data.curveABsmallRadius.getCircleCenter(), Vector3.up, directionVector, data.curveABsmallRadius.getAngle(), data.curveABsmallRadius.getRadius());
            directionVector = data.jointPointB.getPosition() - data.curveABlargeRadius.getCircleCenter();
            Handles.DrawWireArc(data.curveABlargeRadius.getCircleCenter(), Vector3.up, directionVector, data.curveABlargeRadius.getAngle(), data.curveABlargeRadius.getRadius());
            directionVector = data.jointPointC.getPosition() - data.curveBCsmallRadius.getCircleCenter();
            Handles.DrawWireArc(data.curveBCsmallRadius.getCircleCenter(), Vector3.up, directionVector, data.curveBCsmallRadius.getAngle(), data.curveBCsmallRadius.getRadius());
            directionVector = data.jointPointC.getPosition() - data.curveBClargeRadius.getCircleCenter();
            Handles.DrawWireArc(data.curveBClargeRadius.getCircleCenter(), Vector3.up, directionVector, data.curveBClargeRadius.getAngle(), data.curveBClargeRadius.getRadius());
            directionVector = data.jointPointA.getPosition() - data.curveACsmallRadius.getCircleCenter();
            Handles.DrawWireArc(data.curveACsmallRadius.getCircleCenter(), Vector3.up, directionVector, data.curveACsmallRadius.getAngle(), data.curveACsmallRadius.getRadius());
            directionVector = data.jointPointA.getPosition() - data.curveAClargeRadius.getCircleCenter();
            Handles.DrawWireArc(data.curveAClargeRadius.getCircleCenter(), Vector3.up, directionVector, data.curveAClargeRadius.getAngle(), data.curveAClargeRadius.getRadius());
        }

        // Virtual world intersections
        if (showIntersections)
        {
            Handles.color = Color.green;
            foreach (VirtualIntersection intersection in data.intersections)
            {
                // Change color for currently chosen intersection
                if (intersection.Equals(data.intersections[intersectionIndex]))
                    Handles.color = Color.yellow;
                else
                    Handles.color = Color.green;

                Handles.SphereCap(0, intersection.getPosition(), Quaternion.identity, 0.5f);
                Handles.Label(intersection.getPosition(), intersection.getLabel(), style);
            }
        }

        // Virtual world paths
        if (showPaths)
        {
            Handles.color = Color.green;
            foreach (VirtualPath path in data.paths)
            {
                Vector3 directionVector = path.getEndPoints()[0].getPosition() - path.getCircleCenter();
                int sign = data.getSignOfCurve(path.getEndPoints()[0].getJoint(), path.getEndPoints()[1].getJoint());
                Handles.DrawWireArc(path.getCircleCenter(), Vector3.up, directionVector, sign * path.getAngle(), path.getRadius());
            }
        }

        Handles.BeginGUI();
        // GUI stuff comes here
        Handles.EndGUI();
    }
}
