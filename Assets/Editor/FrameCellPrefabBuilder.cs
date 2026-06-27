using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class FrameCellPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefab/FrameCell.prefab";

    [MenuItem("Tools/Rebuild FrameCell Prefab Layout")]
    public static void RebuildFrameCellPrefab()
    {
        var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existingPrefab == null)
        {
            Debug.LogError($"FrameCell prefab not found at path: {PrefabPath}");
            return;
        }

        var root = new GameObject("FrameCell", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(FrameUI));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(720f, 140f);

        var background = root.GetComponent<Image>();
        background.color = new Color32(20, 28, 40, 220);
        background.raycastTarget = false;

        var frameUI = root.GetComponent<FrameUI>();

        var font = TMP_Settings.defaultFontAsset;
        if (font == null)
        {
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        var frameText = CreateText("FrameText", "1", 26, TextAlignmentOptions.Center, font, root.transform);
        SetRect(frameText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(80, 30));

        var playerNameText = CreateText("PlayerNameText", "Player 1", 24, TextAlignmentOptions.MidlineLeft, font, root.transform);
        SetRect(playerNameText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(110, -20), new Vector2(260, 30));

        var throwsContainer = new GameObject("ThrowsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        throwsContainer.transform.SetParent(root.transform, false);
        var throwsRect = throwsContainer.GetComponent<RectTransform>();
        SetRect(throwsRect, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(140, 0), new Vector2(260, 60));
        var layout = throwsContainer.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 12f;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var throw1Text = CreateText("Throw1Text", "0", 28, TextAlignmentOptions.Center, font, throwsContainer.transform);
        SetRect(throw1Text.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(60, 60));
        var throw2Text = CreateText("Throw2Text", "0", 28, TextAlignmentOptions.Center, font, throwsContainer.transform);
        SetRect(throw2Text.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(60, 60));
        var throw3Text = CreateText("Throw3Text", "0", 28, TextAlignmentOptions.Center, font, throwsContainer.transform);
        SetRect(throw3Text.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(60, 60));

        var frameScoreText = CreateText("FrameScore", "0", 24, TextAlignmentOptions.Center, font, root.transform);
        SetRect(frameScoreText.rectTransform, new Vector2(1, 0.7f), new Vector2(1, 0.7f), new Vector2(-70, 0), new Vector2(100, 40));

        var totalScoreText = CreateText("TotalScoreText", "0", 22, TextAlignmentOptions.Center, font, root.transform);
        SetRect(totalScoreText.rectTransform, new Vector2(1, 0.2f), new Vector2(1, 0.2f), new Vector2(-70, 0), new Vector2(100, 34));

        frameUI.frameText = frameText;
        frameUI.playerNameText = playerNameText;
        frameUI.throw1Text = throw1Text;
        frameUI.throw2Text = throw2Text;
        frameUI.throw3Text = throw3Text;
        frameUI.scoreText = frameScoreText;
        frameUI.totalScoreText = totalScoreText;

        PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.UserAction);
        Object.DestroyImmediate(root);

        Debug.Log("FrameCell prefab layout rebuilt successfully.");
    }

    private static TextMeshProUGUI CreateText(string name, string content, int fontSize, TextAlignmentOptions alignment, TMP_FontAsset font, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        if (font != null)
            tmp.font = font;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0.5f, 0.5f);

        return tmp;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }
}
