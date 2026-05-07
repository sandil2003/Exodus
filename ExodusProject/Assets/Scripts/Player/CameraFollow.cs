using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 4f, 8f); 
    public float smoothTime = 0.12f; // Time it takes to reach the target
    public float lookAtHeight = 1.5f;

    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        if (target)
        {
            SnapToTarget();
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // 1. Calculate desired position
        Vector3 desiredPosition = target.TransformPoint(offset);
        
        // 2. Use SmoothDamp instead of Lerp for a much more fluid follow
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        // 3. Always look at the car
        transform.LookAt(target.position + Vector3.up * lookAtHeight);
    }

    public void SnapToTarget()
    {
        transform.position = target.TransformPoint(offset);
        transform.LookAt(target.position + Vector3.up * lookAtHeight);
    }
}