using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIManager : MonoBehaviour
{
    [Header("Frame Cell Prefab")]
    [SerializeField] private GameObject frameCellPrefab;

    [Header("Parents")]
    [SerializeField] private Transform currentFrameParent;
    [SerializeField] private Transform finalBoardParent;

    [Header("Player Display")]
    [SerializeField] private TextMeshProUGUI playerNameText;

    private FrameUI currentFrameUI;
    private List<FrameUI> finalFrameUIs = new();
    private string currentPlayerName = string.Empty;

    private Canvas uiCanvas;
    private GameObject scoreOverlayRoot;
    private GameObject scoreContentRoot;
    private Button showScoreButton;
    private Button closePanelButton;
    private BallSelector ballSelector;
    private FrameManager frameManager;
    private Button homeButton;
    public string homeSceneName = "Home"; // ← Inspectorで設定可能
    private Button nextGameButton;
    private GameManager gameManager;
    void Awake()
    {
        if (playerNameText == null)
        {
            playerNameText = FindPlayerNameText();
            if (playerNameText != null)
                Debug.Log("ScoreUIManager: PlayerNameText auto-assigned from scene.");
        }
    }

    void Start()
    {
        frameManager = Object.FindAnyObjectByType<FrameManager>();
        gameManager = Object.FindAnyObjectByType<GameManager>();
        uiCanvas = GetComponentInParent<Canvas>() ?? Object.FindAnyObjectByType<Canvas>();
        ballSelector = Object.FindAnyObjectByType<BallSelector>();
        CreateScoreOverlayUI();
        CreateShowScoreButton();
    }

    private TextMeshProUGUI FindPlayerNameText()
    {
        var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in allTexts)
        {
            if (tmp.gameObject.name.IndexOf("PlayerName", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return tmp;
        }

        foreach (var tmp in allTexts)
        {
            if (tmp.gameObject.name.IndexOf("player", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                tmp.gameObject.name.IndexOf("name", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return tmp;
        }

        if (allTexts.Length > 0)
            return allTexts[0];

        var allRootTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var tmp in allRootTexts)
        {
            if (tmp.gameObject.scene.isLoaded && tmp.gameObject.name.IndexOf("PlayerName", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return tmp;
        }

        foreach (var tmp in allRootTexts)
        {
            if (tmp.gameObject.scene.isLoaded &&
                (tmp.gameObject.name.IndexOf("player", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                 tmp.gameObject.name.IndexOf("name", System.StringComparison.OrdinalIgnoreCase) >= 0))
                return tmp;
        }

        foreach (var tmp in allRootTexts)
        {
            if (tmp.gameObject.scene.isLoaded)
                return tmp;
        }

        return null;
    }

    private TextMeshProUGUI CreateFallbackPlayerNameText()
    {
        Transform parent = currentFrameParent != null ? currentFrameParent : transform;
        var canvas = GetComponentInParent<Canvas>() ?? Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
            parent = canvas.transform;

        GameObject go = new GameObject("PlayerNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = "Player 1";
        tmp.fontSize = 40;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -30f);
        rect.sizeDelta = new Vector2(300f, 50f);

        return tmp;
    }

    private void CreateShowScoreButton()
    {
        if (uiCanvas == null)
            return;

        GameObject buttonRoot = new GameObject("ShowScoreButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonRoot.transform.SetParent(uiCanvas.transform, false);
        var buttonRect = buttonRoot.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-120f, -50f);
        buttonRect.sizeDelta = new Vector2(140f, 46f);

        var buttonImage = buttonRoot.GetComponent<Image>();
        buttonImage.color = new Color32(40, 56, 80, 220);
        buttonImage.raycastTarget = true;

        showScoreButton = buttonRoot.GetComponent<Button>();
        showScoreButton.targetGraphic = buttonImage;
        showScoreButton.onClick.AddListener(ShowScorePanel);

        var label = CreateText("ShowScoreLabel", "Show Scores", 18, TextAlignmentOptions.Center, null, buttonRoot.transform);
        label.color = Color.white;
        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    private void CreateScoreOverlayUI()
    {
        if (uiCanvas == null)
            return;

        scoreOverlayRoot = new GameObject("ScoreOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        scoreOverlayRoot.transform.SetParent(uiCanvas.transform, false);
        var overlayRect = scoreOverlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var overlayImage = scoreOverlayRoot.GetComponent<Image>();
        overlayImage.color = new Color32(0, 0, 0, 180);
        scoreOverlayRoot.SetActive(false);

        var panel = new GameObject("ScorePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(scoreOverlayRoot.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.08f, 0.08f);
        panelRect.anchorMax = new Vector2(0.92f, 0.92f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color32(24, 34, 52, 240);

        var title = CreateText("ScorePanelTitle", "Score History", 28, TextAlignmentOptions.Center, null, panel.transform);
        title.color = Color.white;
        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -24f);
        titleRect.sizeDelta = new Vector2(420f, 42f);

        var closeRoot = new GameObject("CloseScorePanelButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeRoot.transform.SetParent(panel.transform, false);
        var closeRect = closeRoot.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-16f, -16f);
        closeRect.sizeDelta = new Vector2(120f, 36f);

        var closeImage = closeRoot.GetComponent<Image>();
        closeImage.color = new Color32(62, 76, 97, 220);
        closeImage.raycastTarget = true;

        closePanelButton = closeRoot.GetComponent<Button>();
        closePanelButton.targetGraphic = closeImage;
        closePanelButton.onClick.AddListener(HideScorePanel);

        var closeLabel = CreateText("CloseLabel", "Back to Game", 16, TextAlignmentOptions.Center, null, closeRoot.transform);
        closeLabel.color = Color.white;
        var closeLabelRect = closeLabel.rectTransform;
        closeLabelRect.anchorMin = Vector2.zero;
        closeLabelRect.anchorMax = Vector2.one;
        closeLabelRect.offsetMin = Vector2.zero;
        closeLabelRect.offsetMax = Vector2.zero;
        // ホームに戻るボタン
        var homeRoot = new GameObject("HomeButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        homeRoot.transform.SetParent(panel.transform, false);
        var homeRect = homeRoot.GetComponent<RectTransform>();
        homeRect.anchorMin = new Vector2(0.5f, 0f);
        homeRect.anchorMax = new Vector2(0.5f, 0f);
        homeRect.pivot = new Vector2(0.5f, 0f);
        homeRect.anchoredPosition = new Vector2(0f, 16f);
        homeRect.sizeDelta = new Vector2(180f, 46f);
        homeRoot.GetComponent<Image>().color = new Color32(80, 40, 40, 220);
        homeRoot.SetActive(true); // 最初は非表示

        homeButton = homeRoot.GetComponent<Button>();
        homeButton.targetGraphic = homeRoot.GetComponent<Image>();
        homeButton.onClick.AddListener(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(homeSceneName);
        });

        var homeLabel = CreateText("HomeLabel", "Back to Home", 18, TextAlignmentOptions.Center, null, homeRoot.transform);
        homeLabel.color = Color.white;
        homeLabel.rectTransform.anchorMin = Vector2.zero;
        homeLabel.rectTransform.anchorMax = Vector2.one;
        homeLabel.rectTransform.offsetMin = Vector2.zero;
        homeLabel.rectTransform.offsetMax = Vector2.zero;

        var nextGameRoot = new GameObject("NextGameButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        nextGameRoot.transform.SetParent(panel.transform, false);
        var nextGameRect = nextGameRoot.GetComponent<RectTransform>();
        nextGameRect.anchorMin = new Vector2(0.5f, 0f);
        nextGameRect.anchorMax = new Vector2(0.5f, 0f);
        nextGameRect.pivot = new Vector2(0.5f, 0f);
        nextGameRect.anchoredPosition = new Vector2(200f, 16f); // ホームボタンの右側
        nextGameRect.sizeDelta = new Vector2(200f, 46f);
        nextGameRoot.GetComponent<Image>().color = new Color32(40, 90, 60, 220);
        nextGameRoot.SetActive(false); // 通常は非表示

        nextGameButton = nextGameRoot.GetComponent<Button>();
        nextGameButton.targetGraphic = nextGameRoot.GetComponent<Image>();
        nextGameButton.onClick.AddListener(() => {
            if (gameManager != null)
                gameManager.StartNextGame();
            HideScorePanel();
            if (homeButton != null) homeButton.gameObject.SetActive(false);
            if (nextGameButton != null) nextGameButton.gameObject.SetActive(false);
        });

        var nextGameLabel = CreateText("NextGameLabel", "Next Game", 18, TextAlignmentOptions.Center, null, nextGameRoot.transform);
        nextGameLabel.color = Color.white;
        nextGameLabel.rectTransform.anchorMin = Vector2.zero;
        nextGameLabel.rectTransform.anchorMax = Vector2.one;
        nextGameLabel.rectTransform.offsetMin = Vector2.zero;
        nextGameLabel.rectTransform.offsetMax = Vector2.zero;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(panel.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0.1f);
        contentRect.anchorMax = new Vector2(1f, 0.85f);
        contentRect.offsetMin = new Vector2(16f, 0f);
        contentRect.offsetMax = new Vector2(-16f, 0f);

        var contentLayout = content.GetComponent<VerticalLayoutGroup>();
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.spacing = 12f;
        contentLayout.padding = new RectOffset(8, 8, 8, 8);
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = false;
        contentLayout.childControlWidth = false;
        contentLayout.childControlHeight = false;

        var contentFitter = content.GetComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scoreContentRoot = content;
    }

    public void ShowScorePanel()
    {
        if (scoreOverlayRoot == null) return;
        if (showScoreButton != null) showScoreButton.gameObject.SetActive(false);
        if (ballSelector != null) ballSelector.DisableBallChange();
        scoreOverlayRoot.SetActive(true);
        RefreshScorePanel();
    }

    public void HideScorePanel()
    {
        if (scoreOverlayRoot == null)
            return;
        if (showScoreButton != null) showScoreButton.gameObject.SetActive(true);
        if (ballSelector != null) ballSelector.EnableBallChange();
        scoreOverlayRoot.SetActive(false);
    }
    public void ShowGameEndPanel(bool hasNextGame)
    {
        if (scoreOverlayRoot == null) return;
        if (showScoreButton != null) showScoreButton.gameObject.SetActive(false);

        if (hasNextGame)
        {
            if (nextGameButton != null) nextGameButton.gameObject.SetActive(true);
            if (homeButton != null) homeButton.gameObject.SetActive(false);
        }
        else
        {
            if (nextGameButton != null) nextGameButton.gameObject.SetActive(false);
            if (homeButton != null) homeButton.gameObject.SetActive(true);
        }

        scoreOverlayRoot.SetActive(true);
        RefreshScorePanel();
    }

    public void RefreshScorePanel()
    {
        if (scoreContentRoot == null) return;

        foreach (Transform child in scoreContentRoot.transform)
            Object.Destroy(child.gameObject);

        if (frameManager == null || frameManager.players == null) return;

        for (int playerIndex = 0; playerIndex < frameManager.players.Count; playerIndex++)
        {
            var player = frameManager.players[playerIndex];

            // プレイヤーブロック
            var block = new GameObject($"PlayerScore_{playerIndex + 1}", typeof(RectTransform), typeof(LayoutElement));
            block.transform.SetParent(scoreContentRoot.transform, false);
            var blockLE = block.GetComponent<LayoutElement>();
            blockLE.preferredWidth = 740f;
            blockLE.preferredHeight = 80f;
            blockLE.minWidth = 740f;    // ★ 追加
            blockLE.minHeight = 100f;   // ★ 追加

            // プレイヤー名ヘッダー
            int cumulativeTotal = player.cumulativeScore + player.totalScore;
            var header = CreateText($"PlayerHeader_{playerIndex + 1}",
                $"{player.playerName}  Game Total: {player.totalScore}  Total: {cumulativeTotal}", 18,
                TextAlignmentOptions.MidlineLeft, null, block.transform);
            header.color = Color.white;
            var headerRect = header.rectTransform;
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0f, 1f);
            headerRect.anchoredPosition = new Vector2(8f, 0f);
            headerRect.sizeDelta = new Vector2(-8f, 24f);

            // 10フレーム分を横に並べる
            for (int frameIndex = 0; frameIndex < 10; frameIndex++)
            {
                string throw1 = "-";
                string throw2 = "-";
                string throw3 = "-";
                string score = "-";

                if (player.frames != null && frameIndex < player.frames.Count)
                {
                    var frame = player.frames[frameIndex];
                    throw1 = frame.firstThrow >= 0 ? frame.firstThrow.ToString() : "-";
                    throw2 = frame.secondThrow >= 0 ? frame.secondThrow.ToString() : "-";
                    throw3 = frame.thirdThrow >= 0 ? frame.thirdThrow.ToString() : "-";
                    score = frame.isScoreFixed ? frame.frameScore.ToString() : "-";
                    if (frame.isStrike) { throw1 = "X"; throw2 = "-"; }
                    if (frame.isSpare) throw2 = "/";
                }

                float cellWidth = frameIndex == 9 ? 80f : 64f;
                float cellX = 8f + frameIndex * 66f;

                // フレームセル背景
                var cell = new GameObject($"Frame_{frameIndex + 1}", typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(block.transform, false);
                var cellImage = cell.GetComponent<Image>();
                cellImage.color = new Color32(40, 55, 75, 180);
                var cellRect = cell.GetComponent<RectTransform>();
                cellRect.anchorMin = new Vector2(0f, 0f);
                cellRect.anchorMax = new Vector2(0f, 0f);
                cellRect.pivot = new Vector2(0f, 0f);
                cellRect.anchoredPosition = new Vector2(cellX, 4f);
                cellRect.sizeDelta = new Vector2(cellWidth, 70f);

                // フレーム番号
                var numText = CreateText("Num", (frameIndex + 1).ToString(), 10,
                    TextAlignmentOptions.Center, null, cell.transform);
                numText.color = new Color(0.6f, 0.6f, 0.6f);
                var numRect = numText.rectTransform;
                numRect.anchorMin = new Vector2(0f, 1f);
                numRect.anchorMax = new Vector2(1f, 1f);
                numRect.pivot = new Vector2(0.5f, 1f);
                numRect.anchoredPosition = new Vector2(0f, -2f);
                numRect.sizeDelta = new Vector2(0f, 14f);

                // 投球（1投目）
                var t1Text = CreateText("T1", throw1, 13,
                    TextAlignmentOptions.Center, null, cell.transform);
                t1Text.color = Color.white;
                var t1Rect = t1Text.rectTransform;
                t1Rect.anchorMin = new Vector2(0f, 1f);
                t1Rect.anchorMax = new Vector2(0.5f, 1f);
                t1Rect.pivot = new Vector2(0.5f, 1f);
                t1Rect.anchoredPosition = new Vector2(0f, -18f);
                t1Rect.sizeDelta = new Vector2(0f, 20f);

                // 投球（2投目）
                var t2Text = CreateText("T2", throw2, 13,
                    TextAlignmentOptions.Center, null, cell.transform);
                t2Text.color = Color.white;
                var t2Rect = t2Text.rectTransform;
                t2Rect.anchorMin = new Vector2(0.5f, 1f);
                t2Rect.anchorMax = new Vector2(1f, 1f);
                t2Rect.pivot = new Vector2(0.5f, 1f);
                t2Rect.anchoredPosition = new Vector2(0f, -18f);
                t2Rect.sizeDelta = new Vector2(0f, 20f);

                // 10フレームのみ3投目
                if (frameIndex == 9)
                {
                    t1Rect.anchorMin = new Vector2(0f, 1f);
                    t1Rect.anchorMax = new Vector2(0.33f, 1f);
                    t2Rect.anchorMin = new Vector2(0.33f, 1f);
                    t2Rect.anchorMax = new Vector2(0.66f, 1f);

                    var t3Text = CreateText("T3", throw3, 13,
                        TextAlignmentOptions.Center, null, cell.transform);
                    t3Text.color = Color.white;
                    var t3Rect = t3Text.rectTransform;
                    t3Rect.anchorMin = new Vector2(0.66f, 1f);
                    t3Rect.anchorMax = new Vector2(1f, 1f);
                    t3Rect.pivot = new Vector2(0.5f, 1f);
                    t3Rect.anchoredPosition = new Vector2(0f, -18f);
                    t3Rect.sizeDelta = new Vector2(0f, 20f);
                }

                // スコア
                var scoreText = CreateText("Score", score, 15,
                    TextAlignmentOptions.Center, null, cell.transform);
                scoreText.color = Color.yellow;
                var scoreRect = scoreText.rectTransform;
                scoreRect.anchorMin = new Vector2(0f, 0f);
                scoreRect.anchorMax = new Vector2(1f, 0f);
                scoreRect.pivot = new Vector2(0.5f, 0f);
                scoreRect.anchoredPosition = new Vector2(0f, 6f);
                scoreRect.sizeDelta = new Vector2(0f, 22f);
            }
        }
    }

    private TextMeshProUGUI CreateText(string name, string content, int fontSize, TextAlignmentOptions alignment, TMP_FontAsset font, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        if (font != null)
            tmp.font = font;
        return tmp;
    }

    public void CreateFrames()
    {
        // ★最初はFinalBoardを非表示
        finalBoardParent.gameObject.SetActive(false);

        // Current用（1個だけ）
        GameObject currentObj = Instantiate(frameCellPrefab, currentFrameParent);
        currentFrameUI = currentObj.GetComponent<FrameUI>();
        if (currentFrameUI != null && !string.IsNullOrEmpty(currentPlayerName))
        {
            currentFrameUI.SetPlayerName(currentPlayerName);
        }

        // Final用（10個）
        for (int i = 0; i < 10; i++)
        {
            GameObject obj = Instantiate(frameCellPrefab, finalBoardParent);
            FrameUI ui = obj.GetComponent<FrameUI>();

            ui.SetFrameIndex(i);
            ui.Clear();

            finalFrameUIs.Add(ui);
        }
    }

    public void SetPlayerName(string playerName)
    {
        currentPlayerName = playerName ?? string.Empty;

        if (playerNameText == null)
        {
            playerNameText = FindPlayerNameText();
            if (playerNameText == null)
            {
                playerNameText = CreateFallbackPlayerNameText();
                Debug.Log("ScoreUIManager: Created fallback PlayerNameText.");
            }
        }

        if (playerNameText == null)
        {
            Debug.LogWarning("ScoreUIManager: Cannot set player name because PlayerNameText is missing.");
        }
        else
        {
            playerNameText.text = playerName;
        }

        if (currentFrameUI != null)
        {
            currentFrameUI.SetPlayerName(playerName);
        }
    }

    public void UpdateCurrentFrameUI(int frameIndex, FrameData data, int totalScore)
    {
        if (currentFrameUI == null) return;

        currentFrameUI.SetFrameIndex(frameIndex);
        currentFrameUI.SetData(data);

        currentFrameUI.SetTotalScore(totalScore);
    }

    public void UpdateFinalFrameUI(int frameIndex, FrameData data)
    {
        if (frameIndex < 0 || frameIndex >= finalFrameUIs.Count) return;

        finalFrameUIs[frameIndex].SetData(data);
    }

    public void ShowFinalBoard()
    {
        finalBoardParent.gameObject.SetActive(true);
    }
}