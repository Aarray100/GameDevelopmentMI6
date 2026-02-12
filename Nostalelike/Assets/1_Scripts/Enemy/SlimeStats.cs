using UnityEngine;

/// <summary>
/// Spezielle Stats für Slimes - geben 30% mehr XP als normale Gegner!
/// </summary>
public class SlimeStats : EnemyStats
{
    protected override void Awake()
    {
        xpBonusMultiplier = 1.3f;
        base.Awake();
    }
}
