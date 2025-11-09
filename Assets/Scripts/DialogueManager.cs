using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Manages communication with the Python dialogue API.
/// Sends NPC data and player input, receives AI-generated responses.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("API Configuration")]
    [Tooltip("URL of the Python FastAPI server")]
    public string apiUrl = "http://localhost:8000/npc_conversation";
    
    [Tooltip("Request timeout in seconds")]
    public float requestTimeout = 10f;

    [Header("UI Reference")]
    [Tooltip("Reference to the dialogue UI manager")]
 public DialogueUIManager uiManager;

    private Coroutine activeRequest;
    private Animator currentNPCAnimator;

    /// <summary>
    /// Starts a conversation with an NPC using a ScriptableObject reference.
    /// </summary>
    /// <param name="npcData">The NPC to talk to</param>
    /// <param name="playerInput">What the player said</param>
    /// <param name="maxEvents">Maximum number of events to send (default: 2)</param>
    public void StartConversation(NPCData npcData, string playerInput, int maxEvents = 2)
    {
        StartConversation(npcData, playerInput, maxEvents, null);
    }

    /// <summary>
    /// Starts a conversation with an NPC, optionally passing the NPC's Animator.
    /// </summary>
    public void StartConversation(NPCData npcData, string playerInput, int maxEvents, Animator npcAnimator)
    {
     if (npcData == null)
        {
            Debug.LogError("DialogueManager: NPCData is null!");
   return;
        }

 if (uiManager == null)
    {
     Debug.LogError("DialogueManager: DialogueUIManager reference is missing!");
            return;
        }

        // Stop any active request
   if (activeRequest != null)
        {
 StopCoroutine(activeRequest);
        }

  // Store animator reference for talk animation
      currentNPCAnimator = npcAnimator;

  // Start new conversation request
        activeRequest = StartCoroutine(SendConversationRequest(npcData, playerInput, maxEvents));
    }

    /// <summary>
    /// Coroutine that sends the conversation request to the API.
    /// </summary>
    private IEnumerator SendConversationRequest(NPCData npcData, string playerInput, int maxEvents)
    {
        // Build the request payload
        var payload = new ConversationPayload
        {
      npc_data = npcData.ToDTO(maxEvents),
     player_input = playerInput
        };

 string jsonPayload = JsonUtility.ToJson(payload, true);
        Debug.Log($"[DialogueManager] Sending request:\n{jsonPayload}");

        // Create UnityWebRequest
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
  request.timeout = (int)requestTimeout;

        // Send request
        yield return request.SendWebRequest();

        // Handle response
   if (request.result == UnityWebRequest.Result.Success)
        {
       string responseText = request.downloadHandler.text;
            Debug.Log($"[DialogueManager] Received response:\n{responseText}");

   try
        {
      var response = JsonUtility.FromJson<ConversationResponse>(responseText);

          if (!string.IsNullOrEmpty(response.reply))
        {
     // Pass portrait and animator to UI
            uiManager.ShowReply(response.reply, npcData.portrait, currentNPCAnimator);
  }
     else
                {
         Debug.LogWarning("DialogueManager: Response contained empty reply.");
  uiManager.ShowReply("...", npcData.portrait, currentNPCAnimator);
        }
  }
            catch (Exception ex)
          {
            Debug.LogError($"DialogueManager: Failed to parse response JSON: {ex.Message}");
    uiManager.ShowReply("Error: Could not understand server response.", npcData.portrait, currentNPCAnimator);
       }
  }
     else
        {
  Debug.LogError($"[DialogueManager] Request failed: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response Body: {request.downloadHandler?.text}");

      uiManager.ShowReply($"Error: {request.error}", npcData.portrait, currentNPCAnimator);
 }

        activeRequest = null;
        request.Dispose();
    }

    /// <summary>
 /// Called when dialogue is closed to clean up NPC animator state.
 /// </summary>
    public void OnDialogueClosed()
    {
      currentNPCAnimator = null;
    }
}

/// <summary>
/// Request payload structure for the conversation API.
/// </summary>
[Serializable]
public class ConversationPayload
{
    public NPCDataDTO npc_data;
    public string player_input;
}

/// <summary>
/// Response structure from the conversation API.
/// </summary>
[Serializable]
public class ConversationResponse
{
    public string reply;
}
