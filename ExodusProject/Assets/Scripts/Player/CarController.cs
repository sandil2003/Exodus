using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 30f; // Increased speed
    public float boostMultiplier = 2f; 
    public float turnSpeed = 150f; // Increased turn speed

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;
    private bool isBoosting;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Initial snap to height
        Vector3 pos = transform.position;
        pos.y = 1f;
        transform.position = pos;

        // Keep it flat and stable
        rb.constraints = RigidbodyConstraints.FreezePositionY | 
                         RigidbodyConstraints.FreezeRotationX | 
                         RigidbodyConstraints.FreezeRotationZ;

        // Higher damping stops the "sliding on ice" feeling
        rb.linearDamping = 2f;
        rb.angularDamping = 5f;
        
        rb.WakeUp();
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        isBoosting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    void FixedUpdate()
    {
        MoveCar();
        TurnCar();
    }

    void MoveCar()
    {
        float currentSpeed = isBoosting ? moveSpeed * boostMultiplier : moveSpeed;

        if (Mathf.Abs(moveInput) < 0.01f)
        {
            // Stop forward movement but allow gravity/physics to settle
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        // FORCE ALIGNMENT: We set the velocity strictly to the forward direction.
        // This prevents the "sideways" sliding.
        Vector3 forwardVelocity = transform.forward * moveInput * currentSpeed;
        rb.linearVelocity = new Vector3(forwardVelocity.x, rb.linearVelocity.y, forwardVelocity.z);
    }

    void TurnCar()
    {
        // Only allow turning if we are providing move input
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float steerDirection = moveInput > 0 ? 1 : -1;
            float rotationAmount = turnInput * steerDirection * turnSpeed * Time.fixedDeltaTime;
            
            Quaternion turnRotation = Quaternion.Euler(0f, rotationAmount, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
        else
        {
            // Kill any spinning when not actively steering
            rb.angularVelocity = Vector3.zero;
        }
    }
}