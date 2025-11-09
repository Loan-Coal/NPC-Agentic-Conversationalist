using UnityEngine;
using TMPro;

/// <summary>
/// Controls a simple interaction prompt UI element.
/// Shows/hides with optional fade or instant toggle.
/// </summary>
public class InteractionPrompt : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The prompt GameObject (can be the same as this object)")]
    public GameObject promptObject;

    [Tooltip("Optional TextMeshPro component for prompt text")]
    public TextMeshProUGUI promptText;

  [Header("Settings")]
    [Tooltip("Default prompt message")]
    public string defaultMessage = "Press E to talk";

    private void Start()
    {
        // Use this GameObject if no specific prompt object is assigned
      if (promptObject == null)
 {
            promptObject = gameObject;
   }

        // Hide by default
        Hide();
    }

    /// <summary>
    /// Shows the prompt with optional custom text.
    /// </summary>
 /// <param name="message">Custom message to display (uses default if null)</param>
    public void Show(string message = null)
    {
        if (promptObject != null)
        {
            promptObject.SetActive(true);
        }

        if (promptText != null)
  {
   promptText.text = message ?? defaultMessage;
        }
    }

    /// <summary>
    /// Hides the prompt.
    /// </summary>
    public void Hide()
    {
  if (promptObject != null)
        {
        promptObject.SetActive(false);
     }
 }
}
