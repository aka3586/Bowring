using UnityEngine;

public class PinReset : MonoBehaviour
{
    Rigidbody rb;

    Vector3 startPos;
    Quaternion startRot;

    Pin pin;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pin = GetComponent<Pin>();

        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void ResetPin()
    {
        gameObject.SetActive(true);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = startPos;
        transform.rotation = startRot;

        rb.isKinematic = true; // ★リセット直後はKinematicのまま
        rb.Sleep();

        pin.ResetState();
    }    

    public void FreezePin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.Sleep();
    }

    public void ResetStandingPinRotation()
    {
        transform.rotation = startRot;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true; // ★追加
        rb.Sleep();
    }
}