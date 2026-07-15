using UnityEngine;

public class Sensor : MonoBehaviour
{
    public float sensorLength = 10f;

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position,
                            transform.forward,
                            out hit,
                            sensorLength))
        {
            Debug.DrawRay(transform.position,
                          transform.forward * hit.distance,
                          Color.red);

            Debug.Log(hit.distance);
        }
        else
        {
            Debug.DrawRay(transform.position,
                          transform.forward * sensorLength,
                          Color.green);
        }
    }
}