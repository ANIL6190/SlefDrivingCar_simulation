using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach this to your car in Manual Driving Mode. 
/// It will automatically drop a checkpoint every X meters as you drive!
/// </summary>
public class CheckpointRecorder : MonoBehaviour
{
    [Header("Settings")]
    public float recordDistance = 20f; // Drop a checkpoint every 20 meters
    [Tooltip("Turn ON to see spheres while recording. Turn OFF for production — no spheres created.")]
    public bool showVisuals = false;
    
    [Header("Generated Object")]
    public Transform checkpointParent; // Create an empty GameObject in your scene to hold them

    Vector3 lastPos;
    int cpCount = 0;

    void Start()
    {
        lastPos = transform.position;

        if (checkpointParent == null)
        {
            GameObject go = new GameObject("Checkpoints_Generated");
            checkpointParent = go.transform;
        }

        // Place the very first checkpoint
        CreateCheckpoint();
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, lastPos) >= recordDistance)
        {
            CreateCheckpoint();
        }
    }

    void CreateCheckpoint()
    {
        GameObject cp = new GameObject("CP_" + cpCount);
        cp.transform.position = transform.position;
        cp.transform.rotation = transform.rotation;
        cp.transform.SetParent(checkpointParent);

        // Optional visual sphere — only created when showVisuals is ON
        if (showVisuals)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(cp.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(2, 2, 2);
            Destroy(visual.GetComponent<Collider>());
        }

        lastPos = transform.position;
        cpCount++;
    }
}
