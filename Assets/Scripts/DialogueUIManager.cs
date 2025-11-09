using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Manages the dialogue UI panel, showing and hiding NPC responses.
/// Controls cursor lock state during dialogue and displays NPC portrait.
/// </summary>
public class DialogueUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The dialogue panel GameObject")]
  public GameObject dialoguePanel;
 
    [Tooltip("TextMeshPro text component for displaying NPC replies")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("Image component for displaying NPC portrait (256x256 recommended)")]
    public Image portraitImage;

    [Header("Settings")]
    [Tooltip("Whether to unlock cursor when dialogue is shown")]
    public bool unlockCursorOnShow = true;

    private Animator currentNPCAnimator;

    private void Start()
    {
        // Ensure dialogue panel is hidden at start
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
     }
    }

    /// <summary>
    /// Shows the dialogue panel with the given reply text and portrait.
    /// </summary>
    /// <param name="reply">The NPC's response to display</param>
 /// <param name="portrait">The NPC's portrait sprite</param>
    /// <param name="npcAnimator">Optional animator to set isTalking parameter</param>
    public void ShowReply(string reply, Sprite portrait = null, Animator npcAnimator = null)
{
        if (dialoguePanel == null || dialogueText == null)
      {
 Debug.LogError("DialogueUIManager: Missing UI references!");
        return;
        }

        // Set text and portrait
   dialogueText.text = reply;

        if (portraitImage != null && portrait != null)
     {
      portraitImage.sprite = portrait;
     portraitImage.enabled = true;
        }
   else if (portraitImage != null)
        {
  portraitImage.enabled = false;
     }

        // Show panel
  dialoguePanel.SetActive(true);

   // Unlock cursor if enabled
   if (unlockCursorOnShow)
        {
     Cursor.lockState = CursorLockMode.None;
 Cursor.visible = true;
   }

        // Set NPC talking animation
        currentNPCAnimator = npcAnimator;
    if (currentNPCAnimator != null)
  {
       currentNPCAnimator.SetBool("isTalking", true);
        }

  Debug.Log($"[DialogueUIManager] Showing reply: {reply}");
    }

    /// <summary>
    /// Closes the dialogue panel and re-locks the cursor.
    /// </summary>
    public void Close()
    {
        if (dialoguePanel != null)
    {
dialoguePanel.SetActive(false);
     }

   // Clear talking animation
     if (currentNPCAnimator != null)
  {
       currentNPCAnimator.SetBool("isTalking", false);
       currentNPCAnimator = null;
   }

        // Lock cursor for gameplay
     Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[DialogueUIManager] Dialogue closed.");

    // Notify DialogueManager
   DialogueManager dm = FindObjectOfType<DialogueManager>();
      if (dm != null)
        {
   dm.OnDialogueClosed();
  }
    }

    private void Update()
    {
     // Allow closing dialogue with Escape key
   if (dialoguePanel != null && dialoguePanel.activeSelf && WasEscapePressed())
  {
            Close();
        }
    }

    private bool WasEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
  return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
  return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
