using UnityEngine;

/// <summary>
/// Deaktiviert SpriteRenderer von Objekten die zu weit vom Spieler entfernt sind.
/// Lege dieses Script auf ein Parent-GameObject das viele Kinder mit SpriteRenderers hat
/// (z.B. Dekorationen, Bäume, Hintergrund-Objekte).
/// 
/// WICHTIG: Nicht auf Enemies oder interaktive Objekte legen - nur auf rein visuelle Deko!
/// </summary>
public class CullingOptimizer : MonoBehaviour
{
    [Header("Culling Settings")]
    [Tooltip("Objekte weiter weg als diese Distanz werden unsichtbar")]
    [SerializeField] private float cullingDistance = 25f;
    
    [Tooltip("Wie oft geprüft wird (in Sekunden). Höher = weniger CPU.")]
    [SerializeField] private float checkInterval = 0.5f;

    private Transform playerTransform;
    private SpriteRenderer[] childRenderers;
    private float nextCheckTime;
    private float sqrCullingDistance;
    private bool hasPlayer = false;

    void Start()
    {
        // Squared distance einmal berechnen (sqrMagnitude ist schneller als Distance)
        sqrCullingDistance = cullingDistance * cullingDistance;
        
        // Alle SpriteRenderer in Kindern cachen
        childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        
        // Player suchen
        FindPlayer();
        
        Debug.Log($"<color=cyan>CullingOptimizer: {childRenderers.Length} Renderer auf '{gameObject.name}' überwacht</color>");
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            hasPlayer = true;
        }
    }

    void Update()
    {
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        // Player suchen falls noch nicht gefunden
        if (!hasPlayer)
        {
            FindPlayer();
            if (!hasPlayer) return;
        }

        Vector3 playerPos = playerTransform.position;

        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] == null) continue;

            float sqrDist = (childRenderers[i].transform.position - playerPos).sqrMagnitude;
            childRenderers[i].enabled = sqrDist < sqrCullingDistance;
        }
    }
}
