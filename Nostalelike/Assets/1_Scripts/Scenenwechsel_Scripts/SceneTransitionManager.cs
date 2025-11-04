using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{

    public static SceneTransitionManager instance;
    public string targetSpawnPointID;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Stelle sicher, dass das GameObject ein Root-Objekt ist
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
            Debug.Log("SceneTransitionManager: Initialisiert und DontDestroyOnLoad gesetzt");
        }
        else
        {
            Debug.Log("SceneTransitionManager: Duplikat gefunden und wird zerstört");
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // Reset der Instanz wenn dieser Manager zerstört wird
        if (instance == this)
        {
            instance = null;
            Debug.Log("SceneTransitionManager: Wurde zerstört - Instanz zurückgesetzt");
        }
    }
    
    /// <summary>
    /// Erstellt einen SceneTransitionManager falls noch keiner existiert
    /// </summary>
    public static SceneTransitionManager EnsureInstance()
    {
        if (instance == null)
        {
            // Suche ob einer in der Szene existiert
            instance = FindFirstObjectByType<SceneTransitionManager>();
            
            if (instance == null)
            {
                // Erstelle einen neuen
                GameObject go = new GameObject("SceneTransitionManager");
                instance = go.AddComponent<SceneTransitionManager>();
                Debug.Log("SceneTransitionManager: Automatisch erstellt");
            }
        }
        return instance;
    }
}


