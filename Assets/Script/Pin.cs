using UnityEngine;

public class Pin : MonoBehaviour
{
    public bool isDown = false;
    public bool counted = false;

    [Tooltip("初期位置からこの高さ以上下がったら倒れた判定")]
    public float fallHeightThreshold = 0.1f;

    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        if (!isDown)
        {
            CheckIfDown();
        }
    }

    void CheckIfDown()
    {
        // ★角度で判定（前後左右に倒れた場合）
        float angle = Vector3.Angle(transform.up, Vector3.up);
        if (angle > 45f)
        {
            isDown = true;
            return;
        }

        // ★高さで判定（後ろに落ちてきれいに立っている場合）
        if (transform.position.y < startY - fallHeightThreshold)
        {
            isDown = true;
            return;
        }
    }

    public void ResetState()
    {
        isDown = false;
        counted = false;
        startY = transform.position.y; // ★リセット時に高さも更新
    }
}