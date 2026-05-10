using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// DebugVisualizer.cs - IS/GV Module | Toggleable Debug System
/// Provides a live view of the Pathfinding Graph, A* Search state, and Drone logic.
/// Toggle on/off with [F1].
/// </summary>
public class DebugVisualizer : MonoBehaviour
{
    public static DebugVisualizer Instance { get; private set; }

    [Header("Colors")]
    public Color graphNodeColor = new Color(0, 0.5f, 1, 0.3f);
    public Color graphEdgeColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
    public Color pathColor = Color.green;
    public Color openSetColor = Color.yellow;
    public Color closedSetColor = Color.red;
    public Color detectionColor = new Color(1, 1, 1, 0.2f);

    [Header("Settings")]
    public bool showDebug = false;
    public float navMapZoom = 1.5f; // Increase this to see more of the city!
    public GameObject cityRoot;     // Drag your City object here for perfect map bounds!
    
    // Performance Caching
    private GameObject _player;
    private DroneAnimator[] _cachedDrones;
    private float _lastDroneUpdateTime = 0f;
    private const float DRONE_UPDATE_INTERVAL = 0.5f;
    private Rect _cityBoundsRect;
    private bool _hasCityBounds = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        // Toggle visualization with F1
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showDebug = !showDebug;
            Debug.Log($"[DebugVisualizer] Visualization: {(showDebug ? "ENABLED" : "DISABLED")}");
        }

        if (!showDebug) return;

        // Cache player if lost
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");

        // Update drone list periodically
        if (Time.time - _lastDroneUpdateTime > DRONE_UPDATE_INTERVAL)
        {
            _cachedDrones = Object.FindObjectsByType<DroneAnimator>(FindObjectsInactive.Exclude);
            _lastDroneUpdateTime = Time.time;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebug) return;

        // 1. Render NavMeshGraph
        DrawGraph();

        // 2. Render A* Open/Closed Sets
        DrawAStarSets();

        // 3. Render Active Drone Data (Paths & Detection)
        DrawDroneData();
    }

    private void DrawGraph()
    {
        if (NavMeshGraph.Instance == null) return;

        foreach (var node in NavMeshGraph.Instance.Nodes)
        {
            Gizmos.color = graphNodeColor;
            Gizmos.DrawSphere(node.Value.worldPosition, 0.2f);

            Gizmos.color = graphEdgeColor;
            foreach (var edge in node.Value.edges)
            {
                Gizmos.DrawLine(node.Value.worldPosition, NavMeshGraph.Instance.GetNodePosition(edge.toNodeId));
            }
        }
    }

    private void DrawAStarSets()
    {
        if (AStarPathfinder.Instance == null) return;

        // Draw Closed Set (Red - Explored)
        Gizmos.color = closedSetColor;
        foreach (int nodeId in AStarPathfinder.Instance.LastClosedSetNodes)
        {
            Vector3 pos = NavMeshGraph.Instance.GetNodePosition(nodeId);
            Gizmos.DrawCube(pos, Vector3.one * 0.4f);
        }

        // Draw Open Set (Yellow - Waiting)
        Gizmos.color = openSetColor;
        foreach (int nodeId in AStarPathfinder.Instance.LastOpenSetNodes)
        {
            Vector3 pos = NavMeshGraph.Instance.GetNodePosition(nodeId);
            Gizmos.DrawCube(pos, Vector3.one * 0.4f);
        }
    }

    private void DrawDroneData()
    {
        // Use cached drones
        if (_cachedDrones == null) return;

        foreach (var drone in _cachedDrones)
        {
            if (drone == null) continue;
            // Draw Detection Sphere
            Gizmos.color = detectionColor;
            Gizmos.DrawWireSphere(drone.transform.position, 12f);
        }
    }

    private void OnGUI()
    {
        if (!showDebug) return;

        // Overlay text info
        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(20, 20, 300, 200));
        GUILayout.Label("<b>DEBUG VISUALIZER [F1]</b>", GUILayout.Width(250));
        GUILayout.Label($"Graph Nodes: {(NavMeshGraph.Instance != null ? NavMeshGraph.Instance.Nodes.Count.ToString() : "N/A")}");
        GUILayout.Label($"A* Explored (Red): {(AStarPathfinder.Instance != null ? AStarPathfinder.Instance.LastClosedSetNodes.Count.ToString() : "0")}");
        GUILayout.Label($"A* Frontier (Yellow): {(AStarPathfinder.Instance != null ? AStarPathfinder.Instance.LastOpenSetNodes.Count.ToString() : "0")}");
        GUILayout.EndArea();

        DrawMinimap();
        DrawBottomLeftDots();
    }

    private void DrawMinimap()
    {
        float size = 200f;
        Rect mapRect = new Rect(Screen.width - size - 20, 20, size, size);
        
        GUI.Box(mapRect, "<b>NAV-MAP</b>");

        // Use a Group so (0,0) is the top-left of the map box
        GUI.BeginGroup(mapRect);

        if ((NavMeshGraph.Instance != null && NavMeshGraph.Instance.Nodes.Count > 0) || cityRoot != null)
        {
            // 1. Calculate the bounds of the city
            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            if (cityRoot != null && !_hasCityBounds)
            {
                // Calculate bounds ONCE
                float bMinX = float.MaxValue, bMaxX = float.MinValue;
                float bMinZ = float.MaxValue, bMaxZ = float.MinValue;

                Renderer[] renderers = cityRoot.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    if (r.bounds.min.x < bMinX) bMinX = r.bounds.min.x;
                    if (r.bounds.max.x > bMaxX) bMaxX = r.bounds.max.x;
                    if (r.bounds.min.z < bMinZ) bMinZ = r.bounds.min.z;
                    if (r.bounds.max.z > bMaxZ) bMaxZ = r.bounds.max.z;
                }
                
                minX = bMinX; maxX = bMaxX; minZ = bMinZ; maxZ = bMaxZ;
                _cityBoundsRect = new Rect(minX, minZ, maxX - minX, maxZ - minZ);
                _hasCityBounds = true;
            }
            else if (_hasCityBounds)
            {
                minX = _cityBoundsRect.x;
                maxX = _cityBoundsRect.x + _cityBoundsRect.width;
                minZ = _cityBoundsRect.y;
                maxZ = _cityBoundsRect.y + _cityBoundsRect.height;
            }
            else
            {
                // Fallback to NavMesh nodes
                foreach (var node in NavMeshGraph.Instance.Nodes.Values)
                {
                    if (node.worldPosition.x < minX) minX = node.worldPosition.x;
                    if (node.worldPosition.x > maxX) maxX = node.worldPosition.x;
                    if (node.worldPosition.z < minZ) minZ = node.worldPosition.z;
                    if (node.worldPosition.z > maxZ) maxZ = node.worldPosition.z;
                }
            }

            float width = maxX - minX;
            float height = maxZ - minZ;
            float worldRange = Mathf.Max(width, height) / 2f; 
            if (worldRange < 1f) worldRange = 100f; // fallback

            float centerX = (minX + maxX) / 2f;
            float centerZ = (minZ + maxZ) / 2f;
            
            // Apply zoom (Divide by zoom to see more area)
            float scale = size / (worldRange * 2 * navMapZoom);

            // 2. Draw City Layout (Building Footprints) - Note: This is still heavy if not optimized
            // In a real production build, you'd draw this once to a texture.
            /*
            if (cityRoot != null)
            {
                // ...
            }
            */

            // 3. Draw Graph Nodes (Background)
            GUI.color = new Color(0, 0.5f, 1, 0.2f); // Light blue for nodes
            foreach (var node in NavMeshGraph.Instance.Nodes.Values)
            {
                float px = size / 2 + (node.worldPosition.x - centerX) * scale;
                float py = size / 2 - (node.worldPosition.z - centerZ) * scale;
                
                if (px >= 0 && px <= size && py >= 0 && py <= size)
                    GUI.DrawTexture(new Rect(px - 1, py - 1, 2, 2), Texture2D.whiteTexture);
            }

            // 3. Draw A* Paths (Red and Yellow)
            if (AStarPathfinder.Instance != null)
            {
                // Explored Nodes (Red)
                GUI.color = Color.red;
                foreach (int nodeId in AStarPathfinder.Instance.LastClosedSetNodes)
                {
                    if (NavMeshGraph.Instance.Nodes.TryGetValue(nodeId, out GraphNode node))
                    {
                        float px = size / 2 + (node.worldPosition.x - centerX) * scale;
                        float py = size / 2 - (node.worldPosition.z - centerZ) * scale;
                        
                        if (px >= 0 && px <= size && py >= 0 && py <= size)
                            GUI.DrawTexture(new Rect(px - 1.5f, py - 1.5f, 3, 3), Texture2D.whiteTexture);
                    }
                }

                // Frontier Nodes (Yellow)
                GUI.color = Color.yellow;
                foreach (int nodeId in AStarPathfinder.Instance.LastOpenSetNodes)
                {
                    if (NavMeshGraph.Instance.Nodes.TryGetValue(nodeId, out GraphNode node))
                    {
                        float px = size / 2 + (node.worldPosition.x - centerX) * scale;
                        float py = size / 2 - (node.worldPosition.z - centerZ) * scale;
                        
                        if (px >= 0 && px <= size && py >= 0 && py <= size)
                            GUI.DrawTexture(new Rect(px - 1.5f, py - 1.5f, 3, 3), Texture2D.whiteTexture);
                    }
                }
            }

            // 4. Draw Drones (Using cache)
            if (_cachedDrones != null)
            {
                foreach (var drone in _cachedDrones)
                {
                    if (drone == null) continue;
                DroneStateMachine sm = drone.GetComponent<DroneStateMachine>();
                Color droneColor = Color.white;

                if (sm != null)
                {
                    switch (sm.CurrentState)
                    {
                        case DroneState.PATROL: droneColor = Color.cyan; break;
                        case DroneState.ALERT: droneColor = Color.red; break;
                        case DroneState.SEARCH: droneColor = Color.yellow; break;
                    }
                }

                GUI.color = droneColor;
                float px = size / 2 + (drone.transform.position.x - centerX) * scale;
                float py = size / 2 - (drone.transform.position.z - centerZ) * scale;
                
                    if (px >= 0 && px <= size && py >= 0 && py <= size)
                        GUI.DrawTexture(new Rect(px - 3, py - 3, 6, 6), Texture2D.whiteTexture);
                }
            }

            // 5. Draw Player (The Car)
            if (_player != null)
            {
                GUI.color = Color.blue;
                float px = size / 2 + (_player.transform.position.x - centerX) * scale;
                float py = size / 2 - (_player.transform.position.z - centerZ) * scale;
                if (px >= 0 && px <= size && py >= 0 && py <= size)
                    GUI.DrawTexture(new Rect(px - 4, py - 4, 8, 8), Texture2D.whiteTexture);
            }

            // 6. Draw Deposit Zone
            GameObject zone = GameObject.FindWithTag("DepositZone");
            if (zone != null)
            {
                GUI.color = Color.green;
                float px = size / 2 + (zone.transform.position.x - centerX) * scale;
                float py = size / 2 - (zone.transform.position.z - centerZ) * scale;
                if (px >= 0 && px <= size && py >= 0 && py <= size)
                    GUI.DrawTexture(new Rect(px - 5, py - 5, 10, 10), Texture2D.whiteTexture);
            }
        }
        
        GUI.EndGroup();
        GUI.color = Color.white;
    }

    private void DrawBottomLeftDots()
    {
        if (_player == null) return;

        // Find exact UI Rect of the Minimap Image if it exists
        Rect mapRect = new Rect(0, Screen.height - 250, 250, 250); // Default fallback
        UnityEngine.UI.RawImage[] rawImages = Object.FindObjectsByType<UnityEngine.UI.RawImage>(FindObjectsInactive.Exclude);
        foreach (var img in rawImages)
        {
            if (img.name.Contains("Minimap", System.StringComparison.OrdinalIgnoreCase) || 
                img.name.Contains("Map", System.StringComparison.OrdinalIgnoreCase))
            {
                Vector3[] corners = new Vector3[4];
                img.rectTransform.GetWorldCorners(corners);
                float minX = corners[0].x;
                float minY = corners[0].y;
                float maxX = corners[2].x;
                float maxY = corners[2].y;
                
                // Convert screen coords to GUI coords (Y is inverted)
                mapRect = new Rect(minX, Screen.height - maxY, maxX - minX, maxY - minY);
                break;
            }
        }

        // We assume the bottom-left map is a zoomed-in view of the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Try to get dynamic zoom from the Minimap camera
        float zoomRange = 50f; 
        Camera[] cams = Camera.allCameras;
        foreach (var cam in cams)
        {
            if (cam.name.Contains("Minimap", System.StringComparison.OrdinalIgnoreCase))
            {
                zoomRange = cam.orthographicSize;
                break;
            }
        }

        float scaleX = mapRect.width / (zoomRange * 2);
        float scaleY = mapRect.height / (zoomRange * 2);

        GUI.BeginGroup(mapRect);

        // 1. Draw Vehicle (Blue Dot) exactly at the center of the minimap rect
        GUI.color = Color.blue;
        GUI.DrawTexture(new Rect(mapRect.width / 2 - 4, mapRect.height / 2 - 4, 8, 8), Texture2D.whiteTexture);

        // 2. Draw Drones relative to player
        if (_cachedDrones != null)
        {
            foreach (var drone in _cachedDrones)
            {
                if (drone == null) continue;
                Vector3 relativePos = drone.transform.position - _player.transform.position;
                
                float px = mapRect.width / 2 + relativePos.x * scaleX;
                float py = mapRect.height / 2 - relativePos.z * scaleY;

                // Only draw if it's within the minimap bounds
                if (px >= 0 && px <= mapRect.width && py >= 0 && py <= mapRect.height)
                {
                    GUI.color = Color.red;
                    GUI.DrawTexture(new Rect(px - 3, py - 3, 6, 6), Texture2D.whiteTexture);
                }
            }
        }

        GUI.EndGroup();
        GUI.color = Color.white;
    }
}
