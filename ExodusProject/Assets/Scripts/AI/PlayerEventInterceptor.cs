using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerEventInterceptor.cs — IS Module | Student 2 (Dynamic Adaptation & Event Interception)
///
/// Responsibilities:
///   - Detect when the player car enters/exits each drone's detection sphere (12m radius)
///   - Fire PlayerDetected / PlayerLost events and trigger DroneStateMachine transitions
///   - Poll player position every FixedUpdate and snap to nearest graph node
///   - Guard against redundant A* calls using node-change detection + recalcPending flag
///   - Handle null path fallback when player is in a drone-inaccessible zone
/// </summary>
public class PlayerEventInterceptor : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Inspector References
    // -----------------------------------------------------------------------

    [Header("Drone Identity")]
    [SerializeField] private int droneId;

    [Header("Detection")]
    [Tooltip("Must match the SphereCollider trigger radius on this drone — default 12m per GDD")]
    [SerializeField] private float detectionRadius = 12f;

    [Header("Player Reference")]
    [Tooltip("Assign the player car Transform here, or it will be found by tag at Start")]
    [SerializeField] private Transform playerCarTransform;

    // -----------------------------------------------------------------------
    // Private State
    // -----------------------------------------------------------------------

    private bool playerInRange = false;
    private bool recalcPending = false;
    private int lastPlayerNodeId = -1;

    // Cached component references
    private DroneStateMachine stateMachine;

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    void Awake()
    {
        stateMachine = GetComponent<DroneStateMachine>();

        if (stateMachine == null)
            Debug.LogError($"[PlayerEventInterceptor] Drone {droneId}: DroneStateMachine not found on same GameObject!");
    }

    void Start()
    {
        // Find player car by tag if not manually assigned in Inspector
        if (playerCarTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerCarTransform = playerObj.transform;
            }
            else
            {
                Debug.LogError($"[PlayerEventInterceptor] Drone {droneId}: Player car not found! " +
                               "Make sure the player GameObject is tagged 'Player'.");
            }
        }

        // Subscribe to path events so we can clear recalcPending flag
        if (PathfindingCoordinator.Instance != null)
        {
            PathfindingCoordinator.Instance.PathCalculated += OnPathCalculated;
            PathfindingCoordinator.Instance.PathFailed += OnPathFailed;
        }
    }

    void OnDestroy()
    {
        if (PathfindingCoordinator.Instance != null)
        {
            PathfindingCoordinator.Instance.PathCalculated -= OnPathCalculated;
            PathfindingCoordinator.Instance.PathFailed -= OnPathFailed;
        }
    }

    // -----------------------------------------------------------------------
    // Physics Trigger Detection
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called by Unity physics when the player car enters this drone's
    /// SphereCollider trigger zone (radius 12m, isTrigger = true).
    /// Transitions drone from PATROL to ALERT.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playerInRange)
            return; // already tracking — ignore duplicate enter events

        playerInRange = true;

        Debug.Log($"[PlayerEventInterceptor] Drone {droneId}: Player DETECTED.");

        // Snap player position to nearest graph node
        int playerNode = NavMeshGraph.Instance.GetNearestNode(other.transform.position);

        // Give state machine the player data it needs before transitioning
        stateMachine.SetLastKnownPlayerData(other.transform.position, playerNode);
        stateMachine.UpdatePlayerNode(playerNode);

        // Transition to ALERT
        stateMachine.TransitionTo(DroneState.ALERT);

        // Seed the last known node immediately
        lastPlayerNodeId = playerNode;
    }

    /// <summary>
    /// Called by Unity physics when the player car exits this drone's
    /// SphereCollider trigger zone.
    /// Transitions drone from ALERT to SEARCH.
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!playerInRange)
            return;

        playerInRange = false;
        recalcPending = false; // clear any pending recalc — no longer chasing

        Debug.Log($"[PlayerEventInterceptor] Drone {droneId}: Player LOST.");

        // Store last known position and node before losing track
        int lastNode = NavMeshGraph.Instance.GetNearestNode(other.transform.position);
        stateMachine.SetLastKnownPlayerData(other.transform.position, lastNode);

        // Transition to SEARCH
        stateMachine.TransitionTo(DroneState.SEARCH);
    }

    // -----------------------------------------------------------------------
    // Dynamic Goal Node Tracking — FixedUpdate
    // -----------------------------------------------------------------------

    /// <summary>
    /// Polls the player car's position every physics tick (50Hz / 0.02s).
    /// Snaps it to the nearest graph node.
    /// Only triggers an A* recalculation when the node ID actually changes
    /// AND no recalculation is already pending.
    ///
    /// This is the node-change guard:
    ///   - Per tick cost: O(1) — one integer comparison
    ///   - A* cost O((V+E) log V) is only paid when node boundary is crossed
    ///   - recalcPending flag prevents stacking duplicate path requests
    /// </summary>
    void FixedUpdate()
    {
        // Only track when drone is actively chasing
        if (!playerInRange || stateMachine.CurrentState != DroneState.ALERT)
            return;

        if (playerCarTransform == null)
            return;

        if (NavMeshGraph.Instance == null)
            return;

        // Snap current player world position to nearest graph node — O(N) or O(1) with spatial hash
        int currentPlayerNodeId = NavMeshGraph.Instance.GetNearestNode(playerCarTransform.position);

        if (currentPlayerNodeId == -1)
            return; // graph not ready yet

        // --- NODE CHANGE GUARD ---
        // Only recalculate A* if:
        //   (a) the player has moved to a different graph node, AND
        //   (b) no recalculation is already queued
        if (currentPlayerNodeId != lastPlayerNodeId && !recalcPending)
        {
            lastPlayerNodeId = currentPlayerNodeId;
            recalcPending = true;

            // Update state machine with latest player data
            stateMachine.SetLastKnownPlayerData(playerCarTransform.position, currentPlayerNodeId);
            stateMachine.UpdatePlayerNode(currentPlayerNodeId);

            Debug.Log($"[PlayerEventInterceptor] Drone {droneId}: Player moved to node {currentPlayerNodeId} — recalculating path.");
        }
    }

    // -----------------------------------------------------------------------
    // Path Event Handlers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called when PathfindingCoordinator finishes a path for this drone.
    /// Clears recalcPending so the next node change can trigger a new request.
    /// </summary>
    void OnPathCalculated(int id, List<Vector3> path)
    {
        if (id != droneId)
            return;

        recalcPending = false;
    }

    /// <summary>
    /// Called when A* fails to find a path for this drone.
    /// This happens when the player is in a narrow alley excluded from the Drone NavMesh.
    /// Clears recalcPending and lets the DroneStateMachine handle the fallback
    /// (it listens to PathFailed separately and transitions to SEARCH).
    /// </summary>
    void OnPathFailed(int id)
    {
        if (id != droneId)
            return;

        recalcPending = false;

        Debug.Log($"[PlayerEventInterceptor] Drone {droneId}: Path failed — player may be in inaccessible zone.");

        // Force exit detection range tracking — state machine will handle SEARCH transition
        playerInRange = false;
    }

    // -----------------------------------------------------------------------
    // Editor Gizmos — Visualize Detection Radius in Scene View
    // -----------------------------------------------------------------------

#if UNITY_EDITOR
    /// <summary>
    /// Draws the detection sphere in Scene view so you can visually confirm
    /// it matches the SphereCollider radius on the drone.
    /// Red when player is in range, white when not.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = playerInRange ? Color.red : Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}
