using System.Collections;
using UnityEngine;

public class CPUPlayer : MonoBehaviour
{
    [Header("CPU Settings")]
    public bool isCPUEnabled = true;
    public float minThrowDelay = 1.5f; // 投げるまでの最小待機時間
    public float maxThrowDelay = 3.0f; // 投げるまでの最大待機時間
    public float cpuPower = 75f;       // 投げる力（固定）
    public float positionRandomRange = 0.35f; // 横位置のランダム幅

    [Header("Ball Management")]
    private BallManager ballManager;
    private BallThrower ballThrower;
    private BallReset ballReset;
    private GameManager gameManager;
    private Rigidbody ballRb;
    private bool isThrowingAsCPU = false;
    
    void Start()
    {
                // BallManagerを初期化
        if (ballManager == null)
            ballManager = FindObjectOfType<BallManager>();

        // ballRbをBallManagerから取得
        ballReset = ballManager?.GetCurrentBallReset();
        ballRb = ballManager?.GetCurrentRigidbody();
        ballThrower = ballManager?.GetCurrentBallThrower();
        gameManager = FindObjectOfType<GameManager>();
    }

    // ============================
    // CPU投球開始
    // ============================
    public void StartCPUThrow()
    {
        if (!isCPUEnabled) return;
        if (isThrowingAsCPU) return;

        StartCoroutine(CPUThrowRoutine());
    }

    IEnumerator CPUThrowRoutine()
    {
        isThrowingAsCPU = true;

        float delay = Random.Range(minThrowDelay, maxThrowDelay);
        yield return new WaitForSeconds(delay);


        // ★現在のプレイヤーのボールを取得
        ballThrower = ballManager.GetCurrentBallThrower();
        ballRb = ballManager.GetCurrentRigidbody();
        ballReset = ballManager.GetCurrentBallReset();


        if (ballThrower == null || ballRb == null)
        {
            Debug.LogError("CPU: ボール取得失敗");
            isThrowingAsCPU = false;
            yield break;
        }


        float randomX = Random.Range(-positionRandomRange, positionRandomRange);

        Vector3 pos = ballThrower.transform.position;
        pos.x = randomX;
        ballThrower.transform.position = pos;


        yield return new WaitForSeconds(0.3f);

        ThrowAsCPU();

        isThrowingAsCPU = false;
    }

    void ThrowAsCPU()
    {
        if (ballRb == null)
            return;

        gameManager.StartRoll();

        ballRb.useGravity = true;

        ballRb.AddForce(
            Vector3.forward * cpuPower,
            ForceMode.Impulse
        );

        Debug.Log($"CPU投球: X={ballThrower.transform.position.x:F2}, Power={cpuPower}");
    }
}