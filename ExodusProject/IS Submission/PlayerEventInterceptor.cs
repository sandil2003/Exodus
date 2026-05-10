using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerEventInterceptor.cs - IS Module | Student 2 (Dynamic Adaptation & Event Interception)
/// Optimized with a stability grace period to prevent flickering detection.
/// </summary>
public class PlayerEventInterceptor : MonoBehaviour
{
    [Header("Drone Identity")]
    [SerializeField] private int droneId;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float trackingRadius = 40f; // Keep tracking up to 40m once alerted
    [SerializeField] private float detectionGracePeriod = 3.0f;

    [Header("Player Tracking")]
    [SerializeField] private Transform playerCarTransform;
    [SerializeField] private float pathUpdateDistance = 15.0f;
    private float lastPathRequestTime = 0f;

    private DroneStateMachine stateMachine;
    private float lastSeenTime = -1f;
    private Vector3 lastPathRequestPos = Vector3.zero;

    // Optimization
    private float _lastUpdateCheck = 0f;
    private const float UPDATE_INTERVAL = 0.2f;

    void Awake()
    {
        stateMachine = GetComponent<DroneStateMachine>();
    }

    void Start()
    {
        if (playerCarTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerCarTransform = player.transform;
        }

        // Ensure tracking radius is always larger than detection radius
        if (trackingRadius <= detectionRadius)
        {
            trackingRadius = detectionRadius * 1.5f;
        }
    }

    void Update()
    {
        if (stateMachine == null || playerCarTransform == null) return;

        // Optimization: Run detection and tracking only 5 times per second
        if (Time.time - _lastUpdateCheck < UPDATE_INTERVAL) return;
        _lastUpdateCheck = Time.time;

        float distToPlayer = Vector3.Distance(transform.position, playerCarTransform.position);

        // 1. PROXIMITY DETECTION
        // If we are patrolling/searching and the player gets close, start the chase!
        if (stateMachine.CurrentState != DroneState.ALERT)
        {
            if (distToPlayer < detectionRadius)
            {
                lastSeenTime = Time.time;
                if (NavMeshGraph.Instance != null)
                {
                    int playerNode = NavMeshGraph.Instance.GetNearestNode(playerCarTransform.position);
                    stateMachine.SetLastKnownPlayerData(playerCarTransform.position, playerNode);
                    stateMachine.TransitionTo(DroneState.ALERT);
                    lastPathRequestPos = playerCarTransform.position;
                    Debug.Log($"[PlayerEventInterceptor] Drone {droneId}: Player DETECTED by radar (Dist: {distToPlayer:F1}m).");
                }
            }
        }
        else // CHASE LOGIC
        {
            // Keep detection alive as long as player is within tracking range
            if (distToPlayer < trackingRadius)
            {
                lastSeenTime = Time.time;
            }

            // If we haven't seen the player for a while, give up and search
            if (Time.time - lastSeenTime > detectionGracePeriod)
            {
                Debug.Log($"[PlayerEventInterceptor] Drone {droneId}: Player LOST (Distance timeout).");
                stateMachine.TransitionTo(DroneState.SEARCH);
                return;
            }

            // Periodically update the path to follow the car's current position
            // Added a 1-second cooldown so it doesn't stutter and freeze
            float distMoved = Vector3.Distance(playerCarTransform.position, lastPathRequestPos);
            if (distMoved > pathUpdateDistance || (Time.time - lastPathRequestTime > 1.5f))
            {
                if (NavMeshGraph.Instance != null)
                {
                    int playerNode = NavMeshGraph.Instance.GetNearestNode(playerCarTransform.position);
                    stateMachine.SetLastKnownPlayerData(playerCarTransform.position, playerNode);
                    stateMachine.UpdatePlayerNode(playerNode);
                    
                    lastPathRequestPos = playerCarTransform.position;
                    lastPathRequestTime = Time.time;
                    Debug.Log($"[PlayerEventInterceptor] Drone {droneId}: Updating chase path.");
                }
            }
        }
    }

    // -----------------------------------------------------------------------
    // Editor Gizmos - Visualize Detection Radius in Scene View
    // -----------------------------------------------------------------------

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = (stateMachine != null && stateMachine.CurrentState == DroneState.ALERT) ? Color.red : Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        if (stateMachine != null && stateMachine.CurrentState == DroneState.ALERT)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, trackingRadius);
        }
    }
#endif
}
