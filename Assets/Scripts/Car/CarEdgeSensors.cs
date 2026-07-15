using UnityEngine;

/// <summary>
/// Attach to the car. Casts a fan of rays outward and reports normalized
/// distance-to-edge for each. Feed GetObservations() into your RL agent's
/// CollectObservations(). Only hits colliders tagged/layered "TrackEdge",
/// which matches the *_EdgeLeft / *_EdgeRight objects tagged by EdgeTagSetup.
/// </summary>
public class CarEdgeSensors : MonoBehaviour
{
    [Header("Sensor Setup")]
    [Tooltip("Angles in degrees, relative to forward. 0 = straight ahead.")]
    public float[] rayAngles = { -90f, -60f, -45f, -30f, -20f, 0f, 20f, 30f, 45f, 60f, 90f };
    public float maxDistance = 30f;
    public LayerMask edgeLayerMask;
    public Transform rayOrigin; // usually the car's front bumper transform

    [Header("Debug")]
    public bool drawDebugRays = true;

    float[] lastDistances;

    void Awake()
    {
        lastDistances = new float[rayAngles.Length];
        if (rayOrigin == null) rayOrigin = transform;
    }

    /// <summary>
    /// Returns one normalized value per ray: 0 = edge touching the car,
    /// 1 = no edge within maxDistance (open road).
    /// </summary>
    public float[] GetObservations()
    {
        // If edgeLayerMask is not configured (Nothing = 0), fall back to hitting everything
        // This avoids the common mistake of forgetting to set the layer mask
        int mask = (edgeLayerMask.value == 0) ? ~0 : (int)edgeLayerMask;

        for (int i = 0; i < rayAngles.Length; i++)
        {
            Vector3 origin = (rayOrigin != null) ? rayOrigin.position : transform.position;
            Vector3 dir = Quaternion.Euler(0, rayAngles[i], 0) * ((rayOrigin != null) ? rayOrigin.forward : transform.forward);
            float dist = maxDistance;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, mask, QueryTriggerInteraction.Collide))
            {
                // Skip if we hit our own car parts
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    dist = maxDistance;
                }
                else
                {
                    dist = hit.distance;
                    if (drawDebugRays)
                        Debug.DrawLine(origin, hit.point, Color.red);
                }
            }
            else if (drawDebugRays)
            {
                Debug.DrawRay(origin, dir * maxDistance, Color.green);
            }

            lastDistances[i] = dist / maxDistance; // normalize to [0,1]
        }
        return lastDistances;
    }

    /// <summary>Convenience: true if any ray reports the car is essentially on an edge.</summary>
    /// <summary>Returns true if any ray is very close to a wall (early warning).</summary>
    public bool IsTouchingEdge(float threshold = 0.15f) // Raised from 0.05 to 0.15 (triggers ~3m away)
    {
        foreach (var d in lastDistances)
            if (d <= threshold) return true;
        return false;
    }
}
