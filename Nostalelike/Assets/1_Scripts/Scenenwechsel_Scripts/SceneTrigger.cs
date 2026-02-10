using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    public string sceneToLoad;

    [Tooltip("ID des Spawn-Punkts, zu dem der Spieler teleportiert werden soll")]
    public string targetSpawnPointID;
    
   
   private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Stelle sicher dass SceneTransitionManager existiert (erstelle falls nötig)
            SceneTransitionManager manager = SceneTransitionManager.EnsureInstance();
            
            if (manager == null)
            {
                Debug.LogError("SceneTrigger: SceneTransitionManager konnte nicht erstellt werden!");
                return;
            }
            
            manager.targetSpawnPointID = targetSpawnPointID;
            Debug.Log($"SceneTrigger: Target Spawn Point ID gesetzt auf: '{targetSpawnPointID}'");
            
            // Prüfe, ob wir in der gleichen Szene bleiben
            string currentSceneName = SceneManager.GetActiveScene().name;
            
            if (sceneToLoad == currentSceneName)
            {
                // Gleiche Szene - Teleport mit Loading Screen
                Debug.Log($"SceneTrigger: Teleportiere in gleicher Szene zum Spawn-Punkt: {targetSpawnPointID}");
                if (LoadingScreen.Instance != null)
                    LoadingScreen.Instance.TeleportWithScreen(targetSpawnPointID);
                else
                    TeleportPlayerToSpawnPoint(targetSpawnPointID, other.gameObject);
            }
            else
            {
                // Andere Szene - normal laden
                Debug.Log($"SceneTrigger: Wechsel von '{currentSceneName}' zu Szene: '{sceneToLoad}' → Spawn bei: '{targetSpawnPointID}'");
                if (LoadingScreen.Instance != null)
                    LoadingScreen.Instance.LoadSceneWithScreen(sceneToLoad, targetSpawnPointID);
                else
                    SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
    
    void TeleportPlayerToSpawnPoint(string spawnPointID, GameObject player)
    {
        // Finde den Spawn-Punkt in der aktuellen Szene
        SceneSpawnPoint[] spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsSortMode.None);
        
        foreach (SceneSpawnPoint sp in spawnPoints)
        {
            if (sp.spawnPointID == spawnPointID)
            {
                player.transform.position = sp.transform.position;
                Debug.Log("Spieler teleportiert zu: " + spawnPointID + " an Position: " + sp.transform.position);
                return;
            }
        }
        
        Debug.LogWarning("Spawn-Punkt mit ID '" + spawnPointID + "' nicht gefunden!");
    }



}
