using UnityEngine;
using UnityEngine.AI;

public class DroneStateMachine : MonoBehaviour
{
    public enum DroneState { Patrol, Alert, Search }
    public DroneState currentState = DroneState.Patrol;

    private NavMeshAgent _agent;
    private Transform _player;
    private Vector3 _lastKnownPos;
    private float _searchTimer = 0f;
    public float searchDuration = 5f;

    public WaypointPatrol waypointPatrol;
    public PlayerDetector detector;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        switch (currentState)
        {
            case DroneState.Patrol:
                waypointPatrol.DoPatrol(_agent);
                break;

            case DroneState.Alert:
                _agent.SetDestination(_player.position);
                _lastKnownPos = _player.position;
                if (!detector.IsPlayerInRange())
                {
                    currentState = DroneState.Search;
                    _searchTimer = searchDuration;
                }
                break;

            case DroneState.Search:
                _agent.SetDestination(_lastKnownPos);
                _searchTimer -= Time.deltaTime;
                if (_searchTimer <= 0)
                    currentState = DroneState.Patrol;
                if (detector.IsPlayerInRange())
                    currentState = DroneState.Alert;
                break;
        }
    }

    public void AlertDrone() => currentState = DroneState.Alert;
}