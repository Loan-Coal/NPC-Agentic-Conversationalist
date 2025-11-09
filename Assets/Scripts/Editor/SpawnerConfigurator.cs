using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Quick spawner setup from JSON configuration.
/// Menu: Tools > NPC System > Setup NPC Spawner
/// </summary>
public class SpawnerConfigurator : EditorWindow
{
    private string configPath = "Assets/Data/NPCConfiguration.json";
    
    [MenuItem("Tools/NPC System/Setup NPC Spawner")]
    public static void ShowWindow()
    {
     GetWindow<SpawnerConfigurator>("Spawner Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("NPC Spawner Setup", EditorStyles.boldLabel);
  EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Creates and configures NPCSpawner GameObject\n" +
            "based on NPCConfiguration.json",
     MessageType.Info);

     EditorGUILayout.Space();

        configPath = EditorGUILayout.TextField("Config Path", configPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Setup Spawner", GUILayout.Height(40)))
        {
     SetupSpawner();
        }
    }

private void SetupSpawner()
    {
if (!File.Exists(configPath))
        {
            EditorUtility.DisplayDialog("Error", $"Config file not found: {configPath}", "OK");
     return;
        }

        string json = File.ReadAllText(configPath);
   NPCConfig config = JsonUtility.FromJson<NPCConfig>(json);

    if (config == null)
        {
  EditorUtility.DisplayDialog("Error", "Failed to parse JSON", "OK");
  return;
   }

// Find or create spawner
        NPCSpawner spawner = FindObjectOfType<NPCSpawner>();
  if (spawner == null)
   {
      GameObject spawnerObj = new GameObject("NPCSpawner");
  spawner = spawnerObj.AddComponent<NPCSpawner>();
   Debug.Log("Created NPCSpawner GameObject");
     }

// Load NPC prefabs and data
        List<NPCData> npcDatas = new List<NPCData>();
   
        List<int> patrolIndices = new List<int>();
List<int> wanderIndices = new List<int>();

   // Load first prefab as template
   GameObject templatePrefab = null;

        for (int i = 0; i < config.npcs.Length; i++)
{
       var npc = config.npcs[i];

// Load NPCData
    string dataPath = $"Assets/NPCs/{npc.name}/NPC_{npc.name}.asset";
       NPCData data = AssetDatabase.LoadAssetAtPath<NPCData>(dataPath);
       
            if (data != null)
      {
           npcDatas.Add(data);
       }
            else
    {
  Debug.LogWarning($"Could not load NPCData at {dataPath}");
     }

     // Load prefab for template (use first one found)
   if (templatePrefab == null)
      {
     string prefabPath = $"Assets/Prefabs/NPCs/NPC_{npc.name}.prefab";
 templatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

   // Track behavior indices
    if (npc.behavior == "patrol")
       {
     patrolIndices.Add(i);
            }
   else if (npc.behavior == "wander")
    {
      wanderIndices.Add(i);
       }
   }

        // Configure spawner
 spawner.npcPrefab = templatePrefab;
spawner.npcDataAssets = npcDatas.ToArray();
 spawner.spawnCount = config.npcs.Length;
        spawner.spawnRadius = 12f;
        spawner.patrolIndices = patrolIndices.ToArray();
        spawner.wanderIndices = wanderIndices.ToArray();

     // Find managers
        spawner.dialogueManager = FindObjectOfType<DialogueManager>();
 spawner.interactionPrompt = FindObjectOfType<InteractionPrompt>();

        EditorUtility.SetDirty(spawner);

   Debug.Log($"Configured NPCSpawner with {config.npcs.Length} NPCs");
        EditorUtility.DisplayDialog("Success",
  $"NPCSpawner configured!\n\n" +
          $"NPCs: {config.npcs.Length}\n" +
       $"Patrol: {patrolIndices.Count}\n" +
$"Wander: {wanderIndices.Count}\n\n" +
          "IMPORTANT: Manually assign individual prefabs\n" +
       "to NPCSpawner in Inspector for unique models!\n\n" +
            "Press Play to spawn NPCs!",
      "OK");
   }
}
