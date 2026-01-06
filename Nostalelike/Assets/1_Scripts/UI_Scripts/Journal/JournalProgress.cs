using UnityEngine;

public static class JournalProgress
{
    public static bool IsUnlocked(string id) =>
        PlayerPrefs.GetInt("journal_" + id, 0) == 1; 

    public static void Unlock(string id)
    {
        PlayerPrefs.SetInt("journal_" + id, 1);
        PlayerPrefs.Save();
    }
}