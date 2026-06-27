using UnityEngine;

public class BallReturnTrigger : MonoBehaviour
{
    public GameManager gameManager;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Ball"))
        {
            triggered = true;

            gameManager.OnBallReachedGoal();
        }
    }

    public void ResetTrigger()
    {
        triggered = false;
    }
}