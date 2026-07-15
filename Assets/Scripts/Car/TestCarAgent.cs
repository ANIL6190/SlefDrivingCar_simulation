using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// A clean version of the CarAgent used purely for testing/inference.
/// This script does not contain any training logic, rewards, or episode resets.
/// 
/// Setup:
/// 1. Remove CarAgent from your car and attach this TestCarAgent instead.
/// 2. Assign the Checkpoints (the model still needs these to see where the track goes).
/// 3. Add a BehaviorParameters component, assign your trained .onnx Model, and set Behavior Type to "Inference Only".
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TestCarAgent : Agent
{
    [Header("References")]
    [Tooltip("Checkpoints are still required because the trained model expects them as part of its 'eyes/GPS'.")]
    public Transform[] checkpoints;

    [Header("Driving")]
    public float maxSteerAngle = 150f;
    public float motorPower = 800f;
    public float speedLimit = 12f;
    public float decelerationRate = 10f;

    Rigidbody rb;
    int nextCheckpoint = 0;
    float checkpointRadius = 10f; // Distance to consider checkpoint passed

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        
        // Find nearest checkpoint at start
        nextCheckpoint = 0;
        if (checkpoints.Length > 0)
        {
            float minDist = float.MaxValue;
            for (int i = 0; i < checkpoints.Length; i++)
            {
                float d = Vector3.Distance(transform.position, checkpoints[i].position);
                if (d < minDist) { minDist = d; nextCheckpoint = i; }
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Car Speed & Direction (2 observations)
        sensor.AddObservation(rb.linearVelocity.magnitude / speedLimit);
        sensor.AddObservation(Vector3.Dot(transform.forward, rb.linearVelocity.normalized));

        // 2. Direction to the next checkpoint (2 observations)
        if (checkpoints.Length > 0)
        {
            Vector3 dirToNext = (checkpoints[nextCheckpoint].position - transform.position).normalized;
            Vector3 localDir = transform.InverseTransformDirection(dirToNext);
            sensor.AddObservation(localDir.x); // Local right/left direction
            sensor.AddObservation(localDir.z); // Local forward/backward direction
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float steer = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float throttleInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        float currentSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);

        // -- Arcade Braking / Reversing Logic --
        if (throttleInput < -0.1f && currentSpeed > 1f) 
        {
            // Braking
            Vector3 brakeTarget = Vector3.zero;
            brakeTarget.y = rb.linearVelocity.y;
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, brakeTarget, Time.fixedDeltaTime * decelerationRate);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * decelerationRate);
        }
        else if (Mathf.Abs(throttleInput) > 0.05f)
        {
            // Throttle
            Vector3 targetVelocity = transform.forward * throttleInput * speedLimit;
            targetVelocity.y = rb.linearVelocity.y; // keep gravity
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 5f);
        }
        else
        {
            // Coasting
            Vector3 coastVelocity = rb.linearVelocity;
            coastVelocity.x = Mathf.Lerp(coastVelocity.x, 0, Time.fixedDeltaTime * 1.5f);
            coastVelocity.z = Mathf.Lerp(coastVelocity.z, 0, Time.fixedDeltaTime * 1.5f);
            rb.linearVelocity = coastVelocity;
        }

        // -- Steering Logic --
        float speedRatio = Mathf.Clamp01((Mathf.Abs(currentSpeed) + 2f) / (speedLimit * 0.5f)); 
        float direction = currentSpeed < -0.1f ? -1f : 1f;
        float finalSteer = steer * maxSteerAngle * direction * speedRatio * Time.fixedDeltaTime;
        Quaternion deltaRot = Quaternion.Euler(0f, finalSteer, 0f);
        rb.MoveRotation(rb.rotation * deltaRot);

        // -- Update Next Checkpoint (No Rewards Assigned) --
        // This is purely so the observations update and point toward the next path node
        if (checkpoints.Length > 0)
        {
            float dist = Vector3.Distance(transform.position, checkpoints[nextCheckpoint].position);
            if (dist < checkpointRadius)
            {
                nextCheckpoint = (nextCheckpoint + 1) % checkpoints.Length;
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var kb = Keyboard.current;
        float steer = 0f, throttle = 0f;
        
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  steer    = -1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) steer    =  1f;
            
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    throttle =  1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed || kb.spaceKey.isPressed) throttle = -1f;
        }
        
        continuousActions[0] = steer;
        continuousActions[1] = throttle;
    }

    void OnCollisionEnter(Collision collision)
    {
        // For testing, we just ignore crashes or log them.
        // We do not reset the car.
        string objTag = collision.gameObject.tag;
        if (objTag == "TrackEdge" || objTag == "Wall")
        {
            Debug.Log("The testing car hit a wall! (No reset applied)");
        }
    }
}
