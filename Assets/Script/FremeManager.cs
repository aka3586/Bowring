using System.Collections.Generic;
using UnityEngine;

public class FrameManager : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerCount = 1;
    public int currentPlayerIndex = 0;

    public PinManager pinManager;
    public List<PlayerData> players = new List<PlayerData>();
    private ScoreUIManager scoreUIManager;
    void Awake()
    {
        playerCount = GameSettings.PlayerCount;
        InitializePlayers();
        scoreUIManager = Object.FindAnyObjectByType<ScoreUIManager>();
    }
    private void SaveAndRefreshUI(PlayerData player)
    {
        player.SaveFrame();
        if (scoreUIManager != null) scoreUIManager.RefreshScorePanel();
    }
    void InitializePlayers()
    {
        playerCount = Mathf.Clamp(playerCount, 1, 4);
        players.Clear();

        for (int i = 0; i < playerCount; i++)
        {
            PlayerData player = new PlayerData();
            player.playerName = GameSettings.GetPlayerName(i + 1);
            player.StartNewFrame();
            players.Add(player);
        }

        currentPlayerIndex = 0;
    }

    public PlayerData GetCurrentPlayer()
    {
        if (players == null || players.Count == 0)
            return null;

        currentPlayerIndex = Mathf.Clamp(currentPlayerIndex, 0, players.Count - 1);
        return players[currentPlayerIndex];
    }

    public FrameData GetCurrentFrameData()
    {
        PlayerData player = GetCurrentPlayer();
        return player != null ? player.currentFrameData : null;
    }

    // ボールを投げた瞬間に呼ぶ
    public void OnBallThrown()
    {
        PlayerData player = GetCurrentPlayer();
        if (player == null)
            return;

        player.throwCount++;
        if (player.throwCount == 2)
        {
            pinManager.UnfreezeStandingPins();
        }
    }

    // 倒したピン数を記録する
    public void RecordThrow(int pins)
    {
        PlayerData player = GetCurrentPlayer();
        if (player == null)
            return;

        if (player.throwCount == 1)
        {
            player.currentFrameData.firstThrow = pins;
            if (pins == 10)
            {
                player.currentFrameData.isStrike = true;
            }
        }
        else if (player.throwCount == 2)
        {
            player.currentFrameData.secondThrow = pins;
            if (!player.currentFrameData.isStrike && player.currentFrameData.firstThrow + pins == 10)
            {
                player.currentFrameData.isSpare = true;
            }
            else if (player.currentFrameData.secondThrow == 10)
            {
                player.currentFrameData.isStrike = true;
            }
        }
        else if (player.throwCount == 3 && player.currentFrame == 10)
        {
            player.currentFrameData.thirdThrow = pins;
        }
    }

    // フレームが終了したかどうか判定して、終了なら保存して次へ進む
    public void CheckFrameEnd()
    {
        PlayerData player = GetCurrentPlayer();
        if (player == null)
            return;

        if (player.currentFrame < 10)
        {
            if (player.currentFrameData.isStrike)
            {
                SaveAndRefreshUI(player);
                player.NextFrame();
                NextPlayer();
                return;
            }

            if (player.throwCount >= 2)
            {
                SaveAndRefreshUI(player);
                player.NextFrame();
                NextPlayer();
                return;
            }

            return;
        }

        if (player.currentFrame == 10)
        {
            if (player.currentFrameData.isStrike)
            {
                if (player.throwCount >= 3)
                {
                    SaveAndRefreshUI(player);
                    NextPlayer();
                }
                return;
            }

            if (player.currentFrameData.isSpare)
            {
                if (player.throwCount >= 3)
                {
                    SaveAndRefreshUI(player);
                    NextPlayer();
                }
                return;
            }

            if (player.throwCount >= 2)
            {
                SaveAndRefreshUI(player);
                NextPlayer();
            }
        }
    }

    void SaveFrame()
    {
        PlayerData player = GetCurrentPlayer();
        if (player == null)
            return;

        int first = Mathf.Max(player.currentFrameData.firstThrow, 0);
        int second = Mathf.Max(player.currentFrameData.secondThrow, 0);
        int third = Mathf.Max(player.currentFrameData.thirdThrow, 0);

        player.currentFrameData.frameScore = first + second + third;
        player.frames.Add(player.currentFrameData);

        Debug.Log($"[{player.playerName}] Frame {player.currentFrame} saved : {first}, {second}, {third}");
    }

    void NextPlayer()
    {
        if (players == null || players.Count == 0)
            return;

        for (int offset = 1; offset <= players.Count; offset++)
        {
            int nextIndex = (currentPlayerIndex + offset) % players.Count;
            if (!players[nextIndex].IsFinished())
            {
                currentPlayerIndex = nextIndex;
                return;
            }
        }
    }

    public void StartNextGame()
    {
        foreach (var player in players)
        {
            player.ResetForNextGame();
        }
        currentPlayerIndex = 0;
    }

    public bool IsGameFinished()
    {
        if (players == null)
            return true;

        foreach (PlayerData player in players)
        {
            if (!player.IsFinished())
                return false;
        }

        return true;
    }
}

[System.Serializable]
public class PlayerData
{
    public string playerName = "Player";
    public int currentFrame = 1;
    public int throwCount = 0;
    public List<FrameData> frames = new List<FrameData>();
    public FrameData currentFrameData = new FrameData();
    public int totalScore = 0;
    public int cumulativeScore = 0;
    public bool IsFinished()
    {
        return frames.Count >= 10;
    }

    public void StartNewFrame()
    {
        currentFrameData = new FrameData();
        throwCount = 0;
    }

    public void SaveFrame()
    {
        int first = Mathf.Max(currentFrameData.firstThrow, 0);
        int second = Mathf.Max(currentFrameData.secondThrow, 0);
        int third = Mathf.Max(currentFrameData.thirdThrow, 0);

        currentFrameData.frameScore = first + second + third;
        currentFrameData.isScoreFixed = true;
        frames.Add(currentFrameData);

        Debug.Log($"[{playerName}] Frame {currentFrame} saved : {first}, {second}, {third}");
    }

    public void NextFrame()
    {
        currentFrame++;
        StartNewFrame();
    }

    public void ResetForNextGame()
    {
        cumulativeScore += totalScore;
        currentFrame = 1;
        throwCount = 0;
        frames.Clear();
        currentFrameData = new FrameData();
        totalScore = 0;
    }

}
