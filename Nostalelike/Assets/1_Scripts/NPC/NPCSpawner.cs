using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawnt alle Charaktere als NPCs, außer den vom Spieler ausgewählten Charakter.
/// Die NPCs laufen in der Stadt herum (nutzen NPC_Wander oder NPCBehavior).
/// </summary>
public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabs (Index 0-4 entspricht den 5 Charakteren)")]
    [Tooltip("NPC_prefab0 - Charakter 0 als NPC")]
    public GameObject npcPrefab0;
    [Tooltip("NPC_prefab1 - Charakter 1 als NPC")]
    public GameObject npcPrefab1;
    [Tooltip("NPC_prefab2 - Charakter 2 als NPC")]
    public GameObject npcPrefab2;
    [Tooltip("NPC_prefab3 - Charakter 3 als NPC")]
    public GameObject npcPrefab3;
    [Tooltip("NPC_prefab4 - Charakter 4 als NPC")]
    public GameObject npcPrefab4;

    [Header("Spawn Einstellungen")]
    [Tooltip("Spawn-Punkte für die NPCs.")]
    public Transform[] spawnPoints;
    
    [Tooltip("Falls keine Spawn-Punkte gesetzt sind, spawne in diesem Bereich um den Spawner")]
    public float spawnRadius = 5f;
    
    [Header("NPC Anzahl (pro Charakter-Typ)")]
    [Tooltip("Wie viele NPCs von Charakter 0 spawnen")]
    [Min(0)] public int spawnCount0 = 1;
    [Tooltip("Wie viele NPCs von Charakter 1 spawnen")]
    [Min(0)] public int spawnCount1 = 1;
    [Tooltip("Wie viele NPCs von Charakter 2 spawnen")]
    [Min(0)] public int spawnCount2 = 1;
    [Tooltip("Wie viele NPCs von Charakter 3 spawnen")]
    [Min(0)] public int spawnCount3 = 1;
    [Tooltip("Wie viele NPCs von Charakter 4 spawnen")]
    [Min(0)] public int spawnCount4 = 1;

    [Header("Wander Einstellungen (für NPC_Wander Komponente)")]
    [Tooltip("Breite des Wanderbereichs für jeden NPC")]
    public float wanderWidth = 8f;
    [Tooltip("Höhe des Wanderbereichs für jeden NPC")]
    public float wanderHeight = 8f;
    [Tooltip("Bewegungsgeschwindigkeit der NPCs")]
    public float npcSpeed = 2f;
    [Tooltip("Pause zwischen Bewegungen")]
    public float pauseDuration = 2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private List<GameObject> spawnedNPCs = new List<GameObject>();
    
    // Entfernt: Singleton Pattern - jetzt können mehrere Spawner existieren
    private bool hasSpawned = false;

    private void Start()
    {
        // Jeder Spawner spawnt einmal seine NPCs
        if (!hasSpawned)
        {
            SpawnNPCs();
            hasSpawned = true;
        }
    }

    /// <summary>
    /// Spawnt alle NPCs außer dem ausgewählten Spieler-Charakter
    /// </summary>
    public void SpawnNPCs()
    {
        // Hole den vom Spieler ausgewählten Charakter-Index
        int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSpawner] Spieler hat Charakter {selectedCharacterIndex} gewählt. Spawne die anderen als NPCs.");
        }

        // Array aller NPC-Prefabs und deren Spawn-Anzahlen
        GameObject[] allNPCPrefabs = new GameObject[]
        {
            npcPrefab0,
            npcPrefab1,
            npcPrefab2,
            npcPrefab3,
            npcPrefab4
        };
        
        int[] spawnCounts = new int[]
        {
            spawnCount0,
            spawnCount1,
            spawnCount2,
            spawnCount3,
            spawnCount4
        };

        int spawnPointIndex = 0;
        int totalNPCIndex = 0;

        // Gehe durch alle 5 Charaktere
        for (int i = 0; i < allNPCPrefabs.Length; i++)
        {
            // Überspringe den ausgewählten Spieler-Charakter
            if (i == selectedCharacterIndex)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCSpawner] Überspringe Charakter {i} (ist der Spieler-Charakter)");
                }
                continue;
            }

            // Prüfe ob das Prefab zugewiesen ist
            if (allNPCPrefabs[i] == null)
            {
                Debug.LogWarning($"[NPCSpawner] NPC Prefab {i} ist nicht zugewiesen!");
                continue;
            }
            
            // Spawne die gewünschte Anzahl dieses NPC-Typs
            int countToSpawn = spawnCounts[i];
            
            for (int j = 0; j < countToSpawn; j++)
            {
                // Bestimme die Spawn-Position
                Vector3 spawnPosition = GetSpawnPosition(spawnPointIndex);
                spawnPointIndex++;

                // Spawne den NPC
                GameObject npcInstance = Instantiate(allNPCPrefabs[i], spawnPosition, Quaternion.identity);
                npcInstance.name = $"NPC_Character_{i}_{j}";
                
                // Setze den Layer auf "NPC" falls vorhanden
                int npcLayer = LayerMask.NameToLayer("NPC");
                if (npcLayer != -1)
                {
                    SetLayerRecursively(npcInstance, npcLayer);
                }

                // Konfiguriere das Wander-Verhalten
                ConfigureNPCWander(npcInstance, spawnPosition);

                // Füge zur Liste hinzu
                spawnedNPCs.Add(npcInstance);
                
                // Animator-Culling für Performance
                Animator npcAnimator = npcInstance.GetComponentInChildren<Animator>();
                if (npcAnimator != null && npcAnimator.GetComponent<AnimatorCulling>() == null)
                {
                    npcAnimator.gameObject.AddComponent<AnimatorCulling>();
                }
                
               
                
                totalNPCIndex++;

                if (showDebugInfo)
                {
                    Debug.Log($"[NPCSpawner] NPC Charakter {i} (Kopie {j+1}/{countToSpawn}) gespawnt an Position {spawnPosition}");
                }
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[NPCSpawner] Insgesamt {spawnedNPCs.Count} NPCs gespawnt.");
        }
    }

    /// <summary>
    /// Gibt eine Spawn-Position zurück (entweder vom SpawnPoint-Array oder zufällig im Radius)
    /// </summary>
    private Vector3 GetSpawnPosition(int index)
    {
        // Falls Spawn-Punkte definiert sind, nutze diese
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int pointIndex = index % spawnPoints.Length;
            if (spawnPoints[pointIndex] != null)
            {
                return spawnPoints[pointIndex].position;
            }
        }

        // Fallback: Zufällige Position im Radius um den Spawner
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        return transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
    }

    /// <summary>
    /// Konfiguriert das NPC_Wander-Verhalten auf dem NPC
    /// </summary>
    private void ConfigureNPCWander(GameObject npc, Vector3 spawnPosition)
    {
        // Versuche NPC_Wander Komponente zu finden oder hinzuzufügen
        NPC_Wander wanderComponent = npc.GetComponent<NPC_Wander>();
        
        if (wanderComponent == null)
        {
            // Falls keine NPC_Wander Komponente vorhanden, füge eine hinzu
            wanderComponent = npc.AddComponent<NPC_Wander>();
        }

        // Konfiguriere die Wander-Einstellungen
        wanderComponent.startingPosition = spawnPosition;
        wanderComponent.wanderWidth = wanderWidth;
        wanderComponent.wanderHeight = wanderHeight;
        wanderComponent.speed = npcSpeed;
        wanderComponent.pauseDuration = pauseDuration;

        // Stelle sicher, dass ein Rigidbody2D vorhanden ist und konfiguriere ihn
        Rigidbody2D rb = npc.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = npc.AddComponent<Rigidbody2D>();
        }
        
        // Konfiguriere Rigidbody für "feste" NPCs (nicht wegschiebbar)
        rb.gravityScale = 0f;
        rb.mass = 1000f;              // Sehr schwer = nicht wegschiebbar
        rb.linearDamping = 10f;       // Stoppt sofort wenn geschubst (ersetzt drag)
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Bessere Kollisionserkennung
    }

    /// <summary>
    /// Setzt den Layer für ein GameObject und alle seine Kinder
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    /// <summary>
    /// Entfernt alle gespawnten NPCs (nützlich für Szenen ohne NPCs)
    /// </summary>
    public void DespawnAllNPCs()
    {
        foreach (GameObject npc in spawnedNPCs)
        {
            if (npc != null)
            {
                Destroy(npc);
            }
        }
        spawnedNPCs.Clear();
        hasSpawned = false;

        if (showDebugInfo)
        {
            Debug.Log("[NPCSpawner] Alle NPCs entfernt.");
        }
    }

    /// <summary>
    /// Gibt die Liste der gespawnten NPCs zurück
    /// </summary>
    public List<GameObject> GetSpawnedNPCs()
    {
        return spawnedNPCs;
    }

    /// <summary>
    /// Zeichnet den Spawn-Radius im Editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Zeichne Wander-Bereich als Rechteck
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(wanderWidth, wanderHeight, 0));
    }
}
