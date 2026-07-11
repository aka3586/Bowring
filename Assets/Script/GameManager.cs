using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public BallReset ballReset;
    public PinManager pinManager;
    public BallThrower ballThrower;
    public Rigidbody ballRb;
    public Pin[] pins;
    public BallReturnTrigger ballReturnTrigger;
    public ScoreManager scoreManager;
    public FrameManager frameManager;
    public CameraBallFollower cameraBallFollower;
    float rollTimer = 0f;
    [Header("UI")]
    public ScoreUIManager scoreUIManager;

    bool isRolling = false;

    void Awake()
    {
        pins = FindObjectsByType<Pin>(FindObjectsInactive.Exclude);
    }

    void Start()
    {
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

        if (ballRb == null && ballThrower != null)
            ballRb = ballThrower.GetComponent<Rigidbody>();

        if (ballRb == null)
        {
            var ballObject = GameObject.FindWithTag("Ball");
            if (ballObject != null)
                ballRb = ballObject.GetComponent<Rigidbody>();
        }

        if (ballRb == null)
        {
            var ballObject = GameObject.Find("ball") ?? GameObject.Find("Ball");
            if (ballObject != null)
                ballRb = ballObject.GetComponent<Rigidbody>();
        }

        if (cameraBallFollower == null)
            Debug.LogWarning("GameManager: CameraBallFollower not found. Camera follow will be disabled.");
        if (ballRb == null)
            Debug.LogWarning("GameManager: ballRb not assigned or found. Camera follow will be disabled.");
        var ballSelector = FindObjectOfType<BallSelector>();
        if (ballSelector != null)
            ballSelector.EnableBallChange();
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
    public void StartRoll()
    {
        isRolling = true;

        frameManager.OnBallThrown();

        if (cameraBallFollower != null && ballRb != null)
        {
            cameraBallFollower.StartFollowing(ballRb.transform);
        }
        else if (cameraBallFollower != null && ballRb == null)
        {
            Debug.LogWarning("GameManager: CameraBallFollower exists but ballRb is missing. Cannot start camera follow.");
        }
        var ballSelector = FindObjectOfType<BallSelector>();
        if (ballSelector != null)
            ballSelector.DisableBallChange();
        // UI更新（投球回数とか表示したいならここ）
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

        ballReset.ResetBall();
        ballReturnTrigger.ResetTrigger();
        yield return new WaitForSeconds(0.5f);
        ballReset.CheckAndResetIfNeeded();
        yield return new WaitForSeconds(0.3f);
        var ballSelector = FindObjectOfType<BallSelector>();
        if (ballSelector != null)
            ballSelector.EnableBallChange();
        ballThrower.EnableThrow();
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
}