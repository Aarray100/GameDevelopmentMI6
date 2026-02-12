using UnityEngine;

/// <summary>
/// Verwaltet die Stats eines Gegners basierend auf seinem Level.
/// Das Level wird vom EnemySpawnZone beim Spawnen gesetzt.
/// </summary>
public class EnemyStats : MonoBehaviour
{
    [Header("Base Stats (Level 1)")]
    [Tooltip("Basis-Gesundheit bei Level 1")]
    public float baseHealth = 50f;
    
    [Tooltip("Basis-Schaden bei Level 1")]
    public float baseDamage = 7f; // Erhöht für mehr Gefahr
    
    [Tooltip("Basis-XP Belohnung bei Level 1")]
    public int baseXPReward = 20;
    
    [Header("Enemy Type Bonus")]
    [Tooltip("XP-Multiplikator für spezielle Gegnertypen (1.3 = +30% XP für Slimes)")]
    [Range(1.0f, 2.0f)]
    public float xpBonusMultiplier = 1.0f; // 1.0 = normal, 1.3 = +30% für Slimes
    
    [Header("Current Level & Stats (Calculated)")]
    [SerializeField] private int _level = 1;
    public int Level 
    { 
        get => _level; 
        private set => _level = Mathf.Max(1, value); 
    }
    
    // Berechnete Stats (werden nach Level-Zuweisung aktualisiert)
    public float MaxHealth { get; private set; }
    public float Damage { get; private set; }
    public int XPReward { get; private set; }
    
    [Header("Scaling Settings")]
    [Tooltip("Wie stark die Stats pro Level steigen (0.08 = 8% pro Level)")]
    [Range(0.03f, 0.15f)]
    public float statsPerLevelMultiplier = 0.11f; // 11% pro Level - Gegner skalieren stärker
    [Header("Defense")]
    [Tooltip("Rüstung des Gegners. Reduziert eingehenden Schaden prozentual.")]
    public float defense = 10f; // Gegner haben jetzt Rüstung
    
    [Tooltip("Wie stark der XP-Reward pro Level steigt (0.12 = 12% pro Level)")]
    [Range(0.05f, 0.20f)]
    public float xpPerLevelMultiplier = 0.12f;
    
    // Events
    public event System.Action OnStatsCalculated;
    
    private bool statsInitialized = false;

    protected virtual void Awake()
    {
        // Fallback: Falls Level nicht von außen gesetzt wird
        if (!statsInitialized)
        {
            CalculateStats();
        }
    }

    /// <summary>
    /// Setzt das Level des Gegners und berechnet alle Stats neu.
    /// Wird normalerweise vom EnemySpawnZone aufgerufen.
    /// </summary>
    public void SetLevel(int newLevel)
    {
        Level = Mathf.Max(1, newLevel);
        CalculateStats();
    }

    /// <summary>
    /// Setzt das Level basierend auf dem Spieler-Level mit zufälliger Variation.
    /// </summary>
    /// <param name="playerLevel">Das aktuelle Level des Spielers</param>
    /// <param name="minOffset">Minimaler Offset (z.B. -3 für 3 Level unter Spieler)</param>
    /// <param name="maxOffset">Maximaler Offset (z.B. +1 für 1 Level über Spieler)</param>
    public void SetLevelBasedOnPlayer(int playerLevel, int minOffset = -3, int maxOffset = 1)
    {
        int newLevel = playerLevel + Random.Range(minOffset, maxOffset + 1);
        SetLevel(Mathf.Max(1, newLevel)); // Mindestens Level 1
    }

    /// <summary>
    /// Berechnet alle Stats basierend auf dem aktuellen Level.
    /// Gegner skalieren schwächer als der Spieler (8% vs ~15% beim Spieler).
    /// </summary>
    private void CalculateStats()
    {
        // Skalierungsformel: baseStat * (1 + (level-1) * multiplier)
        // Bei Level 10 mit 8% Multiplier: baseStat * 1.72
        // Zum Vergleich: Spieler bei Level 10 mit ~15%: baseStat * 2.35
        
        float levelMultiplier = 1f + ((Level - 1) * statsPerLevelMultiplier);
        float xpLevelMultiplier = 1f + ((Level - 1) * xpPerLevelMultiplier);
        
        MaxHealth = Mathf.Round(baseHealth * levelMultiplier);
        Damage = Mathf.Round(baseDamage * levelMultiplier * 10f) / 10f; // Eine Dezimalstelle
        XPReward = Mathf.RoundToInt(baseXPReward * xpLevelMultiplier * xpBonusMultiplier); // XP-Bonus für spezielle Gegnertypen
        
        statsInitialized = true;
        OnStatsCalculated?.Invoke();
        
        // Log mit Bonus-Hinweis
        string bonusInfo = xpBonusMultiplier > 1.0f ? $" <color=yellow>(+{(xpBonusMultiplier - 1f) * 100}% XP Bonus!)</color>" : "";
        Debug.Log($"<color=orange>{gameObject.name} Stats berechnet:</color> " +
                  $"Level {Level} | HP: {MaxHealth} | DMG: {Damage} | XP: {XPReward}{bonusInfo}");
    }

    /// <summary>
    /// Gibt eine Übersicht der aktuellen Stats zurück (für UI/Debug).
    /// </summary>
    public string GetStatsInfo()
    {
        return $"Lv.{Level} | HP: {MaxHealth} | ATK: {Damage}";
    }
}

/*
 * =====================================================
 * BALANCING VORSCHLAG - Gegner vs Spieler Skalierung
 * =====================================================
 * 
 * GEGNER (8% pro Level):
 * Level 1:  HP 50,  DMG 5,   XP 20
 * Level 5:  HP 66,  DMG 6.6, XP 30
 * Level 10: HP 86,  DMG 8.6, XP 42
 * Level 15: HP 106, DMG 10.6, XP 55
 * Level 20: HP 126, DMG 12.6, XP 70
 * 
 * SPIELER (ca. 15% pro Level - geschätzt):
 * Level 1:  HP 100, DMG 10
 * Level 5:  HP 160, DMG 16
 * Level 10: HP 235, DMG 23.5
 * Level 15: HP 310, DMG 31
 * Level 20: HP 385, DMG 38.5
 * 
 * ERGEBNIS:
 * - Spieler skaliert ca. 1.9x stärker als Gegner
 * - Ein Level 10 Spieler macht ~2.7x mehr Schaden als ein Level 10 Gegner
 * - Spieler muss trotzdem aufpassen bei mehreren Gegnern
 * - Höhere Gegner bleiben gefährlich, niedrigere werden leichter
 * 
 * ANPASSUNGEN:
 * - statsPerLevelMultiplier erhöhen für härtere Gegner
 * - baseHealth/baseDamage pro Enemy-Typ anpassen (Boss höher, etc.)
 * =====================================================
 */
