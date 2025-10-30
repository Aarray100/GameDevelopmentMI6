using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    public string sceneToLoad;

    [Tooltip("Name der Szene, zu der gewechselt werden soll")]
    public string targetSpawnPointID;
    
   
   private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneTransitionManager.instance.targetSpawnPointID = targetSpawnPointID;
            Debug.Log("Wechsel zu Szene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }



}
