using UnityEngine;

public class DroneAnimator : MonoBehaviour
{
    public float hoverAmplitude = 0.15f;
    public float hoverSpeed = 1.5f;
    public float bankAngle = 20f;

    private Vector3 _startPos;
    private Rigidbody _rb;
    private UnityEngine.AI.NavMeshAgent _agent;

    void Start()
    {
        _startPos = transform.position;
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    void Update()
    {
        // Hover bob
        float newY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position += new Vector3(0, newY * Time.deltaTime, 0);

        // Banking on turns
        Vector3 vel = _agent.velocity;
        if (vel.magnitude > 0.5f)
        {
            float bank = -Vector3.Dot(transform.right, vel.normalized) * bankAngle;
            Quaternion targetRot = Quaternion.LookRotation(vel) * Quaternion.Euler(0, 0, bank);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }
}