using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HomeMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text playerCountTextTMP; // optional
    public Text playerCountTextUI; // optional fallback
    [Header("Player Name Fields")]
    public RectTransform namesContainer; // parent with VerticalLayoutGroup
    public TMP_InputField nameFieldPrefab; // prefab for a single name input

    [Header("Game Count UI")]
    public TMP_Text gameCountTextTMP;
    public Text gameCountTextUI;

    [Header("Settings")]
    [Tooltip("Name of the scene to load for the bowling game")]
    public string gameSceneName = "SampleScene";

    private int playerCount = 1;
    private const int minPlayers = 1;
    private const int maxPlayers = 4;

    private int gameCount = 1;
    private const int minGames = 1;
    private const int maxGames = 5;

    private readonly System.Collections.Generic.List<TMP_InputField> nameFields = new System.Collections.Generic.List<TMP_InputField>();
    [Header("Game Count UI Auto-Create")]
    public RectTransform gameCountAnchor; // この近くに生成する（例: StartButtonの上、未設定ならPanel直下に作る）
    [Header("Scene Names")]
    public string ballCustomSceneName = "BallCustom";
    public string purchaseSceneName = "Purchase";

    private Button customizeButton;
    private TextMeshProUGUI customizeButtonLabel;
    void Start()
    {
        GameSettings.ClearPlayerData();
        playerCount = 1;
        gameCount = 1;

        if (gameCountTextTMP == null)
            CreateGameCountUI();

        UpdatePlayerCountDisplay();
        UpdateGameCountDisplay();
        RebuildNameFields();
        PurchaseManager.Load();
        CreateCustomizeButton();
    }

    public void IncreasePlayerCount()
    {
        Debug.Log("HomeMenuManager: IncreasePlayerCount called (before) " + playerCount);
        SetPlayerCount(playerCount + 1);
        Debug.Log("HomeMenuManager: IncreasePlayerCount new value " + playerCount);
    }

    public void DecreasePlayerCount()
    {
        Debug.Log("HomeMenuManager: DecreasePlayerCount called (before) " + playerCount);
        SetPlayerCount(playerCount - 1);
        Debug.Log("HomeMenuManager: DecreasePlayerCount new value " + playerCount);
    }

    public void SetPlayerCount(int count)
    {
        playerCount = Mathf.Clamp(count, minPlayers, maxPlayers);
        UpdatePlayerCountDisplay();
        RebuildNameFields();
    }

    public void SetPlayerCountFromDropdown(int dropdownIndex)
    {
        // dropdownIndex is zero-based; map to 1..4
        SetPlayerCount(dropdownIndex + 1);
    }

    private void UpdatePlayerCountDisplay()
    {
        var s = playerCount.ToString();
        if (playerCountTextTMP != null)
            playerCountTextTMP.text = s;
        if (playerCountTextUI != null)
            playerCountTextUI.text = s;
    }

    // ============================
    // Game Count
    // ============================
    public void IncreaseGameCount()
    {
        SetGameCount(gameCount + 1);
    }

    public void DecreaseGameCount()
    {
        SetGameCount(gameCount - 1);
    }

    public void SetGameCount(int count)
    {
        gameCount = Mathf.Clamp(count, minGames, maxGames);
        UpdateGameCountDisplay();
    }

    private void UpdateGameCountDisplay()
    {
        var s = gameCount.ToString();
        if (gameCountTextTMP != null)
            gameCountTextTMP.text = s;
        if (gameCountTextUI != null)
            gameCountTextUI.text = s;
    }

    private void RebuildNameFields()
    {
        EnsureNameFieldTemplate();
        CollectExistingNameFields();

        if (namesContainer == null || nameFieldPrefab == null)
            return;

        // destroy excess
        for (int i = nameFields.Count - 1; i >= playerCount; i--)
        {
            if (nameFields[i] != null)
                Destroy(nameFields[i].gameObject);
            nameFields.RemoveAt(i);
        }

        // create missing fields
        for (int i = nameFields.Count; i < playerCount; i++)
        {
            var go = Instantiate(nameFieldPrefab.gameObject, namesContainer);
            go.name = $"PlayerNameField_{i + 1}";
            var input = go.GetComponent<TMP_InputField>();
            if (input == null)
                input = go.AddComponent<TMP_InputField>();

            var textComponent = input.textComponent;
            if (textComponent != null)
                textComponent.text = string.Empty;

            nameFields.Add(input);
            input.gameObject.SetActive(true);
        }

        UpdateNameFieldListFromContainer();

        // populate current fields with saved names or defaults
        for (int i = 0; i < nameFields.Count; i++)
        {
            if (nameFields[i] == null)
                continue;

            int idx = i + 1;
            string savedName = GameSettings.GetPlayerName(idx);
            if (string.IsNullOrEmpty(nameFields[i].text) || nameFields[i].text.StartsWith("Player "))
            {
                nameFields[i].text = savedName;
            }
        }
    }

    private void EnsureNameFieldTemplate()
    {
        if (nameFieldPrefab != null || namesContainer == null)
            return;

        CollectExistingNameFields();
        if (nameFieldPrefab != null)
            return;

        var templateGO = new GameObject("PlayerNameFieldTemplate", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        templateGO.transform.SetParent(namesContainer, false);
        var rect = templateGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500, 60);

        var image = templateGO.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.9f);

        var input = templateGO.GetComponent<TMP_InputField>();

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(templateGO.transform, false);
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

        input.textComponent = textTMP;

        var placeholderGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholderGO.transform.SetParent(templateGO.transform, false);
        var placeholderTMP = placeholderGO.GetComponent<TextMeshProUGUI>();
        placeholderTMP.text = "Player name...";
        placeholderTMP.fontSize = 24;
        placeholderTMP.color = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        placeholderTMP.alignment = TextAlignmentOptions.Left;
        var placeholderRect = placeholderGO.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 10);
        placeholderRect.offsetMax = new Vector2(-10, -10);

        input.placeholder = placeholderTMP;
        input.text = string.Empty;
        input.targetGraphic = image;
        input.interactable = true;

        templateGO.SetActive(false);
        nameFieldPrefab = input;
    }

    private void UpdateNameFieldListFromContainer()
    {
        if (namesContainer == null)
            return;

        nameFields.Clear();
        var existingInputs = namesContainer.GetComponentsInChildren<TMP_InputField>(true);
        foreach (var input in existingInputs)
        {
            if (input == null)
                continue;

            if (input.gameObject.name.Contains("Template"))
            {
                if (nameFieldPrefab == null)
                    nameFieldPrefab = input;
                continue;
            }

            if (!nameFields.Contains(input))
            {
                nameFields.Add(input);
            }
        }

        nameFields.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
    }

    private void CollectExistingNameFields()
    {
        if (namesContainer == null)
            return;

        var existingInputs = namesContainer.GetComponentsInChildren<TMP_InputField>(true);
        foreach (var input in existingInputs)
        {
            if (input == null)
                continue;

            if (input.gameObject.name.Contains("Template") || input.gameObject == nameFieldPrefab?.gameObject)
                continue;

            if (!nameFields.Contains(input))
            {
                nameFields.Add(input);
            }
        }

        if (nameFieldPrefab == null)
        {
            foreach (var input in existingInputs)
            {
                if (input.gameObject.name.Contains("Template"))
                {
                    nameFieldPrefab = input;
                    break;
                }
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("HomeMenuManager: StartGame called. playerCount=" + playerCount + ", gameSceneName='" + gameSceneName + "'");
        GameSettings.PlayerCount = playerCount;
        GameSettings.TotalGameCount = gameCount;
        GameSettings.CurrentGameNumber = 1;

        if (namesContainer != null)
        {
            var allFields = namesContainer.GetComponentsInChildren<TMP_InputField>(true)
                .Where(field => field != null && !field.gameObject.name.Contains("Template"))
                .OrderBy(field => field.transform.GetSiblingIndex())
                .ToArray();

            for (int i = 0; i < playerCount; i++)
            {
                string defaultName = GameSettings.GetPlayerName(i + 1);
                string name = defaultName;
                if (i < allFields.Length && allFields[i] != null)
                {
                    name = string.IsNullOrWhiteSpace(allFields[i].text) ? defaultName : allFields[i].text;
                }
                GameSettings.SetPlayerName(i + 1, name);
                Debug.Log($"HomeMenuManager: Saved player {i + 1} name '{name}'");
            }
        }
        else
        {
            for (int i = 0; i < playerCount; i++)
            {
                string name = GameSettings.GetPlayerName(i + 1);
                GameSettings.SetPlayerName(i + 1, name);
                Debug.Log($"HomeMenuManager: Saved player {i + 1} name '{name}' (fallback)");
            }
        }

        GameSettings.Save();

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("HomeMenuManager: gameSceneName is empty. Set it in the inspector.");
            return;
        }

        // Try to load the scene; log an error if it isn't in Build Settings
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("HomeMenuManager: Scene '" + gameSceneName + "' cannot be loaded. Add it to Build Settings or check the name.");
        }
    }

    private void CreateGameCountUI()
    {
        Transform parent = gameCountAnchor != null ? gameCountAnchor : transform;

        // 既存のPlayers UIと同じ親（Panel）を探す
        if (gameCountAnchor == null)
        {
            var panel = GameObject.Find("Panel");
            if (panel != null) parent = panel.transform;
        }

        // Label
        var labelGO = new GameObject("GamesLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(parent, false);
        var labelTMP = labelGO.GetComponent<TextMeshProUGUI>();
        labelTMP.text = "Games";
        labelTMP.fontSize = 36;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.color = Color.white;
        var labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(300, 60);
        labelRect.anchoredPosition = new Vector2(0, -40); // ★位置は後で調整してください

        // Value
        var valueGO = new GameObject("GamesValue", typeof(RectTransform), typeof(TextMeshProUGUI));
        valueGO.transform.SetParent(parent, false);
        var valueTMP = valueGO.GetComponent<TextMeshProUGUI>();
        valueTMP.text = "1";
        valueTMP.fontSize = 48;
        valueTMP.alignment = TextAlignmentOptions.Center;
        valueTMP.color = Color.white;
        var valueRect = valueGO.GetComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(160, 60);
        valueRect.anchoredPosition = new Vector2(0, -110); // ★位置は後で調整してください

        gameCountTextTMP = valueTMP;

        // Minus Button
        var minusGO = new GameObject("GamesMinusButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        minusGO.transform.SetParent(parent, false);
        minusGO.GetComponent<Image>().color = Color.white;
        var minusRect = minusGO.GetComponent<RectTransform>();
        minusRect.sizeDelta = new Vector2(80, 60);
        minusRect.anchoredPosition = new Vector2(-120, -110); // ★位置は後で調整してください
        var minusBtn = minusGO.GetComponent<Button>();
        minusBtn.targetGraphic = minusGO.GetComponent<Image>();
        minusBtn.onClick.AddListener(DecreaseGameCount);

        var minusLabel = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        minusLabel.transform.SetParent(minusGO.transform, false);
        var minusLabelTMP = minusLabel.GetComponent<TextMeshProUGUI>();
        minusLabelTMP.text = "-";
        minusLabelTMP.fontSize = 32;
        minusLabelTMP.alignment = TextAlignmentOptions.Center;
        minusLabelTMP.color = Color.black;
        var minusLabelRect = minusLabel.GetComponent<RectTransform>();
        minusLabelRect.anchorMin = Vector2.zero;
        minusLabelRect.anchorMax = Vector2.one;
        minusLabelRect.offsetMin = Vector2.zero;
        minusLabelRect.offsetMax = Vector2.zero;

        // Plus Button
        var plusGO = new GameObject("GamesPlusButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        plusGO.transform.SetParent(parent, false);
        plusGO.GetComponent<Image>().color = Color.white;
        var plusRect = plusGO.GetComponent<RectTransform>();
        plusRect.sizeDelta = new Vector2(80, 60);
        plusRect.anchoredPosition = new Vector2(120, -110); // ★位置は後で調整してください
        var plusBtn = plusGO.GetComponent<Button>();
        plusBtn.targetGraphic = plusGO.GetComponent<Image>();
        plusBtn.onClick.AddListener(IncreaseGameCount);

        var plusLabel = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        plusLabel.transform.SetParent(plusGO.transform, false);
        var plusLabelTMP = plusLabel.GetComponent<TextMeshProUGUI>();
        plusLabelTMP.text = "+";
        plusLabelTMP.fontSize = 32;
        plusLabelTMP.alignment = TextAlignmentOptions.Center;
        plusLabelTMP.color = Color.black;
        var plusLabelRect = plusLabel.GetComponent<RectTransform>();
        plusLabelRect.anchorMin = Vector2.zero;
        plusLabelRect.anchorMax = Vector2.one;
        plusLabelRect.offsetMin = Vector2.zero;
        plusLabelRect.offsetMax = Vector2.zero;
    }

    private void CreateCustomizeButton()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var panel = GameObject.Find("Panel");
        Transform parent = panel != null ? panel.transform : canvas.transform;

        var btnGO = new GameObject("CustomizeButton",
            typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        var rect = btnGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(650, 930);
        rect.sizeDelta = new Vector2(200f, 60f);

        customizeButton = btnGO.GetComponent<Button>();
        customizeButton.targetGraphic = btnGO.GetComponent<Image>();
        customizeButton.onClick.AddListener(OnCustomizeButtonClicked);

        // ラベル
        var labelGO = new GameObject("Label",
            typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(btnGO.transform, false);
        customizeButtonLabel = labelGO.GetComponent<TextMeshProUGUI>();
        customizeButtonLabel.fontSize = 22;
        customizeButtonLabel.alignment = TextAlignmentOptions.Center;
        var labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        // 鍵アイコンのラベル
        var lockGO = new GameObject("LockIcon",
            typeof(RectTransform), typeof(TextMeshProUGUI));
        lockGO.transform.SetParent(btnGO.transform, false);
        var lockTMP = lockGO.GetComponent<TextMeshProUGUI>();
        lockTMP.fontSize = 18;
        lockTMP.alignment = TextAlignmentOptions.Center;
        lockTMP.color = Color.yellow;
        var lockRect = lockGO.GetComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0f, 0f);
        lockRect.anchorMax = new Vector2(1f, 0f);
        lockRect.pivot = new Vector2(0.5f, 0f);
        lockRect.anchoredPosition = new Vector2(0f, 2f);
        lockRect.sizeDelta = new Vector2(0f, 20f);

        UpdateCustomizeButton();
    }

    private void UpdateCustomizeButton()
    {
        if (customizeButton == null) return;

        bool isPurchased = PurchaseManager.IsPurchased("ballCustomize");

        if (isPurchased)
        {
            customizeButton.GetComponent<Image>().color = new Color32(40, 100, 60, 220);
            customizeButtonLabel.text = "Customize";
            customizeButtonLabel.color = Color.white;

            // 鍵アイコンを非表示
            var lockIcon = customizeButton.transform.Find("LockIcon");
            if (lockIcon != null)
                lockIcon.GetComponent<TextMeshProUGUI>().text = "";
        }
        else
        {
            customizeButton.GetComponent<Image>().color = new Color32(60, 60, 60, 220);
            customizeButtonLabel.text = "Customize";
            customizeButtonLabel.color = new Color(0.7f, 0.7f, 0.7f);

            // 鍵アイコンを表示
            var lockIcon = customizeButton.transform.Find("LockIcon");
            if (lockIcon != null)
                lockIcon.GetComponent<TextMeshProUGUI>().text = "price";
        }
    }

    private void OnCustomizeButtonClicked()
    {
        if (PurchaseManager.IsPurchased("ballCustomize"))
        {
            // 購入済み → BallCustomへ
            if (Application.CanStreamedLevelBeLoaded(ballCustomSceneName))
                SceneManager.LoadScene(ballCustomSceneName);
            else
                Debug.LogError($"Scene '{ballCustomSceneName}' not found in Build Settings.");
        }
        else
        {
            // 未購入 → 購入画面へ
            if (Application.CanStreamedLevelBeLoaded(purchaseSceneName))
                SceneManager.LoadScene(purchaseSceneName);
            else
                Debug.LogError($"Scene '{purchaseSceneName}' not found in Build Settings.");
        }
    }
}