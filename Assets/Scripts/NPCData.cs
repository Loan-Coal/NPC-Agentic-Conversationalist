using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a relationship between this NPC and another NPC.
/// </summary>
[Serializable]
public class NPCRelationship
{
    [Tooltip("The other NPC in this relationship")]
    public NPCData otherNPC;
    
    [Tooltip("Affinity level: -1 (hostile) to 1 (friendly)")]
  [Range(-1f, 1f)]
    public float affinity = 0f;
 
    [Tooltip("Relationship tag (e.g., 'rival', 'friend', 'family', 'customer')")]
    public string relationshipTag = "acquaintance";
    
    [TextArea(2, 5)]
    [Tooltip("Notes about this relationship")]
 public string notes = "";
}

/// <summary>
/// ScriptableObject representing an NPC with personality, backstory, and event history.
/// Create via: Assets > Create > NPC > New NPC
/// </summary>
[CreateAssetMenu(menuName = "NPC/New NPC", fileName = "NewNPC")]
public class NPCData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Name of the NPC")]
    public string npcName;
 
    [Tooltip("Role/occupation in the village")]
    public string role;

    [Header("Visual")]
    [Tooltip("Portrait sprite for dialogue UI (256x256 recommended)")]
    public Sprite portrait;

    [Header("Personality")]
  [Tooltip("Core personality traits (e.g., 'brave', 'cautious', 'friendly')")]
    public string[] personalityTraits = new string[0];
    
    [Tooltip("Unique quirks and mannerisms")]
    public string[] quirks = new string[0];

    [TextArea(3, 10)]
    [Tooltip("Backstory of the NPC")]
    public string backstory;

    [Header("Relationships")]
    [Tooltip("Relationships with other NPCs")]
    public List<NPCRelationship> relationships = new List<NPCRelationship>();

    [Header("Event History")]
    [Tooltip("List of events this NPC has experienced")]
    public List<EventData> events = new List<EventData>();

    /// <summary>
    /// Gets the first N events from this NPC's history.
    /// Safe if fewer than N events exist.
    /// </summary>
    public List<EventData> GetFirstEvents(int count)
    {
        int safeCount = Mathf.Min(count, events.Count);
   return events.GetRange(0, safeCount);
    }

    /// <summary>
    /// Converts this NPC data to a DTO suitable for API serialization.
    /// Portrait and relationships are NOT included in DTO (local only).
    /// </summary>
    public NPCDataDTO ToDTO(int maxEvents = 2)
 {
        var dto = new NPCDataDTO
        {
       name = npcName,
            role = role,
    personality_traits = personalityTraits,
        quirks = quirks,
       backstory = backstory,
   events = new List<EventDTO>(),
            relationships = new List<RelationshipDTO>()
 };

   // Get first N events
        var selectedEvents = GetFirstEvents(maxEvents);
        foreach (var evt in selectedEvents)
        {
            if (evt != null)
            {
     dto.events.Add(evt.ToDTO());
            }
  }
     
        // Add relationship summaries (names and tags only, not full NPCData)
  foreach (var rel in relationships)
        {
            if (rel != null && rel.otherNPC != null)
            {
                dto.relationships.Add(new RelationshipDTO
          {
     npc_name = rel.otherNPC.npcName,
     affinity = rel.affinity,
  tag = rel.relationshipTag
    });
      }
        }

 return dto;
    }
}

/// <summary>
/// Data Transfer Object for NPC serialization to API.
/// JsonUtility requires simple serializable classes.
/// </summary>
[Serializable]
public class NPCDataDTO
{
    public string name;
    public string role;
    public string[] personality_traits;
    public string[] quirks;
    public string backstory;
  public List<EventDTO> events;
    public List<RelationshipDTO> relationships;
}

/// <summary>
/// Simplified relationship data for API transfer.
/// </summary>
[Serializable]
public class RelationshipDTO
{
    public string npc_name;
    public float affinity;
  public string tag;
}
