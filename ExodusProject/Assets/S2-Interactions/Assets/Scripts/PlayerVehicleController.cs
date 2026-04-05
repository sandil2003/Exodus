using UnityEngine;

public class PlayerVehicleController : MonoBehaviour
{
    public float acceleration = 800f;
    public float steering = 120f;
    public float maxSpeed = 20f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        MoveVehicle();
        SteerVehicle();
    }

    void MoveVehicle()
    {
        float moveInput = Input.GetAxis("Vertical");

        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * moveInput * acceleration);
        }
    }

    void SteerVehicle()
    {
        float turnInput = Input.GetAxis("Horizontal");

        float turn = turnInput * steering * Time.fixedDeltaTime;

        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}