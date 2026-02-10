using UnityEngine;
using System.Collections;

public class Enemy_Movement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float wanderSpeed = 1f;          // Langsamere Geschwindigkeit beim Wandern
    
    [Header("Detection")]
    public float detectionRange = 5f;       // Reichweite um Spieler zu entdecken
    public float attackRange = 2.0f;        // Reichweite für Angriffs-ENTSCHEIDUNG (MUSS größer sein!)
    public float attackHitRange = 1.0f;     // Reichweite für tatsächlichen TREFFER (kleiner, Spieler kann ausweichen)
    
    [Header("Combat")]
    public float attackCooldown = 1.5f;     // Zeit zwischen Angriffen
    public float attackDamage = 10f;
    public float attackWindupTime = 0.4f;   // Zeit bevor der Schlag trifft (Ausweichen möglich!)
    
    [Header("Wandering (wenn Spieler nicht da)")]
    public bool enableWandering = true;
    public float wanderRadius = 3f;         // Radius um Spawn-Position
    public float minWanderPause = 1f;
    public float maxWanderPause = 4f;
    
    [Header("Obstacle Avoidance")]
    public bool enableObstacleAvoidance = true;
    public float raycastDistance = 1.5f;
    public LayerMask obstacleLayer;
    public float avoidanceStrength = 2f;    // Wie stark der Enemy ausweicht
    
    [Header("Visuals")]
    public Transform visualsTransform;      // Das Sprite/Animator-Objekt zum Flippen
    
    private EnemyState enemyState;
    private float nextAttackTime = 0f;
    private Vector2 lastDirection = Vector2.down;
    private float initialFacingDirection = 1f;
    private float lastStableHorizontal = 1f;
    private bool isAttacking = false;       // Verhindert Spam während Attack-Animation

    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;
    private EnemyHealth enemyHealth;
    
    // Wandering Variablen
    private Vector2 spawnPosition;
    private Vector2 wanderTarget;
    private bool isWandering = false;
    private float wanderPauseTimer = 0f;
    
    // Obstacle Avoidance
    private Vector2 lastAvoidanceDirection = Vector2.zero;
    private float avoidanceCooldown = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        
        // Spawn-Position für Wandering speichern
        spawnPosition = transform.position;
        
        // Obstacle Layer automatisch finden falls nicht gesetzt
        if (obstacleLayer == 0)
        {
            int layer = LayerMask.NameToLayer("NPC_Collision");
            if (layer != -1)
            {
                obstacleLayer = 1 << layer;
            }
        }
        
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
        // Spieler entdeckt?
        if (distanceToPlayer <= detectionRange)
        {
            isWandering = false;
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // Wandering Logik
        if (enableWandering)
        {
            HandleWandering();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// Zufälliges Herumwandern wenn kein Spieler in der Nähe
    /// </summary>
    void HandleWandering()
    {
        // Pause-Timer läuft?
        if (wanderPauseTimer > 0)
        {
            wanderPauseTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            
            // Idle Animation
            if (anim != null)
            {
                anim.SetBool("isChasing", false);
            }
            return;
        }
        
        // Neues Ziel wählen wenn keins vorhanden
        if (!isWandering)
        {
            PickNewWanderTarget();
            isWandering = true;
        }
        
        // Zum Wander-Ziel bewegen
        float distanceToTarget = Vector2.Distance(transform.position, wanderTarget);
        
        if (distanceToTarget < 0.3f)
        {
            // Ziel erreicht - Pause machen
            isWandering = false;
            wanderPauseTimer = Random.Range(minWanderPause, maxWanderPause);
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Bewegung zum Ziel mit Hindernissvermeidung
        Vector2 direction = (wanderTarget - (Vector2)transform.position).normalized;
        
        if (enableObstacleAvoidance)
        {
            direction = ApplyObstacleAvoidance(direction);
        }
        
        rb.linearVelocity = direction * wanderSpeed;
        
        // Animation für Bewegung
        if (anim != null)
        {
            anim.SetBool("isChasing", true); // Nutzt dieselbe Walk-Animation
        }
        
        // Sprite flippen
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            lastStableHorizontal = Mathf.Sign(direction.x);
            Flip(lastStableHorizontal);
        }
        UpdateAnimatorDirection(direction);
    }
    
    /// <summary>
    /// Wählt ein zufälliges Wander-Ziel im Radius um die Spawn-Position
    /// </summary>
    void PickNewWanderTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = spawnPosition + randomOffset;
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
        
        // Spieler in Angriffsreichweite? Angreifen!
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Richtung zum Spieler berechnen
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Hindernissvermeidung anwenden
        if (enableObstacleAvoidance)
        {
            direction = ApplyObstacleAvoidance(direction);
        }
        
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
    
    /// <summary>
    /// Wendet Hindernissvermeidung auf eine Bewegungsrichtung an
    /// </summary>
    Vector2 ApplyObstacleAvoidance(Vector2 desiredDirection)
    {
        if (obstacleLayer == 0) return desiredDirection;
        
        // Cooldown für sanftere Bewegung
        if (avoidanceCooldown > 0)
        {
            avoidanceCooldown -= Time.deltaTime;
            if (lastAvoidanceDirection != Vector2.zero)
            {
                return (desiredDirection + lastAvoidanceDirection * avoidanceStrength).normalized;
            }
        }
        
        // Raycast in Bewegungsrichtung
        RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredDirection, raycastDistance, obstacleLayer);
        
        if (hit.collider != null)
        {
            // Hindernis erkannt! Berechne Ausweichrichtung
            Vector2 avoidDirection = CalculateAvoidanceDirection(desiredDirection, hit.normal);
            lastAvoidanceDirection = avoidDirection;
            avoidanceCooldown = 0.2f; // Kurzer Cooldown für sanftes Ausweichen
            
            return (desiredDirection + avoidDirection * avoidanceStrength).normalized;
        }
        
        // Zusätzliche Raycasts für breitere Erkennung (links und rechts)
        Vector2 leftRay = RotateVector(desiredDirection, 30f);
        Vector2 rightRay = RotateVector(desiredDirection, -30f);
        
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, leftRay, raycastDistance * 0.7f, obstacleLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, rightRay, raycastDistance * 0.7f, obstacleLayer);
        
        if (hitLeft.collider != null && hitRight.collider == null)
        {
            // Hindernis links - nach rechts ausweichen
            lastAvoidanceDirection = RotateVector(desiredDirection, -45f);
            avoidanceCooldown = 0.15f;
            return (desiredDirection + lastAvoidanceDirection * avoidanceStrength * 0.5f).normalized;
        }
        else if (hitRight.collider != null && hitLeft.collider == null)
        {
            // Hindernis rechts - nach links ausweichen
            lastAvoidanceDirection = RotateVector(desiredDirection, 45f);
            avoidanceCooldown = 0.15f;
            return (desiredDirection + lastAvoidanceDirection * avoidanceStrength * 0.5f).normalized;
        }
        else if (hitLeft.collider != null && hitRight.collider != null)
        {
            // Hindernisse auf beiden Seiten - Umdrehen
            lastAvoidanceDirection = -desiredDirection;
            avoidanceCooldown = 0.3f;
            return lastAvoidanceDirection;
        }
        
        // Kein Hindernis - normaler Bewegung folgen
        lastAvoidanceDirection = Vector2.zero;
        return desiredDirection;
    }
    
    /// <summary>
    /// Berechnet die beste Ausweichrichtung basierend auf der Hindernis-Normale
    /// </summary>
    Vector2 CalculateAvoidanceDirection(Vector2 moveDirection, Vector2 hitNormal)
    {
        // Reflektiere die Bewegungsrichtung an der Normale
        Vector2 reflected = Vector2.Reflect(moveDirection, hitNormal);
        
        // Wähle die Seite die mehr in Richtung Ziel geht
        Vector2 left = RotateVector(moveDirection, 90f);
        Vector2 right = RotateVector(moveDirection, -90f);
        
        // Welche Seite ist besser?
        float leftDot = Vector2.Dot(left, reflected);
        float rightDot = Vector2.Dot(right, reflected);
        
        return (leftDot > rightDot) ? left : right;
    }
    
    /// <summary>
    /// Rotiert einen Vector2 um einen Winkel in Grad
    /// </summary>
    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
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
        
        // Wander Radius (cyan) - um Spawn-Position
        if (enableWandering)
        {
            Gizmos.color = Color.cyan;
            Vector2 center = Application.isPlaying ? spawnPosition : (Vector2)transform.position;
            Gizmos.DrawWireSphere(center, wanderRadius);
        }
        
        // Raycast Distanz (magenta)
        if (enableObstacleAvoidance)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * raycastDistance);
            
            // Zeige auch seitliche Raycasts
            Vector3 left = Quaternion.Euler(0, 0, 30) * Vector3.right * raycastDistance * 0.7f;
            Vector3 right = Quaternion.Euler(0, 0, -30) * Vector3.right * raycastDistance * 0.7f;
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f); // Halbtransparent
            Gizmos.DrawLine(transform.position, transform.position + left);
            Gizmos.DrawLine(transform.position, transform.position + right);
        }
        
        // Wander Target (grün) - nur während Spielmodus
        if (Application.isPlaying && isWandering)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, wanderTarget);
            Gizmos.DrawWireSphere(wanderTarget, 0.2f);
        }
    }
}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking
}