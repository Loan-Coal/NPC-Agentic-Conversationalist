using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Quick village scene builder.
/// Creates ground, 4 houses, and basic layout.
/// Menu: Tools > NPC System > Build Village Scene
/// </summary>
public class VillageBuilder : EditorWindow
{
    private bool usePrimitives = true;
    private string villagePackPath = "Assets/Environments/MedievalVillage";
    
    [MenuItem("Tools/NPC System/Build Village Scene")]
    public static void ShowWindow()
    {
  GetWindow<VillageBuilder>("Village Builder");
    }

    private void OnGUI()
    {
 GUILayout.Label("Village Scene Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
         "Quickly build a small medieval village with:\n" +
      "• Ground plane\n" +
     "• 4 houses (primitives or from imported pack)\n" +
     "• Centered at (0,0,0)",
    MessageType.Info);

   EditorGUILayout.Space();

  usePrimitives = EditorGUILayout.Toggle("Use Primitive Houses", usePrimitives);

if (!usePrimitives)
        {
  villagePackPath = EditorGUILayout.TextField("Village Pack Path", villagePackPath);
    }

        EditorGUILayout.Space();

     if (GUILayout.Button("Build Village", GUILayout.Height(40)))
        {
      BuildVillage();
        }
    }

private void BuildVillage()
    {
        // Create ground
 GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
   ground.name = "Ground";
   ground.transform.position = Vector3.zero;
ground.transform.localScale = new Vector3(10, 1, 10);
        ground.isStatic = true;

        // Add material
  Material groundMat = new Material(Shader.Find("Standard"));
   groundMat.color = new Color(0.4f, 0.35f, 0.25f); // Brown-ish
        ground.GetComponent<Renderer>().material = groundMat;

  Debug.Log("Created ground plane");

     // Create 4 houses
        Vector3[] housePositions = {
     new Vector3(-8, 0, 8),
            new Vector3(8, 0, 8),
       new Vector3(-8, 0, -8),
       new Vector3(8, 0, -8)
  };

        for (int i = 0; i < 4; i++)
        {
       if (usePrimitives)
       {
          CreatePrimitiveHouse(i, housePositions[i]);
        }
      else
     {
      CreatePackHouse(i, housePositions[i]);
  }
        }

        // Create directional light
        Light light = FindObjectOfType<Light>();
        if (light == null)
   {
      GameObject lightObj = new GameObject("Directional Light");
     light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
     }

  Debug.Log("Village scene built successfully!");

EditorUtility.DisplayDialog("Success", 
   "Village scene created!\n\n" +
  "Next steps:\n" +
    "1. Window ? AI ? Navigation ? Bake NavMesh\n" +
     "2. Add NPCSpawner and configure",
  "OK");
    }

private void CreatePrimitiveHouse(int index, Vector3 position)
  {
        GameObject house = new GameObject($"House_{index + 1}");
house.transform.position = position;
  house.isStatic = true;

        // Base (walls)
        GameObject walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
  walls.name = "Walls";
        walls.transform.SetParent(house.transform);
  walls.transform.localPosition = new Vector3(0, 1.5f, 0);
      walls.transform.localScale = new Vector3(4, 3, 4);
 walls.isStatic = true;

        Material wallMat = new Material(Shader.Find("Standard"));
   wallMat.color = new Color(0.8f, 0.75f, 0.65f); // Beige
        walls.GetComponent<Renderer>().material = wallMat;

// Roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
roof.name = "Roof";
   roof.transform.SetParent(house.transform);
   roof.transform.localPosition = new Vector3(0, 3.5f, 0);
        roof.transform.localScale = new Vector3(5, 0.5f, 5);
   roof.transform.rotation = Quaternion.Euler(0, 45, 0);
        roof.isStatic = true;

   Material roofMat = new Material(Shader.Find("Standard"));
 roofMat.color = new Color(0.5f, 0.3f, 0.2f); // Dark brown
 roof.GetComponent<Renderer>().material = roofMat;

    Debug.Log($"Created primitive house {index + 1}");
}

    private void CreatePackHouse(int index, Vector3 position)
    {
        // Try to find house prefabs in the village pack
   string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab house", new[] { villagePackPath });
      
   if (prefabGuids.Length == 0)
{
          Debug.LogWarning($"No house prefabs found in {villagePackPath}, using primitive instead");
  CreatePrimitiveHouse(index, position);
       return;
 }

        // Load first house prefab
GameObject housePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
  AssetDatabase.GUIDToAssetPath(prefabGuids[0]));

        GameObject house = PrefabUtility.InstantiatePrefab(housePrefab) as GameObject;
        house.name = $"House_{index + 1}";
   house.transform.position = position;
  house.isStatic = true;

 Debug.Log($"Created pack house {index + 1}");
    }
}
