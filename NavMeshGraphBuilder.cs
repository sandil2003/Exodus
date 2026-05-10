using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NavMeshGraphBuilder : MonoBehaviour
{
    [Header("Grid Settings")]
    public float resolution = 8f;
    public float sampleRadius = 5f;
    public float searchBounds = 1000f; // Increased to 1000 to cover later stages

    [Header("Connection Settings")]
    public float maxConnectionDistance = 25f;

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
        Dictionary<Vector2Int, List<GraphNode>> spatialGrid = new Dictionary<Vector2Int, List<GraphNode>>();

        // 1. PHASE 1: Create Nodes on a Grid
        for (float x = -searchBounds; x <= searchBounds; x += resolution)
        {
            for (float z = -searchBounds; z <= searchBounds; z += resolution)
            {
                Vector3 candidate = new Vector3(x, samplingHeight, z);
                
                // Search for the nearest walkable NavMesh point
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, areaMask))
                {
                    Vector2Int gridCoord = new Vector2Int(Mathf.RoundToInt(hit.position.x / resolution), Mathf.RoundToInt(hit.position.z / resolution));
                    
                    // Basic overlap check using spatial grid to avoid duplicate nodes at same spot
                    bool tooClose = false;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            if (spatialGrid.TryGetValue(new Vector2Int(gridCoord.x + dx, gridCoord.y + dz), out List<GraphNode> existingNodes))
                            {
                                foreach (GraphNode existingNode in existingNodes)
                                {
                                    if (Vector3.Distance(hit.position, existingNode.worldPosition) < (resolution * 0.5f))
                                    {
                                        tooClose = true;
                                        break;
                                    }
                                }
                            }
                            if (tooClose) break;
                        }
                        if (tooClose) break;
                    }

                    if (!tooClose)
                    {
                        GraphNode newNode = new GraphNode(nodeId, hit.position);
                        NavMeshGraph.Instance.Nodes.Add(nodeId, newNode);
                        
                        if (!spatialGrid.ContainsKey(gridCoord))
                            spatialGrid[gridCoord] = new List<GraphNode>();
                        spatialGrid[gridCoord].Add(newNode);
                        
                        nodeId++;
                    }
                }
            }
        }

        Debug.Log($"[NavMeshGraphBuilder] Phase 1 Complete: {NavMeshGraph.Instance.Nodes.Count} nodes created at Y={samplingHeight}.");

        // 2. PHASE 2: Connect Nodes (Building Edges) using O(N) Spatial Grid
        int edgeCount = 0;
        int checkRange = Mathf.CeilToInt(maxConnectionDistance / resolution);

        foreach (var kvp in spatialGrid)
        {
            Vector2Int currentCoord = kvp.Key;
            
            foreach (GraphNode nodeA in kvp.Value)
            {
                for (int dx = -checkRange; dx <= checkRange; dx++)
                {
                    for (int dz = -checkRange; dz <= checkRange; dz++)
                    {
                        Vector2Int neighborCoord = new Vector2Int(currentCoord.x + dx, currentCoord.y + dz);
                        
                        if (spatialGrid.TryGetValue(neighborCoord, out List<GraphNode> neighborNodes))
                        {
                            foreach (GraphNode nodeB in neighborNodes)
                            {
                                // Prevent self-connections and checking same pair twice
                                if (nodeB.nodeId <= nodeA.nodeId) continue;

                                float dist = Vector3.Distance(nodeA.worldPosition, nodeB.worldPosition);

                                // Only try to connect nodes that are within range
                                if (dist <= maxConnectionDistance)
                                {
                                    // Use NavMesh.Raycast to check for wall intersections
                                    if (!NavMesh.Raycast(nodeA.worldPosition, nodeB.worldPosition, out NavMeshHit hit, areaMask))
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
                }
            }
        }

        Debug.Log($"[NavMeshGraphBuilder] Phase 2 Complete: {edgeCount} edges created. Graph is ready!");
    }
}