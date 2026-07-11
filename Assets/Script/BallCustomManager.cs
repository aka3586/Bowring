using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class BallCustomManager : MonoBehaviour
{
    [Header("Ball")]
    public GameObject ballPrefab;

    [Header("Paint Settings")]
    public int textureSize = 512;

    [Header("Price Settings")]
    [Tooltip("customball")]
    public int ballPrice = 0;

    [Header("Scene")]
    public string homeSceneName = "Home";

    private GameObject ballInstance;
    private Renderer ballRenderer;
    private Texture2D paintTexture;
    private Material paintMaterial;

    private Color selectedColor = Color.white;
    private float brushSize = 10f;

    private Camera mainCamera;
    private bool isPainting = false;

    // UI
    private Canvas uiCanvas;
    private GameObject colorPalette;
    private Slider brushSizeSlider;
    private TMP_Text brushSizeLabel;
    private TMP_Text priceText;
    private TMP_InputField ballNameInput;
    private TMP_InputField massInput;

    [SerializeField]
    float zoomSpeed = 5f;

    [SerializeField]
    float minDistance = 30f;

    [SerializeField]
    float maxDistance = 9f;

    Vector3 cameraOffset;

    void Start()
    {
        mainCamera = Camera.main;
        cameraOffset = mainCamera.transform.position;
        SetupUI();
        SpawnBall();
    }

    void Update()
    {
        HandleBallRotation();
        HandlePainting();
        HandleZoom();
    }

    // ============================
    // ボール生成
    // ============================
    void SpawnBall()
    {
        // ballPrefabがなければ球体で代用
        if (ballPrefab != null)
        {
            ballInstance = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            ballInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballInstance.transform.position = Vector3.zero;
        }

        // ペイント用テクスチャを生成
        paintTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        // 白で初期化
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        paintTexture.SetPixels(pixels);
        paintTexture.Apply();

        // マテリアル設定
        ballRenderer = ballInstance.GetComponentInChildren<Renderer>();
        paintMaterial = new Material(Shader.Find("Standard"));
        paintMaterial.mainTexture = paintTexture;
        ballRenderer.material = paintMaterial;
    }

    // ============================
    // ボール回転（右クリックドラッグ）
    // ============================
    private Vector3 lastMousePos;
    public float rotationSensitivity = 0.5f;

    void HandleBallRotation()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            lastMousePos = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 currentPos = Mouse.current.position.ReadValue();
            Vector2 delta = currentPos - (Vector2)lastMousePos;

            // ★ボールのメッシュ中心を計算して軸にする
            Vector3 ballCenter = ballRenderer.bounds.center;

            ballInstance.transform.RotateAround(
                ballCenter,
                mainCamera.transform.up,
                -delta.x * rotationSensitivity);

            ballInstance.transform.RotateAround(
                ballCenter,
                mainCamera.transform.right,
                delta.y * rotationSensitivity);

            lastMousePos = currentPos;
        }
    }
    // ============================
    // ペイント処理
    // ============================
    void HandlePainting()
    {
        if (Mouse.current == null)
            return;
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);           
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == ballInstance ||
                    hit.collider.transform.IsChildOf(ballInstance.transform))
                {
                    PaintAt(hit.textureCoord);
                }
            }
        }
    }

    void PaintAt(Vector2 uv)
    {
        int x = (int)(uv.x * textureSize);
        int y = (int)(uv.y * textureSize);
        int radius = (int)(brushSize * 0.5f);

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int px = Mathf.Clamp(x + dx, 0, textureSize - 1);
                    int py = Mathf.Clamp(y + dy, 0, textureSize - 1);

                    // ブラシの端をぼかす
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = 1f - (dist / radius);
                    Color existing = paintTexture.GetPixel(px, py);
                    Color blended = Color.Lerp(existing, selectedColor, alpha);
                    paintTexture.SetPixel(px, py, blended);
                }
            }
        }

        paintTexture.Apply();
    }

    // ============================
    // UI構築
    // ============================
    void SetupUI()
    {
        var canvasGO = new GameObject("Canvas",
            typeof(Canvas),
            typeof(UnityEngine.UI.CanvasScaler),
            typeof(UnityEngine.UI.GraphicRaycaster));
        uiCanvas = canvasGO.GetComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // タイトル
        CreateText("Title", "Ball Custom", 36,
            new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(400f, 60f),
            canvasGO.transform);

        // 説明
        CreateText("Info", "Left click: Paint  Right-click drag: Rotation", 18,
            new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(600f, 40f),
            canvasGO.transform);

        // ====== 左パネル（カラーパレット・ブラシ） ======
        var leftPanel = CreatePanel("LeftPanel",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(16f, 0f), new Vector2(200f, 500f),
            canvasGO.transform);

        CreateText("ColorLabel", "Color", 22,
            new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(180f, 36f),
            leftPanel.transform);

        // カラーパレット
        Color[] palette = new Color[]
        {
            Color.white, Color.black, Color.red, Color.blue,
            Color.green, Color.yellow, new Color(1f, 0.5f, 0f), // オレンジ
            new Color(0.5f, 0f, 0.5f), // 紫
            new Color(1f, 0.75f, 0.8f), // ピンク
            new Color(0f, 1f, 1f), // シアン
            new Color(0.5f, 0.25f, 0f), // 茶
            new Color(0.5f, 0.5f, 0.5f), // グレー
        };

        for (int i = 0; i < palette.Length; i++)
        {
            int col = i % 4;
            int row = i / 4;
            Color c = palette[i];

            var btn = CreateColorButton(c,
                new Vector2(-70f + col * 46f, -60f - row * 46f),
                leftPanel.transform);
        }

        // ブラシサイズ
        CreateText("BrushLabel", "Brush Size", 20,
            new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(180f, 36f),
            leftPanel.transform);

        brushSizeLabel = CreateText("BrushSizeValue", "10", 18,
            new Vector2(0.5f, 1f), new Vector2(0f, -258f), new Vector2(180f, 30f),
            leftPanel.transform);

        // スライダー
        var sliderGO = new GameObject("BrushSlider",
            typeof(RectTransform),
            typeof(UnityEngine.UI.Slider));
        sliderGO.transform.SetParent(leftPanel.transform, false);
        var sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 1f);
        sliderRect.anchorMax = new Vector2(0.5f, 1f);
        sliderRect.anchoredPosition = new Vector2(0f, -295f);
        sliderRect.sizeDelta = new Vector2(160f, 30f);
        brushSizeSlider = sliderGO.GetComponent<UnityEngine.UI.Slider>();
        brushSizeSlider.minValue = 2f;
        brushSizeSlider.maxValue = 40f;
        brushSizeSlider.value = 10f;

        // スライダーの背景
        var sliderBG = new GameObject("Background",
            typeof(RectTransform), typeof(UnityEngine.UI.Image));
        sliderBG.transform.SetParent(sliderGO.transform, false);
        var sliderBGRect = sliderBG.GetComponent<RectTransform>();
        sliderBGRect.anchorMin = new Vector2(0f, 0.25f);
        sliderBGRect.anchorMax = new Vector2(1f, 0.75f);
        sliderBGRect.offsetMin = Vector2.zero;
        sliderBGRect.offsetMax = Vector2.zero;
        sliderBG.GetComponent<UnityEngine.UI.Image>().color =
            new Color(0.5f, 0.5f, 0.5f);

        // スライダーのハンドル
        var handleGO = new GameObject("Handle",
            typeof(RectTransform), typeof(UnityEngine.UI.Image));
        handleGO.transform.SetParent(sliderGO.transform, false);
        var handleRect = handleGO.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20f, 20f);
        handleGO.GetComponent<UnityEngine.UI.Image>().color = Color.white;
        brushSizeSlider.handleRect = handleRect;
        brushSizeSlider.targetGraphic =
            handleGO.GetComponent<UnityEngine.UI.Image>();

        brushSizeSlider.onValueChanged.AddListener(v =>
        {
            brushSize = v;
            brushSizeLabel.text = ((int)v).ToString();
        });

        // クリアボタン
        var clearBtn = CreateButton("Clear", new Vector2(0f, -360f),
            new Vector2(160f, 46f), leftPanel.transform,
            new Color32(80, 40, 40, 220));
        clearBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ClearTexture);

        // ====== 右パネル（ボール設定・購入） ======
        var rightPanel = CreatePanel("RightPanel",
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-16f, 0f), new Vector2(220f, 400f),
            canvasGO.transform);

        CreateText("SettingsLabel", "BallSetting", 22,
            new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(200f, 36f),
            rightPanel.transform);

        // ボール名
        CreateText("NameLabel", "Ballname", 18,
            new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(200f, 30f),
            rightPanel.transform);

        var nameFieldGO = CreateInputField("BallNameInput", "MyBall",
            new Vector2(0f, -90f), new Vector2(190f, 40f), rightPanel.transform);
        ballNameInput = nameFieldGO.GetComponent<TMP_InputField>();
        ballNameInput.contentType = TMP_InputField.ContentType.Standard;
        ballNameInput.characterLimit = 20;
        // 重さ
        CreateText("MassLabel", "pond", 18,
            new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(200f, 30f),
            rightPanel.transform);

        CreateText("MassHint", "10〜15", 14,
            new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(200f, 26f),
            rightPanel.transform);

        var massFieldGO = CreateInputField("MassInput", "11",
            new Vector2(0f, -205f), new Vector2(190f, 40f), rightPanel.transform);
        massInput = massFieldGO.GetComponent<TMP_InputField>();
        massInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        massInput.characterLimit = 2;

        massInput.onEndEdit.AddListener(value =>
        {
            if (int.TryParse(value, out int pound))
            {
                pound = Mathf.Clamp(pound, 10, 15);
                massInput.text = pound.ToString();
            }
            else
            {
                massInput.text = "10";
            }
        });
        // 価格表示
        priceText = CreateText("PriceText", $": {ballPrice}yen",
            20, new Vector2(0.5f, 1f), new Vector2(0f, -260f),
            new Vector2(200f, 36f), rightPanel.transform);
        priceText.color = Color.yellow;

        // 購入ボタン
        var buyBtn = CreateButton("price", new Vector2(0f, -310f),
            new Vector2(190f, 50f), rightPanel.transform,
            new Color32(40, 100, 60, 220));
        buyBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PurchaseAndSave);

        // 戻るボタン
        var backBtn = CreateButton("Home", new Vector2(0f, -370f),
            new Vector2(190f, 46f), rightPanel.transform,
            new Color32(80, 40, 40, 220));
        backBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(
            () => SceneManager.LoadScene(homeSceneName));

        // 価格更新ボタン（デバッグ用・Inspectorで変えた価格を反映）
        UpdatePriceDisplay();
    }

    void HandleZoom()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            cameraOffset += mainCamera.transform.forward *
                            (scroll * 0.01f * zoomSpeed);

            float distance = cameraOffset.magnitude;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            cameraOffset = cameraOffset.normalized * distance;

            mainCamera.transform.position = cameraOffset;
        }
    }
    // ============================
    // カラーボタン生成
    // ============================
    GameObject CreateColorButton(Color color, Vector2 pos, Transform parent)
    {
        var go = new GameObject($"ColorBtn_{ColorUtility.ToHtmlStringRGB(color)}",
            typeof(RectTransform),
            typeof(UnityEngine.UI.Image),
            typeof(UnityEngine.UI.Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(40f, 40f);
        go.GetComponent<UnityEngine.UI.Image>().color = color;

        var btn = go.GetComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = go.GetComponent<UnityEngine.UI.Image>();
        btn.onClick.AddListener(() => selectedColor = color);

        return go;
    }

    // ============================
    // テクスチャクリア
    // ============================
    void ClearTexture()
    {
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        paintTexture.SetPixels(pixels);
        paintTexture.Apply();
    }

    float ConvertPoundToKg(int pound)
    {
        switch (pound)
        {
            case 10:
                return 4.5f;
            case 11:
                return 5.0f;
            case 12:
                return 5.4f;
            case 13:
                return 5.9f;
            case 14:
                return 6.4f;
            case 15:
                return 6.8f;
            default:
                return 5.0f;
        }
    }

    // ============================
    // 購入・保存
    // ============================
    void PurchaseAndSave()
    {
        string name = ballNameInput != null ?
            ballNameInput.text : "MyBall";
        if (string.IsNullOrWhiteSpace(name))
            name = "MyBall";

        float mass = 5.0f;

        if (massInput != null &&
            int.TryParse(massInput.text, out int pound))
        {
            pound = Mathf.Clamp(pound, 10, 15);
            mass = ConvertPoundToKg(pound);
        }

        // テクスチャをPNGとして保存
        byte[] pngData = paintTexture.EncodeToPNG();
        string texPath = Application.persistentDataPath +
            $"/CustomBall_{System.DateTime.Now:yyyyMMddHHmmss}.png";
        System.IO.File.WriteAllBytes(texPath, pngData);

        // PlayerPrefsに保存
        int count = PlayerPrefs.GetInt("CustomBallCount", 0);
        PlayerPrefs.SetString($"CustomBall_{count}_Name", name);
        PlayerPrefs.SetFloat($"CustomBall_{count}_Mass", mass);
        PlayerPrefs.SetString($"CustomBall_{count}_TexPath", texPath);
        PlayerPrefs.SetInt("CustomBallCount", count + 1);
        PlayerPrefs.Save();

        Debug.Log($"Ballsave: {name} / {mass}kg / {texPath}");

        // ホームへ戻る
        SceneManager.LoadScene(homeSceneName);
    }

    void UpdatePriceDisplay()
    {
        if (priceText != null)
            priceText.text = $"price: {ballPrice}yen";
    }

    // ============================
    // UI生成ヘルパー
    // ============================
    TMP_Text CreateText(string name, string content, int fontSize,
        Vector2 anchor, Vector2 pos, Vector2 size, Transform parent)
    {
        var go = new GameObject(name,
            typeof(RectTransform),
            typeof(TMPro.TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TMPro.TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return tmp;
    }

    GameObject CreatePanel(string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pos, Vector2 size, Transform parent)
    {
        var go = new GameObject(name,
            typeof(RectTransform),
            typeof(UnityEngine.UI.Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        go.GetComponent<UnityEngine.UI.Image>().color =
            new Color32(24, 34, 52, 220);
        return go;
    }

    GameObject CreateButton(string label, Vector2 pos,
        Vector2 size, Transform parent, Color32 color)
    {
        var go = new GameObject($"Btn_{label}",
            typeof(RectTransform),
            typeof(UnityEngine.UI.Image),
            typeof(UnityEngine.UI.Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        go.GetComponent<UnityEngine.UI.Image>().color = color;
        var btn = go.GetComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = go.GetComponent<UnityEngine.UI.Image>();

        var txtGO = new GameObject("Text",
            typeof(RectTransform),
            typeof(TMPro.TextMeshProUGUI));
        txtGO.transform.SetParent(go.transform, false);
        var txt = txtGO.GetComponent<TMPro.TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 18;
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.color = Color.white;
        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return go;
    }

    GameObject CreateInputField(string name, string placeholder,
        Vector2 pos, Vector2 size, Transform parent)
    {
        var go = new GameObject(name,
            typeof(RectTransform),
            typeof(UnityEngine.UI.Image),
            typeof(CanvasRenderer),
            typeof(TMP_InputField));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        go.GetComponent<UnityEngine.UI.Image>().color =
            new Color(1f, 1f, 1f, 0.9f);

        var textGO = new GameObject("Text",
            typeof(RectTransform),
            typeof(TMPro.TextMeshProUGUI));
        textGO.transform.SetParent(go.transform, false);
        var textTMP = textGO.GetComponent<TMPro.TextMeshProUGUI>();
        textTMP.fontSize = 20;
        textTMP.color = Color.black;
        textTMP.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        var phGO = new GameObject("Placeholder",
            typeof(RectTransform),
            typeof(TMPro.TextMeshProUGUI));
        phGO.transform.SetParent(go.transform, false);
        var phTMP = phGO.GetComponent<TMPro.TextMeshProUGUI>();
        phTMP.text = placeholder;
        phTMP.fontSize = 20;
        phTMP.color = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        phTMP.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        var phRect = phGO.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = new Vector2(8f, 4f);
        phRect.offsetMax = new Vector2(-8f, -4f);

        var input = go.GetComponent<TMP_InputField>();

        input.textComponent = textTMP;
        input.placeholder = phTMP;
        input.targetGraphic = go.GetComponent<UnityEngine.UI.Image>();

        input.text = "";
        input.interactable = true;

        // 入力可能にする
        input.readOnly = false;

        // キャレット表示
        input.caretWidth = 2;
        input.caretColor = Color.black;

        return go;
    }
}