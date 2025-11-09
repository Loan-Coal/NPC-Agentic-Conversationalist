using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Wander behavior using NavMeshAgent.
/// Picks random destinations periodically and toggles Animator isWalking parameter.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("Wander Configuration")]
    [Tooltip("Minimum time before picking new destination")]
    public float minWanderInterval = 5f;

    [Tooltip("Maximum time before picking new destination")]
    public float maxWanderInterval = 10f;

    [Tooltip("Maximum distance from spawn point to wander")]
    public float wanderRadius = 8f;

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 spawnPosition;
    private float wanderTimer;
    private float currentWanderInterval;

    private void Start()
    {
 agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    spawnPosition = transform.position;

        if (agent == null)
        {
   Debug.LogError($"NPCWander on {gameObject.name}: NavMeshAgent component missing!");
            enabled = false;
   return;
   }

     // Disable root motion for NavMesh-driven movement
        if (animator != null)
    {
  animator.applyRootMotion = false;
   }

        // Start wandering
  PickNewDestination();
    }

    private void Update()
    {
   // Update wander timer
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
   {
PickNewDestination();
        }

   // Update animator based on movement
        bool isMoving = agent.velocity.magnitude > 0.1f;
  UpdateAnimator(isMoving);
    }

    /// <summary>
    /// Picks a random NavMesh destination within wander radius.
    /// </summary>
    private void PickNewDestination()
    {
        // Random point in circle around spawn position
      Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
  Vector3 targetPosition = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

   // Sample NavMesh to find valid position
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
   {
    agent.SetDestination(hit.position);
  Debug.Log($"[NPCWander] {gameObject.name} wandering to {hit.position}");
  }
      else
        {
       // Fallback: stay at current position
       Debug.LogWarning($"[NPCWander] {gameObject.name} could not find valid NavMesh position");
  }

        // Set next wander time
     currentWanderInterval = Random.Range(minWanderInterval, maxWanderInterval);
   wanderTimer = currentWanderInterval;
    }

  /// <summary>
    /// Updates Animator isWalking parameter.
    /// </summary>
    private void UpdateAnimator(bool walking)
    {
  if (animator != null)
     {
  animator.SetBool("isWalking", walking);
        }
    }
}
