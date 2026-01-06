using UnityEngine;
using System.Collections;

public class Enemy_Movement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    
    [Header("Detection")]
    public float detectionRange = 5f;   // Reichweite um Spieler zu entdecken
    public float attackRange = 2.0f;    // Reichweite für Angriffs-ENTSCHEIDUNG (MUSS größer sein!)
    public float attackHitRange = 1.2f; // Reichweite für tatsächlichen TREFFER (kleiner, Spieler kann ausweichen)
    
    [Header("Combat")]
    public float attackCooldown = 1.5f;  // Zeit zwischen Angriffen
    public float attackDamage = 10f;
    public float attackWindupTime = 0.4f; // Zeit bevor der Schlag trifft (Ausweichen möglich!)
    
    [Header("Visuals")]
    public Transform visualsTransform;   // Das Sprite/Animator-Objekt zum Flippen
    
    private EnemyState enemyState;
    private float nextAttackTime = 0f;
    private Vector2 lastDirection = Vector2.down;
    private float initialFacingDirection = 1f;
    private float lastStableHorizontal = 1f;
    private bool isAttacking = false;    // Verhindert Spam während Attack-Animation

    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;
    private EnemyHealth enemyHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        
        // Visuals Transform automatisch finden falls nicht zugewiesen
        if (visualsTransform == null)
        {
            // Versuche SpriteRenderer zu finden
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                visualsTransform = sr.transform;
                Debug.Log($"{gameObject.name}: visualsTransform automatisch gefunden: {sr.gameObject.name}");
            }
            else
            {
                visualsTransform = transform; // Fallback auf eigenes Transform
                Debug.LogWarning($"{gameObject.name}: Kein SpriteRenderer gefunden, nutze eigenes Transform als Fallback!");
            }
        }
        
        // Initiale Blickrichtung speichern
        initialFacingDirection = Mathf.Abs(visualsTransform.localScale.x);
        lastStableHorizontal = initialFacingDirection;
        
        // Spieler-Referenz holen
        TryFindPlayer();
        
        ChangeState(EnemyState.Idle);
    }
    
    /// <summary>
    /// Versucht den Player zu finden. Wird in Start() und Update() aufgerufen,
    /// falls der Player erst später gespawnt wird.
    /// </summary>
    void TryFindPlayer()
    {
        if (player != null) return; // Bereits gefunden
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"{gameObject.name}: Player gefunden: {playerObj.name}");
        }
    }

    void Update()
    {
        // Falls Player noch nicht gefunden, weiter suchen
        if (player == null)
        {
            TryFindPlayer();
            return; // Warte bis Player gefunden
        }
        
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
        // Combat Music aktivieren
        AudioManager.Instance?.EnterCombat();
        
        // Spieler außer Reichweite? Zurück zu Idle
        if (distanceToPlayer > detectionRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

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
        
        // Sprite flippen basierend auf horizontaler Richtung
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            lastStableHorizontal = Mathf.Sign(direction.x);
            Flip(lastStableHorizontal);
        }
        
        // Animator Parameter setzen für Blend Tree
        UpdateAnimatorDirection(direction);
    }
    
    void HandleAttackingState(float distanceToPlayer)
    {
        AudioManager.Instance?.EnterCombat();
        rb.linearVelocity = Vector2.zero;
        
        // IMMER Richtung zum Spieler aktualisieren (auch während Angriff für nächsten Angriff)
        if (player != null && !isAttacking)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            lastDirection = directionToPlayer;
            
            // Sprite flippen und Animator aktualisieren
            if (Mathf.Abs(directionToPlayer.x) > 0.1f)
            {
                lastStableHorizontal = Mathf.Sign(directionToPlayer.x);
                Flip(lastStableHorizontal);
            }
            UpdateAnimatorDirection(directionToPlayer);
            
            // EnemyHealth auch aktualisieren
            if (enemyHealth != null)
            {
                enemyHealth.SetFacingDirection(directionToPlayer);
            }
        }
        
        // Während eines Angriffs nicht unterbrechen
        if (isAttacking) return;
        
        // Spieler zu weit weg? Zurück zum Verfolgen
        if (distanceToPlayer > attackRange * 2f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Angriff ausführen wenn Cooldown abgelaufen
        if (Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackWithWindup());
            nextAttackTime = Time.time + attackCooldown;
        }
    }
    
    /// <summary>
    /// Angriff mit Wind-up Zeit - Spieler kann ausweichen!
    /// </summary>
    IEnumerator AttackWithWindup()
    {
        isAttacking = true;
        
        // WICHTIG: Richtung zum Spieler DIREKT VOR dem Angriff nochmal aktualisieren!
        if (player != null)
        {
            Vector2 attackDirection = (player.position - transform.position).normalized;
            lastDirection = attackDirection;
            UpdateAnimatorDirection(attackDirection);
            
            if (Mathf.Abs(attackDirection.x) > 0.1f)
            {
                lastStableHorizontal = Mathf.Sign(attackDirection.x);
                Flip(lastStableHorizontal);
            }
        }
        
        // Animation starten
        anim.SetTrigger("Attack");
        
        Debug.Log($"{gameObject.name} beginnt Angriff in Richtung {lastDirection}! (Wind-up: {attackWindupTime}s)");
        
        // Wind-up Zeit - Spieler kann noch ausweichen!
        yield return new WaitForSeconds(attackWindupTime);
        
        // JETZT prüfen ob Spieler noch in Reichweite ist
        if (player != null)
        {
            float currentDistance = Vector2.Distance(transform.position, player.position);
        
            if (currentDistance <= attackHitRange)
            {
                // TREFFER! Spieler ist noch in Range
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(attackDamage);
                    Debug.Log($"{gameObject.name} TRIFFT für {attackDamage} Schaden!");
                }
            }
            else
            {
                // VERFEHLT! Spieler ist ausgewichen
                Debug.Log($"{gameObject.name} hat VERFEHLT! Spieler ist ausgewichen.");
            }
        }
        
        // Kurz warten bis Animation fertig ist
        yield return new WaitForSeconds(0.3f);
        
        isAttacking = false;
        
        // Nach Angriff: Prüfen ob wir weiter angreifen oder verfolgen sollen
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > attackRange)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
    }
    
    void UpdateAnimatorDirection(Vector2 direction)
    {
        // Für Blend Tree mit 4 Animationen (Down, Up, Left, Right)
        // Left-Animation wird für Right wiederverwendet (mit Sprite Flip)
        anim.SetFloat("FaceX", direction.x);
        anim.SetFloat("FaceY", direction.y);
    }
    
    /// <summary>
    /// Flippt das Sprite horizontal basierend auf der Bewegungsrichtung.
    /// Gleiche Logik wie beim Player für Konsistenz.
    /// </summary>
    void Flip(float horizontalDirection)
    {
        if (visualsTransform == null) return;

        float targetScaleX = visualsTransform.localScale.x;

        // INVERTIERT: Slime-Sprite schaut standardmäßig nach links
        if (horizontalDirection > 0) 
            targetScaleX = -Mathf.Abs(initialFacingDirection); // Rechts = negative Scale (geflippt)
        else if (horizontalDirection < 0) 
            targetScaleX = Mathf.Abs(initialFacingDirection);  // Links = positive Scale (normal)

        if (!Mathf.Approximately(visualsTransform.localScale.x, targetScaleX))
        {
            visualsTransform.localScale = new Vector3(
                targetScaleX, 
                visualsTransform.localScale.y, 
                visualsTransform.localScale.z
            );
        }
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