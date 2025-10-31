using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneHandler : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
