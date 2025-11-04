using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Stellt sicher, dass nur ein EventSystem über alle Szenen hinweg existiert.
/// Verhindert die "Multiple EventSystems" Warnung.
/// </summary>
[RequireComponent(typeof(EventSystem))]
public class PersistentEventSystem : MonoBehaviour
{
    private static PersistentEventSystem instance;
    
    private void Awake()
    {
        // Singleton Pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("PersistentEventSystem: Initialisiert und DontDestroyOnLoad gesetzt");
        }
        else
        {
            // Duplikat gefunden - zerstöre es
            Debug.Log("PersistentEventSystem: Duplikat gefunden und wird zerstört");
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
