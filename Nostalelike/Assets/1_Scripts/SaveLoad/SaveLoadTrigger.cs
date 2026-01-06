using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadTrigger : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Prüfe ob es pending Load Data gibt
        if (SaveDataHolder.PendingLoadData != null)
        {
            // Warte kurz damit alle Objekte spawnen können
            StartCoroutine(ApplyDataDelayed());
        }
    }
    
    private System.Collections.IEnumerator ApplyDataDelayed()
    {
        // Warte 2 Frames damit Character gespawnt wird
        yield return null;
        yield return null;
        
        SaveManager.Instance?.ApplyLoadedData(SaveDataHolder.PendingLoadData);
        SaveDataHolder.PendingLoadData = null;
    }
}