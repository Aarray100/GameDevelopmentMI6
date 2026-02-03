using UnityEngine;

/// <summary>
/// Speichert die Map-Informationen für eine Szene.
/// Dieses Script kommt auf ein leeres GameObject in JEDER Szene, die eine Minimap hat.
/// </summary>
public class MapSceneInfo : MonoBehaviour
{
    [Header("Map Einstellungen")]
    [Tooltip("Das statische Bild (Sprite), das diese Szene repräsentiert.")]
    public Sprite mapSprite;

    [Header("Welt Grenzen definieren")]
    [Tooltip("Ziehe hier einen BoxCollider rein, der den Bereich der Map visuell abdeckt.")]
    public BoxCollider boundaryReference;

    /// <summary>
    /// Gibt die Welt-Grenzen zurück, die durch den BoxCollider definiert sind
    /// </summary>
    public Bounds WorldBounds
    {
        get
        {
            if (boundaryReference == null)
            {
                Debug.LogError("MapSceneInfo: Boundary Reference fehlt! Bitte BoxCollider zuweisen.");
                return new Bounds(Vector3.zero, Vector3.one * 100);
            }
            return boundaryReference.bounds;
        }
    }

    /// <summary>
    /// Singleton-ähnlicher Zugriff, damit das UI die aktuelle Map-Info findet
    /// </summary>
    public static MapSceneInfo Current { get; private set; }

    private void Awake()
    {
        // Wenn dieses Level geladen wird, ist DIESE Info die aktuelle
        Current = this;

        // Sicherstellen, dass der Collider nicht physikalisch stört
        if (boundaryReference != null)
        {
            boundaryReference.isTrigger = true;
        }

        Debug.Log("MapSceneInfo: Map-Informationen für Szene geladen: " + gameObject.scene.name);
    }

    private void OnValidate()
    {
        // Sicherstellen, dass der Collider als Trigger gesetzt ist
        if (boundaryReference != null && !boundaryReference.isTrigger)
        {
            boundaryReference.isTrigger = true;
            Debug.Log("MapSceneInfo: BoxCollider wurde automatisch als Trigger gesetzt.");
        }
    }

    private void OnDrawGizmos()
    {
        // Visualisiere die Map-Grenzen im Editor
        if (boundaryReference != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(boundaryReference.bounds.center, boundaryReference.bounds.size);
        }
    }
}
