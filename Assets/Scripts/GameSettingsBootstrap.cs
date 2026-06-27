using System;
using System.Reflection;
using UnityEngine;

public static class GameSettingsBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyPlayerCount()
    {
        try
        {
            int count = GameSettings.PlayerCount;
            var mbs = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                var t = mb.GetType();

                // Try common field/property names
                FieldInfo fi = t.GetField("playerCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (fi == null)
                    fi = t.GetField("PlayerCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (fi != null && fi.FieldType == typeof(int))
                {
                    fi.SetValue(mb, count);
                    Debug.Log($"GameSettingsBootstrap: Set {t.FullName}.{fi.Name} = {count}");
                    continue;
                }

                PropertyInfo pi = t.GetProperty("PlayerCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (pi != null && pi.PropertyType == typeof(int) && pi.CanWrite)
                {
                    pi.SetValue(mb, count);
                    Debug.Log($"GameSettingsBootstrap: Set {t.FullName}.{pi.Name} = {count}");
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("GameSettingsBootstrap error: " + ex);
        }
    }
}
