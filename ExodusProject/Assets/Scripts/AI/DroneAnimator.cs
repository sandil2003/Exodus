using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DroneAnimator.cs — GV Module | Student 4 (Agent Controller)
/// 
/// Responsibilities:
///   - Consume IS path arrays (List<Vector3>) from PathfindingCoordinator
///   - Move the drone smoothly along those waypoints at ground/hover level
///   - Apply smooth yaw rotation to face movement direction (Quaternion.Slerp)
///   - Apply banking tilt on turns to the drone body child mesh
///   - Apply idle hover bob when stationary
///   - Respond to IS DroneStateMachine state changes (PATROL / ALERT / SEARCH)
///   - Toggle bottom spotlight on ALERT state
/// </summary>
public class DroneAnimator : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Inspector References
    // -----------------------------------------------------------------------

    [Header("Child References")]
    [Tooltip("The drone body mesh child object — banking and hover bob applied here only")]
    [SerializeField] private Transform droneBodyTransform;

    [Tooltip("Bottom-mounted spotlight — activates during ALERT state")]
    [SerializeField] private Light spotLight;

    [Header("Drone Identity")]
    [SerializeField] private int droneId;

    // -----------------------------------------------------------------------
    // Movement Settings
    // -----------------------------------------------------------------------

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 4f;
    [SerializeField] private float alertSpeed = 9f;
    [SerializeField] private float searchSpeed = 3f;

    [Header("Movement Tuning")]
    [Tooltip("How close the drone must get to a waypoint before moving to the next one")]
    [SerializeField] private float arrivalThreshold = 0.25f;

    [Tooltip("Small Y offset above ground so the drone appears to hover slightly")]
    [SerializeField] private float hoverHeightOffset = 0.3f;

    // -----------------------------------------------------------------------
    // Rotation Settings
    // -----------------------------------------------------------------------

    [Header("Rotation")]
    [Tooltip("How fast the drone yaws to face its movement direction")]
    [SerializeField] private float rotationSpeed = 6f;

    // -----------------------------------------------------------------------
    // Banking Settings
    // -----------------------------------------------------------------------

    [Header("Banking (applied to body child only)")]
    [Tooltip("Maximum roll angle in degrees when turning")]
    [SerializeField] private float maxBankAngle = 22f;

    [Tooltip("Multiplier — higher = more aggressive tilt on turns")]
    [SerializeField] private float bankMultiplier = 2.5f;

    [Tooltip("How smoothly the bank angle ramps up and down")]
    [SerializeField] private float bankSmoothSpeed = 5f;

    // -----------------------------------------------------------------------
    // Hover Bob Settings
    // -----------------------------------------------------------------------

    [Header("Hover Bob (applied to body child only)")]
    [Tooltip("Speed of the up/down hover oscillation")]
    [SerializeField] private float hoverFrequency = 1.4f;

    [Tooltip("Height of the hover oscillation in world units")]
    [SerializeField] private float hoverAmplitude = 0.06f;

    [Tooltip("Slow idle Y rotation drift on the body to give life when stationary")]
    [SerializeField] private float idleDriftSpeed = 12f;

    // -----------------------------------------------------------------------
    // Private State
    // -----------------------------------------------------------------------

    private List<Vector3> currentPath;
    private int waypointIndex = 0;
    private float currentFlightSpeed;
    private float currentBankAngle = 0f;
    private Vector3 lastPosition;
    private DroneState currentState = DroneState.PATROL;

    // Cached component references
    private DroneStateMachine stateMachine;
    private PlayerEventInterceptor interceptor;

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    void Awake()
    {
        // Cache components on the same GameObject
        stateMachine = GetComponent<DroneStateMachine>();
        interceptor = GetComponent<PlayerEventInterceptor>();

        // Default speed
        currentFlightSpeed = patrolSpeed;

        // Store starting position to avoid a spike on frame 1 velocity calc
        lastPosition = transform.position;

        // Safety check — warn in editor if body child not assigned
        if (droneBodyTransform == null)
            Debug.LogWarning($"[DroneAnimator] Drone {droneId}: droneBodyTransform not assigned in Inspector!");

        if (spotLight != null)
            spotLight.enabled = false;
    }

    void OnEnable()
    {
        // Subscribe to IS events
        if (PathfindingCoordinator.Instance != null)
        {
            PathfindingCoordinator.Instance.PathCalculated += OnNewPathReceived;
            PathfindingCoordinator.Instance.PathFailed += OnPathFailed;
        }

        if (stateMachine != null)
            stateMachine.StateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks / null ref errors
        if (PathfindingCoordinator.Instance != null)
        {
            PathfindingCoordinator.Instance.PathCalculated -= OnNewPathReceived;
            PathfindingCoordinator.Instance.PathFailed -= OnPathFailed;
        }

        if (stateMachine != null)
            stateMachine.StateChanged -= OnStateChanged;
    }

    /// <summary>
    /// Called by DroneStateMachine during SEARCH state to follow a BFS path.
    /// Separate from OnNewPathReceived so SEARCH paths don't interfere with A* paths.
    /// </summary>
    public void ReceiveBFSPath(List<Vector3> bfsPath)
    {
        if (bfsPath == null || bfsPath.Count == 0)
            return;

        currentPath = bfsPath;
        waypointIndex = 0;
    }

    void Update()
    {
        MoveAlongPath();
        SmoothYawToFaceDirection();
        ApplyBankingToBodyChild();
        ApplyHoverBob();
    }

    // -----------------------------------------------------------------------
    // Movement
    // -----------------------------------------------------------------------

    /// <summary>
    /// Moves the drone toward the next waypoint in currentPath.
    /// Applies a small Y offset so the drone hovers just above the ground.
    /// Advances waypointIndex when arrival threshold is reached.
    /// </summary>
    void MoveAlongPath()
    {
        if (currentPath == null || waypointIndex >= currentPath.Count)
            return;

        // Apply hover height — keeps drone slightly above the ground NavMesh surface
        Vector3 rawWaypoint = currentPath[waypointIndex];
        Vector3 targetPosition = new Vector3(rawWaypoint.x,
                                             rawWaypoint.y + hoverHeightOffset,
                                             rawWaypoint.z);

        // Smooth translation — never teleport
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            currentFlightSpeed * Time.deltaTime
        );

        // Arrive at waypoint — move to next
        if (Vector3.Distance(transform.position, targetPosition) < arrivalThreshold)
        {
            waypointIndex++;
        }
    }

    // -----------------------------------------------------------------------
    // Rotation — Yaw Only on Root Transform
    // -----------------------------------------------------------------------

    /// <summary>
    /// Smoothly rotates the drone root to face its movement direction.
    /// Only rotates on the Y axis (yaw) — pitch/roll are handled separately on the body child.
    /// Uses Quaternion.Slerp for constant-speed smooth turning.
    /// </summary>
    void SmoothYawToFaceDirection()
    {
        if (currentPath == null || waypointIndex >= currentPath.Count)
            return;

        Vector3 rawWaypoint = currentPath[waypointIndex];
        Vector3 targetPosition = new Vector3(rawWaypoint.x,
                                             transform.position.y, // flatten Y — yaw only
                                             rawWaypoint.z);

        Vector3 direction = (targetPosition - transform.position).normalized;

        if (direction == Vector3.zero)
            return;

        // Build target rotation looking along the movement direction, keeping world up
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        // Slerp toward it — smooth, constant angular speed
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // -----------------------------------------------------------------------
    // Banking — Applied to Body Child Only
    // -----------------------------------------------------------------------

    /// <summary>
    /// Calculates how much the drone is turning sideways this frame
    /// and tilts the drone body child mesh to simulate a banking turn.
    /// 
    /// Applied to droneBodyTransform (child), NOT the root transform.
    /// Root keeps its yaw for correct LookRotation — body handles the visual roll.
    /// </summary>
    void ApplyBankingToBodyChild()
    {
        if (droneBodyTransform == null)
            return;

        // Frame velocity based on how far root moved since last frame
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Convert world velocity to local space — x component = lateral (sideways) movement
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // Negative because turning right means tilting right (positive Z in local = tilt left)
        float targetBank = Mathf.Clamp(
            -localVelocity.x * bankMultiplier,
            -maxBankAngle,
            maxBankAngle
        );

        // Smooth the bank angle so it ramps gracefully rather than snapping
        currentBankAngle = Mathf.Lerp(
            currentBankAngle,
            targetBank,
            bankSmoothSpeed * Time.deltaTime
        );

        // Get current hover bob Y offset so we dont overwrite it
        float bobY = droneBodyTransform.localPosition.y;

        // Combine: hover bob position on Y, banking roll on Z
        // We preserve any existing X rotation (e.g. slight pitch) via Euler
        droneBodyTransform.localRotation = Quaternion.Euler(
            droneBodyTransform.localEulerAngles.x,  // preserve pitch if any
            0f,                                      // no extra local yaw on body
            currentBankAngle                         // banking roll
        );

        // Keep body position at hover bob offset (bob is written in ApplyHoverBob)
        // This line intentionally left to ApplyHoverBob() to avoid double-writing localPosition
    }

    // -----------------------------------------------------------------------
    // Hover Bob — Applied to Body Child Only
    // -----------------------------------------------------------------------

    /// <summary>
    /// Applies a gentle sine wave up/down oscillation to the drone body child.
    /// Also adds a slow Y-axis drift rotation when the drone is idle/patrolling.
    /// This makes the drone look alive even when stationary.
    /// </summary>
    void ApplyHoverBob()
    {
        if (droneBodyTransform == null)
            return;

        // Sine wave on Y — smooth oscillation
        float bobOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;

        // Apply Y position offset to body child (independent of root Y position)
        droneBodyTransform.localPosition = new Vector3(0f, bobOffset, 0f);

        // Idle drift — only rotate slowly when in PATROL or SEARCH (not aggressively chasing)
        if (currentState != DroneState.ALERT)
        {
            droneBodyTransform.Rotate(0f, idleDriftSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }

    // -----------------------------------------------------------------------
    // Event Handlers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called by PathfindingCoordinator when a new A* path is ready for this drone.
    /// Resets waypoint index so drone begins following the new path from the start.
    /// </summary>
    void OnNewPathReceived(int id, List<Vector3> newPath)
    {
        if (id != droneId)
            return;

        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning($"[DroneAnimator] Drone {droneId}: Received empty or null path.");
            return;
        }

        currentPath = newPath;
        waypointIndex = 0; // always restart from beginning of new path
    }

    /// <summary>
    /// Called by PathfindingCoordinator when A* cannot find a path (player in inaccessible zone).
    /// Clears current path so drone stops moving and waits.
    /// </summary>
    void OnPathFailed(int id)
    {
        if (id != droneId)
            return;

        currentPath = null;
        waypointIndex = 0;
        Debug.Log($"[DroneAnimator] Drone {droneId}: Path failed — drone holding position.");
    }

    /// <summary>
    /// Called by DroneStateMachine when state changes.
    /// Updates speed, bank angle limits, and spotlight accordingly.
    /// </summary>
    void OnStateChanged(DroneState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case DroneState.PATROL:
                currentFlightSpeed = patrolSpeed;
                maxBankAngle = 12f;
                if (spotLight != null) spotLight.enabled = false;
                break;

            case DroneState.ALERT:
                currentFlightSpeed = alertSpeed;
                maxBankAngle = 22f;
                if (spotLight != null) spotLight.enabled = true;
                break;

            case DroneState.SEARCH:
                currentFlightSpeed = searchSpeed;
                maxBankAngle = 8f;
                if (spotLight != null) spotLight.enabled = true;
                break;
        }
    }

    // -----------------------------------------------------------------------
    // Public Utility
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns true if the drone has finished following its current path.
    /// Used by DroneStateMachine to know when to request the next patrol waypoint.
    /// </summary>
    public bool HasReachedEndOfPath()
    {
        return currentPath == null || waypointIndex >= currentPath.Count;
    }

    /// <summary>
    /// Clears the current path immediately. Called when drone enters SEARCH state
    /// and needs to stop following its ALERT intercept path before BFS path arrives.
    /// </summary>
    public void ClearPath()
    {
        currentPath = null;
        waypointIndex = 0;
    }

    // -----------------------------------------------------------------------
    // Editor Debug Gizmos
    // -----------------------------------------------------------------------

#if UNITY_EDITOR
    /// <summary>
    /// Draws the current path in the Scene view as a yellow line for debugging.
    /// Only visible in the Unity Editor — zero runtime cost in builds.
    /// </summary>
    void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        Gizmos.color = Color.yellow;

        // Draw the remaining path from current waypoint onward
        for (int i = waypointIndex; i < currentPath.Count - 1; i++)
        {
            Vector3 from = currentPath[i]     + Vector3.up * hoverHeightOffset;
            Vector3 to   = currentPath[i + 1] + Vector3.up * hoverHeightOffset;
            Gizmos.DrawLine(from, to);
            Gizmos.DrawSphere(from, 0.15f);
        }

        // Draw the final waypoint
        if (waypointIndex < currentPath.Count)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                currentPath[currentPath.Count - 1] + Vector3.up * hoverHeightOffset,
                0.2f
            );
        }
    }
#endif
}
