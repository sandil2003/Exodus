using UnityEngine;
using System.Collections.Generic;

public class BFSPathfinder : MonoBehaviour
{
    public static BFSPathfinder Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<Vector3> FindPath(int startNodeId, int goalNodeId, NavMeshGraph graph)
    {
        if (!graph.Nodes.ContainsKey(startNodeId) || !graph.Nodes.ContainsKey(goalNodeId))
            return null;

        // BFS uses a FIFO Queue
        Queue<int> frontier = new Queue<int>();
        frontier.Enqueue(startNodeId);

        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        cameFrom[startNodeId] = -1; // Root node

        bool foundGoal = false;

        while (frontier.Count > 0)
        {
            int current = frontier.Dequeue();

            if (current == goalNodeId)
            {
                foundGoal = true;
                break;
            }

            foreach (GraphEdge edge in graph.GetNeighbours(current))
            {
                if (!cameFrom.ContainsKey(edge.toNodeId))
                {
                    frontier.Enqueue(edge.toNodeId);
                    cameFrom[edge.toNodeId] = current;
                }
            }
        }

        if (foundGoal)
        {
            return ReconstructPath(cameFrom, goalNodeId, graph);
        }

        return null; // No path found
    }

    private List<Vector3> ReconstructPath(Dictionary<int, int> cameFrom, int goalId, NavMeshGraph graph)
    {
        List<Vector3> path = new List<Vector3>();
        int current = goalId;

        while (current != -1)
        {
            path.Add(graph.GetNodePosition(current));
            current = cameFrom[current];
        }

        path.Reverse(); // Reverse to get start-to-goal order
        return path;
    }
}