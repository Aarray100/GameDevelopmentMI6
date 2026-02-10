using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnZone : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Liste der Enemy-Prefabs die hier spawnen können")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    
    [Tooltip("Maximale Anzahl Enemies die gleichzeitig existieren dürfen")]
    public int maxEnemies = 5;
    
    [Tooltip("Zeit zwischen Spawns in Sekunden")]
    public float spawnInterval = 3f;
    
    [Tooltip("Spawn beim Start der Szene?")]
    public bool spawnOnStart = true;
    
    [Tooltip("Anzahl Enemies die beim Start gespawnt werden")]
    public int initialSpawnCount = 2;
    
    [Header("Level Scaling")]
    [Tooltip("Sollen Gegner basierend auf Spieler-Level skalieren?")]
    public bool scaleToPlayerLevel = true;
    
    [Tooltip("Minimaler Level-Offset zum Spieler (z.B. -3 = 3 Level unter Spieler möglich)")]
    public int minLevelOffset = -3;
    
    [Tooltip("Maximaler Level-Offset zum Spieler (z.B. +1 = 1 Level über Spieler möglich)")]
    public int maxLevelOffset = 1;
    
    [Tooltip("Festes Level wenn scaleToPlayerLevel = false")]
    public int fixedEnemyLevel = 1;
    
    [Header("Zone Settings")]
    [Tooltip("Größe der Spawn-Zone (Box)")]
    public Vector2 zoneSize = new Vector2(5f, 5f);
    
    [Tooltip("Nur innerhalb der Zone spawnen, nicht am Rand")]
    public float edgePadding = 0.5f;
    
    [Header("Optional Filter")]
    [Tooltip("Wenn aktiviert, werden nur bestimmte Prefabs zufällig gewählt")]
    public bool useWeightedSpawn = false;
    
    [Tooltip("Spawn-Gewichtung für jedes Prefab (gleiche Reihenfolge wie enemyPrefabs)")]
    public List<int> spawnWeights = new List<int>();
    
    // Interne Variablen
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private float nextSpawnTime = 0f;
    private bool isSpawning = true;
    private float nextCleanupTime = 0f;
    private const float cleanupInterval = 0.5f;
    
    // Spieler-Referenz für Level-Skalierung
    private PlayerStats playerStats;
    private LevelSystem levelSystem;

    void Start()
    {
        // Spieler finden für Level-Skalierung
        FindPlayerReferences();

        // Bereinige null-Einträge aus der Prefab-Liste
        CleanupPrefabList();

        // Initial Spawns
        if (spawnOnStart)
        {
            for (int i = 0; i < initialSpawnCount && i < maxEnemies; i++)
            {
                SpawnEnemy();
            }
        }
    }

    private void FindPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            levelSystem = player.GetComponent<LevelSystem>();
        }
    }

    private void CleanupPrefabList()
    {
        int originalCount = enemyPrefabs.Count;
        enemyPrefabs.RemoveAll(prefab => prefab == null);

        if (enemyPrefabs.Count < originalCount)
        {
            Debug.LogWarning($"{gameObject.name}: {originalCount - enemyPrefabs.Count} null-Einträge aus enemyPrefabs-Liste entfernt. Verbleibende Prefabs: {enemyPrefabs.Count}");
        }
    }

    private int GetPlayerLevel()
    {
        // Versuche Level von verschiedenen Quellen zu bekommen
        if (playerStats != null && playerStats.currentLevel > 0)
        {
            return playerStats.currentLevel;
        }
        if (levelSystem != null && levelSystem.level > 0)
        {
            return levelSystem.level;
        }
        return 1; // Fallback
    }

    void Update()
    {
        // Entferne zerstörte Enemies aus der Liste (nur alle 0.5 Sekunden statt jeden Frame)
        if (Time.time >= nextCleanupTime)
        {
            nextCleanupTime = Time.time + cleanupInterval;
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if (spawnedEnemies[i] == null)
                    spawnedEnemies.RemoveAt(i);
            }
        }
        
        // Spawne neue Enemies wenn unter Maximum
        if (isSpawning && Time.time >= nextSpawnTime && spawnedEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    /// <summary>
    /// Spawnt einen zufälligen Enemy aus der Liste
    /// </summary>
    public void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: Keine Enemy Prefabs zugewiesen!");
            return;
        }
        
        if (spawnedEnemies.Count >= maxEnemies)
        {
            return;
        }
        
        // Zufälliges Prefab wählen
        GameObject prefabToSpawn = GetRandomPrefab();

        // Sicherheitscheck falls Prefab null ist
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"{gameObject.name}: GetRandomPrefab() hat null zurückgegeben!");
            return;
        }

        // Zufällige Position in der Zone
        Vector2 spawnPos = GetRandomPositionInZone();
        
        // Spawnen
        GameObject newEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        
        // Animator-Culling für Performance
        Animator enemyAnimator = newEnemy.GetComponentInChildren<Animator>();
        if (enemyAnimator != null && enemyAnimator.GetComponent<AnimatorCulling>() == null)
        {
            enemyAnimator.gameObject.AddComponent<AnimatorCulling>();
        }
        
        // --- LEVEL SKALIERUNG ---
        EnemyStats enemyStats = newEnemy.GetComponent<EnemyStats>();
        if (enemyStats != null)
        {
            if (scaleToPlayerLevel)
            {
                int playerLevel = GetPlayerLevel();
                enemyStats.SetLevelBasedOnPlayer(playerLevel, minLevelOffset, maxLevelOffset);
            }
            else
            {
                enemyStats.SetLevel(fixedEnemyLevel);
            }
        }
        // ------------------------
        
        // Layer auf Enemy setzen
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            newEnemy.layer = enemyLayer;
            foreach (Transform child in newEnemy.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = enemyLayer;
            }
        }
        
        spawnedEnemies.Add(newEnemy);
        Debug.Log($"{gameObject.name}: Spawned {prefabToSpawn.name} at {spawnPos}");
    }

    /// <summary>
    /// Wählt ein zufälliges Prefab (optional gewichtet)
    /// </summary>
    GameObject GetRandomPrefab()
    {
        if (!useWeightedSpawn || spawnWeights.Count != enemyPrefabs.Count)
        {
            // Einfach zufällig
            return enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        }
        
        // Gewichtete Auswahl
        int totalWeight = 0;
        foreach (int weight in spawnWeights)
        {
            totalWeight += weight;
        }
        
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            currentWeight += spawnWeights[i];
            if (randomValue < currentWeight)
            {
                return enemyPrefabs[i];
            }
        }
        
        return enemyPrefabs[0]; // Fallback
    }

    /// <summary>
    /// Gibt eine zufällige Position innerhalb der Zone zurück
    /// </summary>
    Vector2 GetRandomPositionInZone()
    {
        float halfWidth = (zoneSize.x / 2f) - edgePadding;
        float halfHeight = (zoneSize.y / 2f) - edgePadding;
        
        float randomX = Random.Range(-halfWidth, halfWidth);
        float randomY = Random.Range(-halfHeight, halfHeight);
        
        return (Vector2)transform.position + new Vector2(randomX, randomY);
    }

    /// <summary>
    /// Stoppt das Spawning
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
    }

    /// <summary>
    /// Startet das Spawning wieder
    /// </summary>
    public void StartSpawning()
    {
        isSpawning = true;
    }

    /// <summary>
    /// Zerstört alle gespawnten Enemies
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    /// <summary>
    /// Gibt die aktuelle Anzahl gespawnter Enemies zurück
    /// </summary>
    public int GetCurrentEnemyCount()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        return spawnedEnemies.Count;
    }

    // Visualisierung im Editor
    void OnDrawGizmos()
    {
        // Zone als halbtransparentes Rechteck zeichnen
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, new Vector3(zoneSize.x, zoneSize.y, 0.1f));
        
        // Rand der Zone
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(zoneSize.x, zoneSize.y, 0.1f));
    }
    
    void OnDrawGizmosSelected()
    {
        // Spawn-Bereich (mit Padding) anzeigen wenn selektiert
        Gizmos.color = Color.green;
        Vector2 innerSize = new Vector2(zoneSize.x - edgePadding * 2, zoneSize.y - edgePadding * 2);
        Gizmos.DrawWireCube(transform.position, new Vector3(innerSize.x, innerSize.y, 0.1f));
    }
}
