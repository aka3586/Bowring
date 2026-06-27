using UnityEngine;

public class Pin : MonoBehaviour
{
    public bool isDown = false;
    public bool counted = false;

    void Update()
    {
        if (!isDown)
        {
            CheckIfDown();
        }
    }

    void CheckIfDown()
    {
        float angle = Vector3.Angle(transform.up, Vector3.up);

        if (angle > 75f)
        {
            isDown = true;
        }
    }

    public void ResetState()
    {
        isDown = false;
        counted = false;
    }
}