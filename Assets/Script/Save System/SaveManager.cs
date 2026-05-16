using UnityEngine;

public static class SaveManager
{
    private const string HasSaveKey = "HasSave";
    private const string EnemyCountKey = "EnemyCount";

    public static bool HasSave(int slot)
    {
        return PlayerPrefs.GetInt(GetKey(HasSaveKey, slot), 0) == 1;
    }

    public static int LoadEnemyCount(int slot)
    {
        return PlayerPrefs.GetInt(GetKey(EnemyCountKey, slot), 1);
    }

    public static void SaveEnemyCount(int slot, int enemyCount)
    {
        PlayerPrefs.SetInt(GetKey(HasSaveKey, slot), 1);
        PlayerPrefs.SetInt(GetKey(EnemyCountKey, slot), enemyCount);
        PlayerPrefs.Save();
    }

    public static void DeleteSave(int slot)
    {
        PlayerPrefs.DeleteKey(GetKey(HasSaveKey, slot));
        PlayerPrefs.DeleteKey(GetKey(EnemyCountKey, slot));
        PlayerPrefs.Save();
    }

    private static string GetKey(string baseKey, int slot)
    {
        return $"{baseKey}_Slot_{slot}";
    }
}
