using UnityEngine;

/// <summary>
/// Stellt sicher, dass das Canvas über Szenenwechsel persistent bleibt.
/// Dieses Script sollte auf dem ROOT Canvas GameObject sein.
/// </summary>
public class PersistentCanvas : MonoBehaviour
{
    private static PersistentCanvas instance;
    
    [Header("Info")]
    [Tooltip("Identifikation für Debug-Zwecke")]
    public string canvasName = "Main UI Canvas";
    
    private void Awake()
    {
        // Singleton Pattern
        if (instance == null)
        {
            instance = this;
            
            // Stelle sicher, dass dies ein Root-Objekt ist
            if (transform.parent != null)
            {
                Debug.LogWarning($"PersistentCanvas ({canvasName}) ist kein Root-Objekt! Setze auf Root.");
                transform.SetParent(null);
            }
            
            DontDestroyOnLoad(gameObject);
            Debug.Log($"PersistentCanvas ({canvasName}) set to DontDestroyOnLoad");
        }
        else
        {
            // Duplikat gefunden - zerstöre es
            Debug.Log($"PersistentCanvas Duplikat gefunden und wird zerstört: {gameObject.name}");
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // Reset der Instanz wenn dieses Canvas zerstört wird
        if (instance == this)
        {
            instance = null;
            Debug.Log($"PersistentCanvas ({canvasName}) wurde zerstört - Instanz zurückgesetzt");
        }
    }
    
    /// <summary>
    /// Prüft ob dieses Canvas noch in DontDestroyOnLoad ist
    /// </summary>
    private void Update()
    {
        // Debug-Check (nur im Editor, kann später entfernt werden)
        #if UNITY_EDITOR
        if (gameObject.scene.name != "DontDestroyOnLoad")
        {
            Debug.LogWarning($"WARNUNG: PersistentCanvas ({canvasName}) ist nicht mehr in DontDestroyOnLoad! Scene: {gameObject.scene.name}");
        }
        #endif
    }
}
