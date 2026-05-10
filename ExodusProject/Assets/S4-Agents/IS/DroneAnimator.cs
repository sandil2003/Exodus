using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// DroneAnimator.cs - GV Module | Student 4 (Agent Controller)
/// Optimized to support direct following when pathfinding gaps occur.
/// </summary>
public class DroneAnimator : MonoBehaviour
{
    [Header("Child References")]
    [SerializeField] private Transform droneBodyTransform;
    [SerializeField] private Light spotLight;

    [Header("Drone Identity")]
    [SerializeField] private int droneId;

    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 4f;
    [SerializeField] private float alertSpeed = 18f;
    [SerializeField] private float searchSpeed = 3f;
    [SerializeField] private float arrivalThreshold = 0.5f;
    [SerializeField] private float hoverHeightOffset = 1.2f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 6f;

    private List<Vector3> currentPath;
    private int waypointIndex = 0;
    private float currentFlightSpeed;
    private DroneStateMachine stateMachine;
    private Vector3 lastPos;

    void Awake()
    {
        stateMachine = GetComponent<DroneStateMachine>();
        currentFlightSpeed = patrolSpeed;
        lastPos = transform.position;
    }

    void OnEnable()
    {
        if (PathfindingCoordinator.Instance != null)
        {
            PathfindingCoordinator.Instance.PathCalculated += OnNewPathReceived;
            PathfindingCoordinator.Instance.PathFailed += OnPathFailed;
        }
        if (stateMachine != null) stateMachine.StateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        if (PathfindingCoordinator.Instance != null)
        {
            PathfindingCoordinator.Instance.PathCalculated -= OnNewPathReceived;
            PathfindingCoordinator.Instance.PathFailed -= OnPathFailed;
        }
        if (stateMachine != null) stateMachine.StateChanged -= OnStateChanged;
    }

    void Update()
    {
        MoveAlongPath();
        SmoothYawToFaceDirection();
    }

    void MoveAlongPath()
    {
        // If we are chasing and close enough to see the car, check line of sight before direct follow
        if (stateMachine != null && stateMachine.CurrentState == DroneState.ALERT)
        {
            float distToPlayer = Vector3.Distance(transform.position, stateMachine.LastKnownPlayerPos);
            if (distToPlayer < 100f)
            {
                Vector3 dirToPlayer = (stateMachine.LastKnownPlayerPos - transform.position).normalized;
                if (!Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, distToPlayer) || 
                    hit.collider.CompareTag("Player") || hit.collider.GetComponentInParent<CarController>() != null)
                {
                    FollowPlayerDirectly();
                    return;
                }
            }
        }

        if (currentPath == null || waypointIndex >= currentPath.Count)
        {
            if (stateMachine != null && stateMachine.CurrentState == DroneState.ALERT)
                FollowPlayerDirectly();
            return;
        }

        Vector3 targetPos = currentPath[waypointIndex];
        targetPos.y += hoverHeightOffset;

        MoveWithCollisionCheck(targetPos);

        if (Vector3.Distance(transform.position, targetPos) < arrivalThreshold)
            waypointIndex++;
    }

    private void FollowPlayerDirectly()
    {
        if (stateMachine == null) return;
        Vector3 targetPos = stateMachine.LastKnownPlayerPos;
        targetPos.y += hoverHeightOffset;

        MoveWithCollisionCheck(targetPos);
    }

    /// <summary>
    /// Moves toward target but stops if a wall is in the way.
    /// Constraints movement strictly to the NavMesh surface.
    /// </summary>
    private void MoveWithCollisionCheck(Vector3 targetPos)
    {
        Vector3 moveDir = (targetPos - transform.position).normalized;
        float moveDist = currentFlightSpeed * Time.deltaTime;
        
        Vector3 desiredPos = transform.position;

        // Check for walls, but ignore the player car so we don't stop when we get close to it
        bool hasHit = Physics.Raycast(transform.position, moveDir, out RaycastHit hit, moveDist + 0.8f);
        bool hitPlayer = hasHit && (hit.collider.CompareTag("Player") || hit.collider.GetComponentInParent<CarController>() != null);

        if (!hasHit || hitPlayer)
        {
            desiredPos += moveDir * moveDist;
        }
        else
        {
            // If we hit a REAL wall (not the player), try to slide along it
            Vector3 slideDir = Vector3.ProjectOnPlane(moveDir, hit.normal);
            if (!Physics.Raycast(transform.position, slideDir, moveDist + 0.5f))
            {
                desiredPos += slideDir * moveDist * 0.5f;
            }
        }

        // Sample the NavMesh to ensure the drone is over valid ground
        Vector3 groundPos = desiredPos;
        groundPos.y -= hoverHeightOffset;

        if (NavMesh.SamplePosition(groundPos, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
        {
            // Snap the position to the exact NavMesh bounds, ensuring it stays on surface
            transform.position = new Vector3(navHit.position.x, navHit.position.y + hoverHeightOffset, navHit.position.z);
        }
        else
        {
            // If we are completely outside NavMesh bounds, prevent the movement to stop flying off
        }
    }

    void SmoothYawToFaceDirection()
    {
        Vector3 moveDir = transform.position - lastPos;
        if (moveDir.magnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
        lastPos = transform.position;
    }

    public bool HasReachedEndOfPath() => currentPath == null || waypointIndex >= currentPath.Count;

    public void ReceiveBFSPath(List<Vector3> bfsPath)
    {
        currentPath = bfsPath;
        waypointIndex = 0;
        
        // Skip the starting node so the drone always moves forward instead of turning around
        if (currentPath != null && currentPath.Count > 1)
        {
            waypointIndex = 1;
        }
    }

    void OnNewPathReceived(int id, List<Vector3> path)
    {
        if (id == droneId)
        {
            currentPath = path;
            waypointIndex = 0;
            
            // Skip the starting node so the drone always moves forward instead of turning around
            if (currentPath != null && currentPath.Count > 1)
            {
                waypointIndex = 1;
            }
        }
    }

    void OnPathFailed(int id) 
    { 
        if (id == droneId) currentPath = null; 
    }

    void OnStateChanged(DroneState newState)
    {
        switch (newState)
        {
            case DroneState.PATROL: currentFlightSpeed = patrolSpeed; break;
            case DroneState.ALERT: currentFlightSpeed = alertSpeed; break;
            case DroneState.SEARCH: currentFlightSpeed = searchSpeed; break;
        }
        if (spotLight != null) spotLight.enabled = (newState == DroneState.ALERT);
    }
}
