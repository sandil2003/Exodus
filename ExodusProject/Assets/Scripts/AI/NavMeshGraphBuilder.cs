using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NavMeshGraphBuilder : MonoBehaviour
{
    [Header("Grid Settings")]
    public float resolution = 8f;
    public float sampleRadius = 5f;
    public float searchBounds = 200f;

    [Header("Connection Settings")]
    public float maxConnectionDistance = 15f;

    void Start()
    {
        BuildGraph();
    }

    public void BuildGraph()
    {
        int nodeId = 0;
        int areaMask = NavMesh.AllAreas;
        float samplingHeight = transform.position.y;

        NavMeshGraph.Instance.Nodes.Clear();

        // 1. PHASE 1: Create Nodes on a Grid
        for (float x = -searchBounds; x <= searchBounds; x += resolution)
        {
            for (float z = -searchBounds; z <= searchBounds; z += resolution)
            {
                Vector3 candidate = new Vector3(x, samplingHeight, z);
                
                // Search for the nearest walkable NavMesh point
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, areaMask))
                {
                    // Basic overlap check to avoid duplicate nodes at same spot
                    bool tooClose = false;
                    foreach (var existingNode in NavMeshGraph.Instance.Nodes.Values)
                    {
                        if (Vector3.Distance(hit.position, existingNode.worldPosition) < (resolution * 0.5f))
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        NavMeshGraph.Instance.Nodes.Add(nodeId, new GraphNode(nodeId, hit.position));
                        nodeId++;
                    }
                }
            }
        }

        Debug.Log($"[NavMeshGraphBuilder] Phase 1 Complete: {NavMeshGraph.Instance.Nodes.Count} nodes created at Y={samplingHeight}.");

        // 2. PHASE 2: Connect Nodes (Building Edges)
        int edgeCount = 0;
        var allNodes = new List<GraphNode>(NavMeshGraph.Instance.Nodes.Values);

        for (int i = 0; i < allNodes.Count; i++)
        {
            for (int j = i + 1; j < allNodes.Count; j++)
            {
                GraphNode nodeA = allNodes[i];
                GraphNode nodeB = allNodes[j];

                float dist = Vector3.Distance(nodeA.worldPosition, nodeB.worldPosition);

                // Only try to connect nodes that are within range
                if (dist <= maxConnectionDistance)
                {
                    // Check if there is a valid NavMesh path between them
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(nodeA.worldPosition, nodeB.worldPosition, areaMask, path))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            // Add bidirectional edges
                            nodeA.edges.Add(new GraphEdge(nodeB.nodeId, dist));
                            nodeB.edges.Add(new GraphEdge(nodeA.nodeId, dist));
                            edgeCount += 2;
                        }
                    }
                }
            }
        }

        Debug.Log($"[NavMeshGraphBuilder] Phase 2 Complete: {edgeCount} edges created. Graph is ready!");
    }
}