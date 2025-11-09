using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class NPCModelFixer : EditorWindow
{
    private Dictionary<string, string> modelFolderMapping = new Dictionary<string, string>()
    {
        { "Elara", "Merchant" },
        { "Maris", "Healer" },
 { "Theron", "Farmer" },
     { "Garrick", "Guard" },
        { "Cedric", "Smith" }
    };

    [MenuItem("Tools/NPC System/Fix NPC Models")]
    public static void ShowWindow()
    {
        GetWindow<NPCModelFixer>("NPC Model Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("NPC Model Diagnostics", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This will check if NPC models exist and are properly assigned to prefabs.\n\n" +
            "Expected structure:\n" +
            "Assets/Models/Characters/Merchant/[model].fbx\n" +
       "Assets/Models/Characters/Healer/[model].fbx\n" +
            "etc.",
    MessageType.Info
        );
        
 GUILayout.Space(10);
        
      if (GUILayout.Button("Check NPC Models", GUILayout.Height(30)))
     {
        CheckModels();
        }

        GUILayout.Space(5);
      
        if (GUILayout.Button("List All Character Models", GUILayout.Height(30)))
     {
    ListAllModels();
        }
        
        GUILayout.Space(5);
        
 if (GUILayout.Button("Create Capsule Placeholders (Temporary)", GUILayout.Height(30)))
        {
    CreatePlaceholders();
        }
        
        GUILayout.Space(10);
     
        EditorGUILayout.HelpBox(
            "If you don't have character models yet:\n" +
        "1. Click 'Create Capsule Placeholders' to make NPCs visible\n" +
            "2. Download models from Mixamo later\n" +
            "3. Run Auto Builder again to assign real models",
            MessageType.None
        );
    }

    private void CheckModels()
    {
        Debug.Log("=== NPC MODEL CHECK ===");
   
        foreach (var npc in modelFolderMapping)
        {
            string npcName = npc.Key;
     string folderName = npc.Value;
          string folderPath = $"Assets/Models/Characters/{folderName}";
            
          if (!Directory.Exists(folderPath))
            {
             Debug.LogError($"[NPCModelFixer] ? Folder missing: {folderPath}");
             continue;
       }
   
            // Find FBX files in folder
       string[] files = Directory.GetFiles(folderPath, "*.fbx", SearchOption.AllDirectories);
   if (files.Length == 0)
{
     Debug.LogWarning($"[NPCModelFixer] ?? No .fbx files in {folderPath}");
  }
    else
            {
             Debug.Log($"[NPCModelFixer] ? {npcName} ({folderName}): Found {files.Length} model(s)");
      foreach (string file in files)
       {
   Debug.Log($"    - {Path.GetFileName(file)}");
        }
            }
 
          // Check prefab
         string prefabPath = $"Assets/Prefabs/NPCs/NPC_{npcName}.prefab";
    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
     {
         Debug.LogError($"[NPCModelFixer] ? Prefab missing: {prefabPath}");
            }
            else
            {
         // Check if prefab has mesh renderer
     SkinnedMeshRenderer[] renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
                MeshRenderer[] meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
   
     if (renderers.Length == 0 && meshRenderers.Length == 0)
     {
              Debug.LogWarning($"[NPCModelFixer] ?? {npcName} prefab has NO MESH! (invisible)");
        }
  else
     {
             Debug.Log($"[NPCModelFixer] ? {npcName} prefab has {renderers.Length + meshRenderers.Length} renderer(s)");
       }
  }
        }
        
   Debug.Log("=== CHECK COMPLETE ===");
        
     EditorUtility.DisplayDialog("Model Check Complete", 
            "Check the Console for detailed results.\n\n" +
     "Look for:\n" +
        "? = OK\n" +
       "?? = Warning\n" +
            "? = Error (missing files)",
            "OK");
    }

    private void ListAllModels()
    {
        Debug.Log("=== ALL CHARACTER MODELS ===");
        
        string basePath = "Assets/Models/Characters";
    if (!Directory.Exists(basePath))
        {
   Debug.LogError($"[NPCModelFixer] Directory not found: {basePath}");
       
       // Try alternate paths
            string[] alternatePaths = new string[]
   {
     "Assets/Models",
          "Assets/Characters",
       "Assets"
        };
  
Debug.Log("[NPCModelFixer] Searching for any .fbx files in Assets...");
          string[] allFBX = Directory.GetFiles("Assets", "*.fbx", SearchOption.AllDirectories);
         
            if (allFBX.Length == 0)
    {
                Debug.LogWarning("[NPCModelFixer] No .fbx files found anywhere in Assets/");
            }
            else
     {
           Debug.Log($"[NPCModelFixer] Found {allFBX.Length} .fbx files:");
foreach (string file in allFBX)
                {
         Debug.Log($"  - {file.Replace("\\", "/")}");
       }
     }
  return;
        }
        
        string[] characterFBX = Directory.GetFiles(basePath, "*.fbx", SearchOption.AllDirectories);
        
        if (characterFBX.Length == 0)
  {
            Debug.LogWarning("[NPCModelFixer] No .fbx files found in Assets/Models/Characters/");
  }
        else
        {
   Debug.Log($"[NPCModelFixer] Found {characterFBX.Length} character models:");
          foreach (string file in characterFBX)
            {
       Debug.Log($"  - {file.Replace("\\", "/")}");
      }
        }
        
      Debug.Log("=== LIST COMPLETE ===");
    }

    private void CreatePlaceholders()
    {
        int created = 0;
     
        foreach (var npc in modelFolderMapping)
        {
            string npcName = npc.Key;
            string prefabPath = $"Assets/Prefabs/NPCs/NPC_{npcName}.prefab";
            
   GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
         {
            Debug.LogWarning($"[NPCModelFixer] Prefab not found: {prefabPath}. Run Auto Builder first!");
         continue;
            }
  
          // Check if already has a mesh
        if (prefab.GetComponentInChildren<Renderer>() != null)
   {
       Debug.Log($"[NPCModelFixer] {npcName} already has a mesh, skipping");
       continue;
   }
            
  // Open prefab for editing
   string assetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(assetPath);
     
            // Create capsule child
       GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "PlaceholderMesh";
            capsule.transform.SetParent(prefabInstance.transform);
          capsule.transform.localPosition = new Vector3(0, 1, 0);
capsule.transform.localRotation = Quaternion.identity;
         capsule.transform.localScale = Vector3.one;
            
        // Color it differently for each NPC
  Renderer renderer = capsule.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            
            // Assign unique colors
    switch (npcName)
            {
           case "Elara": mat.color = Color.yellow; break;  // Merchant - gold
    case "Maris": mat.color = Color.green; break;   // Healer - green
   case "Theron": mat.color = new Color(0.6f, 0.4f, 0.2f); break; // Farmer - brown
        case "Garrick": mat.color = Color.red; break;   // Guard - red
        case "Cedric": mat.color = Color.gray; break;   // Blacksmith - gray
}
 
        renderer.sharedMaterial = mat;
  
            // Remove collider from capsule (prefab root has the trigger collider)
            DestroyImmediate(capsule.GetComponent<Collider>());
       
          // Save prefab
      PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
     PrefabUtility.UnloadPrefabContents(prefabInstance);
            
            Debug.Log($"[NPCModelFixer] Added {mat.color} capsule placeholder to {npcName}");
    created++;
        }
        
        AssetDatabase.Refresh();
 
     if (created > 0)
   {
            EditorUtility.DisplayDialog("Placeholders Created", 
           $"Added capsule placeholders to {created} NPCs.\n\n" +
        "They will now be VISIBLE (as colored capsules) until you import actual models.\n\n" +
   "Colors:\n" +
       "• Elara (Merchant) = Yellow\n" +
          "• Maris (Healer) = Green\n" +
       "• Theron (Farmer) = Brown\n" +
          "• Garrick (Guard) = Red\n" +
     "• Cedric (Blacksmith) = Gray",
             "OK");
        }
        else
    {
   EditorUtility.DisplayDialog("No Changes", 
           "All NPCs already have meshes, or prefabs don't exist.\n\n" +
    "Run 'Auto Build NPCs from JSON' first if you haven't already.",
        "OK");
        }
    }
}
