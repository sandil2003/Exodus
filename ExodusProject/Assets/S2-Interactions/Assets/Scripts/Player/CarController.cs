using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 15f; 
    public float turnSpeed = 100f;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    void Start()
    {
        // Get the Rigidbody component attached to the car
        rb = GetComponent<Rigidbody>();
        
        // This stops the car from tipping over when turning
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // 1. Get Input from Arrow Keys/WASD
        moveInput = Input.GetAxis("Vertical");   // Up/Down
        turnInput = Input.GetAxis("Horizontal"); // Left/Right
    }

    void FixedUpdate()
    {
        // We use FixedUpdate for physics-based movement (Rigidbody)
        MoveCar();
        TurnCar();
    }

    void MoveCar()
    {
        // Move the car forward/backward based on the Blue arrow (Vector3.forward)
        Vector3 movement = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    void TurnCar()
    {
        // Only allow turning if the car is moving (forward or backward)
        if (moveInput != 0)
        {
            // If moving backward, invert the steering so it feels natural
            float steerDirection = moveInput > 0 ? 1 : -1;
            
            // Rotate around the Green arrow (Vector3.up)
            float rotation = turnInput * steerDirection * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
            
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }
}