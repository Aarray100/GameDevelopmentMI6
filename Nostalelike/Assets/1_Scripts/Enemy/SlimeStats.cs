using UnityEngine;

/// <summary>
/// Spezielle Stats für Slimes - geben 30% mehr XP als normale Gegner!
/// Ersetze das normale EnemyStats-Component durch dieses Script bei allen Slime-Prefabs.
/// </summary>
public class SlimeStats : EnemyStats
{
    [Header("Slime Bonus")]
    [Tooltip("Slimes geben zusätzliche XP! (1.3 = 30% mehr XP)")]
    [SerializeField] private float xpBonusMultiplier = 1.3f;

    /// <summary>
    /// Überschreibt die normale XP-Berechnung und fügt den Slime-Bonus hinzu.
    /// </summary>
    private void OnEnable()
    {
        // Registriere uns für Stats-Berechnungen
        OnStatsCalculated += ApplySlimeBonus;
    }

    private void OnDisable()
    {
        OnStatsCalculated -= ApplySlimeBonus;
    }

    private void ApplySlimeBonus()
    {
        // Erhöhe die XP-Belohnung um 30%
        int originalXP = XPReward;
        int bonusXP = Mathf.RoundToInt(originalXP * xpBonusMultiplier);
        
        // Nutze Reflection um das readonly Property zu setzen
        // (Alternative: Mache XPReward in EnemyStats protected settable)
        var xpProperty = typeof(EnemyStats).GetProperty("XPReward");
        if (xpProperty != null)
        {
            xpProperty.SetValue(this, bonusXP);
            Debug.Log($"<color=cyan>{gameObject.name} (Slime): XP erhöht von {originalXP} auf {bonusXP} (+30%)</color>");
        }
    }
}
