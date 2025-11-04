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
        // Teleportiere den Spieler zum richtigen Spawn-Punkt nach Szenenwechsel
        if (SceneTransitionManager.instance != null && 
            !string.IsNullOrEmpty(SceneTransitionManager.instance.targetSpawnPointID))
        {
            TeleportToSpawnPoint(SceneTransitionManager.instance.targetSpawnPointID);
            
            // Reset für nächsten Wechsel
            SceneTransitionManager.instance.targetSpawnPointID = "";
        }
    }
    
    void TeleportToSpawnPoint(string spawnPointID)
    {
        // Finde den Spawn-Punkt in der Szene
        SceneSpawnPoint[] spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsSortMode.None);
        
        foreach (SceneSpawnPoint sp in spawnPoints)
        {
            if (sp.spawnPointID == spawnPointID)
            {
                transform.position = sp.transform.position;
                Debug.Log("Spieler teleportiert zu: " + spawnPointID + " an Position: " + sp.transform.position);
                return;
            }
        }
        
        Debug.LogWarning("Spawn-Punkt mit ID '" + spawnPointID + "' nicht gefunden!");
    }
}
