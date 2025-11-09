using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor utility to quickly set up the dialogue system in a scene.
/// Access via: Tools > NPC System > Setup Scene
/// </summary>
public class SceneSetupWizard : EditorWindow
{
    [MenuItem("Tools/NPC System/Setup Scene")]
    public static void ShowWindow()
    {
        GetWindow<SceneSetupWizard>("Scene Setup Wizard");
    }

    private void OnGUI()
    {
      GUILayout.Label("Scene Setup Wizard", EditorStyles.boldLabel);
    EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
  "This wizard will set up a basic dialogue system scene with:\n" +
   "• Ground plane\n" +
            "• Player with controller\n" +
        "• Camera\n" +
 "• Dialogue UI\n" +
            "• Manager objects\n" +
  "• Example NPC",
  MessageType.Info);

  EditorGUILayout.Space();

        if (GUILayout.Button("Create Complete Scene Setup", GUILayout.Height(40)))
        {
    CreateSceneSetup();
  }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

     GUILayout.Label("Individual Components", EditorStyles.boldLabel);

        if (GUILayout.Button("Create Managers Only"))
        {
  CreateManagers();
        }

if (GUILayout.Button("Create UI Only"))
        {
 CreateDialogueUI();
        }

   if (GUILayout.Button("Create Player Only"))
   {
            CreatePlayer();
        }
    }

    private void CreateSceneSetup()
    {
        // Create ground
  GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
     ground.transform.localScale = new Vector3(10, 1, 10);

        // Create player
        GameObject player = CreatePlayer();

        // Create managers
   var managers = CreateManagers();

        // Create UI
        CreateDialogueUI();

        // Create example NPC
   CreateExampleNPC(managers.dialogueManager, managers.interactionPrompt);

        EditorUtility.DisplayDialog("Success", 
      "Scene setup complete!\n\n" +
            "Next steps:\n" +
     "1. Create an NPC asset (Tools > NPC System > Create NPC Package)\n" +
        "2. Assign it to the ExampleNPC's NPCInteraction component\n" +
   "3. Start your Python API server\n" +
          "4. Press Play and test!",
            "OK");
  }

    private GameObject CreatePlayer()
    {
   GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
    player.transform.position = new Vector3(0, 1, 0);

        // Remove default collider and add CharacterController
     DestroyImmediate(player.GetComponent<Collider>());
        CharacterController cc = player.AddComponent<CharacterController>();
 cc.center = Vector3.zero;
        cc.radius = 0.5f;
      cc.height = 2f;

      // Add PlayerController
        PlayerController pc = player.AddComponent<PlayerController>();
        
   // Setup camera
   Camera mainCam = Camera.main;
        if (mainCam == null)
     {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
    camObj.tag = "MainCamera";
        }

        pc.playerCamera = mainCam.transform;
        pc.moveSpeed = 5f;
        pc.rotationSpeed = 2f;
        pc.cameraHeightOffset = 1.5f;
        pc.cameraDistanceOffset = 3f;

        Selection.activeGameObject = player;
        return player;
    }

    private (DialogueManager dialogueManager, GameObject interactionPrompt) CreateManagers()
    {
     // Create DialogueManager
    GameObject dmObj = new GameObject("DialogueManager");
     DialogueManager dm = dmObj.AddComponent<DialogueManager>();
        dm.apiUrl = "http://localhost:8000/npc_conversation";
        dm.requestTimeout = 10f;

        // Create DialogueUIManager
      GameObject duiObj = new GameObject("DialogueUIManager");
        DialogueUIManager dui = duiObj.AddComponent<DialogueUIManager>();

        // Link them
  dm.uiManager = dui;

        // Find or create UI elements
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
         var uiElements = CreateDialogueUI();
    dui.dialoguePanel = uiElements.dialoguePanel;
     dui.dialogueText = uiElements.dialogueText;
 return (dm, uiElements.interactionPrompt);
        }

        return (dm, null);
    }

    private (GameObject dialoguePanel, TextMeshProUGUI dialogueText, GameObject interactionPrompt) CreateDialogueUI()
    {
    // Create or find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
   if (canvas == null)
        {
         GameObject canvasObj = new GameObject("Canvas");
  canvas = canvasObj.AddComponent<Canvas>();
         canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

    // Create Dialogue Panel
        GameObject panelObj = new GameObject("DialoguePanel");
     panelObj.transform.SetParent(canvas.transform, false);
   UnityEngine.UI.Image panelImage = panelObj.AddComponent<UnityEngine.UI.Image>();
   panelImage.color = new Color(0, 0, 0, 0.8f);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.05f);
        panelRect.anchorMax = new Vector2(0.9f, 0.25f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Create Dialogue Text
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
   tmpText.text = "Dialogue text will appear here...";
        tmpText.fontSize = 24;
        tmpText.alignment = TextAlignmentOptions.MidlineLeft;
     tmpText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
    textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);

        // Hide panel by default
        panelObj.SetActive(false);

        // Create Interaction Prompt
     GameObject promptObj = new GameObject("InteractionPrompt");
   promptObj.transform.SetParent(canvas.transform, false);
   UnityEngine.UI.Image promptImage = promptObj.AddComponent<UnityEngine.UI.Image>();
   promptImage.color = new Color(0, 0, 0, 0.7f);

        RectTransform promptRect = promptObj.GetComponent<RectTransform>();
     promptRect.anchorMin = new Vector2(0.5f, 0.5f);
        promptRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptRect.sizeDelta = new Vector2(200, 50);
        promptRect.anchoredPosition = new Vector2(0, -100);

        GameObject promptTextObj = new GameObject("PromptText");
    promptTextObj.transform.SetParent(promptObj.transform, false);
        TextMeshProUGUI promptTMP = promptTextObj.AddComponent<TextMeshProUGUI>();
        promptTMP.text = "Press E to talk";
 promptTMP.fontSize = 18;
   promptTMP.alignment = TextAlignmentOptions.Center;
     promptTMP.color = Color.white;

        RectTransform promptTextRect = promptTextObj.GetComponent<RectTransform>();
promptTextRect.anchorMin = Vector2.zero;
        promptTextRect.anchorMax = Vector2.one;
        promptTextRect.offsetMin = Vector2.zero;
        promptTextRect.offsetMax = Vector2.zero;

        InteractionPrompt promptScript = promptObj.AddComponent<InteractionPrompt>();
   promptScript.promptObject = promptObj;
        promptScript.promptText = promptTMP;

   return (panelObj, tmpText, promptObj);
    }

    private void CreateExampleNPC(DialogueManager dm, GameObject interactionPrompt)
    {
        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Cube);
      npc.name = "ExampleNPC";
   npc.transform.position = new Vector3(5, 0.5f, 5);

      // Add trigger collider
        SphereCollider trigger = npc.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 3f;

        // Add NPCInteraction
        NPCInteraction interaction = npc.AddComponent<NPCInteraction>();
        interaction.dialogueManager = dm;
        interaction.interactionPrompt = interactionPrompt?.GetComponent<InteractionPrompt>();
      interaction.interactionKey = KeyCode.E;
     interaction.defaultPlayerInput = "Hello!";
        interaction.maxEventsToSend = 2;

     // Add visual indicator
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
   indicator.name = "InteractionRangeIndicator";
        indicator.transform.SetParent(npc.transform);
        indicator.transform.localPosition = Vector3.zero;
        indicator.transform.localScale = Vector3.one * 6f;
        
    Material indicatorMat = new Material(Shader.Find("Standard"));
        indicatorMat.color = new Color(1, 1, 0, 0.1f);
        indicatorMat.SetFloat("_Mode", 3); // Transparent mode
        indicatorMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
   indicatorMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        indicatorMat.SetInt("_ZWrite", 0);
        indicatorMat.DisableKeyword("_ALPHATEST_ON");
        indicatorMat.EnableKeyword("_ALPHABLEND_ON");
        indicatorMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        indicatorMat.renderQueue = 3000;
        
        indicator.GetComponent<Renderer>().material = indicatorMat;
        DestroyImmediate(indicator.GetComponent<Collider>());

        EditorGUIUtility.PingObject(npc);
    }
}
