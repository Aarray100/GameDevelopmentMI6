using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats - Werden von EnemyStats überschrieben wenn vorhanden")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    [Header("XP Reward - Wird von EnemyStats überschrieben wenn vorhanden")]
    public int xpReward = 25;

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
            currentHealth = maxHealth;
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
        currentHealth = maxHealth;
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

        currentHealth -= damage;
        AudioManager.Instance?.PlayHitSFX();

        if (anim != null)
        {
            SetAnimationDirection();
            anim.SetTrigger("Hurt");
        }

        Debug.Log($"<color=red>{gameObject.name} took {damage} damage. HP: {currentHealth}</color>");

        if (currentHealth <= 0)
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
        GiveGoldToPlayer(); // Wird jetzt garantiert nur 1x ausgeführt
        
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

    private void GiveGoldToPlayer()
    {
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
    
    private void SetAnimationDirection()
    {
        if (anim != null)
        {
            anim.SetFloat("FaceX", facingDirection.x);
            anim.SetFloat("FaceY", facingDirection.y);
        }
    }
}