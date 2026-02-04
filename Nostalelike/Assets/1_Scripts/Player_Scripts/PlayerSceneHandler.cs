using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneHandler : MonoBehaviour
{
    // Singleton Pattern um sicherzustellen, dass nur ein Player existiert
    private static PlayerSceneHandler instance;
    
    private void Awake()
    {
        // Prüfe ob bereits eine Player-Instanz existiert
        if (instance != null && instance != this)
        {
            Debug.Log("Duplicate Player detected - destroying this instance");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Reset der Instanz wenn dieser Player zerstört wird
        if (instance == this)
        {
            instance = null;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"PlayerSceneHandler: Scene loaded: {scene.name}");
        
        // WICHTIG: Wenn SaveData geladen wird, Position nicht überschreiben!
        if (SaveDataHolder.PendingLoadData != null)
        {
            Debug.Log("<color=yellow>PlayerSceneHandler: SaveData pending - skipping spawn point positioning</color>");
            return;
        }
        
        // Stelle sicher dass SceneTransitionManager existiert
        SceneTransitionManager manager = SceneTransitionManager.EnsureInstance();
        
        // Teleportiere den Spieler zum richtigen Spawn-Punkt nach Szenenwechsel
        if (manager != null && !string.IsNullOrEmpty(manager.targetSpawnPointID))
        {
            string targetID = manager.targetSpawnPointID;
            Debug.Log($"PlayerSceneHandler: Teleporting to spawn point: {targetID}");
            
            // Verwende Coroutine um sicherzustellen, dass alle Spawn-Points initialisiert sind
            StartCoroutine(TeleportAfterFrame(targetID));
            
            // Reset für nächsten Wechsel
            manager.targetSpawnPointID = "";
        }
        else
        {
            Debug.Log("PlayerSceneHandler: No target spawn point ID - staying at current position");
        }
    }
    
    // Warte einen Frame damit alle Spawn-Points garantiert initialisiert sind
    System.Collections.IEnumerator TeleportAfterFrame(string spawnPointID)
    {
        yield return null; // Warte einen Frame
        TeleportToSpawnPoint(spawnPointID);
    }
    
    public void TeleportToSpawnPoint(string spawnPointID)
    {
        // Finde den Spawn-Punkt in der Szene
        SceneSpawnPoint[] spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsSortMode.None);
        
        Debug.Log($"PlayerSceneHandler: === TELEPORT DEBUG ===");
        Debug.Log($"PlayerSceneHandler: Suche nach Spawn-Point ID: '{spawnPointID}'");
        Debug.Log($"PlayerSceneHandler: Gefunden: {spawnPoints.Length} Spawn-Points in der Szene");
        Debug.Log($"PlayerSceneHandler: Aktuelle Player Position VOR Teleport: {transform.position}");
        
        foreach (SceneSpawnPoint sp in spawnPoints)
        {
            Debug.Log($"PlayerSceneHandler: - Spawn-Point gefunden: '{sp.spawnPointID}' an Position {sp.transform.position}");
            
            if (sp.spawnPointID == spawnPointID)
            {
                Vector3 oldPosition = transform.position;
                transform.position = sp.transform.position;
                Debug.Log($"PlayerSceneHandler: ✓✓✓ MATCH! Spieler teleportiert von {oldPosition} zu '{spawnPointID}' an Position: {sp.transform.position}");
                Debug.Log($"PlayerSceneHandler: Player Position NACH Teleport: {transform.position}");
                return;
            }
        }
        
        Debug.LogWarning($"PlayerSceneHandler: ✗✗✗ FEHLER: Spawn-Punkt mit ID '{spawnPointID}' nicht gefunden!");
        Debug.LogWarning($"PlayerSceneHandler: Verfügbare IDs waren: {string.Join(", ", System.Array.ConvertAll(spawnPoints, sp => $"'{sp.spawnPointID}'"))}");
    }
}
