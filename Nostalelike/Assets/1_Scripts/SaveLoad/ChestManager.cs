using UnityEngine;
using System.Collections.Generic;

public class ChestManager : MonoBehaviour
{
    public static ChestManager Instance { get; private set; }
    
    private HashSet<string> openedChests = new HashSet<string>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("<color=green>ChestManager: Initialized!</color>");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Markiert eine Truhe als geöffnet
    /// </summary>
    public void MarkChestOpened(string chestID)
    {
        if (string.IsNullOrEmpty(chestID)) return;
        
        openedChests.Add(chestID);
        Debug.Log($"<color=cyan>ChestManager: Chest '{chestID}' marked as opened</color>");
    }
    
    /// <summary>
    /// Prüft ob eine Truhe bereits geöffnet wurde
    /// </summary>
    public bool IsChestOpened(string chestID)
    {
        if (string.IsNullOrEmpty(chestID)) return false;
        return openedChests.Contains(chestID);
    }
    
    /// <summary>
    /// Gibt die Save-Daten für alle geöffneten Truhen zurück
    /// </summary>
    public List<ChestStateData> GetSaveData()
    {
        List<ChestStateData> data = new List<ChestStateData>();
        foreach (string chestID in openedChests)
        {
            data.Add(new ChestStateData { chestID = chestID, isOpened = true });
        }
        Debug.Log($"<color=cyan>ChestManager: Saving {data.Count} opened chests</color>");
        return data;
    }
    
    /// <summary>
    /// Lädt die Save-Daten für geöffnete Truhen
    /// </summary>
    public void LoadSaveData(List<ChestStateData> data)
    {
        openedChests.Clear();
        
        if (data == null || data.Count == 0)
        {
            Debug.Log("<color=cyan>ChestManager: No chest data to load</color>");
            return;
        }
        
        foreach (var chest in data)
        {
            if (chest.isOpened && !string.IsNullOrEmpty(chest.chestID))
            {
                openedChests.Add(chest.chestID);
            }
        }
        
        Debug.Log($"<color=cyan>ChestManager: Loaded {openedChests.Count} opened chests</color>");
        
        // Aktualisiere alle Truhen in der Szene
        RefreshAllChestsInScene();
    }
    
    /// <summary>
    /// Aktualisiert alle Truhen in der aktuellen Szene basierend auf dem gespeicherten Zustand
    /// </summary>
    public void RefreshAllChestsInScene()
    {
        LootChest[] allChests = FindObjectsByType<LootChest>(FindObjectsSortMode.None);
        foreach (var chest in allChests)
        {
            chest.CheckAndApplyOpenedState();
        }
        Debug.Log($"<color=cyan>ChestManager: Refreshed {allChests.Length} chests in scene</color>");
    }
    
    /// <summary>
    /// Setzt alle Truhen zurück (für neues Spiel)
    /// </summary>
    public void ResetAllChests()
    {
        openedChests.Clear();
        Debug.Log("<color=cyan>ChestManager: All chests reset</color>");
    }
}
