using UnityEngine;

public static class GameSettings
{
    private const string PlayerCountKey = "Game_PlayerCount";
    private const string PlayerNameKeyPrefix = "Game_PlayerName_"; // append 1..4
    private const string TotalGameCountKey = "Game_TotalGameCount";
    private const string CurrentGameNumberKey = "Game_CurrentGameNumber";

    public static int PlayerCount
    {
        get => PlayerPrefs.GetInt(PlayerCountKey, 1);
        set => PlayerPrefs.SetInt(PlayerCountKey, Mathf.Clamp(value, 1, 4));
    }

    public static int TotalGameCount
    {
        get => PlayerPrefs.GetInt(TotalGameCountKey, 1);
        set => PlayerPrefs.SetInt(TotalGameCountKey, Mathf.Clamp(value, 1, 5));
    }

    public static int CurrentGameNumber
    {
        get => PlayerPrefs.GetInt(CurrentGameNumberKey, 1);
        set => PlayerPrefs.SetInt(CurrentGameNumberKey, Mathf.Max(value, 1));
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void ClearPlayerData()
    {
        PlayerPrefs.DeleteKey(PlayerCountKey);
        PlayerPrefs.DeleteKey(TotalGameCountKey);
        PlayerPrefs.DeleteKey(CurrentGameNumberKey);
        for (int i = 1; i <= 4; i++)
        {
            PlayerPrefs.DeleteKey(PlayerNameKeyPrefix + i);
        }
        PlayerPrefs.Save();
    }

    public static void SetPlayerName(int index, string name)
    {
        index = Mathf.Clamp(index, 1, 4);
        PlayerPrefs.SetString(PlayerNameKeyPrefix + index, name ?? string.Empty);
    }

    public static string GetPlayerName(int index)
    {
        index = Mathf.Clamp(index, 1, 4);
        return PlayerPrefs.GetString(PlayerNameKeyPrefix + index, $"Player {index}");
    }
}