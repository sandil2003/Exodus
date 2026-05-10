using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWanderer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 15f;
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;
    public float walkSpeed = 2f;

    [Header("Animation")]
    public Animator animator;
    public string speedFloatParam = "Speed"; // Kevin Iglesias packs usually use 'Speed'
    public string walkBoolParam = "isWalking";

    [Header("Behavior Variation")]
    public float runChance = 0.5f;
    public float runSpeed = 6.0f;
    public float actionChance = 0.3f;
    public string actionTriggerParam = "doAction";

    private NavMeshAgent _agent;
    private bool _isWaiting = false;
    private Transform _targetCar = null;

    public void RunToCar(Transform car)
    {
        HumanNPC human = GetComponent<HumanNPC>();
        if (human != null && human.isRescued) return; // Don't run to car if already rescued
        
        if (_targetCar != null) return; // Already running to a car
        
        _targetCar = car;
        StopAllCoroutines();
        StartCoroutine(RunToCarRoutine());
    }

    private IEnumerator RunToCarRoutine()
    {
        _agent.speed = runSpeed;
        while (_targetCar != null)
        {
            _agent.SetDestination(_targetCar.position);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = walkSpeed;
    }

    void Start()
    {
        // Wander routine is now started in OnEnable to handle being disabled/enabled (e.g. during pickup)
    }

    void OnEnable()
    {
        StopAllCoroutines();
        _isWaiting = false;
        StartCoroutine(WanderRoutine());
    }

    public void SetRescuedMode()
    {
        _targetCar = null;
        wanderRadius = 8f; // Stay close to the rescue point
        walkSpeed = 1.5f;   // Walk a bit slower/calmer
        _agent.speed = walkSpeed;
    }

    void Update()
    {
        // Update animator
        if (animator != null)
        {
            float currentSpeed = _agent.velocity.magnitude;
            
            // Set float for blend trees (0=Idle, 1=Walk, 2=Run roughly)
            if (!string.IsNullOrEmpty(speedFloatParam))
            {
                animator.SetFloat(speedFloatParam, currentSpeed);
            }
            
            // Set bool for simple transitions
            if (!string.IsNullOrEmpty(walkBoolParam) && HasParameter(animator, walkBoolParam))
            {
                bool isMoving = currentSpeed > 0.1f && !_agent.isStopped;
                animator.SetBool(walkBoolParam, isMoving);
            }
        }
    }

    private bool HasParameter(Animator anim, string paramName)
    {
        if (string.IsNullOrEmpty(paramName)) return false;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!_isWaiting && (!_agent.pathPending && _agent.remainingDistance < 0.5f))
            {
                yield return StartCoroutine(PerformRandomBehavior());
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator PerformRandomBehavior()
    {
        _isWaiting = true;
        
        // Randomly do an "Action" (like waving, looking around)
        if (Random.value < actionChance && animator != null)
        {
            animator.SetTrigger(actionTriggerParam);
            yield return new WaitForSeconds(2f); // Wait for animation
        }
        else
        {
            // Just look around
            float lookTime = Random.Range(1f, 3f);
            Quaternion targetRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            float elapsed = 0;
            while (elapsed < lookTime)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Random "Stuff": Wait for a random time
        float waitTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(waitTime);

        // Decide if walking or running
        bool isRunning = Random.value < runChance;
        _agent.speed = isRunning ? runSpeed : walkSpeed;

        // Find a new random point on the NavMesh
        Vector3 newTarget = GetRandomNavMeshPoint(transform.position, wanderRadius);
        _agent.SetDestination(newTarget);

        _isWaiting = false;
    }

    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;
        
        NavMeshHit hit;
        // Search on NavMesh area 1 (usually Walkable)
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return center;
    }
}
