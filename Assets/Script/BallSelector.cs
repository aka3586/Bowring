using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallSelector : MonoBehaviour
{
    [Header("Ball Data")]
    public BallInfo[] balls;

    private int currentIndex = 0;
    private Renderer ballRenderer;
    private Rigidbody rb;

    private Canvas uiCanvas;
    private GameObject overlayRoot;
    private Button openButton;
    private bool canChangeBall = false;
    private int playerIndex;

    // カスタムボールリスト
    private List<BallInfo> allBalls = new List<BallInfo>();

    void Awake()
    {
        canChangeBall = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (ballRenderer == null)
            ballRenderer = GetComponentInChildren<Renderer>();

        uiCanvas = Object.FindAnyObjectByType<Canvas>();

        BuildBallList();

        LoadPlayerBall();

        CreateOpenButton();
        CreateOverlayUI();
    }

    // ============================
    // ボールリスト構築
    // ============================
    private void BuildBallList()
    {
        allBalls.Clear();

        // 固定ボール
        if (balls != null)
        {
            foreach (var b in balls)
                allBalls.Add(b);
        }

        // カスタムボールをPlayerPrefsから読み込み
        int count = PlayerPrefs.GetInt("CustomBallCount", 0);
        for (int i = 0; i < count; i++)
        {
            string name = PlayerPrefs.GetString($"CustomBall_{i}_Name", "カスタム");
            float mass = PlayerPrefs.GetFloat($"CustomBall_{i}_Mass", 5.0f);
            string texPath = PlayerPrefs.GetString($"CustomBall_{i}_TexPath", "");

            var customBall = new BallInfo();
            customBall.ballName = name;
            customBall.mass = mass;

            // テクスチャからマテリアルを生成
            if (!string.IsNullOrEmpty(texPath) &&
                System.IO.File.Exists(texPath))
            {
                byte[] pngData = System.IO.File.ReadAllBytes(texPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(pngData);

                Material mat = new Material(Shader.Find("Standard"));
                mat.mainTexture = tex;
                customBall.material = mat;
            }
            else
            {
                // テクスチャがなければデフォルトマテリアル
                customBall.material = new Material(Shader.Find("Standard"));
                customBall.material.color = Color.white;
            }

            allBalls.Add(customBall);
        }
    }

    // ============================
    // ★GameManagerから呼ぶ
    // ============================
    public void EnableBallChange()
    {
        if (uiCanvas == null)
            uiCanvas = FindAnyObjectByType<Canvas>();

        BuildBallList();

        CreateOverlayUI();

        canChangeBall = true;

        if (openButton != null)
            openButton.gameObject.SetActive(true);
    }
    public void DisableBallChange()
    {
        canChangeBall = false;
        if (openButton != null)
            openButton.gameObject.SetActive(false);
        HideOverlay();
    }

    private void ApplyBall()
    {
        if (allBalls == null || allBalls.Count == 0) return;
        if (currentIndex >= allBalls.Count) currentIndex = 0;

        var ball = allBalls[currentIndex];
        if (ball.material != null)
            ballRenderer.material = ball.material;
        rb.mass = ball.mass;
        Debug.Log($"ボール変更: {ball.ballName} / mass:{ball.mass}");
    }

    private void CreateOpenButton()
    {
        var btnObj = new GameObject("BallSelectButton",
            typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(Button));
        btnObj.transform.SetParent(uiCanvas.transform, false);
        var rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-120f, -100f);
        rect.sizeDelta = new Vector2(140f, 46f);

        btnObj.GetComponent<Image>().color = new Color32(40, 56, 80, 220);
        openButton = btnObj.GetComponent<Button>();
        openButton.targetGraphic = btnObj.GetComponent<Image>();
        openButton.onClick.AddListener(ShowOverlay);

        var label = CreateText("Label", "Ball Change", 16, btnObj.transform);
        label.color = Color.white;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;

        btnObj.SetActive(false);
        openButton = btnObj.GetComponent<Button>();
    }

    private void CreateOverlayUI()
    {
        if (uiCanvas == null)
        {
            uiCanvas = FindAnyObjectByType<Canvas>();

            if (uiCanvas == null)
            {
                Debug.LogError("Canvasが見つかりません");
                return;
            }
        }
        if (overlayRoot != null)
            Destroy(overlayRoot);

        overlayRoot = new GameObject("BallSelectOverlay",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayRoot.transform.SetParent(uiCanvas.transform, false);
        var overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRoot.GetComponent<Image>().color = new Color32(0, 0, 0, 180);
        overlayRoot.SetActive(false);

        // パネル
        var panel = new GameObject("Panel",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(overlayRoot.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.1f);
        panelRect.anchorMax = new Vector2(0.8f, 0.95f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color32(24, 34, 52, 240);

        // タイトル
        var title = CreateText("Title", "Ball Selector", 24, panel.transform);
        title.color = Color.white;
        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(0f, 40f);

        // 閉じるボタン
        var closeObj = new GameObject("CloseButton",
            typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(Button));
        closeObj.transform.SetParent(panel.transform, false);
        var closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-12f, -12f);
        closeRect.sizeDelta = new Vector2(80f, 36f);
        closeObj.GetComponent<Image>().color = new Color32(80, 40, 40, 220);
        var closeBtn = closeObj.GetComponent<Button>();
        closeBtn.targetGraphic = closeObj.GetComponent<Image>();
        closeBtn.onClick.AddListener(HideOverlay);
        var closeLabel = CreateText("CloseLabel", "Close", 14, closeObj.transform);
        closeLabel.color = Color.white;
        closeLabel.rectTransform.anchorMin = Vector2.zero;
        closeLabel.rectTransform.anchorMax = Vector2.one;
        closeLabel.rectTransform.offsetMin = Vector2.zero;
        closeLabel.rectTransform.offsetMax = Vector2.zero;

        // ★スクロールエリア（パネルの内側、タイトルの下）
        var scrollArea = new GameObject("ScrollArea",
            typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollArea.transform.SetParent(panel.transform, false);
        var scrollAreaRect = scrollArea.GetComponent<RectTransform>();
        scrollAreaRect.anchorMin = new Vector2(0f, 0f);
        scrollAreaRect.anchorMax = new Vector2(1f, 1f);
        scrollAreaRect.offsetMin = new Vector2(8f, 8f);
        scrollAreaRect.offsetMax = new Vector2(-8f, -70f);
        scrollArea.GetComponent<Image>().color = new Color(0, 0, 0, 0);

        var sr = scrollArea.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.scrollSensitivity = 30f;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.inertia = true;
        sr.decelerationRate = 0.135f;

        // ★Viewport（ScrollAreaと同じサイズ、Maskで切り取り）
        var viewport = new GameObject("Viewport",
            typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollArea.transform, false);
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        sr.viewport = viewportRect;

        // ★Content（縦に伸びるリスト）
        var content = new GameObject("Content",
            typeof(RectTransform), typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(6, 6, 6, 6);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        var csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        sr.content = contentRect;

        // ボールリスト生成
        for (int i = 0; i < allBalls.Count; i++)
        {
            int index = i;
            var ball = allBalls[i];

            var itemObj = new GameObject($"BallItem_{i}",
                typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button), typeof(LayoutElement));
            itemObj.transform.SetParent(content.transform, false);

            var le = itemObj.GetComponent<LayoutElement>();
            le.preferredHeight = 64f;
            le.minHeight = 64f;

            var itemImage = itemObj.GetComponent<Image>();
            itemImage.color = (i == currentIndex)
                ? new Color32(60, 80, 110, 240)
                : new Color32(35, 48, 68, 220);

            var itemBtn = itemObj.GetComponent<Button>();
            itemBtn.targetGraphic = itemImage;
            itemBtn.onClick.AddListener(() =>
            {
                currentIndex = index;

                PlayerPrefs.SetInt(
                    "PlayerBall_" + playerIndex,
                    currentIndex
                );

                PlayerPrefs.Save();

                ApplyBall();

                HideOverlay();
                CreateOverlayUI();
            });

            // アイコン
            var iconObj = new GameObject("Icon",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(itemObj.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(12f, 0f);
            iconRect.sizeDelta = new Vector2(44f, 44f);
            var iconColor = ball.material != null
                ? ball.material.color : Color.white;
            iconObj.GetComponent<Image>().color = iconColor;

            // ラベル
            var label = CreateText("Label",
                $"{ball.ballName}  ({ball.mass}kg)", 18, itemObj.transform);
            label.color = Color.white;
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(66f, 0f);
            labelRect.offsetMax = Vector2.zero;

            // 選択中マーク
            if (i == currentIndex)
            {
                var check = CreateText("Check", "✓", 20, itemObj.transform);
                check.color = new Color(0.4f, 1f, 0.4f);
                var checkRect = check.rectTransform;
                checkRect.anchorMin = new Vector2(1f, 0.5f);
                checkRect.anchorMax = new Vector2(1f, 0.5f);
                checkRect.pivot = new Vector2(1f, 0.5f);
                checkRect.anchoredPosition = new Vector2(-12f, 0f);
                checkRect.sizeDelta = new Vector2(40f, 40f);
            }
        }
    }
    private void ShowOverlay()
    {
        if (!canChangeBall) return;
        overlayRoot.SetActive(true);
    }

    private void HideOverlay()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    private TextMeshProUGUI CreateText(string name, string content,
        int fontSize, Transform parent)
    {
        var go = new GameObject(name,
            typeof(RectTransform), typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        return tmp;
    }

    public void SetPlayerIndex(int index)
    {
        playerIndex = index;

        currentIndex = PlayerPrefs.GetInt(
            "PlayerBall_" + playerIndex,
            0
        );
    }

    public void LoadPlayerBall()
    {
        currentIndex = PlayerPrefs.GetInt(
            "PlayerBall_" + playerIndex,
            0
        );

        ApplyBall();
    }

    public void ApplyCurrentBall()
    {
        ApplyBall();
    }

}