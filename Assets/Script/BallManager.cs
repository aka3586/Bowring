using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    [Header("Ball Prefab")]
    public GameObject ballPrefab;

    [Header("Ball Start Position")]
    public Vector3 ballStartPosition = new Vector3(0f, 0.293f, -9.246f);

    private List<GameObject> balls = new List<GameObject>();
    private List<BallReset> ballResets = new List<BallReset>();
    private List<BallThrower> ballThrowers = new List<BallThrower>();
    private List<Rigidbody> ballRigidbodies = new List<Rigidbody>();
    private List<BallSelector> ballSelectors = new List<BallSelector>();
    private List<int> playerBallIndexes = new List<int>();

    private int currentBallIndex = 0;
    private int playerCount = 1;

    void Awake()
    {
        playerCount = Mathf.Clamp(GameSettings.PlayerCount, 1, 4);
    }

    public void Initialize(GameManager gameManager)
    {
        // 既存のballを削除
        foreach (var b in balls)
        {
            if (b != null) Destroy(b);
        }

        balls.Clear();
        ballResets.Clear();
        ballThrowers.Clear();
        ballRigidbodies.Clear();
        ballSelectors.Clear();

        playerBallIndexes.Clear();

        for (int i = 0; i < playerCount; i++)
        {
            playerBallIndexes.Add(0);
        }

        // プレイヤー数分のボールを生成
        for (int i = 0; i < playerCount; i++)
        {
            var ballGO = Instantiate(ballPrefab,
                ballStartPosition, Quaternion.identity);
            ballGO.name = $"Ball_{i + 1}";
            ballGO.transform.SetParent(transform);

            // GameManagerの参照を更新
            var thrower = ballGO.GetComponent<BallThrower>();
            if (thrower != null)
                thrower.gameManager = gameManager;

            balls.Add(ballGO);
            ballResets.Add(ballGO.GetComponent<BallReset>());
            ballThrowers.Add(thrower);
            ballRigidbodies.Add(ballGO.GetComponent<Rigidbody>());
            var selector = ballGO.GetComponent<BallSelector>();
            if(selector != null)
            {
                selector.SetPlayerIndex(i);
            }

            ballSelectors.Add(selector);

            // Player1以外は非アクティブ
            ballGO.SetActive(i == 0);

            Debug.Log($"BallManager: Ball_{i + 1} created.");
        }
    }

    // ============================
    // 現在のボールに切り替え
    // ============================
    public void SwitchToPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= balls.Count) return;

        for (int i = 0; i < balls.Count; i++)
        {
            if (balls[i] != null)
                balls[i].SetActive(false);
        }

        currentBallIndex = playerIndex;

        GameObject ball = balls[currentBallIndex];

        if(ball != null)
        {
            ball.SetActive(true);

            BallSelector selector = ballSelectors[currentBallIndex];

            if(selector != null)
            {
                selector.LoadPlayerBall();
            }

            Debug.Log($"BallManager: Switched to Ball_{playerIndex + 1}");
        }
    }

    // ============================
    // 現在のボール取得
    // ============================
    public GameObject GetCurrentBall()
    {
        if (currentBallIndex >= balls.Count) return null;
        return balls[currentBallIndex];
    }

    public BallReset GetCurrentBallReset()
    {
        if (currentBallIndex >= ballResets.Count) return null;
        return ballResets[currentBallIndex];
    }

    public BallThrower GetCurrentBallThrower()
    {
        if (currentBallIndex >= ballThrowers.Count) return null;
        return ballThrowers[currentBallIndex];
    }

    public Rigidbody GetCurrentRigidbody()
    {
        if (currentBallIndex >= ballRigidbodies.Count) return null;
        return ballRigidbodies[currentBallIndex];
    }

    public BallSelector GetCurrentBallSelector()
    {
        if (currentBallIndex >= ballSelectors.Count) return null;
        return ballSelectors[currentBallIndex];
    }

    // ============================
    // 全ボールリセット
    // ============================
    public void ResetCurrentBall()
    {
        var reset = GetCurrentBallReset();
        if (reset != null) reset.ResetBall();
    }

    // ============================
    // Player1のみ操作可能にする
    // ============================
    public void SetPlayerControl(int playerIndex, bool enabled)
    {
        if (playerIndex >= ballThrowers.Count) return;
        var thrower = ballThrowers[playerIndex];
        if (thrower != null)
            thrower.SetPlayerControl(enabled);
    }
}