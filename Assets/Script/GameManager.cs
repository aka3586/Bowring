using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Ball Management")]
    public BallManager ballManager;
    public BallReset ballReset;
    public Rigidbody ballRb;
    public PinManager pinManager;
    public BallThrower ballThrower;
    public Pin[] pins;
    public BallReturnTrigger ballReturnTrigger;
    public ScoreManager scoreManager;
    public FrameManager frameManager;
    public CameraBallFollower cameraBallFollower;
    float rollTimer = 0f;
    [Header("UI")]
    public ScoreUIManager scoreUIManager;

    bool isRolling = false;

    [Header("CPU Settings")]
    public bool useCPU = true; // ★ゲーム中ON/OFFボタンで切り替え
    private CPUPlayer cpuPlayer;
    private GameObject cpuToggleButton;
    private TMPro.TextMeshProUGUI cpuToggleLabel;
    void Awake()
    {
        pins = FindObjectsByType<Pin>(FindObjectsInactive.Exclude);
    }

    void Start()
    {
        // BallManagerを初期化
        if (ballManager == null)
            ballManager = FindObjectOfType<BallManager>();

        if (ballManager != null)
            ballManager.Initialize(this);

        // ballRbをBallManagerから取得
        ballReset = ballManager?.GetCurrentBallReset();
        ballRb = ballManager?.GetCurrentRigidbody();
        ballThrower = ballManager?.GetCurrentBallThrower();

        if (scoreUIManager == null)
        {
            scoreUIManager = FindObjectOfType<ScoreUIManager>();
            if (scoreUIManager != null)
                Debug.Log("GameManager: auto-assigned ScoreUIManager.");
        }

        if (scoreUIManager != null)
        {
            scoreUIManager.CreateFrames();
        }

        if (cameraBallFollower == null)
            cameraBallFollower = FindObjectOfType<CameraBallFollower>();

        if (cameraBallFollower == null)
            Debug.LogWarning("GameManager: CameraBallFollower not found. Camera follow will be disabled.");

        var ballSelector = FindObjectOfType<BallSelector>();
        if (ballSelector != null)
            ballSelector.EnableBallChange();
        cpuPlayer = FindObjectOfType<CPUPlayer>();
        if (cpuPlayer == null)
        {
            var cpuGO = new GameObject("CPUPlayer");
            cpuPlayer = cpuGO.AddComponent<CPUPlayer>();
        }

        CreateCPUToggleButton();
        // 最初の表示用
        scoreManager.CalculateScore();
        UpdateUI();
    }

    void Update()
    {
        if (!isRolling) return;

        rollTimer += Time.deltaTime;

        // 15秒経っても奥に行かなければ強制終了
        if (rollTimer > 15f)
        {
            isRolling = false;
            StartCoroutine(EndRoll());
        }
    }

    // 投げた瞬間に呼ぶ
    // public void StartRoll()
    // {
    //     isRolling = true;

    //     frameManager.OnBallThrown();

    //     if (cameraBallFollower != null && ballRb != null)
    //     {
    //         cameraBallFollower.StartFollowing(ballRb.transform);
    //     }
    //     else if (cameraBallFollower != null && ballRb == null)
    //     {
    //         Debug.LogWarning("GameManager: CameraBallFollower exists but ballRb is missing. Cannot start camera follow.");
    //     }
    //     var ballSelector = FindObjectOfType<BallSelector>();
    //     if (ballSelector != null)
    //         ballSelector.DisableBallChange();
    //     // UI更新（投球回数とか表示したいならここ）
    //     UpdateUI();
    // }
    public void StartRoll()
    {
        isRolling = true;
        rollTimer = 0f;

        frameManager.OnBallThrown();

        if (cameraBallFollower != null && ballRb != null)
            cameraBallFollower.StartFollowing(ballRb.transform);

        var ballSelector = ballManager.GetCurrentBallSelector();
        if (ballSelector != null)
            ballSelector.DisableBallChange();

        UpdateUI();
    }
    IEnumerator EndRoll()
    {
        yield return new WaitForSeconds(6f);

        // カメラを先に止める
        if (cameraBallFollower != null)
            cameraBallFollower.StopFollowing();

        int downCount = GetDownPinCount();

        PlayerData currentPlayer = frameManager.GetCurrentPlayer();
        int beforeFrameCount = currentPlayer != null ? currentPlayer.frames.Count : 0;

        frameManager.RecordThrow(downCount);
        frameManager.CheckFrameEnd();

        bool frameFinished =
            currentPlayer != null && currentPlayer.frames.Count > beforeFrameCount;

        // ★10フレーム目のピンリセットが必要か判定
        bool isTenthFrame = currentPlayer != null && currentPlayer.currentFrame == 10;
        bool needsPinReset = false;

        if (isTenthFrame && !frameFinished)
        {
            // 1投目ストライク → 2投目前にピンリセット
            if (currentPlayer.throwCount == 1 && currentPlayer.currentFrameData.isStrike)
                needsPinReset = true;

            // 2投目でストライクまたはスペア → 3投目前にピンリセット
            if (currentPlayer.throwCount == 2 &&
                (currentPlayer.currentFrameData.isSpare || currentPlayer.currentFrameData.secondThrow == 10))
                needsPinReset = true;
        }

        scoreManager.CalculateScore();
        UpdateUI();

        // カメラ復帰を待ってからボールリセット
        yield return new WaitForSeconds(cameraBallFollower != null ?
            cameraBallFollower.waitBeforeReturn + cameraBallFollower.returnDuration : 0f);

        if (frameFinished || needsPinReset)
        {
            pinManager.ResetAllPins();
            StartCoroutine(UnfreezePinsAfterDelay(1.5f));
        }
        else
        {
            yield return new WaitForSeconds(2f);
            pinManager.RemoveDownPins();
            yield return new WaitForSeconds(1f);
            pinManager.FreezeStandingPins();
            pinManager.StraightenStandingPins(); // ★追加
        }

        if (frameManager.IsGameFinished())
        {
            Debug.Log("ゲーム終了！");
            ballThrower.enabled = false;

            if (GameSettings.CurrentGameNumber < GameSettings.TotalGameCount)
            {
                if (scoreUIManager != null)
                {
                    scoreUIManager.ShowGameEndPanel(true);
                }
            }
            else
            {
                if (scoreUIManager != null)
                {
                    scoreUIManager.ShowGameEndPanel(false);
                }
            }

            yield break;
        }

        // ballReset.ResetBall();
        // ballReturnTrigger.ResetTrigger();
        // yield return new WaitForSeconds(0.5f);
        // ballReset.CheckAndResetIfNeeded();
        // yield return new WaitForSeconds(0.3f);
        // var ballSelector = FindObjectOfType<BallSelector>();
        // if (ballSelector != null)
        //     ballSelector.EnableBallChange();
        // ballThrower.EnableThrow();
        ballReset.ResetBall();
        ballReturnTrigger.ResetTrigger();
        yield return new WaitForSeconds(0.5f);
        ballReset.CheckAndResetIfNeeded();
        yield return new WaitForSeconds(0.3f);

        var ballSelector = ballManager.GetCurrentBallSelector();
        if (ballSelector != null)
            ballSelector.EnableBallChange();

        // ★現在のプレイヤーがCPUかどうか判定
        bool isCurrentPlayerCPU = IsCurrentPlayerCPU();

        // EndRoll()の末尾、ballThrower.EnableThrow()の前に追加
        // 次のプレイヤーのボールに切り替え
        int nextPlayerIndex = frameManager.currentPlayerIndex;
        ballManager.SwitchToPlayer(nextPlayerIndex);

        // 参照を更新
        ballReset = ballManager.GetCurrentBallReset();
        ballRb = ballManager.GetCurrentRigidbody();
        ballThrower = ballManager.GetCurrentBallThrower();

        // カメラのターゲットを更新
        // if (cameraBallFollower != null && ballRb != null)
        //     cameraBallFollower.StartFollowing(ballRb.transform);
        ballThrower.EnableThrow();

        if (isCurrentPlayerCPU && useCPU)
        {
            // CPUのターン → Player1のクリックを無効にしてCPU自動投球
            ballThrower.SetPlayerControl(false);
            cpuPlayer.StartCPUThrow();
        }
        else
        {
            // 人間のターン → クリック操作を有効に
            ballThrower.SetPlayerControl(true);
        }
    }
    bool IsCurrentPlayerCPU()
    {
        // Player1（index 0）は人間、それ以外はCPU
        return frameManager.currentPlayerIndex > 0;
    }
    public int GetDownPinCount()
    {
        int count = 0;

        foreach (Pin pin in pins)
        {
            if (pin.isDown && !pin.counted)
            {
                pin.counted = true;
                count++;
            }
        }

        Debug.Log("今回倒れた本数 : " + count);

        return count;
    }

    // ============================
    // UI更新処理
    // ============================
    void UpdateUI()
    {
        if (scoreUIManager == null) return;

        PlayerData currentPlayer = frameManager.GetCurrentPlayer();
        if (currentPlayer == null)
            return;

        int savedCount = currentPlayer.frames.Count;
        int currentIndex = Mathf.Clamp(savedCount, 0, 9);
        FrameData currentData = frameManager.GetCurrentFrameData();

        scoreUIManager.SetPlayerName(currentPlayer.playerName);
        scoreUIManager.UpdateCurrentFrameUI(currentIndex, currentData, scoreManager.TotalScore);

        for (int i = 0; i < savedCount; i++)
        {
            scoreUIManager.UpdateFinalFrameUI(i, currentPlayer.frames[i]);
        }
        scoreUIManager.RefreshScorePanel();
    }

    public void OnBallReachedGoal()
    {
        if (!isRolling) return;

        isRolling = false;

        if (cameraBallFollower != null)
            cameraBallFollower.StopFollowing();

        StartCoroutine(EndRoll());
    }

    IEnumerator UnfreezePinsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        pinManager.UnfreezeStandingPins();
    }
    public void StartNextGame()
    {
        GameSettings.CurrentGameNumber++;

        frameManager.StartNextGame();
        scoreManager.CalculateScore();

        pinManager.ResetAllPins();
        ballReset.ResetBall();
        ballReturnTrigger.ResetTrigger();

        ballThrower.enabled = true;
        ballThrower.EnableThrow();

        var ballSelector = FindObjectOfType<BallSelector>();
        if (ballSelector != null)
            ballSelector.EnableBallChange();

        UpdateUI();
    }

    void CreateCPUToggleButton()
    {
        if (scoreUIManager == null) return;

        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        cpuToggleButton = new GameObject("CPUToggleButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(UnityEngine.UI.Image),
            typeof(UnityEngine.UI.Button));
        cpuToggleButton.transform.SetParent(canvas.transform, false);

        var rect = cpuToggleButton.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(999.1f, -154.1f);
        rect.sizeDelta = new Vector2(140f, 46f);

        cpuToggleButton.GetComponent<UnityEngine.UI.Image>().color =
            new Color32(40, 80, 120, 220);

        var btn = cpuToggleButton.GetComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = cpuToggleButton.GetComponent<UnityEngine.UI.Image>();
        btn.onClick.AddListener(ToggleCPU);

        var labelGO = new GameObject("Label",
            typeof(RectTransform),
            typeof(TMPro.TextMeshProUGUI));
        labelGO.transform.SetParent(cpuToggleButton.transform, false);
        cpuToggleLabel = labelGO.GetComponent<TMPro.TextMeshProUGUI>();
        cpuToggleLabel.fontSize = 18;
        cpuToggleLabel.alignment = TMPro.TextAlignmentOptions.Center;
        cpuToggleLabel.color = Color.white;
        var labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        // 1人プレイの場合はボタンを非表示
        if (GameSettings.PlayerCount <= 1)
        {
            cpuToggleButton.SetActive(false);
        }

        UpdateCPUToggleLabel();
    }

    void ToggleCPU()
    {
        useCPU = !useCPU;
        if (cpuPlayer != null)
            cpuPlayer.isCPUEnabled = useCPU;
        UpdateCPUToggleLabel();
        Debug.Log($"CPU: {(useCPU ? "ON" : "OFF")}");
    }

    void UpdateCPUToggleLabel()
    {
        if (cpuToggleLabel == null) return;
        cpuToggleLabel.text = useCPU ? "CPU: ON" : "CPU: OFF";
        if (cpuToggleButton != null)
        {
            cpuToggleButton.GetComponent<UnityEngine.UI.Image>().color =
                useCPU ? new Color32(40, 80, 120, 220)
                    : new Color32(80, 40, 40, 220);
        }
    }

    public void SetCPUToggleButtonVisible(bool visible)
    {
        if (GameSettings.PlayerCount <= 1)
        {
            cpuToggleButton.SetActive(false);
        }else if(cpuToggleButton != null)
            cpuToggleButton.SetActive(visible);
    }
}