using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PresentationAutoPilot : MonoBehaviour
{
    [Header("Assign the exact same Checkpoints here!")]
    public Transform[] checkpoints;

    [Header("Driving Settings")]
    public float maxSpeed = 15f;
    public float steerSpeed = 10f; // Increased default turn speed to prevent understeer
    public float acceleration = 5f;
    public float checkpointRadius = 15f;

    Rigidbody rb;
    int currentCP = 0;
    float stuckTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (checkpoints == null || checkpoints.Length == 0) return;

        float closest = float.MaxValue;
        for(int i = 0; i < checkpoints.Length; i++) 
        {
            float dist = Vector3.Distance(transform.position, checkpoints[i].position);
            if(dist < closest) { closest = dist; currentCP = i; }
        }
    }

    void FixedUpdate()
    {
        if (checkpoints == null || checkpoints.Length == 0) return;

        Vector3 targetPos = checkpoints[currentCP].position;
        
        if (Vector3.Distance(transform.position, targetPos) < checkpointRadius) 
        {
            currentCP = (currentCP + 1) % checkpoints.Length;
            targetPos = checkpoints[currentCP].position;
        }

        Vector3 dirToTarget = (targetPos - transform.position).normalized;
        dirToTarget.y = 0; 
        
        // 1. DYNAMIC SPEED (Slow down if the next checkpoint is a sharp turn!)
        float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
        float currentMaxSpeed = maxSpeed;
        
        if (angleToTarget > 15f) 
        {
            currentMaxSpeed = maxSpeed * 0.4f; // Brake hard on corners so it doesn't slam into walls!
        }

        // 2. STEER
        if (dirToTarget != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, steerSpeed * Time.fixedDeltaTime));
        }

        // 3. DRIVE
        Vector3 vel = transform.forward * currentMaxSpeed;
        vel.y = rb.linearVelocity.y; // Keep gravity
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, vel, acceleration * Time.fixedDeltaTime);

        // 4. THE ULTIMATE ANTI-STUCK FAILSAFE
        // If the car ever hits a wall and stops moving, teleport it to the center of the road instantly.
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (flatVelocity.magnitude < 0.5f)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > 1.5f)
            {
                // Teleport to the current target checkpoint to instantly un-jam it!
                transform.position = checkpoints[currentCP].position + new Vector3(0, 0.5f, 0);
                transform.rotation = checkpoints[currentCP].rotation;
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }
}
