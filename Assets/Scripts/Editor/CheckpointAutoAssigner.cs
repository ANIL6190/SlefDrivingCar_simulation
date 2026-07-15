using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Editor utility to auto-populate the checkpoints array on CarAgent and TestCarAgent
/// from a parent GameObject that contains all checkpoint transforms as children.
/// 
/// Usage:
/// 1. In the Unity menu, go to Tools > Auto-Assign Checkpoints
/// 2. Select the parent GameObject that holds all your CP_ objects
/// 3. Select the car that has CarAgent or TestCarAgent on it
/// 4. Click "Assign Checkpoints!" 
/// </summary>
public class CheckpointAutoAssigner : EditorWindow
{
    GameObject checkpointParent;
    GameObject carAgent;
    GameObject testCarAgent;

    [MenuItem("Tools/Auto-Assign Checkpoints")]
    public static void OpenWindow()
    {
        GetWindow<CheckpointAutoAssigner>("Checkpoint Assigner");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto-Assign Checkpoints", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        checkpointParent = (GameObject)EditorGUILayout.ObjectField(
            "Checkpoint Parent", checkpointParent, typeof(GameObject), true);
        
        EditorGUILayout.Space();
        GUILayout.Label("Assign to:", EditorStyles.boldLabel);

        carAgent = (GameObject)EditorGUILayout.ObjectField(
            "Training Car (CarAgent)", carAgent, typeof(GameObject), true);
        
        testCarAgent = (GameObject)EditorGUILayout.ObjectField(
            "Testing Car (TestCarAgent)", testCarAgent, typeof(GameObject), true);

        EditorGUILayout.Space();

        if (checkpointParent == null)
        {
            EditorGUILayout.HelpBox("Drag your 'Checkpoints_Generated' GameObject here.", MessageType.Info);
        }

        GUI.enabled = checkpointParent != null && (carAgent != null || testCarAgent != null);

        if (GUILayout.Button("Assign Checkpoints!", GUILayout.Height(40)))
        {
            AssignCheckpoints();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Cleanup", EditorStyles.boldLabel);

        GUI.enabled = checkpointParent != null;

        if (GUILayout.Button("Remove All Sphere Visuals", GUILayout.Height(35)))
        {
            RemoveAllVisuals();
        }

        GUI.enabled = true;
    }

    void AssignCheckpoints()
    {
        // Get all children in the order they appear in the Hierarchy (sibling index order)
        Transform[] allChildren = checkpointParent
            .GetComponentsInChildren<Transform>(true)
            .Where(t => t != checkpointParent.transform) // Exclude the parent itself
            .OrderBy(t => t.GetSiblingIndex())
            .ToArray();

        Debug.Log($"Found {allChildren.Length} checkpoints in '{checkpointParent.name}'.");

        if (carAgent != null)
        {
            CarAgent agent = carAgent.GetComponent<CarAgent>();
            if (agent != null)
            {
                Undo.RecordObject(agent, "Auto-Assign Checkpoints to CarAgent");
                agent.checkpoints = allChildren;
                EditorUtility.SetDirty(agent);
                Debug.Log($"✅ Assigned {allChildren.Length} checkpoints to CarAgent on '{carAgent.name}'.");
            }
            else
            {
                Debug.LogWarning($"No CarAgent component found on '{carAgent.name}'!");
            }
        }

        if (testCarAgent != null)
        {
            TestCarAgent tAgent = testCarAgent.GetComponent<TestCarAgent>();
            if (tAgent != null)
            {
                Undo.RecordObject(tAgent, "Auto-Assign Checkpoints to TestCarAgent");
                tAgent.checkpoints = allChildren;
                EditorUtility.SetDirty(tAgent);
                Debug.Log($"✅ Assigned {allChildren.Length} checkpoints to TestCarAgent on '{testCarAgent.name}'.");
            }
            else
            {
                Debug.LogWarning($"No TestCarAgent component found on '{testCarAgent.name}'!");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Done! All checkpoints assigned. Save your scene (Ctrl+S).");
    }

    void RemoveAllVisuals()
    {
        // Each CP_X is a child of the parent.
        // Inside each CP_X there can be a Sphere child with MeshRenderer + MeshFilter.
        MeshRenderer[] renderers = checkpointParent.GetComponentsInChildren<MeshRenderer>(true);
        MeshFilter[]   filters   = checkpointParent.GetComponentsInChildren<MeshFilter>(true);

        int count = renderers.Length;

        foreach (var r in renderers) { Undo.DestroyObjectImmediate(r); }
        foreach (var f in filters)   { Undo.DestroyObjectImmediate(f); }

        Debug.Log($"Removed {count} MeshRenderer(s) from checkpoints. Save your scene (Ctrl+S).");
    }
}
