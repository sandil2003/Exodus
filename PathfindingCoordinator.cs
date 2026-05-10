using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// PathfindingCoordinator.cs - IS Module | Student 1 (Centralized Dispatcher)
/// Handles path requests from DroneStateMachines and dispatches them to AStarPathfinder.
/// Fires events when paths are ready or when search fails.
/// </summary>
public class PathfindingCoordinator : MonoBehaviour
{
    public static PathfindingCoordinator Instance { get; private set; }

    public event Action<int, List<Vector3>> PathCalculated;
    public event Action<int> PathFailed;

    private struct PathRequest
    {
        public int droneId;
        public int startNodeId;
        public int goalNodeId;
        public Vector3 targetWorldPos;
    }

    private Queue<PathRequest> _requestQueue = new Queue<PathRequest>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Receives a path request and adds it to the queue to be processed over multiple frames.
    /// </summary>
    public void RequestPath(int droneId, int startNodeId, int goalNodeId, Vector3 targetWorldPos)
    {
        // Avoid duplicate requests for the same drone in the same queue
        // (If a drone requests again before the first is processed, we update its target)
        // For simplicity here, we just add it.
        _requestQueue.Enqueue(new PathRequest {
            droneId = droneId,
            startNodeId = startNodeId,
            goalNodeId = goalNodeId,
            targetWorldPos = targetWorldPos
        });
    }

    void Update()
    {
        // Process only ONE path request per frame to maintain high FPS
        if (_requestQueue.Count > 0)
        {
            PathRequest request = _requestQueue.Dequeue();
            ProcessRequest(request);
        }
    }

    private void ProcessRequest(PathRequest request)
    {
        if (AStarPathfinder.Instance == null || NavMeshGraph.Instance == null) return;

        List<Vector3> path = AStarPathfinder.Instance.FindPath(
            request.startNodeId,
            request.goalNodeId,
            NavMeshGraph.Instance
        );

        if (path != null && path.Count > 0)
        {
            PathCalculated?.Invoke(request.droneId, path);
        }
        else
        {
            PathFailed?.Invoke(request.droneId);
        }
    }
}
