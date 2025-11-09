using System;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Visual")]
    [Tooltip("Portrait sprite for dialogue UI (256x256 recommended)")]
    public Sprite portrait;

    [Header("Personality")]
    [Tooltip("List of personality traits (e.g., 'brave', 'cautious', 'friendly')")]
    public string[] personalityTraits = new string[0];

    [TextArea(3, 10)]
    [Tooltip("Backstory of the NPC")]
    public string backstory;

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
    /// Portrait is NOT included in DTO (local UI only).
    /// </summary>
    public NPCDataDTO ToDTO(int maxEvents = 2)
    {
        var dto = new NPCDataDTO
        {
            name = npcName,
            personality_traits = personalityTraits,
            backstory = backstory,
            events = new List<EventDTO>()
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
    public string[] personality_traits;
    public string backstory;
    public List<EventDTO> events;
}
