using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BallThrower : MonoBehaviour
{
    private Rigidbody rb;

    public float powerMultiplier = 0.07f;
    public float spinMultiplier = 0.005f;
    public float moveSpeed = 1f;
    public float limitX = 0.01f;

    public GameManager gameManager;

    private bool canThrow = true;
    private bool inputStartedOnUI = false;
    private bool inputStartedOnBall = false;

    private Vector2 dragStart;
    private Vector2 dragEnd;
    private float hookAmount;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }
    
    void Update()
    {
        if (!canThrow)
        {
            rb.AddForce(
                Vector3.right * hookAmount,
                ForceMode.Force
            );

            hookAmount *= 0.995f;
            return;
        }

        float move = 0f;

        if (Keyboard.current.aKey.isPressed)
            move = -1;

        if (Keyboard.current.dKey.isPressed)
            move = 1;

        transform.position +=
            Vector3.right * move * moveSpeed * Time.deltaTime;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(
            pos.x,
            -limitX,
            limitX
        );

        transform.position = pos;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var mousePosition = Mouse.current.position.ReadValue();
            if (IsPointerOverUI() || !IsPointerNearBall(mousePosition))
            {
                inputStartedOnUI = true;
                inputStartedOnBall = false;
            }
            else
            {
                inputStartedOnUI = false;
                inputStartedOnBall = true;
                dragStart = mousePosition;
            }
        }

        // ドラッグ終了
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            dragEnd = Mouse.current.position.ReadValue();
            if (!inputStartedOnUI && inputStartedOnBall)
            {
                ThrowBall();
            }
            inputStartedOnUI = false;
            inputStartedOnBall = false;
        }
    }

    void ThrowBall()
    {
        Vector2 drag = dragEnd - dragStart;
        rb.useGravity = true;
        if (drag.y <= 0)
            return;

        float spin = drag.x * spinMultiplier;

        float power = Mathf.Clamp(
            drag.y * powerMultiplier,
            30f,
            70f
        );

        // フック用に保存
        hookAmount = spin;

        canThrow = false;

        gameManager.StartRoll();

        // 前へ投げる
        Vector3 throwDirection =
            Vector3.forward +
            Vector3.up * 0.1f;
        rb.AddForce(
            Vector3.forward * power,
            ForceMode.Impulse
        );

        // 回転を与える
        rb.AddTorque(
            Vector3.forward * spin,
            ForceMode.Impulse
        );

        Debug.Log($"Power : {power}  Spin : {spin}");
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsPointerNearBall(Vector2 screenPosition)
    {
        var cam = Camera.main;
        if (cam == null)
            return false;

        Ray ray = cam.ScreenPointToRay(screenPosition);
        Vector3 ballPos = transform.position;
        Vector3 originToBall = ballPos - ray.origin;
        float distance = Vector3.Cross(ray.direction.normalized, originToBall).magnitude;
        return distance <= 1.5f;
    }

    public void EnableThrow()
    {
        canThrow = true;
    }
}