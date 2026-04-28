/// <summary>
/// Global Enum for Drone AI States
/// Used by IS Student 2 (StateMachine) and GV Student 4 (Animator)
/// </summary>
public enum DroneState
{
    PATROL, // Default movement along waypoints
    ALERT,  // Active A* chasing of the player
    SEARCH  // BFS navigation to last known position
}