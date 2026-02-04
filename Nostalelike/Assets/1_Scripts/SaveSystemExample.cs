using UnityEngine;

/// <summary>
/// Beispiel-Integration für dein Spielstand-System
/// Füge diese Methoden zu deinem SaveManager/GameManager hinzu
/// </summary>
public class SaveSystemExample : MonoBehaviour
{
    [System.Serializable]
    public class GameSaveData
    {
        public Vector3 playerPosition;
        public int health;
        public JournalSaveData journalData; // << Journal-Daten hier einfügen
        // ... weitere Spielstand-Daten
    }

    // === BEIM SPEICHERN ===
    public void SaveGame(int slot)
    {
        var saveData = new GameSaveData
        {
            playerPosition = transform.position,
            health = 100,
            journalData = JournalProgress.GetSaveData() // << Journal-Daten holen
        };

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString($"savegame_{slot}", json);
        PlayerPrefs.Save();
        
        Debug.Log($"💾 Spiel gespeichert (Slot {slot})");
    }

    // === BEIM LADEN ===
    public void LoadGame(int slot)
    {
        string json = PlayerPrefs.GetString($"savegame_{slot}", "");
        
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("Kein Spielstand gefunden - Neues Spiel");
            JournalProgress.Reset(); // << Neues Spiel = Journal zurücksetzen
            return;
        }

        var saveData = JsonUtility.FromJson<GameSaveData>(json);
        
        // Spieler wiederherstellen
        transform.position = saveData.playerPosition;
        // health = saveData.health;
        
        // Journal wiederherstellen
        JournalProgress.LoadSaveData(saveData.journalData); // << Journal laden
        
        Debug.Log($"📂 Spiel geladen (Slot {slot})");
    }

    // === NEUES SPIEL ===
    public void NewGame()
    {
        JournalProgress.Reset(); // << Journal zurücksetzen
        // ... weitere Initialisierung
        
        Debug.Log("🆕 Neues Spiel gestartet");
    }
}
