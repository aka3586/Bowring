using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public FrameManager frameManager;

    public int TotalScore { get; private set; } = 0;

    private int[] frameScores = new int[10];

    public int GetFrameScore(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= frameScores.Length)
            return -1;

        return frameScores[frameIndex];
    }

    public void CalculateScore()
    {
        TotalScore = 0;

        for (int i = 0; i < frameScores.Length; i++)
        {
            frameScores[i] = -1;
        }

        if (frameManager == null)
            return;

        PlayerData currentPlayer = frameManager.GetCurrentPlayer();
        if (currentPlayer == null)
            return;

        List<FrameData> frames = currentPlayer.frames;
        int playerTotal = 0;

        for (int i = 0; i < frames.Count; i++)
        {
            FrameData frame = frames[i];

            if (!IsFrameScoreFixed(frames, i))
            {
                frame.isScoreFixed = false;
                continue;
            }

            int frameScore = 0;

            if (i == 9)
            {
                frameScore =
                    Mathf.Max(frame.firstThrow, 0) +
                    Mathf.Max(frame.secondThrow, 0) +
                    Mathf.Max(frame.thirdThrow, 0);
            }
            else if (frame.isStrike)
            {
                frameScore = 10 + GetStrikeBonus(frames, i);
            }
            else if (frame.isSpare)
            {
                frameScore = 10 + GetSpareBonus(frames, i);
            }
            else
            {
                frameScore =
                    Mathf.Max(frame.firstThrow, 0) +
                    Mathf.Max(frame.secondThrow, 0);
            }

            frame.frameScore = frameScore;
            frame.isScoreFixed = true;

            frameScores[i] = frameScore;
            playerTotal += frameScore;
        }

        currentPlayer.totalScore = playerTotal;
        TotalScore = playerTotal;
    }

    // =====================================================
    // �X�g���C�N�{�[�i�X
    // =====================================================
    int GetStrikeBonus(List<FrameData> frames, int index)
    {
        int bonus = 0;

        if (index + 1 >= frames.Count)
            return 0;

        FrameData next = frames[index + 1];

        // ����1��
        bonus += Mathf.Max(next.firstThrow, 0);

        // �����X�g���C�N
        if (next.isStrike)
        {
            // ����Ɏ��̃t���[���K�v
            if (index + 2 < frames.Count)
            {
                bonus += Mathf.Max(frames[index + 2].firstThrow, 0);
            }
            else
            {
                // 10�t���[���ڑΉ�
                bonus += Mathf.Max(next.secondThrow, 0);
            }
        }
        else
        {
            bonus += Mathf.Max(next.secondThrow, 0);
        }

        return bonus;
    }

    // =====================================================
    // �X�y�A�{�[�i�X
    // =====================================================
    int GetSpareBonus(List<FrameData> frames, int index)
    {
        if (index + 1 >= frames.Count)
            return 0;

        return Mathf.Max(frames[index + 1].firstThrow, 0);
    }

    // =====================================================
    // �X�R�A�m�蔻��
    // =====================================================
    bool IsFrameScoreFixed(List<FrameData> frames, int index)
    {
        FrameData frame = frames[index];

        // =========================
        // 10�t���[����
        // =========================
        if (index == 9)
        {
            // �X�g���C�N
            if (frame.firstThrow == 10)
            {
                return frame.thirdThrow >= 0;
            }

            // �X�y�A
            if (frame.firstThrow + frame.secondThrow == 10)
            {
                return frame.thirdThrow >= 0;
            }

            // �ʏ�
            return frame.secondThrow >= 0;
        }

        // =========================
        // �ʏ�t���[��
        // =========================
        if (!frame.isStrike && !frame.isSpare)
        {
            return frame.secondThrow >= 0;
        }

        // =========================
        // �X�y�A
        // =========================
        if (frame.isSpare)
        {
            if (index + 1 >= frames.Count)
                return false;

            return frames[index + 1].firstThrow >= 0;
        }

        // =========================
        // �X�g���C�N
        // =========================
        if (frame.isStrike)
        {
            if (index + 1 >= frames.Count)
                return false;

            FrameData next = frames[index + 1];

            // �����X�g���C�N
            if (next.isStrike)
            {
                // ����Ɏ��K�v
                if (index + 2 < frames.Count)
                {
                    return frames[index + 2].firstThrow >= 0;
                }

                // 10�t���[����
                return next.secondThrow >= 0;
            }

            // �����ʏ�
            return next.secondThrow >= 0;
        }

        return false;
    }
}