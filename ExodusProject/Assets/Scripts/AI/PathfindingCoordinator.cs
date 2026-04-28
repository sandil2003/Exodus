using UnityEngine; // Required for MonoBehaviour and Vector3
using System; // Required for Action
using System.Collections.Generic; // Required for List

/// <summary>
/// PathfindingCoordinator.cs — IS Module | Student 3 (A* Search & Heuristic Design)
/// Acts as the central hub for all pathfinding requests between modules.
/// </summary>
public class PathfindingCoordinator : MonoBehaviour
{
    public static PathfindingCoordinator Instance { get; private set; }

    // Events that DroneAnimator (GV Student 4) and DroneStateMachine (IS Student 2) listen to
    public event Action<int, List<Vector3>> PathCalculated;
    public event Action<int> PathFailed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Called by DroneStateMachine to request a new A* path for a specific drone.
    /// </summary>
    public void RequestPath(int droneId, int startNode, int goalNode)
    {
        // Calls the A* algorithm implemented by Student 3
        List<Vector3> path = AStarPathfinder.Instance.FindPath(startNode, goalNode, NavMeshGraph.Instance);

        if (path != null && path.Count > 0)
        {
            // Notify movement scripts that a path is ready
            PathCalculated?.Invoke(droneId, path);
        }
        else
        {
            // Notify state machine to trigger the SEARCH fallback
            PathFailed?.Invoke(droneId);
        }
    }
}