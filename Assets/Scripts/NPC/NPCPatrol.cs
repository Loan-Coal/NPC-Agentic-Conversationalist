using UnityEngine;
using UnityEngine.AI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Simple patrol behavior using NavMeshAgent.
/// Moves between waypoints and toggles Animator isWalking parameter.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPCPatrol : MonoBehaviour
{
    [Header("Patrol Configuration")]
    [Tooltip("Waypoints to patrol between")]
    public Transform[] waypoints;

    [Tooltip("Wait time at each waypoint")]
    public float waypointWaitTime = 2f;

    [Tooltip("How close to waypoint before considering reached")]
    public float waypointThreshold = 0.5f;

  private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

  private void Start()
    {
 agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError($"NPCPatrol on {gameObject.name}: NavMeshAgent component missing!");
    enabled = false;
   return;
        }

        // Disable root motion for NavMesh-driven movement
      if (animator != null)
        {
  animator.applyRootMotion = false;
        }

      // Start patrolling if waypoints exist
        if (waypoints != null && waypoints.Length > 0)
        {
   GoToNextWaypoint();
    }
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

  // Handle waiting at waypoint
        if (isWaiting)
        {
waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
         {
       isWaiting = false;
           GoToNextWaypoint();
 }
            return;
   }

     // Check if reached current waypoint
        if (!agent.pathPending && agent.remainingDistance <= waypointThreshold)
  {
   // Arrived at waypoint
   isWaiting = true;
        waitTimer = waypointWaitTime;
            UpdateAnimator(false);
        }
        else
        {
 // Moving toward waypoint
       UpdateAnimator(true);
        }
    }

    /// <summary>
    /// Sets destination to next waypoint in sequence.
 /// </summary>
    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        UpdateAnimator(true);
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

    /// <summary>
    /// Creates default waypoints around spawn position (called by spawner).
    /// </summary>
    public void CreateDefaultWaypoints(Vector3 center)
    {
        // Create 3 waypoints in a triangle pattern
      GameObject waypointParent = new GameObject($"{gameObject.name}_Waypoints");
 waypointParent.transform.position = center;
        waypointParent.transform.SetParent(transform);

        waypoints = new Transform[3];
        float radius = 5f;
        for (int i = 0; i < 3; i++)
        {
  float angle = i * 120f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
       
     GameObject wp = new GameObject($"Waypoint_{i}");
 wp.transform.position = center + offset;
            wp.transform.SetParent(waypointParent.transform);
            waypoints[i] = wp.transform;
        }

        Debug.Log($"[NPCPatrol] Created {waypoints.Length} default waypoints for {gameObject.name}");
    }
}
