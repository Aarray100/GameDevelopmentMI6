using UnityEngine;
using System.Collections.Generic;

public static class JournalProgress
{
    // Runtime-Dictionary (wird bei Spielneustart zurückgesetzt)
    static HashSet<string> unlockedEntries = new HashSet<string>();
    static bool firstStartDone = false;

    public static bool IsUnlocked(string id) => unlockedEntries.Contains(id);

    public static void Unlock(string id)
    {
        if (!unlockedEntries.Contains(id))
        {
            unlockedEntries.Add(id);
            Debug.Log($"📖 Journal Entry '{id}' freigeschaltet (Runtime)");
        }
    }

    public static bool IsFirstStart() => !firstStartDone;
    
    public static void MarkFirstStartDone() => firstStartDone = true;

    // Reset für Spiel-Neustart (oder beim Laden eines Spielstands)
    public static void Reset()
    {
        unlockedEntries.Clear();
        firstStartDone = false;
    }

    // === Für Spielstand-System ===
    
    /// <summary>
    /// Speichere Journal-Daten in deinem Spielstand
    /// Rufe dies auf wenn der Spieler speichert
    /// </summary>
    public static JournalSaveData GetSaveData()
    {
        return new JournalSaveData
        {
            unlockedIds = new List<string>(unlockedEntries),
            firstStartDone = firstStartDone
        };
    }

    /// <summary>
    /// Lade Journal-Daten aus einem Spielstand
    /// Rufe dies auf wenn der Spieler lädt
    /// </summary>
    public static void LoadSaveData(JournalSaveData data)
    {
        if (data == null)
        {
            Reset();
            return;
        }

        unlockedEntries = new HashSet<string>(data.unlockedIds);
        firstStartDone = data.firstStartDone;
        Debug.Log($"📖 Journal geladen: {unlockedEntries.Count} Einträge freigeschaltet");
    }
}

/// <summary>
/// Serialisierbare Daten für Spielstand-System
/// </summary>
[System.Serializable]
public class JournalSaveData
{
    public List<string> unlockedIds = new List<string>();
    public bool firstStartDone = false;
}