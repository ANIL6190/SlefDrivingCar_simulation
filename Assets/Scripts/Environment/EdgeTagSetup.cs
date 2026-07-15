using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Run once after importing a track FBX (e.g. Track_Simple.fbx).
/// Walks every child object, and based on the naming convention used
/// in the Blender export ("_EdgeLeft", "_EdgeRight", "_Road", "_CarStart"),
/// assigns tags/layers and adds the right colliders.
///
/// Setup required first (Edit > Project Settings > Tags and Layers):
///   Tag:   "TrackEdge"
///   Layer: "TrackEdge"
/// </summary>
public class EdgeTagSetup : EditorWindow
{
    [MenuItem("Tools/Track/Tag Imported Track")]
    static void TagSelectedTrack()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Select the imported track root object first.");
            return;
        }

        var root = Selection.activeGameObject;
        var transforms = root.GetComponentsInChildren<Transform>(true);
        int edgeCount = 0, roadCount = 0;

        foreach (var t in transforms)
        {
            var go = t.gameObject;
            var name = go.name;

            if (name.EndsWith("_EdgeLeft") || name.EndsWith("_EdgeRight"))
            {
                go.tag = "TrackEdge";
                go.layer = LayerMask.NameToLayer("TrackEdge");

                var mf = go.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    var mc = go.GetComponent<MeshCollider>();
                    if (mc == null) mc = go.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = false; // static wall, raycasts + physics both work
                }
                edgeCount++;
            }
            else if (name.EndsWith("_Road"))
            {
                var mf = go.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    var mc = go.GetComponent<MeshCollider>();
                    if (mc == null) mc = go.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
                roadCount++;
            }
            else if (name.EndsWith("_CarStart"))
            {
                go.tag = "Respawn"; // built-in Unity tag, used as spawn marker
            }
        }

        Debug.Log($"Tagged {edgeCount} edge objects and {roadCount} road objects on '{root.name}'.");
    }
}
#endif
