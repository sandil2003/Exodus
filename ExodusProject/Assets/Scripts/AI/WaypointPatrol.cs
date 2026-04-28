using UnityEngine;
using UnityEngine.AI;

public class WaypointPatrol : MonoBehaviour
{
    public Transform[] waypoints;
    private int _currentWaypoint = 0;

    public void DoPatrol(NavMeshAgent agent)
    {
        if (waypoints.Length == 0) return;
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            _currentWaypoint = (_currentWaypoint + 1) % waypoints.Length;
            agent.SetDestination(waypoints[_currentWaypoint].position);
        }
    }
}