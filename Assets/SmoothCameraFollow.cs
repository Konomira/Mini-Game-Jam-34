using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform player;          // Reference to the player's transform
    public Vector3 offset;            // Offset from the player's position
    public float smoothSpeed = 0.125f; // Smoothness factor for the camera's movement

    void FixedUpdate()
    {
        if (player != null)
        {
            // Desired position for the camera
            Vector3 desiredPosition = player.position + offset;

            // Smoothly interpolate between the current camera position and the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Update the camera's position
            transform.position = smoothedPosition;
        }
    }
}
