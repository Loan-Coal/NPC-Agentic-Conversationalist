using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.AI;
#endif

public class NavMeshSetupWizard : EditorWindow
{
    [MenuItem("Tools/NPC System/Fix NavMesh & Ground Material")]
    public static void ShowWindow()
    {
        GetWindow<NavMeshSetupWizard>("NavMesh Fixer");
    }

    private void OnGUI()
    {
    GUILayout.Label("NavMesh & Ground Setup", EditorStyles.boldLabel);
        
      EditorGUILayout.HelpBox(
"This will:\n" +
   "1. Find all ground objects\n" +
        "2. Create a simple green ground material\n" +
       "3. Mark them as Navigation Static\n" +
   "4. Bake NavMesh\n" +
       "5. You should see BLUE overlay on walkable surfaces",
   MessageType.Info
        );
        
   GUILayout.Space(10);
        
        if (GUILayout.Button("Fix Ground Material", GUILayout.Height(30)))
        {
            FixGroundMaterial();
        }
   
    GUILayout.Space(5);
        
   if (GUILayout.Button("Mark All Static Objects for NavMesh", GUILayout.Height(30)))
 {
            MarkStaticObjects();
        }
        
  GUILayout.Space(5);
   
        if (GUILayout.Button("Bake NavMesh Now", GUILayout.Height(40)))
        {
       BakeNavMesh();
        }
        
 GUILayout.Space(10);
     
        if (GUILayout.Button("DO ALL (Fix + Mark + Bake)", GUILayout.Height(50)))
        {
    FixGroundMaterial();
            MarkStaticObjects();
     BakeNavMesh();
        }
    }

    private void FixGroundMaterial()
    {
        int objectsFixed = 0;
        
        // Find all objects that should have material
        string[] objectNames = { "Ground", "Plane", "Floor", "House_1", "House_2", "House_3", "House_4" };
        List<GameObject> objectsToFix = new List<GameObject>();
        
        foreach (string objName in objectNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
{
        objectsToFix.Add(obj);
            }
        }
        
        if (objectsToFix.Count == 0)
    {
            EditorUtility.DisplayDialog("Error", 
          "No ground or house objects found in scene!\n\n" +
  "Please run 'Build Village Scene' first.", 
     "OK");
            return;
      }

    Debug.Log($"[NavMeshFixer] Found {objectsToFix.Count} objects to fix");

 // Create or load materials
        Material groundMat = GetOrCreateMaterial("Assets/Materials/GroundMaterial.mat", new Color(0.3f, 0.6f, 0.3f));
        Material houseMat = GetOrCreateMaterial("Assets/Materials/HouseMaterial.mat", new Color(0.7f, 0.5f, 0.3f));

  // Apply materials to objects
        foreach (GameObject obj in objectsToFix)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
          if (renderer != null)
            {
             // Ground gets green material, houses get brown material
    if (obj.name.Contains("Ground") || obj.name.Contains("Plane") || obj.name.Contains("Floor"))
      {
           renderer.sharedMaterial = groundMat;
       Debug.Log($"[NavMeshFixer] Applied GREEN material to {obj.name}");
   }
   else if (obj.name.Contains("House"))
        {
       renderer.sharedMaterial = houseMat;
    Debug.Log($"[NavMeshFixer] Applied BROWN material to {obj.name}");
                }
         objectsFixed++;
        }
            else
            {
 Debug.LogWarning($"[NavMeshFixer] {obj.name} has no Renderer component!");
}
        }
        
        if (objectsFixed > 0)
    {
    EditorUtility.DisplayDialog("Success", 
          $"Fixed materials on {objectsFixed} objects!\n\n" +
     "• Ground = Green\n" +
"• Houses = Brown\n\n" +
   "No more pink!", 
                "OK");
        }
        else
     {
            EditorUtility.DisplayDialog("Warning", 
     "Found objects but couldn't apply materials.\n" +
         "They might be missing Renderer components.", 
    "OK");
        }
    }

    private Material GetOrCreateMaterial(string path, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        
        if (mat == null)
        {
    // Create Materials folder if needed
          if (!AssetDatabase.IsValidFolder("Assets/Materials"))
    {
       AssetDatabase.CreateFolder("Assets", "Materials");
        Debug.Log("[NavMeshFixer] Created Assets/Materials folder");
         }
    
  // Create new material
      mat = new Material(Shader.Find("Standard"));
            mat.color = color;
   AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[NavMeshFixer] Created new material at {path}");
        }
   
    return mat;
    }

 private void MarkStaticObjects()
    {
        int markedCount = 0;
        
        // Find all objects that should be static
        string[] staticNames = { "Ground", "House_1", "House_2", "House_3", "House_4", "Plane", "Floor" };
        
        foreach (string objName in staticNames)
        {
          GameObject obj = GameObject.Find(objName);
            if (obj != null)
   {
        GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.NavigationStatic);
      markedCount++;
     Debug.Log($"[NavMeshFixer] Marked {objName} as Navigation Static");
          }
 }

        if (markedCount > 0)
{
            Debug.Log($"[NavMeshFixer] Marked {markedCount} objects as Navigation Static");
EditorUtility.DisplayDialog("Success", 
   $"Marked {markedCount} objects as Navigation Static.\n\n" +
    "Now you can bake the NavMesh!", 
  "OK");
        }
   else
        {
          EditorUtility.DisplayDialog("Warning", 
      "No ground or house objects found!\n\n" +
                "Make sure you've run 'Build Village Scene' first.", 
    "OK");
        }
    }

    private void BakeNavMesh()
    {
 Debug.Log("[NavMeshFixer] Starting NavMesh bake...");
        
  #if UNITY_EDITOR
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("[NavMeshFixer] NavMesh bake complete! Check Scene view for blue overlay.");
  
EditorUtility.DisplayDialog("NavMesh Baked!", 
            "NavMesh has been baked successfully!\n\n" +
    "? Look for BLUE overlay on ground in Scene view\n" +
     "? If you don't see it, check Window ? AI ? Navigation ? Bake tab\n" +
   "? NPCs should now be able to walk around", 
       "OK");
     #endif
    }
}
