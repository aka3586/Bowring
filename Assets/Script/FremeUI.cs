using TMPro;
using UnityEngine;

public class FrameUI : MonoBehaviour
{
    public TextMeshProUGUI frameText;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI throw1Text;
    public TextMeshProUGUI throw2Text;
    public TextMeshProUGUI throw3Text;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalScoreText;

    private int frameIndex;
    private string currentPlayerName = string.Empty;

    public void SetFrameIndex(int index)
    {
        frameIndex = index;
        UpdatePlayerNameLabel();
    }

    public void SetPlayerName(string playerName)
    {
        currentPlayerName = playerName ?? string.Empty;
        UpdatePlayerNameLabel();
    }

    private void UpdatePlayerNameLabel()
    {
        if (playerNameText != null)
        {
            playerNameText.text = currentPlayerName;
        }

        if (frameText != null)
        {
            Debug.Log($"UpdatePlayerNameLabel frameIndex={frameIndex}");

            frameText.text = (frameIndex + 1).ToString();
        }
    }

    public void SetTotalScore(int totalScore)
    {
        if (totalScoreText == null) return;

        totalScoreText.text = totalScore.ToString();
    }

    public void SetData(FrameData data)
    {
        // ====================================
        // 1投目
        // ====================================
        if (data.firstThrow < 0)
            throw1Text.text = "";
        else if (data.isStrike)
            throw1Text.text = "X";
        else
            throw1Text.text = data.firstThrow.ToString();

        // ====================================
        // 2投目
        // ====================================
        if (data.secondThrow < 0)
            throw2Text.text = "";
        else if (data.isSpare)
            throw2Text.text = "/";
        else if (data.isStrike && frameIndex == 9)
            throw2Text.text = "X";
        else
            throw2Text.text = data.secondThrow.ToString();

        // ====================================
        // 3投目（10フレーム目のみ）
        // ====================================
        if (throw3Text != null)
        {
            if (frameIndex == 9)
            {
                throw3Text.gameObject.SetActive(true);

                if (data.thirdThrow < 0)
                    throw3Text.text = "";
                else if (data.thirdThrow == 10)
                    throw3Text.text = "X";
                else
                    throw3Text.text = data.thirdThrow.ToString();
            }
            else
            {
                throw3Text.gameObject.SetActive(false);
            }
        }

        // ====================================
        // スコア表示
        // ====================================
        if (scoreText != null)
        {
            if (data.isScoreFixed)
                scoreText.text = data.frameScore.ToString();
            else
                scoreText.text = "";
        }
    }

    public void Clear()
    {
        throw1Text.text = "";
        throw2Text.text = "";

        if (throw3Text != null)
        {
            throw3Text.text = "";
            throw3Text.gameObject.SetActive(false);
        }

        if (scoreText != null)
            scoreText.text = "";

        if (totalScoreText != null)
            totalScoreText.text = "";
    }
}