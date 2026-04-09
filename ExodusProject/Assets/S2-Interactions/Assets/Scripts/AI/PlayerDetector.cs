using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    public float detectionRadius = 12f;
    private Transform _player;
    private DroneStateMachine _sm;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _sm = GetComponent<DroneStateMachine>();
    }

    void Update()
    {
        if (IsPlayerInRange()) _sm.AlertDrone();
    }

    public bool IsPlayerInRange()
    {
        return Vector3.Distance(transform.position, _player.position) <= detectionRadius;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}