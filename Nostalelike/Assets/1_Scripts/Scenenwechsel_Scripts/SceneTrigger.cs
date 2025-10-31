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
            SceneTransitionManager.instance.targetSpawnPointID = targetSpawnPointID;
            
            // Prüfe, ob wir in der gleichen Szene bleiben
            string currentSceneName = SceneManager.GetActiveScene().name;
            
            if (sceneToLoad == currentSceneName)
            {
                // Gleiche Szene - nur teleportieren, nicht neu laden
                Debug.Log("Teleportiere zum Spawn-Punkt: " + targetSpawnPointID);
                TeleportPlayerToSpawnPoint(targetSpawnPointID, other.gameObject);
            }
            else
            {
                // Andere Szene - normal laden
                Debug.Log("Wechsel zu Szene: " + sceneToLoad);
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
