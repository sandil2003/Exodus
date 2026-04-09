using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    // X = side to side, Y = height, Z = distance behind
    public Vector3 offset = new Vector3(0, 3, -6); 
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (!target) return;

        // Calculate the position we want the camera to be in
        // TransformDirection converts our offset into "Car Space"
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        
        // Smoothly move the camera to that position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Always look at the car (slightly above the center)
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}