using UnityEngine;
using UnityEditor;

public class AnimationDiagnostics : EditorWindow
{
    [MenuItem("Tools/NPC System/Check Animations")]
    public static void ShowWindow()
    {
GetWindow<AnimationDiagnostics>("Animation Check");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation Diagnostics", EditorStyles.boldLabel);
        
     EditorGUILayout.HelpBox(
          "This will check:\n" +
       "• Animator Controller exists\n" +
          "• NPCs have Animator component\n" +
            "• Animation clips are assigned\n" +
            "• Parameters are set up correctly\n" +
        "• Root motion settings",
         MessageType.Info
     );
        
     GUILayout.Space(10);
 
      if (GUILayout.Button("Check All NPCs", GUILayout.Height(40)))
{
            CheckAnimations();
        }
  
      GUILayout.Space(10);
        
   if (GUILayout.Button("Check Player Animations", GUILayout.Height(30)))
        {
  CheckPlayerAnimations();
      }
  }

    private void CheckAnimations()
    {
        Debug.Log("=== ANIMATION CHECK ===");
        
  // Check main animator controller
        string controllerPath = "Assets/Animations/MedievalNPC_Controller.controller";
   RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        
      if (controller == null)
        {
   Debug.LogError($"[AnimationCheck] ? Main controller not found: {controllerPath}");
       Debug.LogWarning("[AnimationCheck] Run 'Auto Build NPCs from JSON' ? 'Create Animator Controller' first!");
        }
        else
        {
      Debug.Log($"[AnimationCheck] ? Main controller exists: {controllerPath}");
        }
        
        // Check each NPC prefab
        string[] npcNames = { "Elara", "Maris", "Theron", "Garrick", "Cedric" };
        
        int totalNPCs = npcNames.Length;
        int withAnimator = 0;
        int withController = 0;
        int rootMotionOff = 0;
        
        foreach (string npcName in npcNames)
        {
            string prefabPath = $"Assets/Prefabs/NPCs/NPC_{npcName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
          {
      Debug.LogError($"[AnimationCheck] ? {npcName}: Prefab not found at {prefabPath}");
         continue;
         }
            
       Animator animator = prefab.GetComponent<Animator>();
            if (animator == null)
            {
   Debug.LogError($"[AnimationCheck] ? {npcName}: No Animator component!");
       continue;
}
            
            withAnimator++;
        
            if (animator.runtimeAnimatorController == null)
   {
                Debug.LogWarning($"[AnimationCheck] ?? {npcName}: Animator has NO controller assigned!");
     }
        else
   {
            withController++;
           Debug.Log($"[AnimationCheck] ? {npcName}: Controller = {animator.runtimeAnimatorController.name}");
            }
            
    if (animator.applyRootMotion)
    {
   Debug.LogWarning($"[AnimationCheck] ?? {npcName}: applyRootMotion is TRUE (should be FALSE for NavMesh)");
  }
  else
     {
    rootMotionOff++;
            }
 
 // Check for override controller
    AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideController != null)
    {
   Debug.Log($"[AnimationCheck] ? {npcName}: Has AnimatorOverrideController (good!)");
      }
        }
   
     Debug.Log("=== CHECK COMPLETE ===");
        Debug.Log($"Summary: {withAnimator}/{totalNPCs} have Animator, " +
    $"{withController}/{totalNPCs} have controller, " +
      $"{rootMotionOff}/{totalNPCs} have root motion OFF");
        
     string message = $"Animation Check Results:\n\n" +
 $"? NPCs with Animator: {withAnimator}/{totalNPCs}\n" +
         $"? NPCs with Controller: {withController}/{totalNPCs}\n" +
       $"? Root Motion OFF: {rootMotionOff}/{totalNPCs}\n\n";
        
        if (withController < totalNPCs)
        {
   message += "?? Some NPCs missing controllers!\n" +
             "Run 'Auto Build NPCs from JSON' ? 'Create All NPC Prefabs'";
 }
        else
        {
         message += "? All animations properly configured!";
     }
        
        EditorUtility.DisplayDialog("Animation Check", message, "OK");
    }

    private void CheckPlayerAnimations()
    {
        Debug.Log("=== PLAYER ANIMATION CHECK ===");
        
        // Find player in scene
   GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("[AnimationCheck] ? No GameObject with 'Player' tag found in scene!");
         EditorUtility.DisplayDialog("Error", 
                "No Player found in scene!\n\n" +
       "Run 'Setup Player (Storyteller)' first.", 
    "OK");
    return;
        }
        
 Debug.Log($"[AnimationCheck] Found Player: {player.name}");
      
        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
        {
      Debug.LogError("[AnimationCheck] ? Player has no Animator component!");
        }
        else
        {
         Debug.Log("[AnimationCheck] ? Player has Animator");
       
            if (animator.runtimeAnimatorController == null)
   {
          Debug.LogWarning("[AnimationCheck] ?? Player Animator has NO controller assigned!");
    }
        else
            {
    Debug.Log($"[AnimationCheck] ? Player controller: {animator.runtimeAnimatorController.name}");
            }
            
   if (animator.applyRootMotion)
{
    Debug.LogWarning("[AnimationCheck] ?? Player applyRootMotion is TRUE (should be FALSE)");
            }
     else
   {
     Debug.Log("[AnimationCheck] ? Player root motion is OFF");
      }
        }
        
      PlayerController controller = player.GetComponent<PlayerController>();
 if (controller == null)
    {
            Debug.LogWarning("[AnimationCheck] ?? Player missing PlayerController script!");
        }
        else
        {
            Debug.Log("[AnimationCheck] ? Player has PlayerController script");
        }
      
    Debug.Log("=== PLAYER CHECK COMPLETE ===");
    }
}
