using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// DynamicEnvironmentAdapter.cs - IS Module | Student 2 (Dynamic Adaptation)
/// Responsible for detecting environmental changes (like player-dropped barricades)
/// and dynamically severing edges in the NavMeshGraph to force recalculation.
/// </summary>
public class DynamicEnvironmentAdapter : MonoBehaviour
{
    public static DynamicEnvironmentAdapter Instance { get; private set; }

    [Header("Detection Settings")]
    public LayerMask obstacleLayer;
    public float obstacleCheckRadius = 2.0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Called when an environment-altering event occurs (e.g., a barricade is dropped).
    /// Finds all edges that intersect with the new obstacle and removes them.
    /// </summary>
    /// <param name="obstaclePosition">The world position of the new obstacle.</param>
    /// <param name="obstacleBounds">The radius or bounds of the obstacle to check against.</param>
    public void OnEnvironmentAltered(Vector3 obstaclePosition, float obstacleBounds)
    {
        if (NavMeshGraph.Instance == null) return;

        Debug.Log($"[Student 2] Environment Altered at {obstaclePosition}. Severing affected edges...");

        int severedCount = 0;
        HashSet<int> nodesToRecalculate = new HashSet<int>();

        // Iterate through all nodes to find edges passing through the obstacle
        foreach (var nodeEntry in NavMeshGraph.Instance.Nodes)
        {
            GraphNode node = nodeEntry.Value;
            
            // Optimization: Only check nodes within a reasonable distance of the obstacle
            if (Vector3.Distance(node.worldPosition, obstaclePosition) > 50f) continue;

            // Check each edge from this node
            for (int i = node.edges.Count - 1; i >= 0; i--)
            {
                GraphEdge edge = node.edges[i];
                Vector3 neighborPos = NavMeshGraph.Instance.GetNodePosition(edge.toNodeId);

                // Use a line-sphere intersection check to see if the edge hits the obstacle
                if (IsEdgeIntersectingObstacle(node.worldPosition, neighborPos, obstaclePosition, obstacleBounds))
                {
                    node.edges.RemoveAt(i);
                    severedCount++;
                    nodesToRecalculate.Add(node.nodeId);
                    nodesToRecalculate.Add(edge.toNodeId);
                }
            }
        }

        if (severedCount > 0)
        {
            Debug.Log($"[Student 2] Successfully severed {severedCount} edges. NavMeshGraph updated.");
            // Trigger path recalculation for any drones currently using these nodes
            NotifyDronesOfChange();
        }
    }

    /// <summary>
    /// Simple line-sphere intersection check.
    /// </summary>
    private bool IsEdgeIntersectingObstacle(Vector3 start, Vector3 end, Vector3 obsPos, float radius)
    {
        Vector3 lineVec = end - start;
        Vector3 startToObs = obsPos - start;
        float projection = Vector3.Dot(startToObs, lineVec.normalized);
        
        if (projection < 0 || projection > lineVec.magnitude) return false;

        Vector3 closestPoint = start + lineVec.normalized * projection;
        return Vector3.Distance(closestPoint, obsPos) < radius;
    }

    private void NotifyDronesOfChange()
    {
        // In a full implementation, this would tell the PathfindingCoordinator 
        // to invalidate current paths and force drones to request new ones.
        Debug.Log("[Student 2] Notifying all drones to recalculate paths due to environment change.");
    }
}
