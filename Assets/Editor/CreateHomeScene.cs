using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class CreateHomeScene
{
    [MenuItem("Tools/Create Home Scene (Auto)")]
    public static void Create()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Create Home Scene", "Please exit Play Mode before creating the Home scene. Stop Play Mode and try again.", "OK");
            return;
        }

        var scenePath = "Assets/Scenes/Home.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Main Camera (so Game view has a camera rendering)
        var camGO = new GameObject("Main Camera", typeof(Camera));
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0f, 0f, -10f);

        // Ensure an EventSystem exists so UI is interactable
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem", typeof(EventSystem));

            // Prefer the Input System UI module if the new Input System package is installed.
            var inputSystemModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                esGO.AddComponent(inputSystemModuleType);
            }
            else
            {
                esGO.AddComponent(typeof(StandaloneInputModule));
            }
        }

        // Panel
        var panelGO = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImage = panelGO.GetComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);

        // Title
        var title = CreateTMP("Title", "Bowling", 72, TextAlignmentOptions.Center);
        title.transform.SetParent(panelGO.transform, false);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(600, 100);

        // Player label
        var label = CreateTMP("PlayersLabel", "Players", 36, TextAlignmentOptions.Center);
        label.transform.SetParent(panelGO.transform, false);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.7f);
        labelRect.anchorMax = new Vector2(0.5f, 0.7f);
        labelRect.anchoredPosition = new Vector2(0, 40);
        labelRect.sizeDelta = new Vector2(300, 60);

        // Player count value
        var value = CreateTMP("PlayersValue", "1", 48, TextAlignmentOptions.Center);
        value.transform.SetParent(panelGO.transform, false);
        var valueRect = value.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.5f, 0.62f);
        valueRect.anchorMax = new Vector2(0.5f, 0.62f);
        valueRect.anchoredPosition = new Vector2(0, 50);
        valueRect.sizeDelta = new Vector2(160, 60);

        // Minus and Plus buttons
        var minus = CreateButton("MinusButton", "-");
        minus.transform.SetParent(panelGO.transform, false);
        var minusRect = minus.GetComponent<RectTransform>();
        minusRect.anchorMin = new Vector2(0.5f, 0.62f);
        minusRect.anchorMax = new Vector2(0.5f, 0.62f);
        minusRect.anchoredPosition = new Vector2(-120, 50);
        minusRect.sizeDelta = new Vector2(80, 60);

        var plus = CreateButton("PlusButton", "+");
        plus.transform.SetParent(panelGO.transform, false);
        var plusRect = plus.GetComponent<RectTransform>();
        plusRect.anchorMin = new Vector2(0.5f, 0.62f);
        plusRect.anchorMax = new Vector2(0.5f, 0.62f);
        plusRect.anchoredPosition = new Vector2(120, 50);
        plusRect.sizeDelta = new Vector2(80, 60);

        // ★Games label
        var gamesLabel = CreateTMP("GamesLabel", "Games", 36, TextAlignmentOptions.Center);
        gamesLabel.transform.SetParent(panelGO.transform, false);
        var gamesLabelRect = gamesLabel.GetComponent<RectTransform>();
        gamesLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
        gamesLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
        gamesLabelRect.anchoredPosition = new Vector2(0, 90);
        gamesLabelRect.sizeDelta = new Vector2(300, 60);

        // ★Games value
        var gamesValue = CreateTMP("GamesValue", "1", 48, TextAlignmentOptions.Center);
        gamesValue.transform.SetParent(panelGO.transform, false);
        var gamesValueRect = gamesValue.GetComponent<RectTransform>();
        gamesValueRect.anchorMin = new Vector2(0.5f, 0.42f);
        gamesValueRect.anchorMax = new Vector2(0.5f, 0.42f);
        gamesValueRect.anchoredPosition = new Vector2(0, 120);
        gamesValueRect.sizeDelta = new Vector2(160, 60);

        // ★Games minus/plus buttons
        var gamesMinus = CreateButton("GamesMinusButton", "-");
        gamesMinus.transform.SetParent(panelGO.transform, false);
        var gamesMinusRect = gamesMinus.GetComponent<RectTransform>();
        gamesMinusRect.anchorMin = new Vector2(0.5f, 0.42f);
        gamesMinusRect.anchorMax = new Vector2(0.5f, 0.42f);
        gamesMinusRect.anchoredPosition = new Vector2(-120, 120);
        gamesMinusRect.sizeDelta = new Vector2(80, 60);

        var gamesPlus = CreateButton("GamesPlusButton", "+");
        gamesPlus.transform.SetParent(panelGO.transform, false);
        var gamesPlusRect = gamesPlus.GetComponent<RectTransform>();
        gamesPlusRect.anchorMin = new Vector2(0.5f, 0.42f);
        gamesPlusRect.anchorMax = new Vector2(0.5f, 0.42f);
        gamesPlusRect.anchoredPosition = new Vector2(120, 120);
        gamesPlusRect.sizeDelta = new Vector2(80, 60);

        // Player name fields label
        var namesLabel = CreateTMP("NamesLabel", "Player Names", 36, TextAlignmentOptions.Center);
        namesLabel.transform.SetParent(panelGO.transform, false);
        var namesLabelRect = namesLabel.GetComponent<RectTransform>();
        namesLabelRect.anchorMin = new Vector2(0.5f, 0.32f);
        namesLabelRect.anchorMax = new Vector2(0.5f, 0.32f);
        namesLabelRect.anchoredPosition = new Vector2(0, 140);
        namesLabelRect.sizeDelta = new Vector2(300, 50);

        // Names container
        var namesContainerGO = new GameObject("NamesContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        namesContainerGO.transform.SetParent(panelGO.transform, false);
        var namesContainerRect = namesContainerGO.GetComponent<RectTransform>();
        namesContainerRect.anchorMin = new Vector2(0.5f, 0.1f);
        namesContainerRect.anchorMax = new Vector2(0.5f, 0.1f);
        namesContainerRect.anchoredPosition = new Vector2(0, 200);
        namesContainerRect.sizeDelta = new Vector2(520, 180);
        var namesLayout = namesContainerGO.GetComponent<VerticalLayoutGroup>();
        namesLayout.childAlignment = TextAnchor.UpperCenter;
        namesLayout.spacing = 10f;
        namesLayout.childForceExpandHeight = false;
        namesLayout.childForceExpandWidth = true;
        var namesFitter = namesContainerGO.GetComponent<ContentSizeFitter>();
        namesFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Name field template
        var nameFieldTemplate = CreateTMPInputField("PlayerNameFieldTemplate", "Player name...");
        nameFieldTemplate.transform.SetParent(namesContainerGO.transform, false);
        var templateRect = nameFieldTemplate.GetComponent<RectTransform>();
        templateRect.sizeDelta = new Vector2(150, 40);
        nameFieldTemplate.gameObject.SetActive(false);

        // Start button
        var start = CreateButton("StartButton", "Start");
        start.transform.SetParent(panelGO.transform, false);
        var startRect = start.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(0.5f, 0.02f);
        startRect.anchorMax = new Vector2(0.5f, 0.02f);
        startRect.anchoredPosition = new Vector2(0,100);
        startRect.sizeDelta = new Vector2(240, 80);

        // HomeMenuManager
        var managerGO = new GameObject("HomeMenuManager");
        var homeMenu = managerGO.AddComponent<global::HomeMenuManager>();
        homeMenu.playerCountTextTMP = value.GetComponent<TextMeshProUGUI>();
        homeMenu.namesContainer = namesContainerGO.GetComponent<RectTransform>();
        homeMenu.nameFieldPrefab = nameFieldTemplate.GetComponent<TMP_InputField>();
        homeMenu.gameSceneName = "SampleScene";
        homeMenu.gameCountTextTMP = gamesValue.GetComponent<TextMeshProUGUI>(); // ★追加

        // Wire button events and make them persistent so they survive saving the scene
        var minusBtn = minus.GetComponent<Button>();
        var plusBtn = plus.GetComponent<Button>();
        var startBtn = start.GetComponent<Button>();
        var gamesMinusBtn = gamesMinus.GetComponent<Button>(); // ★追加
        var gamesPlusBtn = gamesPlus.GetComponent<Button>();   // ★追加

        UnityEditor.Events.UnityEventTools.AddPersistentListener(minusBtn.onClick, homeMenu.DecreasePlayerCount);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(plusBtn.onClick, homeMenu.IncreasePlayerCount);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(startBtn.onClick, homeMenu.StartGame);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gamesMinusBtn.onClick, homeMenu.DecreaseGameCount); // ★追加
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gamesPlusBtn.onClick, homeMenu.IncreaseGameCount);  // ★追加

        // Ensure buttons are interactable and mark objects dirty so Unity serializes the changes
        minusBtn.interactable = true;
        plusBtn.interactable = true;
        startBtn.interactable = true;
        gamesMinusBtn.interactable = true; // ★追加
        gamesPlusBtn.interactable = true;  // ★追加

        UnityEditor.EditorUtility.SetDirty(minusBtn);
        UnityEditor.EditorUtility.SetDirty(plusBtn);
        UnityEditor.EditorUtility.SetDirty(startBtn);
        UnityEditor.EditorUtility.SetDirty(gamesMinusBtn); // ★追加
        UnityEditor.EditorUtility.SetDirty(gamesPlusBtn);  // ★追加
        UnityEditor.EditorUtility.SetDirty(managerGO);

        // Save scene
        EditorSceneManager.SaveScene(scene, scenePath);

        // Ensure in build settings: add Home scene and try to add the target game scene (SampleScene) if it exists in project
        var existing = EditorBuildSettings.scenes.ToList();
        if (!existing.Any(s => s.path == scenePath))
        {
            existing.Add(new EditorBuildSettingsScene(scenePath, true));
        }

        // If a scene asset matching gameSceneName exists, add it to Build Settings
        var sceneName = homeMenu.gameSceneName;
        var foundSceneGuids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
        if (foundSceneGuids != null && foundSceneGuids.Length > 0)
        {
            var gameScenePath = AssetDatabase.GUIDToAssetPath(foundSceneGuids[0]);
            if (!existing.Any(s => s.path == gameScenePath))
            {
                existing.Add(new EditorBuildSettingsScene(gameScenePath, true));
            }
        }

        EditorBuildSettings.scenes = existing.ToArray();

        Debug.Log("Home scene created at: " + scenePath + " (gameSceneName set to 'BowlingGame').");
    }

    private static GameObject CreateTMP(string name, string text, int fontSize, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;
        return go;
    }

    private static GameObject CreateButton(string name, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var img = go.GetComponent<Image>();
        img.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        var txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        var rt = txtGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return go;
    }

    private static GameObject CreateTMPInputField(string name, string placeholderText)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        var image = go.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.9f);

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(go.transform, false);
        var textTMP = textGO.GetComponent<TextMeshProUGUI>();
        textTMP.text = string.Empty;
        textTMP.fontSize = 24;
        textTMP.color = Color.black;
        textTMP.alignment = TextAlignmentOptions.Left;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);

        var placeholderGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholderGO.transform.SetParent(go.transform, false);
        var placeholderTMP = placeholderGO.GetComponent<TextMeshProUGUI>();
        placeholderTMP.text = placeholderText;
        placeholderTMP.fontSize = 24;
        placeholderTMP.color = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        placeholderTMP.alignment = TextAlignmentOptions.Left;
        var placeholderRect = placeholderGO.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 10);
        placeholderRect.offsetMax = new Vector2(-10, -10);

        var input = go.GetComponent<TMP_InputField>();
        input.textComponent = textTMP;
        input.placeholder = placeholderTMP;
        input.targetGraphic = image;
        input.text = string.Empty;
        input.interactable = true;

        return go;
    }
}