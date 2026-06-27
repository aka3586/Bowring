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

    [Header("Settings")]
    [Tooltip("Name of the scene to load for the bowling game")]
    public string gameSceneName = "SampleScene";

    private int playerCount = 1;
    private const int minPlayers = 1;
    private const int maxPlayers = 4;

    private readonly System.Collections.Generic.List<TMP_InputField> nameFields = new System.Collections.Generic.List<TMP_InputField>();

    void Start()
    {
        GameSettings.ClearPlayerData();
        playerCount = 1;
        UpdatePlayerCountDisplay();
        RebuildNameFields();
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
}
