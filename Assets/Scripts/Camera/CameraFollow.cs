using UnityEngine;

namespace SelfDrive.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 5.0f;
        [SerializeField] private float height = 3.0f;
        [SerializeField] private float positionSmoothSpeed = 10.0f;
        [SerializeField] private float rotationSmoothSpeed = 10.0f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            // Calculate the desired position based on the car's position and rotation
            Vector3 desiredPosition = target.position - target.forward * distance + Vector3.up * height;
            
            // Smoothly move the camera to the desired position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed * Time.deltaTime);

            // Calculate the desired rotation to look at the car
            Vector3 lookAtTarget = target.position + (Vector3.up * (height / 2f)); 
            // Looking slightly above the car's center is usually better visually. Or we can just look exactly at the car.
            
            Quaternion desiredRotation = Quaternion.LookRotation(lookAtTarget - transform.position);
            
            // Smoothly rotate the camera
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }
}
