using UnityEngine;

public class PinManager : MonoBehaviour
{
    PinReset[] pins;

    void Start()
    {
        // 子オブジェクトにあるピンを全部取得
        pins = GetComponentsInChildren<PinReset>();
    }

    public void ResetAllPins()
    {
        foreach (PinReset pin in pins)
        {
            pin.ResetPin();
        }
    }
    public void RemoveDownPins()
    {
        foreach (PinReset pin in pins)
        {
            Pin pinData = pin.GetComponent<Pin>();

            if (pinData.isDown)
            {
                pin.gameObject.SetActive(false);
            }
        }
    }
    public void StraightenStandingPins()
    {
        foreach (PinReset pinReset in pins)
        {
            Pin pin = pinReset.GetComponent<Pin>();
            if (!pin.isDown)
            {
                pinReset.ResetStandingPinRotation();
            }
        }
    }
    
    public void FreezeStandingPins()
    {
        foreach (PinReset pinReset in pins)
        {
            Pin pin = pinReset.GetComponent<Pin>();

            if (!pin.isDown)
            {
                Rigidbody rb = pinReset.GetComponent<Rigidbody>();

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.isKinematic = true;
            }
        }
    }

    public void UnfreezeStandingPins()
    {
        foreach (PinReset pinReset in pins)
        {
            Pin pin = pinReset.GetComponent<Pin>();

            if (!pin.isDown)
            {
                pinReset.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }
}