using System.IO;
using UnityEngine;

[System.Serializable]
public class PurchaseData
{
    public string userId = "player_001";
    public Purchases purchases = new Purchases();
}

[System.Serializable]
public class Purchases
{
    public bool ballCustomize = false;
}

public static class PurchaseManager
{
    private static string FilePath =>
        "Assets/Json" + "/purchases.json";

    private static PurchaseData data;

    // ============================
    // 読み込み
    // ============================
    public static void Load()
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            data = JsonUtility.FromJson<PurchaseData>(json);
            Debug.Log($"PurchaseManager: Loaded. ballCustomize={data.purchases.ballCustomize}");
        }
        else
        {
            data = new PurchaseData();
            Save();
            Debug.Log("PurchaseManager: Created new purchase data.");
        }
    }

    // ============================
    // 保存
    // ============================
    public static void Save()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"PurchaseManager: Saved to {FilePath}");
    }

    // ============================
    // 購入状態確認
    // ============================
    public static bool IsPurchased(string itemId)
    {
        if (data == null) Load();

        switch (itemId)
        {
            case "ballCustomize":
                return data.purchases.ballCustomize;
            default:
                return false;
        }
    }

    // ============================
    // 購入処理（デモ用）
    // ============================
    public static void Purchase(string itemId)
    {
        if (data == null) Load();

        switch (itemId)
        {
            case "ballCustomize":
                data.purchases.ballCustomize = true;
                break;
        }

        Save();
        Debug.Log($"PurchaseManager: Purchased {itemId}");
    }

    // ============================
    // リセット（デバッグ用）
    // ============================
    public static void Reset()
    {
        data = new PurchaseData();
        Save();
        Debug.Log("PurchaseManager: Reset all purchases.");
    }
}