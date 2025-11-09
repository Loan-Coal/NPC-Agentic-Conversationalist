using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to quickly create complete NPC packages with events.
/// Access via: Tools > NPC System > Create NPC Package
/// </summary>
public class NPCCreationWizard : EditorWindow
{
private string npcName = "NewNPC";
    private string npcBackstory = "";
    private string personalityTrait1 = "";
    private string personalityTrait2 = "";
    private string personalityTrait3 = "";
    private int numberOfEvents = 3;

    [MenuItem("Tools/NPC System/Create NPC Package")]
    public static void ShowWindow()
    {
   GetWindow<NPCCreationWizard>("NPC Creation Wizard");
    }

    private void OnGUI()
    {
        GUILayout.Label("NPC Package Creator", EditorStyles.boldLabel);
  EditorGUILayout.Space();

        EditorGUILayout.HelpBox("This wizard creates a complete NPC package with events in Assets/NPCs/{NPCName}/", MessageType.Info);
     EditorGUILayout.Space();

        // NPC Basic Info
        GUILayout.Label("Basic Information", EditorStyles.boldLabel);
        npcName = EditorGUILayout.TextField("NPC Name", npcName);
     npcBackstory = EditorGUILayout.TextField("Backstory", npcBackstory, GUILayout.Height(60));

        EditorGUILayout.Space();

 // Personality Traits
        GUILayout.Label("Personality Traits", EditorStyles.boldLabel);
        personalityTrait1 = EditorGUILayout.TextField("Trait 1", personalityTrait1);
    personalityTrait2 = EditorGUILayout.TextField("Trait 2", personalityTrait2);
        personalityTrait3 = EditorGUILayout.TextField("Trait 3", personalityTrait3);

      EditorGUILayout.Space();

        // Events
   GUILayout.Label("Events", EditorStyles.boldLabel);
        numberOfEvents = EditorGUILayout.IntSlider("Number of Events", numberOfEvents, 1, 10);

        EditorGUILayout.Space();
  EditorGUILayout.Space();

        // Create Button
        if (GUILayout.Button("Create NPC Package", GUILayout.Height(40)))
        {
            CreateNPCPackage();
        }
    }

    private void CreateNPCPackage()
    {
        if (string.IsNullOrEmpty(npcName))
     {
            EditorUtility.DisplayDialog("Error", "NPC Name cannot be empty!", "OK");
            return;
        }

   // Create folder structure
        string basePath = "Assets/NPCs/" + npcName;
      string eventsPath = basePath + "/Events";

        if (!Directory.Exists(basePath))
        {
 Directory.CreateDirectory(basePath);
        }
 if (!Directory.Exists(eventsPath))
        {
  Directory.CreateDirectory(eventsPath);
        }

        // Create event assets
        EventData[] events = new EventData[numberOfEvents];
        for (int i = 0; i < numberOfEvents; i++)
        {
      EventData eventAsset = ScriptableObject.CreateInstance<EventData>();
            eventAsset.eventType = "event";
            eventAsset.timestamp = $"{i + 1} days ago";
            eventAsset.description = $"Event {i + 1} - Edit this description";

string eventPath = $"{eventsPath}/Event_{i + 1}.asset";
            AssetDatabase.CreateAsset(eventAsset, eventPath);
      events[i] = eventAsset;
        }

 // Create NPC asset
  NPCData npcAsset = ScriptableObject.CreateInstance<NPCData>();
   npcAsset.npcName = npcName;
        npcAsset.backstory = npcBackstory;

      // Add personality traits
        var traits = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrEmpty(personalityTrait1)) traits.Add(personalityTrait1);
 if (!string.IsNullOrEmpty(personalityTrait2)) traits.Add(personalityTrait2);
     if (!string.IsNullOrEmpty(personalityTrait3)) traits.Add(personalityTrait3);
    npcAsset.personalityTraits = traits.ToArray();

        // Add events
        npcAsset.events = new System.Collections.Generic.List<EventData>(events);

      string npcPath = $"{basePath}/NPC_{npcName}.asset";
      AssetDatabase.CreateAsset(npcAsset, npcPath);

        // Refresh and select
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = npcAsset;
 EditorGUIUtility.PingObject(npcAsset);

        EditorUtility.DisplayDialog("Success", 
   $"Created NPC package at {basePath}\n\n" +
      $"- NPC Asset: NPC_{npcName}.asset\n" +
          $"- {numberOfEvents} Event assets in Events/ folder\n\n" +
       "Edit the event descriptions in the Inspector!", 
            "OK");
    }
}
