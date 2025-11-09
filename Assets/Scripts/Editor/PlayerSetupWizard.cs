using UnityEngine;
using UnityEditor;
using System.IO;

public class PlayerSetupWizard : EditorWindow
{
    [MenuItem("Tools/NPC System/Setup Player (Storyteller)")]
    public static void ShowWindow()
    {
        GetWindow<PlayerSetupWizard>("Player Setup");
    }

    private void OnGUI()
    {
    GUILayout.Label("Player Character Setup", EditorStyles.boldLabel);
   
        EditorGUILayout.HelpBox(
 "This will create a Player character using the Storyteller model with:\n" +
  "• CharacterController\n" +
            "• PlayerController script\n" +
            "• Animator with Idle/Walk animations\n" +
       "• Player tag",
       MessageType.Info
        );
    
  GUILayout.Space(10);
        
        if (GUILayout.Button("Create Player from Storyteller", GUILayout.Height(40)))
  {
       CreatePlayer();
   }
        
        GUILayout.Space(10);
     
        EditorGUILayout.HelpBox(
    "Expected model location:\n" +
            "Assets/Models/Storyteller/Storyteller.fbx",
          MessageType.None
        );
    }

    private void CreatePlayer()
    {
     // Find Storyteller model
        string modelPath = "Assets/Models/Storyteller/Storyteller.fbx";
        GameObject storytellerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        
    if (storytellerPrefab == null)
        {
     // Try alternate paths
  string[] alternatePaths = new string[]
{
    "Assets/Models/storyteller/Storyteller.fbx",
       "Assets/Models/Storyteller.fbx",
  "Assets/Storyteller/Storyteller.fbx"
    };
     
            foreach (string path in alternatePaths)
     {
      storytellerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
      if (storytellerPrefab != null)
 {
 modelPath = path;
          break;
    }
  }
          
  if (storytellerPrefab == null)
 {
       EditorUtility.DisplayDialog("Error", 
  $"Storyteller.fbx not found!\n\nSearched:\n" +
     $"• {modelPath}\n" +
    $"• Assets/Models/storyteller/Storyteller.fbx\n" +
        $"• Assets/Models/Storyteller.fbx\n\n" +
     $"Please ensure the file exists.", 
       "OK");
    return;
   }
        }

      Debug.Log($"[PlayerSetup] Found Storyteller model at: {modelPath}");

        // Check model import settings
        ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer != null)
        {
          Debug.Log($"[PlayerSetup] Model scale factor: {importer.globalScale}");
     }

// Check if Player already exists in scene
   GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
    if (existingPlayer != null)
        {
          if (!EditorUtility.DisplayDialog("Replace Player?",
$"A Player object already exists: {existingPlayer.name}\n\nDelete and create new?",
     "Yes", "Cancel"))
   {
        return;
            }
            DestroyImmediate(existingPlayer);
        Debug.Log($"[PlayerSetup] Deleted existing player: {existingPlayer.name}");
        }

  // Instantiate in scene
  GameObject player = Instantiate(storytellerPrefab);
  player.name = "Player";
     player.tag = "Player";
        player.transform.position = Vector3.zero;
    player.transform.rotation = Quaternion.identity;
        
        // IMPORTANT: Match NPC scale (they use scale 1)
        player.transform.localScale = Vector3.one * 100f; // Mixamo models are often 0.01 scale

        Debug.Log("[PlayerSetup] Instantiated Storyteller model");

   // Calculate bounds to determine proper size
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        Bounds combinedBounds = new Bounds(player.transform.position, Vector3.zero);
     foreach (Renderer r in renderers)
        {
            combinedBounds.Encapsulate(r.bounds);
        }
    
        float modelHeight = combinedBounds.size.y;
        float modelRadius = Mathf.Max(combinedBounds.size.x, combinedBounds.size.z) / 2f;
  
        Debug.Log($"[PlayerSetup] Model bounds - Height: {modelHeight}, Radius: {modelRadius}");

        // Add CharacterController
      CharacterController controller = player.GetComponent<CharacterController>();
 if (controller == null)
 {
    controller = player.AddComponent<CharacterController>();
Debug.Log("[PlayerSetup] Added CharacterController");
 }
        
        // Set controller size based on model bounds
      controller.height = Mathf.Max(modelHeight * 0.9f, 1.8f); // At least 1.8m tall
        controller.radius = Mathf.Max(modelRadius * 0.8f, 0.3f); // At least 0.3m radius
        controller.center = new Vector3(0, controller.height / 2f, 0);

        Debug.Log($"[PlayerSetup] CharacterController - Height: {controller.height}, Radius: {controller.radius}");

  // Add PlayerController script
      PlayerController playerController = player.GetComponent<PlayerController>();
 if (playerController == null)
        {
            playerController = player.AddComponent<PlayerController>();
  Debug.Log("[PlayerSetup] Added PlayerController script");
        }

// Setup Animator
        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
    {
          animator = player.AddComponent<Animator>();
            Debug.Log("[PlayerSetup] Added Animator component");
        }

  // Try to find existing PlayerController animator
     string animControllerPath = "Assets/Animations/PlayerController.controller";
      RuntimeAnimatorController animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animControllerPath);
        
        if (animController == null)
        {
      Debug.LogWarning($"[PlayerSetup] PlayerController.controller not found at {animControllerPath}. You'll need to assign it manually.");
     }
        else
   {
        animator.runtimeAnimatorController = animController;
         Debug.Log("[PlayerSetup] Assigned PlayerController animator");
     }
        
  animator.applyRootMotion = false;

        // Add camera if not exists
   Camera mainCamera = Camera.main;
        if (mainCamera == null)
      {
   GameObject camObj = new GameObject("Main Camera");
     camObj.tag = "MainCamera";
  mainCamera = camObj.AddComponent<Camera>();
  camObj.AddComponent<AudioListener>();
         Debug.Log("[PlayerSetup] Created Main Camera");
        }

        // Position camera behind and above player (adjusted for scale)
    Vector3 cameraOffset = new Vector3(0, controller.height * 0.6f, -controller.height * 1.5f);
        mainCamera.transform.position = player.transform.position + cameraOffset;
        mainCamera.transform.LookAt(player.transform.position + Vector3.up * controller.height * 0.7f);

        // Select the new player
    Selection.activeGameObject = player;
        EditorGUIUtility.PingObject(player);

 string successMessage = $"? Player created successfully!\n\n" +
$"Model: Storyteller\n" +
     $"Position: {player.transform.position}\n" +
           $"Scale: {player.transform.localScale}\n" +
 $"CharacterController: Height={controller.height:F2}, Radius={controller.radius:F2}\n" +
       $"Tag: Player\n" +
        $"Camera positioned at {mainCamera.transform.position}";

  Debug.Log($"[PlayerSetup] {successMessage}");

      EditorUtility.DisplayDialog("Player Created!", successMessage, "OK");
    }
}
