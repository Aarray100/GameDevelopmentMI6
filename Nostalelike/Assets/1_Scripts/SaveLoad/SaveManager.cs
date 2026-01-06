using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    private const string SAVE_FOLDER = "Saves";
    private const string SAVE_EXTENSION = ".json";
    private const string DEFAULT_SAVE_NAME = "quicksave";
    
    [Header("Item Database")]
    [Tooltip("Alle Items im Spiel - wird automatisch im Editor gefüllt")]
    [SerializeField] private List<ItemData> allGameItems = new List<ItemData>();
    
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
            
            // Scene Load Event registrieren
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Debug.Log($"<color=green>SaveManager: Initialized! Save path: {SavePath}</color>");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"<color=yellow>SaveManager: Scene '{scene.name}' loaded</color>");
        
        if (SaveDataHolder.PendingLoadData != null)
        {
            Debug.Log("<color=yellow>SaveManager: Pending data found, applying after player spawn...</color>");
            StartCoroutine(ApplyDataDelayed());
        }
    }
    
    private System.Collections.IEnumerator ApplyDataDelayed()
    {
        // Warte damit Character gespawnt wird (GameCharacterSpawner braucht Zeit)
        yield return new WaitForSeconds(1.0f);
        
        // Zusätzlich warten bis Player existiert
        GameObject player = null;
        float timeout = 5f;
        float elapsed = 0f;
        
        while (player == null && elapsed < timeout)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
        }
        
        if (player == null)
        {
            Debug.LogError("SaveManager: Player not found after waiting!");
            SaveDataHolder.PendingLoadData = null;
            yield break;
        }
        
        ApplyLoadedData(SaveDataHolder.PendingLoadData);
        SaveDataHolder.PendingLoadData = null;
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
    /// Cached alle Items für schnellen Zugriff per Name
    /// </summary>
    private void CacheAllItems()
    {
        itemCache.Clear();
        
        // Zuerst aus der vordefinierten Liste (vom Editor gefüllt)
        foreach (var item in allGameItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemName))
            {
                if (!itemCache.ContainsKey(item.itemName))
                {
                    itemCache.Add(item.itemName, item);
                }
            }
        }
        
        // Zusätzlich aus Resources laden (falls Items dort liegen)
        ItemData[] resourceItems = Resources.LoadAll<ItemData>("");
        foreach (var item in resourceItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemName))
            {
                if (!itemCache.ContainsKey(item.itemName))
                {
                    itemCache.Add(item.itemName, item);
                }
            }
        }
        
        Debug.Log($"<color=cyan>SaveManager: {itemCache.Count} Items gecached</color>");
        
        // Debug: Zeige alle gecachten Items
        if (itemCache.Count > 0)
        {
            Debug.Log($"<color=cyan>SaveManager: Gecachte Items: {string.Join(", ", itemCache.Keys)}</color>");
        }
        else
        {
            Debug.LogWarning("SaveManager: KEINE Items gecached! Bitte im Inspector 'Collect All Items' Button drücken oder Items manuell zuweisen.");
        }
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor-Funktion: Findet alle ItemData im Projekt und fügt sie zur Liste hinzu
    /// </summary>
    [ContextMenu("Collect All Items From Project")]
    public void CollectAllItemsFromProject()
    {
        allGameItems.Clear();
        
        // Finde alle ItemData Assets im Projekt
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            
            if (item != null && !allGameItems.Contains(item))
            {
                allGameItems.Add(item);
            }
        }
        
        // Sortiere nach Name
        allGameItems = allGameItems.OrderBy(x => x.itemName).ToList();
        
        EditorUtility.SetDirty(this);
        
        Debug.Log($"<color=green>SaveManager: {allGameItems.Count} Items aus dem Projekt gesammelt!</color>");
        
        // Zeige alle gefundenen Items
        foreach (var item in allGameItems)
        {
            Debug.Log($"  - {item.itemName} ({item.itemType})");
        }
    }
    
    private void OnValidate()
    {
        // Automatisch Items sammeln wenn die Liste leer ist
        if (allGameItems.Count == 0)
        {
            // Verzögert ausführen um Fehler zu vermeiden
            EditorApplication.delayCall += () =>
            {
                if (this != null && allGameItems.Count == 0)
                {
                    CollectAllItemsFromProject();
                }
            };
        }
    }
#endif
    
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
        if (data == null)
        {
            Debug.LogError("SaveManager: ApplyLoadedData called with null data!");
            return;
        }
        
        Debug.Log($"<color=green>SaveManager: Applying loaded data...</color>");
        
        PlayerPrefs.SetInt("SelectedCharacterIndex", data.selectedCharacterIndex);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 newPos = data.playerPosition.ToVector3();
            Debug.Log($"<color=green>SaveManager: Moving player from {player.transform.position} to {newPos}</color>");
            
            player.transform.position = newPos;
            
            Debug.Log($"<color=green>SaveManager: Player position after move: {player.transform.position}</color>");
            
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.LoadSaveData(data.currentHealth, data.maxHealth, 
                                   data.currentMana, data.maxMana);
                Debug.Log($"<color=green>SaveManager: Stats loaded</color>");
            }
            
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.LoadSaveData(data.inventoryItems);
                Debug.Log($"<color=green>SaveManager: Inventory loaded ({data.inventoryItems.Count} items)</color>");
            }
            
            PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();
            if (equipment != null)
            {
                equipment.LoadSaveData(data.equippedItems);
                Debug.Log($"<color=green>SaveManager: Equipment loaded ({data.equippedItems.Count} items)</color>");
            }
        }
        else
        {
            Debug.LogError("SaveManager: Player not found during ApplyLoadedData!");
        }
        
        LevelSystem levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null)
        {
            levelSystem.LoadSaveData(data.playerLevel, data.currentXP, data.xpToNextLevel);
            Debug.Log($"<color=green>SaveManager: Level loaded</color>");
        }
        
        Hotbar hotbar = FindFirstObjectByType<Hotbar>();
        if (hotbar != null)
        {
            hotbar.LoadSaveData(data.hotbarSlots);
            Debug.Log($"<color=green>SaveManager: Hotbar loaded</color>");
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(data.musicVolume);
            AudioManager.Instance.SetSFXVolume(data.sfxVolume);
        }
        
        Debug.Log("<color=green>SaveManager: === SAVE DATA APPLIED SUCCESSFULLY ===</color>");
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