using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // The target that the camera will follow
    public Vector3 offset;            // Offset from the target
    public float smoothSpeed = 0.125f; // How smoothly the camera catches up with its target
    public float maxDistance = 5.0f;   // Maximum distance from the target

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;

        // Check the maximum distance constraint
        if (Vector3.Distance(transform.position, desiredPosition) > maxDistance)
        {
            desiredPosition = Vector3.MoveTowards(transform.position, desiredPosition, maxDistance);
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Keep the camera's z position constant
        transform.position = new Vector3(transform.position.x, transform.position.y, offset.z);
    }
}
