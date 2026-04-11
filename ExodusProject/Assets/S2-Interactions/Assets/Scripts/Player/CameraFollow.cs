using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    [Header("Camera Settings")]
    // X = Side to side, Y = Height, Z = Distance from car (flip sign if camera is in front)
    public Vector3 offset = new Vector3(0, 4f, 8f); 
    public float smoothSpeed = 10f;
    public float lookAtHeight = 1.5f;

    void Start()
    {
        if (target)
        {
            // Snap to the target immediately so it doesn't start at (0,0,0)
            SnapToTarget();
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // TransformPoint converts our local offset into a world-space position relative to the car
        Vector3 desiredPosition = target.TransformPoint(offset);
        
        // Smoothly interpolate to the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Always look at the car, slightly above its center for a better view
        transform.LookAt(target.position + Vector3.up * lookAtHeight);
    }

    public void SnapToTarget()
    {
        transform.position = target.TransformPoint(offset);
        transform.LookAt(target.position + Vector3.up * lookAtHeight);
    }
}