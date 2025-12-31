using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    
    [Header("Detection")]
    public float detectionRange = 5f;   // Reichweite um Spieler zu entdecken
    public float attackRange = 1f;       // Reichweite für Angriff
    
    [Header("Combat")]
    public float attackCooldown = 1.5f;  // Zeit zwischen Angriffen
    public float attackDamage = 10f;
    
    private EnemyState enemyState;
    private float nextAttackTime = 0f;
    private Vector2 lastDirection = Vector2.down;

    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;
    private EnemyHealth enemyHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        
        // Spieler-Referenz holen
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // State Machine Logik
        switch (enemyState)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer);
                break;
                
            case EnemyState.Chasing:
                HandleChasingState(distanceToPlayer);
                break;
                
            case EnemyState.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;
        }
    }
    
    void HandleIdleState(float distanceToPlayer)
    {
        rb.linearVelocity = Vector2.zero;
        
        // Spieler entdeckt?
        if (distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chasing);
        }
    }
    
    void HandleChasingState(float distanceToPlayer)
    {
        // Spieler außer Reichweite? Zurück zu Idle
        if (distanceToPlayer > detectionRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // Spieler in Angriffsreichweite? Angreifen!
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Zum Spieler bewegen
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
        
        // Richtung speichern und an EnemyHealth weitergeben
        lastDirection = direction;
        if (enemyHealth != null)
        {
            enemyHealth.SetFacingDirection(direction);
        }
        
        // Animator Parameter setzen für Blend Tree
        UpdateAnimatorDirection(direction);
    }
    
    void HandleAttackingState(float distanceToPlayer)
    {
        rb.linearVelocity = Vector2.zero;
        
        // Spieler zu weit weg? Zurück zum Verfolgen
        if (distanceToPlayer > attackRange * 1.5f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Angriff ausführen wenn Cooldown abgelaufen
        if (Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }
    
    void Attack()
    {
        anim.SetTrigger("Attack");
        
        // Schaden am Spieler (wird durch Animation Event oder direkt aufgerufen)
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(attackDamage);
        }
        
        Debug.Log($"{gameObject.name} attacked player for {attackDamage} damage!");
    }
    
    void UpdateAnimatorDirection(Vector2 direction)
    {
        // Für Blend Tree mit 4 Animationen (Down, Up, Left, Right)
        // Left-Animation wird für Right wiederverwendet (mit PosX = 1)
        anim.SetFloat("FaceX", direction.x);
        anim.SetFloat("FaceY", direction.y);
    }
    
    void ChangeState(EnemyState newState)
    {
        // Exit current state
        if (enemyState == EnemyState.Idle)
        {
            anim.SetBool("isIdle", false);
        }
        else if (enemyState == EnemyState.Chasing)
        {
            anim.SetBool("isChasing", false);
        }
        else if (enemyState == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", false);
        }

        // Update state
        enemyState = newState;

        // Enter new state
        if (enemyState == EnemyState.Idle)
        {
            anim.SetBool("isIdle", true);
            rb.linearVelocity = Vector2.zero;
        }
        else if (enemyState == EnemyState.Chasing)
        {
            anim.SetBool("isChasing", true);
        }
        else if (enemyState == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", true);
        }
        
        Debug.Log($"{gameObject.name} changed state to: {newState}");
    }
    
    // Visualisierung im Editor
    void OnDrawGizmosSelected()
    {
        // Detection Range (gelb)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack Range (rot)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking
}