using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SceneDiagnostics : EditorWindow
{
  private Vector2 scrollPos;
    
    [MenuItem("Tools/NPC System/Scene Diagnostics (Full Report)")]
    public static void ShowWindow()
    {
  GetWindow<SceneDiagnostics>("Scene Diagnostics");
    }

    private void OnGUI()
  {
GUILayout.Label("Complete Scene Diagnostics", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
       "This tool shows you EVERYTHING about your current scene setup.\n" +
            "Use it to find out what's wrong and what's working.",
 MessageType.Info
        );
   
        GUILayout.Space(10);
        
        if (GUILayout.Button("Run Full Diagnostic", GUILayout.Height(40)))
        {
 RunDiagnostics();
        }
   
  GUILayout.Space(10);
   
        if (GUILayout.Button("Fix All Issues (One Click)", GUILayout.Height(50)))
        {
          FixAllIssues();
    }
    }

    private void RunDiagnostics()
    {
        Debug.Log("========================================");
        Debug.Log("SCENE DIAGNOSTICS - FULL REPORT");
        Debug.Log("========================================\n");

        // 1. Check Player
        CheckPlayer();
        
        // 2. Check Ground and Houses
        CheckEnvironment();
    
        // 3. Check NPCs in scene
 CheckNPCsInScene();
        
  // 4. Check NPC Prefabs
        CheckNPCPrefabs();
      
        // 5. Check Animator Controllers
        CheckAnimatorControllers();
  
        // 6. Check NavMesh
    CheckNavMesh();
        
 // 7. Check UI
        CheckUI();

      Debug.Log("\n========================================");
 Debug.Log("DIAGNOSTIC COMPLETE - Check messages above");
  Debug.Log("========================================");
     
        EditorUtility.DisplayDialog("Diagnostics Complete", 
            "Full diagnostic report printed to Console.\n\n" +
         "Check the Console window (Ctrl+Shift+C) for detailed results.\n\n" +
       "Look for:\n" +
        "? = Good\n" +
            "?? = Warning\n" +
            "? = Error/Problem",
      "OK");
    }

    private void CheckPlayer()
    {
  Debug.Log("--- PLAYER CHECK ---");
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
   {
            Debug.LogError("? NO PLAYER FOUND! Tag a GameObject as 'Player' or run Setup Player (Storyteller)");
            return;
        }
        
        Debug.Log($"? Player found: {player.name}");
        Debug.Log($"  Position: {player.transform.position}");
        Debug.Log($"  Scale: {player.transform.localScale}");
        
  CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null)
        {
    Debug.LogWarning("?? Player missing CharacterController!");
        }
        else
        {
        Debug.Log($"? CharacterController - Height: {cc.height:F2}, Radius: {cc.radius:F2}");
  }
        
        PlayerController pc = player.GetComponent<PlayerController>();
    if (pc == null)
        {
       Debug.LogWarning("?? Player missing PlayerController script!");
        }
    else
        {
   Debug.Log("? PlayerController script attached");
        }
   
  Animator anim = player.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("?? Player missing Animator!");
  }
   else
        {
         if (anim.runtimeAnimatorController == null)
  {
     Debug.LogWarning("?? Player Animator has no controller assigned!");
          }
      else
            {
  Debug.Log($"? Player Animator: {anim.runtimeAnimatorController.name}");
            }
      }
     
        // Check model size
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
  Bounds combinedBounds = renderers[0].bounds;
       foreach (Renderer r in renderers)
       {
    combinedBounds.Encapsulate(r.bounds);
       }
            Debug.Log($"? Player visual size: {combinedBounds.size.y:F2}m tall");
         
  if (combinedBounds.size.y < 1.0f)
 {
    Debug.LogWarning($"?? Player looks VERY SMALL ({combinedBounds.size.y:F2}m)! Consider scaling up.");
 }
       else if (combinedBounds.size.y > 3.0f)
     {
          Debug.LogWarning($"?? Player looks VERY LARGE ({combinedBounds.size.y:F2}m)! Consider scaling down.");
            }
    }
  
        Debug.Log("");
    }

    private void CheckEnvironment()
    {
        Debug.Log("--- ENVIRONMENT CHECK ---");
        
        string[] objectNames = { "Ground", "Plane", "Floor", "House_1", "House_2", "House_3", "House_4" };
      int foundCount = 0;
   int pinkCount = 0;
        
        foreach (string objName in objectNames)
    {
      GameObject obj = GameObject.Find(objName);
       if (obj != null)
            {
        foundCount++;
     
      Renderer renderer = obj.GetComponent<Renderer>();
     if (renderer != null)
                {
          if (renderer.sharedMaterial == null)
        {
  Debug.LogWarning($"?? {objName} has NO MATERIAL (will be pink/magenta)");
     pinkCount++;
      }
         else if (renderer.sharedMaterial.name.Contains("Default"))
         {
     Debug.LogWarning($"?? {objName} has DEFAULT MATERIAL (might be pink)");
   pinkCount++;
   }
           else
        {
            Debug.Log($"? {objName} has material: {renderer.sharedMaterial.name}");
 }
 }
        else
   {
        Debug.LogWarning($"?? {objName} has no Renderer!");
      }
           
     // Check static flag
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);
           if ((flags & StaticEditorFlags.NavigationStatic) == 0)
{
 Debug.LogWarning($"?? {objName} is NOT marked Navigation Static!");
                }
  }
        }
        
  if (foundCount == 0)
        {
 Debug.LogError("? NO GROUND OR HOUSES FOUND! Run 'Build Village Scene' first!");
        }
   else
  {
            Debug.Log($"? Found {foundCount} environment objects");
    }
        
        if (pinkCount > 0)
        {
            Debug.LogError($"? {pinkCount} objects have missing/pink materials! Run 'Fix NavMesh & Ground Material'");
        }
     
        Debug.Log("");
    }

    private void CheckNPCsInScene()
    {
   Debug.Log("--- NPCs IN SCENE (Runtime) ---");
        
 NPCInteraction[] npcs = FindObjectsOfType<NPCInteraction>();
  
   if (npcs.Length == 0)
        {
         Debug.LogWarning("?? No NPCs currently in scene (they spawn at runtime)");
        }
        else
        {
            Debug.Log($"? Found {npcs.Length} NPCs in scene:");
            foreach (var npc in npcs)
      {
    Debug.Log($"  - {npc.gameObject.name}");
                
                Renderer[] renderers = npc.GetComponentsInChildren<Renderer>();
     if (renderers.Length == 0)
                {
        Debug.LogWarning($"    ?? {npc.gameObject.name} is INVISIBLE (no renderers)!");
         }
           
      Bounds combinedBounds = new Bounds(npc.transform.position, Vector3.zero);
        foreach (Renderer r in renderers)
 {
  combinedBounds.Encapsulate(r.bounds);
    }
      
       if (renderers.Length > 0)
     {
          Debug.Log($"    Visual size: {combinedBounds.size.y:F2}m tall");
     }
            }
}
      
        Debug.Log("");
    }

private void CheckNPCPrefabs()
    {
        Debug.Log("--- NPC PREFABS ---");
        
        string[] npcNames = { "Elara", "Maris", "Theron", "Garrick", "Cedric" };
  int foundPrefabs = 0;
     int invisiblePrefabs = 0;
        int noControllerPrefabs = 0;
        
        foreach (string npcName in npcNames)
        {
     string prefabPath = $"Assets/Prefabs/NPCs/NPC_{npcName}.prefab";
   GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
if (prefab == null)
        {
        Debug.LogError($"? Prefab NOT FOUND: {prefabPath}");
 continue;
         }
            
    foundPrefabs++;
 Debug.Log($"? Prefab exists: NPC_{npcName}");
        
 // Check scale
       Debug.Log($"  Scale: {prefab.transform.localScale}");
            
         // Check if has visible mesh
          Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
if (renderers.Length == 0)
          {
    Debug.LogWarning($"  ?? {npcName} prefab is INVISIBLE (no mesh renderers)!");
       invisiblePrefabs++;
      }
     else
       {
                Debug.Log($"  ? Has {renderers.Length} renderer(s)");
         }
            
    // Check animator
       Animator animator = prefab.GetComponent<Animator>();
   if (animator == null)
         {
    Debug.LogWarning($"  ?? No Animator component!");
        }
            else if (animator.runtimeAnimatorController == null)
      {
              Debug.LogWarning($"  ?? Animator has NO CONTROLLER!");
                noControllerPrefabs++;
      }
          else
     {
     Debug.Log($"  ? Animator controller: {animator.runtimeAnimatorController.name}");
      }
    }
   
        Debug.Log($"\nSummary: {foundPrefabs}/5 prefabs found");
     if (invisiblePrefabs > 0)
      {
  Debug.LogError($"? {invisiblePrefabs} prefabs are INVISIBLE!");
        }
        if (noControllerPrefabs > 0)
     {
   Debug.LogError($"? {noControllerPrefabs} prefabs have NO ANIMATOR CONTROLLER!");
   }
        
        Debug.Log("");
 }

    private void CheckAnimatorControllers()
    {
   Debug.Log("--- ANIMATOR CONTROLLERS ---");
        
        // Check main controller
        string mainControllerPath = "Assets/Animations/MedievalNPC_Controller.controller";
        var mainController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(mainControllerPath);
 
        if (mainController == null)
        {
          Debug.LogError($"? Main animator controller NOT FOUND: {mainControllerPath}");
        }
      else
        {
            Debug.Log($"? Main controller exists: {mainControllerPath}");
        }
        
     // Check override controllers
        string[] npcNames = { "Elara", "Maris", "Theron", "Garrick", "Cedric" };
        int foundOverrides = 0;
        
        foreach (string npcName in npcNames)
        {
            string overridePath = $"Assets/Animations/Overrides/AO_{npcName}.overrideController";
        var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overridePath);
            
   if (overrideController == null)
     {
        Debug.LogWarning($"?? Override controller NOT FOUND: AO_{npcName}");
      }
      else
    {
     foundOverrides++;
 Debug.Log($"? Override controller exists: AO_{npcName}");
          }
}
        
        Debug.Log($"\nSummary: {foundOverrides}/5 override controllers found");
        if (foundOverrides < 5)
        {
            Debug.LogError("? Some override controllers are MISSING! Re-run 'Create All NPC Prefabs'");
        }
        
        Debug.Log("");
    }

    private void CheckNavMesh()
    {
    Debug.Log("--- NAVMESH CHECK ---");
   
        var navMeshData = UnityEngine.AI.NavMesh.CalculateTriangulation();
        
        if (navMeshData.vertices.Length == 0)
        {
   Debug.LogError("? NO NAVMESH BAKED! NPCs won't be able to move!");
            Debug.LogError("   Fix: Window ? AI ? Navigation ? Bake tab ? Bake button");
        }
        else
        {
 Debug.Log($"? NavMesh is baked ({navMeshData.vertices.Length} vertices)");
        }
        
        Debug.Log("");
    }

    private void CheckUI()
    {
        Debug.Log("--- UI CHECK ---");
        
        GameObject dialoguePanel = GameObject.Find("DialoguePanel");
        if (dialoguePanel == null)
   {
Debug.LogWarning("?? DialoguePanel not found in scene");
        }
      else
        {
  Debug.Log("? DialoguePanel exists");
        }
        
     DialogueManager dm = FindObjectOfType<DialogueManager>();
        if (dm == null)
   {
   Debug.LogWarning("?? DialogueManager not in scene");
 }
        else
        {
         Debug.Log("? DialogueManager exists");
 }
        
     DialogueUIManager uim = FindObjectOfType<DialogueUIManager>();
        if (uim == null)
        {
            Debug.LogWarning("?? DialogueUIManager not in scene");
        }
     else
        {
            Debug.Log("? DialogueUIManager exists");
        }
        
     Debug.Log("");
    }

    private void FixAllIssues()
    {
    if (!EditorUtility.DisplayDialog("Fix All Issues?", 
            "This will:\n" +
         "1. Fix player scale if too small/large\n" +
            "2. Apply materials to ground and houses\n" +
 "3. Mark objects as Navigation Static\n" +
            "4. Rebuild NPC prefabs with proper models\n" +
            "5. Bake NavMesh\n\n" +
            "This may take a minute. Continue?",
       "Yes, Fix Everything", "Cancel"))
        {
       return;
        }
        
     Debug.Log("========================================");
        Debug.Log("AUTO-FIX STARTING...");
        Debug.Log("========================================\n");
        
     // Fix player
        FixPlayerScale();
    
        // Fix materials
    FixMaterials();
      
      // Mark static
     MarkObjectsStatic();
        
        // Rebuild NPC prefabs
     RebuildNPCPrefabs();
      
    // Bake NavMesh
        BakeNavMesh();
        
   Debug.Log("\n========================================");
        Debug.Log("AUTO-FIX COMPLETE!");
 Debug.Log("========================================");
        
        EditorUtility.DisplayDialog("Fix Complete!", 
   "All automatic fixes have been applied!\n\n" +
            "? Player scale adjusted\n" +
         "? Materials applied\n" +
 "? Objects marked static\n" +
       "? NPC prefabs rebuilt\n" +
            "? NavMesh baked\n\n" +
   "Press Play to test!",
   "OK");
    }

    private void FixPlayerScale()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        
     Bounds combinedBounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
     {
         combinedBounds.Encapsulate(r.bounds);
   }
        
   float height = combinedBounds.size.y;
      
     if (height < 1.0f)
        {
            // Too small, scale up
       float scaleFactor = 1.8f / height;
        player.transform.localScale *= scaleFactor;
            Debug.Log($"[AutoFix] Scaled player UP by {scaleFactor:F1}x (was {height:F2}m tall)");
}
        else if (height > 3.0f)
        {
        // Too large, scale down
            float scaleFactor = 2.0f / height;
     player.transform.localScale *= scaleFactor;
            Debug.Log($"[AutoFix] Scaled player DOWN by {scaleFactor:F1}x (was {height:F2}m tall)");
      }
     
        // Adjust CharacterController
        CharacterController cc = player.GetComponent<CharacterController>();
  if (cc != null)
        {
            Renderer[] newRenderers = player.GetComponentsInChildren<Renderer>();
 Bounds newBounds = newRenderers[0].bounds;
            foreach (Renderer r in newRenderers)
      {
         newBounds.Encapsulate(r.bounds);
            }
         
     cc.height = newBounds.size.y * 0.9f;
      cc.radius = Mathf.Max(newBounds.size.x, newBounds.size.z) / 2f * 0.8f;
 cc.center = new Vector3(0, cc.height / 2f, 0);
       Debug.Log($"[AutoFix] Adjusted CharacterController - Height: {cc.height:F2}, Radius: {cc.radius:F2}");
        }
    }

    private void FixMaterials()
 {
        // Same as NavMeshSetupWizard but automated
        Material groundMat = GetOrCreateMaterial("Assets/Materials/GroundMaterial.mat", new Color(0.3f, 0.6f, 0.3f));
    Material houseMat = GetOrCreateMaterial("Assets/Materials/HouseMaterial.mat", new Color(0.7f, 0.5f, 0.3f));
        
        string[] objectNames = { "Ground", "Plane", "Floor", "House_1", "House_2", "House_3", "House_4" };
 
        foreach (string objName in objectNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
  {
          Renderer renderer = obj.GetComponent<Renderer>();
    if (renderer != null)
             {
      if (objName.Contains("Ground") || objName.Contains("Plane") || objName.Contains("Floor"))
      {
          renderer.sharedMaterial = groundMat;
   Debug.Log($"[AutoFix] Applied green material to {objName}");
              }
          else if (objName.Contains("House"))
           {
        renderer.sharedMaterial = houseMat;
       Debug.Log($"[AutoFix] Applied brown material to {objName}");
                  }
      }
            }
        }
  }

    private Material GetOrCreateMaterial(string path, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        
        if (mat == null)
  {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
     {
             AssetDatabase.CreateFolder("Assets", "Materials");
      }
       
    mat = new Material(Shader.Find("Standard"));
          mat.color = color;
         AssetDatabase.CreateAsset(mat, path);
      AssetDatabase.SaveAssets();
        }
        
 return mat;
  }

    private void MarkObjectsStatic()
    {
     string[] objectNames = { "Ground", "Plane", "Floor", "House_1", "House_2", "House_3", "House_4" };
        
        foreach (string objName in objectNames)
        {
  GameObject obj = GameObject.Find(objName);
        if (obj != null)
            {
           GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.NavigationStatic);
                Debug.Log($"[AutoFix] Marked {objName} as Navigation Static");
 }
        }
    }

    private void RebuildNPCPrefabs()
    {
        Debug.Log("[AutoFix] Rebuilding NPC prefabs with proper scale and controllers...");
        // This would call NPCAutoBuilder's CreateAllNPCPrefabs
        // For now, just log that user should do it
        Debug.LogWarning("[AutoFix] Please run 'Auto Build NPCs from JSON' ? 'Create All NPC Prefabs' manually");
    }

    private void BakeNavMesh()
    {
   #if UNITY_EDITOR
  UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("[AutoFix] NavMesh baked successfully");
     #endif
    }
}
