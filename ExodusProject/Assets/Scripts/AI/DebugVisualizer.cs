using UnityEngine; // This line is required to fix the error

/// <summary>
/// DebugVisualizer.cs — IS Module | Student 4 (Secondary Search & Debug Visualizer)
/// Renders the live mathematical state of the graph and search algorithms.
/// </summary>
public class DebugVisualizer : MonoBehaviour
{
    [Header("Settings")]
    public bool debugMode = true;

    /// <summary>
    /// Draws the static graph structure (nodes and edges) in the Scene view.
    /// Used to confirm Student 1's graph matches the city geometry.
    /// </summary>
    void OnDrawGizmos()
    {
        // Guard clause to prevent errors if the graph isn't initialized yet
        if (!debugMode || NavMeshGraph.Instance == null) return;

        // Render nodes as white spheres
        Gizmos.color = Color.white;
        foreach (var node in NavMeshGraph.Instance.Nodes.Values)
        {
            Gizmos.DrawSphere(node.worldPosition, 0.5f);

            // Render edges as grey lines to confirm the adjacency list is correct
            Gizmos.color = Color.grey;
            foreach (var edge in node.edges)
            {
                Vector3 neighbourPos = NavMeshGraph.Instance.GetNodePosition(edge.toNodeId);
                Gizmos.DrawLine(node.worldPosition, neighbourPos);
            }
            Gizmos.color = Color.white; // Reset for next node sphere
        }
    }
}