using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Automated NPC setup from JSON configuration.
/// Creates prefabs, animator controllers, and NPCData assets.
/// Menu: Tools > NPC System > Auto Build NPCs from JSON
/// </summary>
public class NPCAutoBuilder : EditorWindow
{
    private string configPath = "Assets/Data/NPCConfiguration.json";
    private NPCConfig config;
    private bool showLog = true;
  
    [MenuItem("Tools/NPC System/Auto Build NPCs from JSON")]
    public static void ShowWindow()
    {
      GetWindow<NPCAutoBuilder>("NPC Auto Builder");
  }

    private void OnGUI()
    {
        GUILayout.Label("NPC Auto Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

   EditorGUILayout.HelpBox(
     "This tool reads NPCConfiguration.json and creates:\n" +
     "• Prefabs with Animator, NavMeshAgent, NPCInteraction\n" +
  "• Shared Animator Controller\n" +
            "• NPCData ScriptableObject assets\n" +
            "• Relationships between NPCs",
  MessageType.Info);

     EditorGUILayout.Space();

     configPath = EditorGUILayout.TextField("Config JSON Path", configPath);
    showLog = EditorGUILayout.Toggle("Show Detailed Log", showLog);

   EditorGUILayout.Space();

        if (GUILayout.Button("1. Load Configuration", GUILayout.Height(30)))
        {
     LoadConfiguration();
        }

        if (config != null)
  {
 EditorGUILayout.HelpBox($"Loaded {config.npcs.Length} NPCs from config", MessageType.Info);
   }

        EditorGUILayout.Space();

        if (GUILayout.Button("2. Create Animator Controller", GUILayout.Height(30)))
        {
  CreateSharedAnimatorController();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("3. Create All NPC Prefabs", GUILayout.Height(30)))
   {
            CreateAllNPCPrefabs();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("4. Create All NPCData Assets", GUILayout.Height(30)))
        {
    CreateAllNPCDataAssets();
   }

        EditorGUILayout.Space();

        if (GUILayout.Button("5. Setup Relationships", GUILayout.Height(30)))
        {
        SetupRelationships();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("RUN ALL STEPS", GUILayout.Height(50)))
      {
    RunFullBuild();
 }
    }

    private void LoadConfiguration()
    {
        if (!File.Exists(configPath))
        {
            EditorUtility.DisplayDialog("Error", $"Configuration file not found at:\n{configPath}", "OK");
       return;
    }

      string json = File.ReadAllText(configPath);
      config = JsonUtility.FromJson<NPCConfig>(json);

        if (config == null || config.npcs == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to parse JSON configuration", "OK");
   }
        else
      {
            Log($"Loaded {config.npcs.Length} NPCs from configuration");
   }
    }

    private void CreateSharedAnimatorController()
    {
        if (!EnsureDirectories()) return;

        string controllerPath = "Assets/Animations/MedievalNPC_Controller.controller";

        // Check if already exists
        if (File.Exists(controllerPath))
        {
    if (!EditorUtility.DisplayDialog("Overwrite?", 
  $"Animator Controller already exists at {controllerPath}\nOverwrite?", 
         "Yes", "Cancel"))
  {
              return;
            }
        }

        // Create controller
      AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

     // Add parameters
        controller.AddParameter("isWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isTalking", AnimatorControllerParameterType.Bool);

        // Get base layer
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine stateMachine = baseLayer.stateMachine;

        // Create states (no clips assigned yet - will be per-prefab via override controllers)
    AnimatorState idleState = stateMachine.AddState("Idle");
 AnimatorState walkState = stateMachine.AddState("Walk");
        AnimatorState talkState = stateMachine.AddState("Talk");

        // Set Idle as default
    stateMachine.defaultState = idleState;

        // Create transitions
        // Idle -> Walk
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
    idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "isWalking");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.1f;

        // Walk -> Idle
AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isWalking");
        walkToIdle.hasExitTime = false;
      walkToIdle.duration = 0.1f;

        // Any State -> Talk
AnimatorStateTransition anyToTalk = stateMachine.AddAnyStateTransition(talkState);
   anyToTalk.AddCondition(AnimatorConditionMode.If, 0, "isTalking");
 anyToTalk.hasExitTime = false;
        anyToTalk.duration = 0.1f;

        // Talk -> Idle
        AnimatorStateTransition talkToIdle = talkState.AddTransition(idleState);
        talkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isTalking");
        talkToIdle.hasExitTime = true;
  talkToIdle.duration = 0.2f;

        EditorUtility.SetDirty(controller);
AssetDatabase.SaveAssets();
        Log($"Created Animator Controller at {controllerPath}");
    }

    private void CreateAllNPCPrefabs()
    {
        if (config == null)
        {
  EditorUtility.DisplayDialog("Error", "Load configuration first!", "OK");
            return;
}

   if (!EnsureDirectories()) return;

      AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
  "Assets/Animations/MedievalNPC_Controller.controller");

 if (controller == null)
        {
   EditorUtility.DisplayDialog("Error", "Create Animator Controller first!", "OK");
       return;
        }

        foreach (var npc in config.npcs)
      {
      CreateNPCPrefab(npc, controller);
        }

   AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
Log("Created all NPC prefabs");
    }

    private void CreateNPCPrefab(NPCConfigEntry npc, AnimatorController controller)
    {
  string modelPath = $"Assets/Models/Characters/{npc.model_folder}";
    
        // Check if directory exists
        if (!Directory.Exists(modelPath))
  {
       Debug.LogError($"[NPCAutoBuilder] Model directory not found: {modelPath}");
          EditorUtility.DisplayDialog("Missing Model", 
          $"Cannot find model folder for {npc.name}!\n\n" +
       $"Expected path: {modelPath}\n\n" +
       $"Please ensure the models are imported correctly.",
       "OK");
         return;
      }
 
        // Find character FBX (look for the main model, not animations)
        string[] allFbx = Directory.GetFiles(modelPath, "*.fbx", SearchOption.TopDirectoryOnly);
        string characterFbx = null;
 
        foreach (string fbx in allFbx)
      {
     string fileName = Path.GetFileNameWithoutExtension(fbx);
  // Skip animation files (contain @)
    if (!fileName.Contains("@"))
      {
          characterFbx = fbx.Replace("\\", "/");
        break;
     }
        }

      if (characterFbx == null)
{
  Debug.LogError($"[NPCAutoBuilder] Could not find character model for {npc.name} in {modelPath}");
            EditorUtility.DisplayDialog("Model Not Found", 
    $"Could not find main character model for {npc.name}!\n\n" +
       $"Searched in: {modelPath}\n" +
     $"Looking for: {npc.model_folder}.fbx (without @ symbol)\n\n" +
  $"Found these files:\n{string.Join("\n", allFbx.Select(Path.GetFileName))}",
         "OK");
         return;
   }

   Debug.Log($"[NPCAutoBuilder] Using model: {characterFbx}");

  GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterFbx);
        
        if (modelPrefab == null)
        {
  Debug.LogError($"[NPCAutoBuilder] Failed to load model from {characterFbx}");
     return;
        }
        
        // Check if prefab already exists
    string prefabPath = $"Assets/Prefabs/NPCs/NPC_{npc.name}.prefab";
        if (File.Exists(prefabPath))
        {
      if (!EditorUtility.DisplayDialog("Overwrite Prefab?", 
                $"Prefab already exists for {npc.name}.\n\nOverwrite it?", 
      "Yes", "Skip"))
     {
            Debug.Log($"[NPCAutoBuilder] Skipped {npc.name} (user chose not to overwrite)");
      return;
          }
        }
 
 // Instantiate model
     GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
        instance.name = $"NPC_{npc.name}";
        
      // IMPORTANT: Match scale with other characters
     instance.transform.localScale = Vector3.one * 100f; // Mixamo models typically need 100x scale

        // Add Animator
        Animator animator = instance.GetComponent<Animator>();
        if (animator == null)
      {
            animator = instance.AddComponent<Animator>();
}
        
        // Create and assign AnimatorOverrideController
     AnimatorOverrideController overrideController = CreateOverrideController(npc, controller, modelPath);
        
        if (overrideController != null)
        {
       animator.runtimeAnimatorController = overrideController;
     Debug.Log($"[NPCAutoBuilder] Assigned override controller to {npc.name}");
        }
        else
        {
     Debug.LogWarning($"[NPCAutoBuilder] No override controller created for {npc.name} - animations may not work");
    }
   
animator.applyRootMotion = false;

  // Add CapsuleCollider
CapsuleCollider collider = instance.GetComponent<CapsuleCollider>();
        if (collider == null)
 {
         collider = instance.AddComponent<CapsuleCollider>();
 }
collider.isTrigger = true;
     collider.height = 2f;
     collider.radius = 0.5f;
        collider.center = new Vector3(0, 1f, 0);

        // Add NavMeshAgent
     UnityEngine.AI.NavMeshAgent agent = instance.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = 1.5f;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
    agent.stoppingDistance = 0.5f;
        agent.autoBraking = true;
        agent.radius = 0.5f;
        agent.height = 2f;

        // Add NPCInteraction
        NPCInteraction interaction = instance.AddComponent<NPCInteraction>();
    interaction.interactionKey = KeyCode.E;
        interaction.defaultPlayerInput = "Hello!";
        interaction.maxEventsToSend = 2;

        // Save as prefab
  GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

        // Cleanup
  DestroyImmediate(instance);

   Log($"? Created prefab for {npc.name} at {prefabPath}");
 }

    private AnimatorOverrideController CreateOverrideController(NPCConfigEntry npc, AnimatorController baseController, string modelPath)
    {
    // Create override controller
        string overridePath = $"Assets/Animations/Overrides/AO_{npc.name}.overrideController";
      
        // Ensure directory exists
        string dir = Path.GetDirectoryName(overridePath);
 if (!Directory.Exists(dir))
    {
       Directory.CreateDirectory(dir);
      }

        // Check if already exists
        AnimatorOverrideController existingOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overridePath);
  if (existingOverride != null)
    {
  Debug.Log($"[NPCAutoBuilder] Override controller already exists for {npc.name}, deleting old one");
     AssetDatabase.DeleteAsset(overridePath);
      AssetDatabase.Refresh();
     }

   AnimatorOverrideController overrideController = new AnimatorOverrideController(baseController);
        
     // Find animation clips
     AnimationClip idleClip = FindAnimationClip(modelPath, npc.model_folder, config.animation_mapping.idle_pattern);
   AnimationClip walkClip = FindAnimationClip(modelPath, npc.model_folder, config.animation_mapping.walk_pattern);
 AnimationClip talkClip = FindAnimationClip(modelPath, npc.model_folder, config.animation_mapping.talk_pattern);

  // Debug found clips
        if (idleClip != null) Debug.Log($"[NPCAutoBuilder] Found Idle animation for {npc.name}: {idleClip.name}");
 if (walkClip != null) Debug.Log($"[NPCAutoBuilder] Found Walk animation for {npc.name}: {walkClip.name}");
        if (talkClip != null) Debug.Log($"[NPCAutoBuilder] Found Talk animation for {npc.name}: {talkClip.name}");

   // Set overrides
   var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        
    // Get the clips from the base controller to override
        AnimationClip[] baseClips = baseController.animationClips;
   Debug.Log($"[NPCAutoBuilder] Base controller has {baseClips.Length} animation clips");
  
    foreach (var clip in baseClips)
  {
       if (clip.name == "Idle" && idleClip != null)
       {
       overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, idleClip));
        Debug.Log($"[NPCAutoBuilder] Overriding Idle with {idleClip.name}");
     }
            else if (clip.name == "Walk" && walkClip != null)
    {
      overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, walkClip));
    Debug.Log($"[NPCAutoBuilder] Overriding Walk with {walkClip.name}");
   }
   else if (clip.name == "Talk" && talkClip != null)
   {
     overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, talkClip));
        Debug.Log($"[NPCAutoBuilder] Overriding Talk with {talkClip.name}");
   }
    }

     if (overrides.Count > 0)
        {
     overrideController.ApplyOverrides(overrides);
       Debug.Log($"[NPCAutoBuilder] Applied {overrides.Count} animation overrides for {npc.name}");
        }
  else
     {
         Debug.LogWarning($"[NPCAutoBuilder] No animation overrides applied for {npc.name}! Animations may not work.");
   }
 
  // Save the override controller as an asset
   AssetDatabase.CreateAsset(overrideController, overridePath);
 AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Log($"? Created override controller for {npc.name} at {overridePath}");
  
        // Reload it to ensure it's properly saved
   overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overridePath);
        
        return overrideController;
    }

    private AnimationClip FindAnimationClip(string modelPath, string characterName, string pattern)
    {
        // Look for animation FBX with pattern (e.g., "Merchant@Idle.fbx")
        string searchPattern = $"{characterName}{pattern}.fbx";
 string[] files = Directory.GetFiles(modelPath, searchPattern, SearchOption.TopDirectoryOnly);
        
        if (files.Length > 0)
        {
       string fbxPath = files[0].Replace("\\", "/");
  Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            
            foreach (Object asset in assets)
          {
           if (asset is AnimationClip clip && !clip.name.Contains("__preview__"))
          {
      return clip;
        }
            }
        }
     
        Debug.LogWarning($"Could not find animation clip matching {searchPattern} in {modelPath}");
     return null;
    }

    private void CreateAllNPCDataAssets()
 {
        if (config == null)
      {
     EditorUtility.DisplayDialog("Error", "Load configuration first!", "OK");
    return;
        }

        if (!EnsureDirectories()) return;

        foreach (var npc in config.npcs)
        {
            CreateNPCDataAsset(npc);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Log("Created all NPCData assets");
    }

    private void CreateNPCDataAsset(NPCConfigEntry npc)
    {
        string assetPath = $"Assets/NPCs/{npc.name}/NPC_{npc.name}.asset";
        
        // Ensure directory
        string dir = Path.GetDirectoryName(assetPath);
        if (!Directory.Exists(dir))
        {
          Directory.CreateDirectory(dir);
    }
    
        // Check if exists
        NPCData existing = AssetDatabase.LoadAssetAtPath<NPCData>(assetPath);
        if (existing != null)
        {
         if (!EditorUtility.DisplayDialog("Overwrite?", 
         $"NPCData for {npc.name} already exists.\nOverwrite?",
      "Yes", "Skip"))
            {
       return;
      }
     }

     // Create new NPCData
        NPCData npcData = ScriptableObject.CreateInstance<NPCData>();
        npcData.npcName = npc.name;
        npcData.role = npc.role;
        npcData.personalityTraits = npc.personality_traits;
        npcData.quirks = npc.quirks;
        npcData.backstory = npc.backstory;
    npcData.events = new List<EventData>();

        AssetDatabase.CreateAsset(npcData, assetPath);
        Log($"Created NPCData for {npc.name} at {assetPath}");
    }

    private void SetupRelationships()
    {
     if (config == null)
  {
         EditorUtility.DisplayDialog("Error", "Load configuration first!", "OK");
  return;
        }

        // Load all NPCData assets
        Dictionary<string, NPCData> npcDataMap = new Dictionary<string, NPCData>();
   string[] guids = AssetDatabase.FindAssets("t:NPCData", new[] { "Assets/NPCs" });
        
      foreach (string guid in guids)
   {
            string path = AssetDatabase.GUIDToAssetPath(guid);
  NPCData data = AssetDatabase.LoadAssetAtPath<NPCData>(path);
          if (data != null)
  {
     npcDataMap[data.npcName] = data;
            }
        }

        // Setup relationships from config
        foreach (var npc in config.npcs)
        {
      if (!npcDataMap.ContainsKey(npc.name)) continue;
            
      NPCData npcData = npcDataMap[npc.name];
            npcData.relationships.Clear();

      foreach (var rel in npc.relationships)
            {
       if (npcDataMap.ContainsKey(rel.npc_name))
  {
    npcData.relationships.Add(new NPCRelationship
    {
        otherNPC = npcDataMap[rel.npc_name],
         affinity = rel.affinity,
               relationshipTag = rel.tag,
      notes = rel.notes
            });
   }
   }

         EditorUtility.SetDirty(npcData);
        }

        AssetDatabase.SaveAssets();
        Log("Setup all NPC relationships");
    }

    private void RunFullBuild()
  {
        LoadConfiguration();
    if (config == null) return;

        CreateSharedAnimatorController();
        CreateAllNPCPrefabs();
     CreateAllNPCDataAssets();
   SetupRelationships();

        EditorUtility.DisplayDialog("Success", 
 "NPC Auto Build Complete!\n\n" +
        "Next steps:\n" +
        "1. Build village scene\n" +
            "2. Setup NPC Spawner\n" +
  "3. Bake NavMesh\n" +
   "4. Test!",
        "OK");
    }

    private bool EnsureDirectories()
    {
 string[] dirs = {
 "Assets/Animations",
            "Assets/Animations/Overrides",
            "Assets/Prefabs/NPCs",
 "Assets/NPCs"
        };

   foreach (string dir in dirs)
   {
       if (!Directory.Exists(dir))
       {
           Directory.CreateDirectory(dir);
         }
        }

    AssetDatabase.Refresh();
    return true;
    }

    private void Log(string message)
    {
        if (showLog)
        {
            Debug.Log($"[NPCAutoBuilder] {message}");
    }
    }
}

// JSON Configuration Classes
[System.Serializable]
public class NPCConfig
{
    public NPCConfigEntry[] npcs;
    public AnimationMapping animation_mapping;
}

[System.Serializable]
public class NPCConfigEntry
{
    public string name;
    public string role;
    public string model_folder;
    public string[] personality_traits;
    public string[] quirks;
    public string backstory;
    public string behavior;
    public RelationshipConfig[] relationships;
}

[System.Serializable]
public class RelationshipConfig
{
    public string npc_name;
    public float affinity;
 public string tag;
 public string notes;
}

[System.Serializable]
public class AnimationMapping
{
    public string idle_pattern;
public string walk_pattern;
    public string talk_pattern;
}
