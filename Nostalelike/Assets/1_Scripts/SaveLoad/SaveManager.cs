using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    private const string SAVE_FOLDER = "Saves";
    private const string SAVE_EXTENSION = ".json";
    private const string DEFAULT_SAVE_NAME = "quicksave";
    
    // Cache für geladene Items
    private Dictionary<string, ItemData> itemCache = new Dictionary<string, ItemData>();
    
    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSaveFolderExists();
            CacheAllItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void EnsureSaveFolderExists()
    {
        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
            Debug.Log($"Save folder created at: {SavePath}");
        }
    }
    
    /// <summary>
    /// Lädt alle ItemData ScriptableObjects aus Resources und cached sie
    /// </summary>
    private void CacheAllItems()
    {
        itemCache.Clear();
        
        // Lade alle ItemData aus allen Resources Ordnern
        ItemData[] allItems = Resources.LoadAll<ItemData>("");
        
        foreach (var item in allItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemName))
            {
                if (!itemCache.ContainsKey(item.itemName))
                {
                    itemCache.Add(item.itemName, item);
                }
            }
        }
        
        Debug.Log($"SaveManager: {itemCache.Count} Items gecached");
    }
    
    /// <summary>
    /// Findet ein Item anhand seines Namens
    /// </summary>
    public ItemData GetItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        
        if (itemCache.TryGetValue(itemName, out ItemData item))
        {
            return item;
        }
        
        Debug.LogWarning($"SaveManager: Item '{itemName}' nicht gefunden");
        return null;
    }
    
    #region Save Methods
    
    public void SaveGame(string saveName = null)
    {
        saveName ??= DEFAULT_SAVE_NAME;
        
        SaveData data = CollectSaveData();
        data.saveName = saveName;
        
        string json = JsonUtility.ToJson(data, true);
        string filePath = Path.Combine(SavePath, saveName + SAVE_EXTENSION);
        
        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Game saved successfully to: {filePath}");
            
            PlayerPrefs.SetString("LastSaveName", saveName);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }
    
    private SaveData CollectSaveData()
    {
        SaveData data = new SaveData();
        
        // Character Index
        data.selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        
        // Current Scene
        data.currentSceneName = SceneManager.GetActiveScene().name;
        
        // Player finden
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition = new Vector3Serializable(player.transform.position);
            
            // Player Stats
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                data.currentHealth = stats.currentHealth;
                data.maxHealth = stats.maxHealth;
                data.currentMana = stats.currentMana;
                data.maxMana = stats.maxMana;
            }
            
            // Inventory
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                data.inventoryItems = inventory.GetSaveData();
            }
            
            // Equipment
            PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();
            if (equipment != null)
            {
                data.equippedItems = equipment.GetSaveData();
            }
        }
        
        // Level System
        LevelSystem levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null)
        {
            data.playerLevel = levelSystem.level;
            data.currentXP = levelSystem.currentXP;
            data.xpToNextLevel = levelSystem.xpToNextLevel;
        }
        
        // Hotbar
        Hotbar hotbar = FindFirstObjectByType<Hotbar>();
        if (hotbar != null)
        {
            data.hotbarSlots = hotbar.GetSaveData();
        }
        
        // Audio Settings
        if (AudioManager.Instance != null)
        {
            data.musicVolume = AudioManager.Instance.GetMusicVolume();
            data.sfxVolume = AudioManager.Instance.GetSFXVolume();
        }
        
        return data;
    }
    
    #endregion
    
    #region Load Methods
    
    public void LoadGame(string saveName = null)
    {
        saveName ??= PlayerPrefs.GetString("LastSaveName", DEFAULT_SAVE_NAME);
        
        string filePath = Path.Combine(SavePath, saveName + SAVE_EXTENSION);
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save file not found: {filePath}");
            return;
        }
        
        try
        {
            string json = File.ReadAllText(filePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            Debug.Log($"Loading save: {data.saveName} from {data.saveDate}");
            
            SaveDataHolder.PendingLoadData = data;
            
            if (!string.IsNullOrEmpty(data.currentSceneName))
            {
                SceneManager.LoadScene(data.currentSceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
        }
    }
    
    public void ApplyLoadedData(SaveData data)
    {
        if (data == null) return;
        
        PlayerPrefs.SetInt("SelectedCharacterIndex", data.selectedCharacterIndex);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.playerPosition.ToVector3();
            
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.LoadSaveData(data.currentHealth, data.maxHealth, 
                                   data.currentMana, data.maxMana);
            }
            
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.LoadSaveData(data.inventoryItems);
            }
            
            PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();
            if (equipment != null)
            {
                equipment.LoadSaveData(data.equippedItems);
            }
        }
        
        LevelSystem levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null)
        {
            levelSystem.LoadSaveData(data.playerLevel, data.currentXP, data.xpToNextLevel);
        }
        
        Hotbar hotbar = FindFirstObjectByType<Hotbar>();
        if (hotbar != null)
        {
            hotbar.LoadSaveData(data.hotbarSlots);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(data.musicVolume);
            AudioManager.Instance.SetSFXVolume(data.sfxVolume);
        }
        
        Debug.Log("Save data applied successfully!");
    }
    
    #endregion
    
    #region Save File Management
    
    public List<string> GetAllSaveFiles()
    {
        List<string> saves = new List<string>();
        
        if (Directory.Exists(SavePath))
        {
            string[] files = Directory.GetFiles(SavePath, "*" + SAVE_EXTENSION);
            foreach (string file in files)
            {
                saves.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        
        return saves;
    }
    
    public void DeleteSave(string saveName)
    {
        string filePath = Path.Combine(SavePath, saveName + SAVE_EXTENSION);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"Deleted save: {saveName}");
        }
    }
    
    public bool SaveExists(string saveName)
    {
        string filePath = Path.Combine(SavePath, saveName + SAVE_EXTENSION);
        return File.Exists(filePath);
    }
    
    #endregion
}

public static class SaveDataHolder
{
    public static SaveData PendingLoadData { get; set; }
}