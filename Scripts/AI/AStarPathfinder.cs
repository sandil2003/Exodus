using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AStarPathfinder.cs — IS Module | Student 3 (A* Search & Heuristic Design)
/// Implementation of the A* algorithm using an admissible 3D Euclidean heuristic.
/// </summary>
public class AStarPathfinder : MonoBehaviour
{
    public static AStarPathfinder Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<Vector3> FindPath(int startNodeId, int goalNodeId, NavMeshGraph graph)
    {
        if (!graph.Nodes.ContainsKey(startNodeId) || !graph.Nodes.ContainsKey(goalNodeId))
            return null;

        // The "Open Set" managed by a Min-Heap Priority Queue for O(log n) efficiency
        MinHeapPriorityQueue openSet = new MinHeapPriorityQueue();
        openSet.Enqueue(startNodeId, 0);

        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        Dictionary<int, float> gScore = new Dictionary<int, float>(); // Cost from start to node

        gScore[startNodeId] = 0;
        cameFrom[startNodeId] = -1;

        while (openSet.Count > 0)
        {
            int current = openSet.Dequeue();

            if (current == goalNodeId)
                return ReconstructPath(cameFrom, goalNodeId, graph);

            foreach (GraphEdge edge in graph.GetNeighbours(current))
            {
                float tentativeGScore = gScore[current] + edge.cost;

                if (!gScore.ContainsKey(edge.toNodeId) || tentativeGScore < gScore[edge.toNodeId])
                {
                    cameFrom[edge.toNodeId] = current;
                    gScore[edge.toNodeId] = tentativeGScore;

                    // f(n) = g(n) + h(n)
                    float fScore = tentativeGScore + GetHeuristic(edge.toNodeId, goalNodeId, graph);
                    openSet.Enqueue(edge.toNodeId, fScore);
                }
            }
        }

        return null; // No path found
    }

    /// <summary>
    /// Admissible & Consistent 3D Euclidean Heuristic.
    /// A straight line is the shortest possible path, so it never overestimates.
    /// </summary>
    private float GetHeuristic(int nodeId, int goalId, NavMeshGraph graph)
    {
        Vector3 posA = graph.GetNodePosition(nodeId);
        Vector3 posB = graph.GetNodePosition(goalId);
        return Vector3.Distance(posA, posB);
    }

    private List<Vector3> ReconstructPath(Dictionary<int, int> cameFrom, int current, NavMeshGraph graph)
    {
        List<Vector3> path = new List<Vector3>();
        while (current != -1)
        {
            path.Add(graph.GetNodePosition(current));
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }
}