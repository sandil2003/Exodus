using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DroneStateMachine.cs — IS Module | Student 2 (Dynamic Adaptation & Event Interception)
///
/// Responsibilities:
///   - Own and manage the three drone AI states: PATROL, ALERT, SEARCH
///   - Fire StateChanged event so DroneAnimator (GV Student 4) can update visuals
///   - Handle patrol waypoint cycling during PATROL state
///   - Trigger A* path requests during ALERT state via PathfindingCoordinator
///   - Handle SEARCH state coroutine (navigate to last known pos, wait 5s, return to PATROL)
///   - Handle HumanPickedUp event to clear stale patrol waypoints
/// </summary>
public class DroneStateMachine : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Public State & Events
    // -----------------------------------------------------------------------

    public DroneState CurrentState { get; private set; } = DroneState.PATROL;

    /// <summary>
    /// Fired whenever the state changes.
    /// Subscribed to by DroneAnimator (GV Student 4) to update speed/animations.
    /// </summary>
    public event Action<DroneState> StateChanged;

    // -----------------------------------------------------------------------
    // Inspector References
    // -----------------------------------------------------------------------

    [Header("Drone Identity")]
    [SerializeField] private int droneId;
    public int DroneId { get => droneId; set => droneId = value; }

    [Header("Patrol Waypoints")]
    [Tooltip("World-space positions the drone cycles through in PATROL state")]
    [SerializeField] private List<Vector3> patrolWaypoints = new List<Vector3>();

    [Header("Search Settings")]
    [Tooltip("How many seconds the drone circles last known position before returning to PATROL")]
    [SerializeField] private float searchDuration = 5f;

    // -----------------------------------------------------------------------
    // Private State
    // -----------------------------------------------------------------------

    private int patrolIndex = 0;
    private Vector3 lastKnownPlayerPos = Vector3.zero;
    private int lastKnownPlayerNode = -1;

    // Public getter for the Animator to use
    public Vector3 LastKnownPlayerPos => lastKnownPlayerPos;
    private Coroutine searchCoroutine = null;
    private bool isInitialized = false;

    // Cached component references
    private DroneAnimator droneAnimator;

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    void Awake()
    {
        droneAnimator = GetComponent<DroneAnimator>();

        if (patrolWaypoints.Count == 0)
            Debug.LogWarning($"[DroneStateMachine] Drone {droneId}: No patrol waypoints assigned!");
    }

    void Start()
    {
        // Subscribe to human pickup events from GV PickupSystem
        if (PickupSystem.Instance != null)
            PickupSystem.Instance.HumanPickedUp += OnHumanPickedUp;

        // Subscribe to path events from IS Coordinator
        if (PathfindingCoordinator.Instance != null)
        {
            PathfindingCoordinator.Instance.PathCalculated += OnPathCalculated;
            PathfindingCoordinator.Instance.PathFailed += OnPathFailed;
        }

        isInitialized = true;

        // Begin in PATROL — request first patrol path
        EnterPatrol();
    }

    void OnDestroy()
    {
        if (PickupSystem.Instance != null)
            PickupSystem.Instance.HumanPickedUp -= OnHumanPickedUp;

        if (PathfindingCoordinator.Instance != null)
        {
            PathfindingCoordinator.Instance.PathCalculated -= OnPathCalculated;
            PathfindingCoordinator.Instance.PathFailed -= OnPathFailed;
        }
    }

    // -----------------------------------------------------------------------
    // State Transition
    // -----------------------------------------------------------------------

    /// <summary>
    /// Central transition method. All state changes must go through here.
    /// Fires StateChanged event so subscribers (DroneAnimator) are notified.
    /// Guards against redundant transitions to the same state.
    /// </summary>
    public void TransitionTo(DroneState newState)
    {
        if (CurrentState == newState)
            return;

        DroneState previousState = CurrentState;
        CurrentState = newState;

        Debug.Log($"[DroneStateMachine] Drone {droneId}: {previousState} → {newState}");

        // Fire event — DroneAnimator listens to this
        StateChanged?.Invoke(newState);

        // Enter the new state
        switch (newState)
        {
            case DroneState.PATROL: EnterPatrol(); break;
            case DroneState.ALERT: EnterAlert(); break;
            case DroneState.SEARCH: EnterSearch(); break;
        }
    }

    // -----------------------------------------------------------------------
    // PATROL State
    // -----------------------------------------------------------------------

    /// <summary>
    /// Enters PATROL state.
    /// Requests a path to the next waypoint in the patrol sequence.
    /// </summary>
    void EnterPatrol()
    {
        if (!isInitialized || patrolWaypoints.Count == 0)
            return;

        RequestNextPatrolPath();
    }

    /// <summary>
    /// Gets the next patrol waypoint, snaps it to the graph, and requests a path.
    /// Cycles through waypoints in order, looping back to the start.
    /// </summary>
    void RequestNextPatrolPath()
    {
        if (patrolWaypoints.Count == 0)
            return;

        Vector3 targetWaypoint = patrolWaypoints[patrolIndex % patrolWaypoints.Count];
        patrolIndex++;

        int droneNode = NavMeshGraph.Instance.GetNearestNode(transform.position);
        int targetNode = NavMeshGraph.Instance.GetNearestNode(targetWaypoint);

        if (droneNode == -1 || targetNode == -1)
        {
            Debug.LogWarning($"[DroneStateMachine] Drone {droneId}: Could not snap patrol waypoint to graph node.");
            return;
        }

        PathfindingCoordinator.Instance.RequestPath(droneId, droneNode, targetNode, targetWaypoint);
    }

    // -----------------------------------------------------------------------
    // ALERT State
    // -----------------------------------------------------------------------

    /// <summary>
    /// Enters ALERT state.
    /// Immediately requests an A* intercept path to the player's current node.
    /// Called after PlayerDetected event sets lastKnownPlayerNode.
    /// </summary>
    void EnterAlert()
    {
        // Stop any ongoing search coroutine
        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
            searchCoroutine = null;
        }

        RequestInterceptPath();
    }

    /// <summary>
    /// Requests an A* path from the drone's current position to the player's current node.
    /// Called on ALERT entry and every time the player's node changes (via PlayerEventInterceptor).
    /// </summary>
    public void RequestInterceptPath()
    {
        if (lastKnownPlayerNode == -1)
            return;

        int droneNode = NavMeshGraph.Instance.GetNearestNode(transform.position);

        if (droneNode == -1)
        {
            Debug.LogWarning($"[DroneStateMachine] Drone {droneId}: Could not snap drone position to graph node.");
            return;
        }

        PathfindingCoordinator.Instance.RequestPath(droneId, droneNode, lastKnownPlayerNode, lastKnownPlayerPos);
    }

    /// <summary>
    /// Updates the last known player node. Called by PlayerEventInterceptor
    /// every time the player crosses into a new graph node during ALERT state.
    /// </summary>
    public void UpdatePlayerNode(int playerNodeId)
    {
        lastKnownPlayerNode = playerNodeId;

        // Only recalculate if currently chasing
        if (CurrentState == DroneState.ALERT)
            RequestInterceptPath();
    }

    // -----------------------------------------------------------------------
    // SEARCH State
    // -----------------------------------------------------------------------

    /// <summary>
    /// Enters SEARCH state.
    /// Navigates to last known player position using BFS,
    /// waits searchDuration seconds, then returns to PATROL.
    /// </summary>
    void EnterSearch()
    {
        if (searchCoroutine != null)
            StopCoroutine(searchCoroutine);

        searchCoroutine = StartCoroutine(SearchRoutine());
    }

    IEnumerator SearchRoutine()
    {
        // Navigate to last known player position using BFS (Student 4 IS)
        int droneNode = NavMeshGraph.Instance.GetNearestNode(transform.position);
        int searchGoal = lastKnownPlayerNode;

        if (droneNode != -1 && searchGoal != -1)
        {
            List<Vector3> bfsPath = BFSPathfinder.Instance.FindPath(
                droneNode,
                searchGoal,
                NavMeshGraph.Instance
            );

            if (bfsPath != null && bfsPath.Count > 0)
            {
                // Send BFS path directly to DroneAnimator to follow
                droneAnimator?.ReceiveBFSPath(bfsPath);
                
                // Wait until the drone actually reaches the search location
                while (droneAnimator != null && !droneAnimator.HasReachedEndOfPath())
                {
                    yield return null;
                }
            }
            else
            {
                Debug.Log($"[DroneStateMachine] Drone {droneId}: BFS returned no path — holding position during search.");
            }
        }

        // Wait at last known position
        Debug.Log($"[DroneStateMachine] Drone {droneId}: Searching for {searchDuration}s...");
        yield return new WaitForSeconds(searchDuration);

        // Return to patrol
        Debug.Log($"[DroneStateMachine] Drone {droneId}: Search timeout — returning to PATROL.");
        searchCoroutine = null;
        TransitionTo(DroneState.PATROL);
    }

    // -----------------------------------------------------------------------
    // Path Event Handlers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called when PathfindingCoordinator finishes calculating a path for this drone.
    /// If in PATROL and drone has reached end of its current path — request next waypoint.
    /// </summary>
    void OnPathCalculated(int id, List<Vector3> path)
    {
        if (id != droneId)
            return;

        // Nothing extra needed here for ALERT — DroneAnimator handles path following.
        // For PATROL, we watch HasReachedEndOfPath() in Update to chain waypoints.
    }

    /// <summary>
    /// Called when A* fails to find a path (player unreachable — e.g. in a narrow alley).
    /// Transitions drone to SEARCH state so it investigates last known position.
    /// </summary>
    void OnPathFailed(int id)
    {
        if (id != droneId)
            return;

        Debug.Log($"[DroneStateMachine] Drone {droneId}: A* path failed. Falling back to direct follow if in ALERT.");

        // Do NOT transition to SEARCH here. 
        // If the path fails but we are in ALERT, DroneAnimator will naturally fallback to FollowPlayerDirectly()
        // PlayerEventInterceptor alone will decide when the player is truly LOST (distance timeout).
    }

    // -----------------------------------------------------------------------
    // Patrol Chaining — Check if drone finished current path
    // -----------------------------------------------------------------------

    void Update()
    {
        // When in PATROL and drone finishes its current path segment,
        // automatically request the next waypoint path
        if (CurrentState == DroneState.PATROL && droneAnimator != null)
        {
            if (droneAnimator.HasReachedEndOfPath())
                RequestNextPatrolPath();
        }
    }

    // -----------------------------------------------------------------------
    // Human NPC Pickup Event Handler
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called by PickupSystem when the player rescues a human NPC.
    /// Removes that human's position from the patrol waypoint list
    /// so the drone doesn't navigate toward a now-empty location.
    /// </summary>
    void OnHumanPickedUp(int humanId, Vector3 humanWorldPosition)
    {
        // Find and remove the waypoint closest to the rescued human's position
        float closestDist = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < patrolWaypoints.Count; i++)
        {
            float dist = Vector3.Distance(patrolWaypoints[i], humanWorldPosition);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        // Only remove if it's actually close to the human (within 2m tolerance)
        if (closestIndex != -1 && closestDist < 2f)
        {
            patrolWaypoints.RemoveAt(closestIndex);

            // Adjust patrol index so we don't skip or go out of bounds
            if (patrolIndex > closestIndex)
                patrolIndex = Mathf.Max(0, patrolIndex - 1);

            Debug.Log($"[DroneStateMachine] Drone {droneId}: Removed rescued human waypoint. " +
                      $"Remaining patrol points: {patrolWaypoints.Count}");

            // If currently patrolling toward that removed waypoint, request a new one
            if (CurrentState == DroneState.PATROL)
                RequestNextPatrolPath();
        }
    }

    // -----------------------------------------------------------------------
    // Public Setters — called by PlayerEventInterceptor
    // -----------------------------------------------------------------------

    /// <summary>
    /// Sets the last known player position and node.
    /// Called by PlayerEventInterceptor when player is detected or moves.
    /// </summary>
    public void SetLastKnownPlayerData(Vector3 position, int nodeId)
    {
        lastKnownPlayerPos = position;
        lastKnownPlayerNode = nodeId;
    }

    /// <summary>
    /// Dynamically assigns a new set of patrol waypoints.
    /// Used by DroneSpawner.
    /// </summary>
    public void SetPatrolWaypoints(List<Vector3> waypoints)
    {
        patrolWaypoints = new List<Vector3>(waypoints);
        patrolIndex = 0;
        
        // If we are currently patrolling, we might want to reset the current target
        if (CurrentState == DroneState.PATROL && isInitialized)
        {
            EnterPatrol();
        }
    }
}