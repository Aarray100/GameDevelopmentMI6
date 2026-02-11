using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats - Werden von EnemyStats überschrieben wenn vorhanden")]
    public float maxHealth = 100f;
    private float _currentHealth;
    
    // Public Property für Health Bar Zugriff
    public float CurrentHealth => _currentHealth;
    
    [Header("XP Reward - Wird von EnemyStats überschrieben wenn vorhanden")]
    public int xpReward = 25;

    [Header("Gold Reward - Wird von Level-Formel berechnet")]
    [Tooltip("Basis-Gold bei Level 1")]
    public int baseGoldReward = 20;

    [Tooltip("Zusätzliches Gold pro Level")]
    public int goldPerLevel = 5;

    // --- NEU: Sicherung gegen Mehrfach-Drops ---
    private bool isDead = false; 
    // -------------------------------------------

    // Referenz auf EnemyStats (optional)
    private EnemyStats enemyStats;

    private Animator anim;
    private Vector2 facingDirection = Vector2.down;

    void Awake()
    {
        // Versuche EnemyStats zu finden
        enemyStats = GetComponent<EnemyStats>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // Wenn EnemyStats vorhanden, warte auf Stats-Berechnung
        if (enemyStats != null)
        {
            enemyStats.OnStatsCalculated += ApplyStatsFromEnemyStats;
            // Falls Stats bereits berechnet wurden
            ApplyStatsFromEnemyStats();
        }
        else
        {
            // Fallback: Benutze Inspector-Werte
            _currentHealth = maxHealth;
        }
    }

    private void OnDestroy()
    {
        if (enemyStats != null)
        {
            enemyStats.OnStatsCalculated -= ApplyStatsFromEnemyStats;
        }
    }

    private void ApplyStatsFromEnemyStats()
    {
        if (enemyStats == null) return;
        
        maxHealth = enemyStats.MaxHealth;
        _currentHealth = maxHealth;
        xpReward = enemyStats.XPReward;
        
        Debug.Log($"{gameObject.name}: Stats von EnemyStats geladen - HP: {maxHealth}, XP: {xpReward}");
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction != Vector2.zero) facingDirection = direction.normalized;
    }

    public void TakeDamage(float damage)
    {
        // WICHTIG: Wenn er schon tot ist, ignorieren wir weitere Treffer!
        if (isDead) return;

        _currentHealth -= damage;
        AudioManager.Instance?.PlayHitSFX();

        if (anim != null)
        {
            SetAnimationDirection();
            anim.SetTrigger("Hurt");
        }

        Debug.Log($"<color=red>{gameObject.name} took {damage} damage. HP: {_currentHealth}</color>");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // WICHTIG: Doppelte Sicherheit
        if (isDead) return;
        isDead = true; 

        Debug.Log($"{gameObject.name} died!");

        GiveXPToPlayer();
        GiveRewardsToPlayer(); // Wird jetzt garantiert nur 1x ausgeführt
        // DeductDeathPenalty() wurde ENTFERNT - das war der Bug!
        // Diese Methode sollte nur beim Spieler-Tod aufgerufen werden, nicht beim Enemy-Tod!

        AudioManager.Instance?.PlayEnemyDeathSFX();
        
        if (anim != null)
        {
            SetAnimationDirection();
            anim.SetTrigger("Death");
        }
        
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;
        
        Destroy(gameObject, 1f);
    }

    private void GiveRewardsToPlayer()
    {
        // 1. Award gold first (guaranteed reward)
        GiveGoldToEnemy();

        // 2. Then roll for item drops (RNG-based)
        // Versuche zuerst unser neues LootSystem, dann das alte EnemyLoot
        EnemyLootSystem lootSystem = GetComponent<EnemyLootSystem>();
        if (lootSystem != null)
        {
            lootSystem.DropLoot();
            return;
        }

        // Fallback für das alte Asset Pack Script
        EnemyLoot loot = GetComponent<EnemyLoot>();
        if (loot != null) loot.DropLoot();
    }
    
    private void GiveXPToPlayer()
    {
        if (xpReward <= 0) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null) playerStats.GainExperience(xpReward);
        }
    }

    private void GiveGoldToEnemy()
    {
        int goldAmount = CalculateGoldReward();

        if (goldAmount <= 0) return;

        // Award gold via GoldManager singleton
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.GoldHinzufuegen(goldAmount);
            Debug.Log($"<color=yellow>{gameObject.name} dropped {goldAmount} gold!</color>");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: GoldManager not found! Could not award {goldAmount} gold.");
        }

        // TODO: Add audio feedback when gold-specific combat SFX is added to AudioManager
    }

    private int CalculateGoldReward()
    {
        int level = 1; // Default fallback

        // Try to get level from EnemyStats if available
        if (enemyStats != null)
        {
            level = enemyStats.Level;
        }

        // Formula: baseGold + (goldPerLevel * level)
        // Example: 20 + (5 * 3) = 35 gold for level 3 enemy
        int goldAmount = baseGoldReward + (goldPerLevel * level);

        // Safety: Ensure at least 1 gold (in case of negative inspector values)
        return Mathf.Max(1, goldAmount);
    }

    private void SetAnimationDirection()
    {
        if (anim != null)
        {
            anim.SetFloat("FaceX", facingDirection.x);
            anim.SetFloat("FaceY", facingDirection.y);
        }
    }
}