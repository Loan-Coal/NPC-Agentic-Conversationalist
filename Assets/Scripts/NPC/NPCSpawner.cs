using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Spawns NPCs programmatically around the origin point.
/// Assigns unique NPCData to each spawned NPC and configures behavior.
/// </summary>
public class NPCSpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [Tooltip("Prefab to spawn for each NPC")]
    public GameObject npcPrefab;

    [Tooltip("Array of 8 NPCData assets to assign to spawned NPCs")]
    public NPCData[] npcDataAssets = new NPCData[8];

    [Tooltip("Number of NPCs to spawn")]
    public int spawnCount = 8;

    [Tooltip("Radius around origin to spawn NPCs")]
    public float spawnRadius = 12f;

    [Tooltip("Minimum distance between spawned NPCs")]
    public float minSpacing = 2f;

    [Header("References")]
    [Tooltip("Reference to DialogueManager in scene")]
    public DialogueManager dialogueManager;

    [Tooltip("Reference to InteractionPrompt UI")]
    public InteractionPrompt interactionPrompt;

    [Header("Behavior Indices (0-based)")]
    [Tooltip("Indices of NPCs that should patrol (e.g., 1, 6 for Brenn and Sera)")]
    public int[] patrolIndices = new int[] { 1, 6 };

    [Tooltip("Indices of NPCs that should wander (e.g., 5, 7 for Toll and Aldon)")]
    public int[] wanderIndices = new int[] { 5, 7 };

    [Header("Debug")]
    public List<GameObject> spawnedNPCs = new List<GameObject>();

    private void Start()
    {
    SpawnNPCs();
    }

    /// <summary>
    /// Spawns all NPCs at valid positions around the origin.
    /// </summary>
    public void SpawnNPCs()
    {
     if (npcPrefab == null)
        {
   Debug.LogError("NPCSpawner: NPC Prefab is not assigned!");
  return;
        }

        if (npcDataAssets.Length < spawnCount)
        {
   Debug.LogError($"NPCSpawner: Not enough NPCData assets! Need {spawnCount}, have {npcDataAssets.Length}");
  return;
   }

        spawnedNPCs.Clear();
      List<Vector3> spawnedPositions = new List<Vector3>();

        for (int i = 0; i < spawnCount; i++)
        {
        NPCData npcData = npcDataAssets[i];
     if (npcData == null)
       {
 Debug.LogWarning($"NPCSpawner: NPCData at index {i} is null, skipping.");
       continue;
            }

   // Find valid spawn position
       Vector3 spawnPos = FindValidSpawnPosition(spawnedPositions);
     if (spawnPos == Vector3.zero)
            {
     Debug.LogWarning($"NPCSpawner: Could not find valid spawn position for {npcData.npcName}");
         continue;
    }

  // Instantiate NPC
    GameObject npcInstance = Instantiate(npcPrefab, spawnPos, Quaternion.identity, transform);
      npcInstance.name = $"NPC_{npcData.npcName}";

    // Assign NPCData to NPCInteraction
      NPCInteraction interaction = npcInstance.GetComponent<NPCInteraction>();
            if (interaction != null)
            {
   interaction.npcData = npcData;
      interaction.dialogueManager = dialogueManager;
          interaction.interactionPrompt = interactionPrompt;
    }
            else
      {
        Debug.LogError($"NPCSpawner: {npcInstance.name} missing NPCInteraction component!");
       }

            // Configure behavior based on index
    ConfigureBehavior(npcInstance, i);

            spawnedNPCs.Add(npcInstance);
     spawnedPositions.Add(spawnPos);

          Debug.Log($"[NPCSpawner] Spawned {npcData.npcName} at {spawnPos}");
        }

        Debug.Log($"[NPCSpawner] Spawned {spawnedNPCs.Count} NPCs successfully");
    }

    /// <summary>
    /// Finds a valid spawn position with minimum spacing.
  /// </summary>
    private Vector3 FindValidSpawnPosition(List<Vector3> existingPositions)
    {
        int maxAttempts = 30;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
   // Random position in circle
      Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
    Vector3 candidatePos = new Vector3(randomCircle.x, 50f, randomCircle.y);

   // Raycast down to find ground
            if (Physics.Raycast(candidatePos, Vector3.down, out RaycastHit hit, 100f))
     {
 Vector3 groundPos = hit.point;

    // Check spacing from existing NPCs
      bool validSpacing = true;
     foreach (Vector3 existing in existingPositions)
        {
  if (Vector3.Distance(groundPos, existing) < minSpacing)
             {
       validSpacing = false;
  break;
            }
           }

        if (validSpacing)
        {
   return groundPos;
           }
    }
        }

        // Fallback: return position without ground check
        Vector2 fallbackCircle = Random.insideUnitCircle * spawnRadius;
  return new Vector3(fallbackCircle.x, 0f, fallbackCircle.y);
    }

    /// <summary>
    /// Configures NPC behavior (stationary, patrol, wander).
    /// </summary>
    private void ConfigureBehavior(GameObject npc, int index)
    {
      // Check if patrol
        if (System.Array.IndexOf(patrolIndices, index) >= 0)
        {
            NPCPatrol patrol = npc.AddComponent<NPCPatrol>();
      patrol.CreateDefaultWaypoints(npc.transform.position);
  Debug.Log($"[NPCSpawner] {npc.name} configured for Patrol");
        }
        // Check if wander
    else if (System.Array.IndexOf(wanderIndices, index) >= 0)
        {
NPCWander wander = npc.AddComponent<NPCWander>();
          Debug.Log($"[NPCSpawner] {npc.name} configured for Wander");
    }
        // Otherwise stationary (default, no extra script needed)
        else
        {
       Debug.Log($"[NPCSpawner] {npc.name} configured for Stationary");
 }
    }

    /// <summary>
    /// Clears all spawned NPCs (for testing).
    /// </summary>
    public void ClearSpawnedNPCs()
    {
        foreach (GameObject npc in spawnedNPCs)
        {
            if (npc != null)
            {
   DestroyImmediate(npc);
   }
        }
        spawnedNPCs.Clear();
Debug.Log("[NPCSpawner] Cleared all spawned NPCs");
    }
}
