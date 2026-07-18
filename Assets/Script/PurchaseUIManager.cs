using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PurchaseUIManager : MonoBehaviour
{
    [Header("Scene")]
    public string homeSceneName = "Home";

    [Header("Price")]
    public int price = 500;

    private TextMeshProUGUI statusText;

    void Start()
    {
        PurchaseManager.Load();
        SetupUI();
    }

    void SetupUI()
    {
        var canvasGO = new GameObject("Canvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 背景パネル
        var panel = new GameObject("Panel",
            typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.3f, 0.2f);
        panelRect.anchorMax = new Vector2(0.7f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color32(24, 34, 52, 240);

        // タイトル
        CreateText("Title", "Ball Customize", 36,
            new Vector2(0.5f, 1f), new Vector2(0f, -40f),
            new Vector2(400f, 60f), panel.transform);

        // 説明
        CreateText("Desc",
            "Unlock the ball customization feature\nYou can create an original ball",
            22, new Vector2(0.5f, 1f), new Vector2(0f, -130f),
            new Vector2(400f, 80f), panel.transform);

        // 価格
        var priceText = CreateText("Price", $"¥{price}",
            48, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f),
            new Vector2(300f, 80f), panel.transform);
        priceText.color = Color.yellow;

        // ステータステキスト
        statusText = CreateText("Status", "",
            20, new Vector2(0.5f, 0.5f), new Vector2(0f, -20f),
            new Vector2(400f, 40f), panel.transform);
        statusText.color = Color.green;

        // 購入ボタン
        var buyBtn = CreateButton("push",
            new Vector2(0f, -100f), new Vector2(240f, 60f),
            panel.transform, new Color32(40, 120, 60, 255));
        buyBtn.GetComponent<Button>().onClick.AddListener(OnPurchase);

        // 戻るボタン
        var backBtn = CreateButton("back",
            new Vector2(0f, -180f), new Vector2(200f, 50f),
            panel.transform, new Color32(80, 40, 40, 220));
        backBtn.GetComponent<Button>().onClick.AddListener(
            () => SceneManager.LoadScene(homeSceneName));

        // 既に購入済みの場合
        if (PurchaseManager.IsPurchased("ballCustomize"))
        {
            statusText.text = "✓ I've already bought it";
            statusText.color = Color.green;
            buyBtn.GetComponent<Button>().interactable = false;
            buyBtn.GetComponent<Image>().color = new Color32(60, 60, 60, 200);
        }
    }

    void OnPurchase()
    {
        // デモ用：即時購入処理
        PurchaseManager.Purchase("ballCustomize");

        statusText.text = "✓ Purchase complete！";
        statusText.color = Color.green;

        Debug.Log("Purchase complete: ballCustomize");

        // 少し待ってからホームへ
        Invoke(nameof(GoHome), 1.5f);
    }

    void GoHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }

    TextMeshProUGUI CreateText(string name, string content,
        int fontSize, Vector2 anchor, Vector2 pos,
        Vector2 size, Transform parent)
    {
        var go = new GameObject(name,
            typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return tmp;
    }

    GameObject CreateButton(string label, Vector2 pos,
        Vector2 size, Transform parent, Color32 color)
    {
        var go = new GameObject($"Btn_{label}",
            typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();

        var txtGO = new GameObject("Text",
            typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(go.transform, false);
        var txt = txtGO.GetComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 22;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return go;
    }
}