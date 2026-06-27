using System.Collections;
using UnityEngine;

public class CameraBallFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("ボールとのZ距離（固定）")]
    public float followDistance = 6f;

    [Tooltip("追従終了地点（カメラのZ座標）")]
    public float stopFollowZ = 17f;

    [Header("Return Settings")]
    [Tooltip("追従終了後の待機時間")]
    public float waitBeforeReturn = 1.5f;

    [Tooltip("元の位置へ戻る時間")]
    public float returnDuration = 1f;

    private Transform targetBall;
    private Rigidbody targetRb;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isFollowing = false;
    private bool isReturning = false;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (isReturning) return;

        if (isFollowing && targetBall != null)
        {
            // 止まる地点に来たら終了
            if (transform.position.z >= stopFollowZ)
            {
                transform.position = new Vector3(
                    originalPosition.x,
                    originalPosition.y,
                    stopFollowZ
                );
                StopFollowing();
                return;
            }

            // ボールのZ速度に合わせてカメラを動かす
            float ballSpeedZ = targetRb != null ? targetRb.linearVelocity.z : 0f;
            ballSpeedZ = Mathf.Max(ballSpeedZ, 0f); // 後退しないように

            transform.position += Vector3.forward * ballSpeedZ *0.8f* Time.deltaTime;
            transform.position = new Vector3(
                originalPosition.x,
                originalPosition.y,
                transform.position.z
            );
            transform.rotation = originalRotation;
        }
    }

    public void StartFollowing(Transform ballTransform)
    {
        if (ballTransform == null) return;

        StopAllCoroutines();
        targetBall = ballTransform;
        targetRb = ballTransform.GetComponent<Rigidbody>();
        isFollowing = true;
        isReturning = false;

        // カメラをボールのZ - followDistance の位置にスナップ
        transform.position = new Vector3(
            originalPosition.x,
            originalPosition.y,
            Mathf.Min(targetBall.position.z - followDistance, stopFollowZ)
        );

        Debug.Log("カメラ追従開始");
    }

    public void StopFollowing()
    {
        isFollowing = false;
        StopAllCoroutines();
        StartCoroutine(ReturnCameraRoutine());
        Debug.Log("カメラ追従終了");
    }

    private IEnumerator ReturnCameraRoutine()
    {
        isReturning = true;

        yield return new WaitForSeconds(waitBeforeReturn);

        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            transform.rotation = originalRotation;
            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
        targetBall = null;
        targetRb = null;
        isReturning = false;

        Debug.Log("カメラ復帰完了");
    }
}