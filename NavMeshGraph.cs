using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GraphEdge
{
    public int toNodeId;
    public float cost;

    public GraphEdge(int id, float edgeCost)
    {
        toNodeId = id;
        cost = edgeCost;
    }
}

public class GraphNode
{
    public int nodeId;
    public Vector3 worldPosition;
    public List<GraphEdge> edges = new List<GraphEdge>();

    public GraphNode(int id, Vector3 pos)
    {
        nodeId = id;
        worldPosition = pos;
    }
}

public class NavMeshGraph : MonoBehaviour
{
    public static NavMeshGraph Instance { get; private set; }

    public Dictionary<int, GraphNode> Nodes = new Dictionary<int, GraphNode>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Public API for Students 2, 3, and 4
    public int GetNearestNode(Vector3 position)
    {
        int nearestId = -1;
        float minDistance = float.MaxValue;

        foreach (var node in Nodes.Values)
        {
            float dist = Vector3.Distance(position, node.worldPosition);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestId = node.nodeId;
            }
        }
        return nearestId;
    }

    public List<GraphEdge> GetNeighbours(int nodeId)
    {
        if (Nodes.ContainsKey(nodeId))
            return Nodes[nodeId].edges;
        return new List<GraphEdge>();
    }

    public Vector3 GetNodePosition(int nodeId)
    {
        if (Nodes.ContainsKey(nodeId))
            return Nodes[nodeId].worldPosition;
        return Vector3.zero;
    }
}