using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to capture NPC portrait sprites from prefabs.
/// Creates 256x256 headshot renders and assigns them to NPCData assets.
/// </summary>
public class PortraitCaptureEditor : EditorWindow
{
    private const int PORTRAIT_SIZE = 256;
  private const string PORTRAITS_FOLDER = "Assets/Portraits";

    [MenuItem("Tools/NPC System/Capture Portraits")]
    public static void ShowWindow()
    {
        GetWindow<PortraitCaptureEditor>("Portrait Capture");
    }

    private void OnGUI()
    {
        GUILayout.Label("NPC Portrait Capture", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool captures 256x256 portrait sprites from NPC prefabs and assigns them to NPCData assets.\n\n" +
       "Requirements:\n" +
            "- NPCData assets must exist\n" +
            "- Corresponding prefabs should be named matching the NPC name\n" +
            "- Prefabs should have a recognizable head/face area",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Capture All NPC Portraits", GUILayout.Height(40)))
  {
            CaptureAllPortraits();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear All Portraits", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear Portraits",
       "This will remove all portrait references from NPCData assets. Portrait files will remain. Continue?",
              "Yes", "Cancel"))
          {
    ClearAllPortraits();
  }
     }
    }

    private void CaptureAllPortraits()
    {
 // Ensure portraits folder exists
        if (!Directory.Exists(PORTRAITS_FOLDER))
        {
  Directory.CreateDirectory(PORTRAITS_FOLDER);
            AssetDatabase.Refresh();
        }

        // Find all NPCData assets
        string[] guids = AssetDatabase.FindAssets("t:NPCData");
     int capturedCount = 0;

        foreach (string guid in guids)
   {
            string path = AssetDatabase.GUIDToAssetPath(guid);
      NPCData npcData = AssetDatabase.LoadAssetAtPath<NPCData>(path);

       if (npcData != null)
        {
             if (CapturePortraitForNPC(npcData))
        {
     capturedCount++;
  }
  }
}

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Portrait Capture Complete",
 $"Captured {capturedCount} portraits successfully.\n\n" +
            $"Check {PORTRAITS_FOLDER} for generated sprites.",
       "OK");
    }

    private bool CapturePortraitForNPC(NPCData npcData)
    {
        if (npcData == null || string.IsNullOrEmpty(npcData.npcName))
     {
 Debug.LogWarning("PortraitCapture: NPCData is null or has no name");
            return false;
      }

    // Try to find prefab matching NPC name
    GameObject prefab = FindPrefabForNPC(npcData.npcName);
        if (prefab == null)
        {
  Debug.LogWarning($"PortraitCapture: Could not find prefab for {npcData.npcName}");
       return false;
        }

   // Create portrait sprite
        Sprite portrait = CapturePortraitFromPrefab(prefab, npcData.npcName);
 if (portrait != null)
        {
       npcData.portrait = portrait;
    EditorUtility.SetDirty(npcData);
            Debug.Log($"[PortraitCapture] Captured portrait for {npcData.npcName}");
   return true;
  }

        return false;
    }

    private GameObject FindPrefabForNPC(string npcName)
    {
  // Search for prefab with matching name
        string[] prefabGuids = AssetDatabase.FindAssets($"{npcName} t:Prefab");
        if (prefabGuids.Length > 0)
        {
 string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

    // Fallback: search for MedievalNPC prefab
        prefabGuids = AssetDatabase.FindAssets("MedievalNPC t:Prefab");
   if (prefabGuids.Length > 0)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        return null;
    }

    private Sprite CapturePortraitFromPrefab(GameObject prefab, string npcName)
    {
        // Create temporary instance
      GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        instance.name = "TempPortraitCapture";

        // Position for headshot
        instance.transform.position = new Vector3(1000, 1000, 1000); // Far away from scene

   // Find head/uppermost point
 Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
   {
     DestroyImmediate(instance);
            Debug.LogWarning($"PortraitCapture: No renderers found in prefab {prefab.name}");
   return null;
     }

        // Calculate bounds for headshot positioning
        Bounds combinedBounds = renderers[0].bounds;
        foreach (Renderer rend in renderers)
        {
            combinedBounds.Encapsulate(rend.bounds);
    }

    Vector3 headPosition = new Vector3(
            combinedBounds.center.x,
            combinedBounds.max.y - (combinedBounds.size.y * 0.2f), // Upper 20% of model
            combinedBounds.center.z
  );

 // Create temporary camera
     GameObject camObj = new GameObject("PortraitCamera");
   Camera portraitCam = camObj.AddComponent<Camera>();
  portraitCam.transform.position = headPosition + Vector3.forward * -2f;
   portraitCam.transform.LookAt(headPosition);
  portraitCam.clearFlags = CameraClearFlags.SolidColor;
        portraitCam.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        portraitCam.orthographic = false;
        portraitCam.fieldOfView = 30f;

  // Render to texture
        RenderTexture renderTexture = new RenderTexture(PORTRAIT_SIZE, PORTRAIT_SIZE, 24);
  portraitCam.targetTexture = renderTexture;
  portraitCam.Render();

   // Read pixels
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(PORTRAIT_SIZE, PORTRAIT_SIZE, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, PORTRAIT_SIZE, PORTRAIT_SIZE), 0, 0);
        texture.Apply();
  RenderTexture.active = null;

        // Save as PNG
        byte[] pngData = texture.EncodeToPNG();
    string pngPath = $"{PORTRAITS_FOLDER}/{npcName}_Portrait.png";
      File.WriteAllBytes(pngPath, pngData);

// Cleanup
        DestroyImmediate(camObj);
 DestroyImmediate(instance);
        DestroyImmediate(renderTexture);

        // Import and convert to sprite
     AssetDatabase.ImportAsset(pngPath);
        TextureImporter importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
  if (importer != null)
     {
    importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
          importer.spritePixelsPerUnit = 100;
 importer.mipmapEnabled = false;
      importer.SaveAndReimport();
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
        return sprite;
    }

    private void ClearAllPortraits()
    {
    string[] guids = AssetDatabase.FindAssets("t:NPCData");
        int clearedCount = 0;

        foreach (string guid in guids)
        {
          string path = AssetDatabase.GUIDToAssetPath(guid);
   NPCData npcData = AssetDatabase.LoadAssetAtPath<NPCData>(path);

if (npcData != null && npcData.portrait != null)
            {
        npcData.portrait = null;
                EditorUtility.SetDirty(npcData);
     clearedCount++;
    }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[PortraitCapture] Cleared {clearedCount} portrait references");
    }
}
