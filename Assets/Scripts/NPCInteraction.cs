using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Attach to NPC GameObjects to handle player interaction.
/// Detects player proximity and triggers dialogue on key press.
/// Supports both legacy Input Manager and new Input System.
/// </summary>
[RequireComponent(typeof(Collider))]
public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Data")]
    [Tooltip("The ScriptableObject containing this NPC's data")]
    public NPCData npcData;

    [Header("System References")]
    [Tooltip("Reference to the DialogueManager in the scene")]
    public DialogueManager dialogueManager;

    [Tooltip("Reference to the interaction prompt UI")]
    public InteractionPrompt interactionPrompt;

    [Header("Interaction Settings")]
    [Tooltip("Key to press for interaction")]
 public KeyCode interactionKey = KeyCode.E;
    
    [Tooltip("Default greeting when player initiates conversation")]
    public string defaultPlayerInput = "Hello!";

    [Tooltip("Maximum number of events to send to API")]
    public int maxEventsToSend = 2;

    private bool playerInRange = false;
    private Animator npcAnimator;

    private void Start()
    {
        // Validation
 if (npcData == null)
    {
            Debug.LogError($"NPCInteraction on {gameObject.name}: NPCData is not assigned!");
        }

  if (dialogueManager == null)
        {
            Debug.LogError($"NPCInteraction on {gameObject.name}: DialogueManager reference is missing!");
}

   // Get animator component
     npcAnimator = GetComponent<Animator>();

  // Ensure collider is set to trigger
  Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
   {
       Debug.LogWarning($"NPCInteraction on {gameObject.name}: Collider should be a trigger. Setting isTrigger = true.");
    col.isTrigger = true;
  }
    }

  private void Update()
    {
        // Check for interaction input when player is in range
   if (playerInRange && WasInteractionKeyPressed())
   {
      StartConversation();
}
    }

/// <summary>
    /// Checks if the interaction key was pressed this frame.
    /// Supports both legacy Input and new Input System.
    /// </summary>
    private bool WasInteractionKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System
   if (Keyboard.current == null) return false;

        // Map common KeyCodes to new Input System keys
   switch (interactionKey)
        {
   case KeyCode.E:
   return Keyboard.current.eKey.wasPressedThisFrame;
  case KeyCode.F:
   return Keyboard.current.fKey.wasPressedThisFrame;
 case KeyCode.Space:
 return Keyboard.current.spaceKey.wasPressedThisFrame;
    case KeyCode.Return:
         case KeyCode.KeypadEnter:
   return Keyboard.current.enterKey.wasPressedThisFrame;
            case KeyCode.Escape:
        return Keyboard.current.escapeKey.wasPressedThisFrame;
  case KeyCode.Q:
    return Keyboard.current.qKey.wasPressedThisFrame;
  case KeyCode.R:
         return Keyboard.current.rKey.wasPressedThisFrame;
            case KeyCode.T:
      return Keyboard.current.tKey.wasPressedThisFrame;
  default:
         Debug.LogWarning($"NPCInteraction: KeyCode {interactionKey} not mapped for new Input System. Add mapping in WasInteractionKeyPressed()");
  return false;
      }
#else
        // Legacy Input Manager
        return Input.GetKeyDown(interactionKey);
#endif
    }

    /// <summary>
    /// Initiates conversation with this NPC.
    /// </summary>
    private void StartConversation()
    {
     if (npcData == null || dialogueManager == null)
   {
         Debug.LogError($"NPCInteraction on {gameObject.name}: Cannot start conversation - missing references!");
     return;
   }

        Debug.Log($"[NPCInteraction] Starting conversation with {npcData.npcName}");

   // Hide the prompt
   if (interactionPrompt != null)
  {
    interactionPrompt.Hide();
  }

      // Start the conversation, passing animator reference
     dialogueManager.StartConversation(npcData, defaultPlayerInput, maxEventsToSend, npcAnimator);
    }

    private void OnTriggerEnter(Collider other)
    {
    // Check if the player entered the trigger
if (other.CompareTag("Player"))
   {
      playerInRange = true;

 if (interactionPrompt != null)
       {
 interactionPrompt.Show();
        }

   Debug.Log($"[NPCInteraction] Player entered range of {npcData?.npcName ?? gameObject.name}");
  }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player left the trigger
  if (other.CompareTag("Player"))
  {
   playerInRange = false;

            if (interactionPrompt != null)
 {
      interactionPrompt.Hide();
   }

 Debug.Log($"[NPCInteraction] Player left range of {npcData?.npcName ?? gameObject.name}");
        }
    }

    // Visualize interaction range in editor
    private void OnDrawGizmosSelected()
    {
     Collider col = GetComponent<Collider>();
        if (col != null)
        {
    Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, col.bounds.extents.magnitude);
  }
    }
}
