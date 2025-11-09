using System;
using UnityEngine;

/// <summary>
/// Represents a single event in an NPC's history.
/// Used as a ScriptableObject for individual event assets.
/// </summary>
[CreateAssetMenu(menuName = "NPC/Event", fileName = "NewEvent")]
public class EventData : ScriptableObject
{
    [Tooltip("Type of event (e.g., 'combat', 'dialogue', 'discovery')")]
    public string eventType;
    
    [Tooltip("When this event occurred")]
    public string timestamp;
    
    [TextArea(3, 10)]
    [Tooltip("Description of what happened")]
    public string description;

    /// <summary>
    /// Converts this event to a serializable DTO for API calls.
    /// </summary>
    public EventDTO ToDTO()
    {
      return new EventDTO
        {
            type = eventType,
     timestamp = timestamp,
       description = description
        };
  }
}

/// <summary>
/// Data Transfer Object for event serialization.
/// JsonUtility requires simple serializable classes.
/// </summary>
[Serializable]
public class EventDTO
{
    public string type;
    public string timestamp;
    public string description;
}
