using System.Collections;
using UnityEngine;

public class BallReset : MonoBehaviour
{
    Rigidbody rb;

    Vector3 startPos;
    Quaternion startRot;
    private bool isResetting = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void ResetBall()
    {
        if (isResetting) return;
        StartCoroutine(ResetCoroutine());
    }

    IEnumerator ResetCoroutine()
    {
        isResetting = true;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = startPos;
        transform.rotation = startRot;

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isResetting = false;
    }   

    [Tooltip("この距離以上ずれていたら自動リセット")]
    public float resetThreshold = 0.5f;

    public void CheckAndResetIfNeeded()
    {
        float distance = Vector3.Distance(transform.position, startPos);
        if (distance > resetThreshold)
        {
            Debug.Log($"ボール位置ずれ検知（{distance:F2}）→ リセット");
            StartCoroutine(ResetCoroutine());
        }
    }
}