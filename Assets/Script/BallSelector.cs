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

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (ballRenderer == null)
            ballRenderer = GetComponentInChildren<Renderer>();

        uiCanvas = Object.FindAnyObjectByType<Canvas>();

        currentIndex = 0;
        ApplyBall();

        CreateOpenButton();
        CreateOverlayUI();

        // ★SetActive(false)を削除、代わりにAwakeで初期化
    }

    void Awake()
    {
        canChangeBall = false;
    }        // ★GameManagerから呼ぶ
    public void EnableBallChange()
    {
        canChangeBall = true;
        if (openButton != null) // ★nullチェック追加
            openButton.gameObject.SetActive(true);
    }
    public void DisableBallChange()
    {
        canChangeBall = false;
        if (openButton != null) // ★nullチェック追加
            openButton.gameObject.SetActive(false);
        HideOverlay();
    }
    private void ApplyBall()
    {
        if (balls == null || balls.Length == 0) return;
        ballRenderer.material = balls[currentIndex].material;
        rb.mass = balls[currentIndex].mass;
        Debug.Log($"ボール変更: {balls[currentIndex].ballName} / mass:{balls[currentIndex].mass}");
    }

    private void CreateOpenButton()
    {
        var btnObj = new GameObject("BallSelectButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
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

        btnObj.SetActive(true);
    }

    private void CreateOverlayUI()
    {
        overlayRoot = new GameObject("BallSelectOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayRoot.transform.SetParent(uiCanvas.transform, false);
        var overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRoot.GetComponent<Image>().color = new Color32(0, 0, 0, 180);
        overlayRoot.SetActive(false);

        // パネル
        var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(overlayRoot.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.15f);
        panelRect.anchorMax = new Vector2(0.8f, 0.9f);
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
        var closeObj = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
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

        // ボールリスト
        if (balls == null) return;
        for (int i = 0; i < balls.Length; i++)
        {
            int index = i;
            var ball = balls[i];

            var itemObj = new GameObject($"BallItem_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            itemObj.transform.SetParent(panel.transform, false);
            var itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0.05f, 1f);
            itemRect.anchorMax = new Vector2(0.95f, 1f);
            itemRect.pivot = new Vector2(0.5f, 1f);
            itemRect.anchoredPosition = new Vector2(0f, -80f - i * 70f);
            itemRect.sizeDelta = new Vector2(0f, 60f);

            var itemImage = itemObj.GetComponent<Image>();
            itemImage.color = (i == currentIndex)
                ? new Color32(60, 80, 110, 240)
                : new Color32(35, 48, 68, 220);

            var itemBtn = itemObj.GetComponent<Button>();
            itemBtn.targetGraphic = itemImage;
            itemBtn.onClick.AddListener(() =>
            {
                currentIndex = index;
                ApplyBall();
                HideOverlay();
                // 選択中マーク更新のため再生成
                if (overlayRoot != null) Destroy(overlayRoot);
                CreateOverlayUI();
            });

            // マテリアルのカラーをアイコンに表示
            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(itemObj.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(12f, 0f);
            iconRect.sizeDelta = new Vector2(44f, 44f);
            // マテリアルの色をアイコンに反映
            var iconColor = ball.material != null ? ball.material.color : Color.white;
            iconObj.GetComponent<Image>().color = iconColor;

            // ボール名と重さ
            var label = CreateText("Label", $"{ball.ballName}  ({ball.mass}kg)", 18, itemObj.transform);
            label.color = Color.white;
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(66f, 0f);
            labelRect.offsetMax = Vector2.zero;

            // 選択中マーク
            if (i == currentIndex)
            {
                var check = CreateText("Check", "Check", 14, itemObj.transform);
                check.color = new Color(0.4f, 1f, 0.4f);
                var checkRect = check.rectTransform;
                checkRect.anchorMin = new Vector2(1f, 0.5f);
                checkRect.anchorMax = new Vector2(1f, 0.5f);
                checkRect.pivot = new Vector2(1f, 0.5f);
                checkRect.anchoredPosition = new Vector2(-12f, 0f);
                checkRect.sizeDelta = new Vector2(80f, 30f);
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

    private TextMeshProUGUI CreateText(string name, string content, int fontSize, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        return tmp;
    }
}