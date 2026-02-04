
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Wander : MonoBehaviour
{
    [Header("Wander Area")]
    public float wanderWidth = 5f;
    public float wanderHeight = 5f;
    public Vector2 startingPosition;

    [Header("Movement")]
    public float speed = 2;
    public float pauseDuration = 1;
    [Tooltip("Minimale Pause zwischen Bewegungen")]
    public float minPauseDuration = 0.5f;
    [Tooltip("Maximale Pause zwischen Bewegungen")]
    public float maxPauseDuration = 3f;
    
    [Header("Spieler Awareness")]
    [Tooltip("Aktiviert das Hinschauen zum Spieler")]
    public bool enablePlayerAwareness = true;
    [Tooltip("Ab welcher Distanz schaut der NPC zum Spieler")]
    public float awarenessRadius = 3f;
    [Tooltip("NPC stoppt kurz wenn Spieler nah kommt")]
    public bool stopWhenPlayerNear = true;
    [Tooltip("Distanz ab der NPC stoppt")]
    public float stopDistance = 1.5f;
    
    [Header("Hindernissvermeidung (Raycast)")]
    [Tooltip("Aktiviert Raycast-basierte Hindernissvermeidung")]
    public bool enableObstacleAvoidance = true;
    [Tooltip("Wie weit der NPC nach vorne schaut")]
    public float raycastDistance = 1.5f;
    [Tooltip("Layer für Hindernisse (NPC_Collision)")]
    public LayerMask obstacleLayer;
    
    [Header("Stuck Detection")]
    [Tooltip("Aktiviert Erkennung wenn NPC feststeckt")]
    public bool enableStuckDetection = true;
    [Tooltip("Zeit in Sekunden bevor NPC als 'stuck' gilt")]
    public float stuckTimeThreshold = 1.5f;
    [Tooltip("Minimale Distanz die NPC zurücklegen muss um nicht als stuck zu gelten")]
    public float stuckDistanceThreshold = 0.3f;

    [Header("NPC Kollision")]
    [Tooltip("Aktiviert Erkennung von anderen NPCs")]
    public bool enableNPCCollisionAvoidance = true;
    [Tooltip("Abstand zu anderen NPCs")]
    public float npcAvoidanceRadius = 1f;

    [Header("Debug")]
    public bool showDebugGizmos = false;

    // Private Variablen
    public Vector2 target;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isPaused;
    private Transform playerTransform;
    private bool isLookingAtPlayer = false;
    private float initialScaleX;
    private Vector2 currentDirection;
    private int obstacleAvoidanceAttempts = 0;
    private const int maxAvoidanceAttempts = 5;
    
    // Stuck Detection Variablen
    private Vector2 lastPositionCheck;
    private float stuckTimer = 0f;
    private bool isCurrentlyStuck = false;
    
    // Flip Cooldown (verhindert ständiges Flippen)
    private float lastFlipTime = 0f;
    private const float flipCooldown = 0.3f;
    private int lastFlipDirection = 0; // -1 = links, 1 = rechts, 0 = nicht gesetzt
    
    // NPC Kollision
    private bool isAvoidingNPC = false;
    private float npcAvoidanceTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        initialScaleX = Mathf.Abs(transform.localScale.x);
        
        // Falls Layer nicht gesetzt, versuche "NPC_Collision" zu finden
        if (obstacleLayer == 0)
        {
            int layer = LayerMask.NameToLayer("NPC_Collision");
            if (layer != -1)
            {
                obstacleLayer = 1 << layer;
            }
        }
    }

    private void Start()
    {
        // Finde den Spieler (suche nach "Player" Tag)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void OnEnable()
    {
        // Setze Startposition falls nicht gesetzt
        if (startingPosition == Vector2.zero)
        {
            startingPosition = transform.position;
        }
        
        // Initialisiere Stuck Detection
        lastPositionCheck = transform.position;
        stuckTimer = 0f;
        isCurrentlyStuck = false;
        
        StartCoroutine(PauseAndPickNewDestination());
    }

    private void Update()
    {
        // Versuche Spieler zu finden falls noch nicht gefunden
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // Spieler Awareness Check
        if (enablePlayerAwareness && playerTransform != null)
        {
            HandlePlayerAwareness();
        }

        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Stoppe wenn Spieler sehr nah ist
        if (stopWhenPlayerNear && playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < stopDistance)
            {
                rb.linearVelocity = Vector2.zero;
                if (anim != null) anim.SetBool("isMoving", false);
                LookAtPlayer();
                return;
            }
        }

        // === STUCK DETECTION ===
        if (enableStuckDetection && !isPaused)
        {
            CheckIfStuck();
        }

        if (Vector2.Distance(transform.position, target) < .1f)
        {
            StartCoroutine(PauseAndPickNewDestination());
            return;
        }

        Move();
    }

    /// <summary>
    /// Prüft ob der NPC feststeckt (z.B. wenn zwei NPCs sich gegenseitig blockieren)
    /// </summary>
    private void CheckIfStuck()
    {
        float distanceMoved = Vector2.Distance(transform.position, lastPositionCheck);
        
        // Hat sich der NPC kaum bewegt?
        if (distanceMoved < stuckDistanceThreshold)
        {
            stuckTimer += Time.deltaTime;
            
            // Zu lange keine Bewegung? NPC steckt fest!
            if (stuckTimer >= stuckTimeThreshold)
            {
                isCurrentlyStuck = true;
                stuckTimer = 0f;
                
                if (showDebugGizmos)
                {
                    Debug.Log($"[NPC_Wander] {gameObject.name} steckt fest! Wähle neues Ziel...");
                }
                
                // Wähle neues Ziel und mache kurze Pause
                StartCoroutine(UnstuckRoutine());
            }
        }
        else
        {
            // NPC bewegt sich normal, reset Timer
            stuckTimer = 0f;
            isCurrentlyStuck = false;
            lastPositionCheck = transform.position;
        }
    }

    /// <summary>
    /// Routine um NPC zu "entstucken"
    /// </summary>
    private IEnumerator UnstuckRoutine()
    {
        isPaused = true;
        rb.linearVelocity = Vector2.zero;
        
        if (anim != null) anim.SetBool("isMoving", false);
        
        // Kurze Pause
        yield return new WaitForSeconds(0.3f);
        
        // Versuche ein Ziel in die ENTGEGENGESETZTE Richtung zu finden
        Vector2 awayFromTarget = ((Vector2)transform.position - target).normalized;
        Vector2 newTarget = (Vector2)transform.position + awayFromTarget * 2f;
        
        // Stelle sicher, dass neues Ziel im Wanderbereich liegt
        newTarget = ClampToWanderArea(newTarget);
        
        target = newTarget;
        lastPositionCheck = transform.position;
        isCurrentlyStuck = false;
        isPaused = false;
    }

    /// <summary>
    /// Begrenzt eine Position auf den Wanderbereich
    /// </summary>
    private Vector2 ClampToWanderArea(Vector2 position)
    {
        float halfWidth = wanderWidth / 2;
        float halfHeight = wanderHeight / 2;
        
        float clampedX = Mathf.Clamp(position.x, startingPosition.x - halfWidth, startingPosition.x + halfWidth);
        float clampedY = Mathf.Clamp(position.y, startingPosition.y - halfHeight, startingPosition.y + halfHeight);
        
        return new Vector2(clampedX, clampedY);
    }

    /// <summary>
    /// Prüft ob der Spieler in der Nähe ist und reagiert darauf
    /// </summary>
    private void HandlePlayerAwareness()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Spieler ist im Awareness-Radius
        if (distanceToPlayer <= awarenessRadius)
        {
            if (!isLookingAtPlayer)
            {
                isLookingAtPlayer = true;
            }
            
            // Wenn pausiert, zum Spieler schauen
            if (isPaused)
            {
                LookAtPlayer();
            }
        }
        else
        {
            isLookingAtPlayer = false;
        }
    }

    /// <summary>
    /// Dreht den NPC zum Spieler
    /// </summary>
    private void LookAtPlayer()
    {
        if (playerTransform == null) return;
        
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        // Sprite flippen basierend auf Spieler-Position
        if (directionToPlayer.x > 0.1f)
        {
            transform.localScale = new Vector3(initialScaleX, transform.localScale.y, transform.localScale.z);
        }
        else if (directionToPlayer.x < -0.1f)
        {
            transform.localScale = new Vector3(-initialScaleX, transform.localScale.y, transform.localScale.z);
        }
        
        // Optional: Animation Parameter für Blickrichtung setzen
        if (anim != null)
        {
            anim.SetFloat("horizontal", Mathf.Abs(directionToPlayer.x));
            anim.SetFloat("vertical", directionToPlayer.y);
        }
    }

    private void Move()
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        currentDirection = direction; // Für Gizmos speichern
        
        // === NPC KOLLISIONSVERMEIDUNG ===
        if (enableNPCCollisionAvoidance)
        {
            Vector2 avoidanceDirection = GetNPCAvoidanceDirection();
            if (avoidanceDirection != Vector2.zero)
            {
                // Mische Avoidance mit Zielrichtung
                direction = (direction + avoidanceDirection * 0.5f).normalized;
                isAvoidingNPC = true;
                npcAvoidanceTimer = 0.5f;
            }
            else if (npcAvoidanceTimer > 0)
            {
                npcAvoidanceTimer -= Time.deltaTime;
                if (npcAvoidanceTimer <= 0)
                {
                    isAvoidingNPC = false;
                }
            }
        }
        
        // === RAYCAST HINDERNISSVERMEIDUNG ===
        if (enableObstacleAvoidance && obstacleLayer != 0)
        {
            if (CheckForObstacle(direction))
            {
                // Hindernis erkannt! Versuche auszuweichen
                if (TryAvoidObstacle())
                {
                    return; // Neues Ziel gewählt, warte auf nächsten Frame
                }
            }
            else
            {
                // Kein Hindernis, Reset der Versuche
                obstacleAvoidanceAttempts = 0;
            }
        }
        
        // Sprite flippen basierend auf Bewegungsrichtung (MIT COOLDOWN)
        FlipSpriteWithCooldown(direction.x);
        
        rb.linearVelocity = direction * speed;
        
        // Animation Parameter setzen (gleiche wie PlayerMovement2D)
        if (anim != null)
        {
            anim.SetBool("isMoving", true);
            anim.SetFloat("horizontal", Mathf.Abs(direction.x));
            anim.SetFloat("vertical", direction.y);
        }
    }

    /// <summary>
    /// Flippt das Sprite mit einem Cooldown um ständiges Flippen zu verhindern
    /// </summary>
    private void FlipSpriteWithCooldown(float directionX)
    {
        // Bestimme gewünschte Richtung
        int desiredDirection = 0;
        if (directionX > 0.15f) desiredDirection = 1;      // Nach rechts
        else if (directionX < -0.15f) desiredDirection = -1; // Nach links
        
        // Keine Änderung nötig wenn Richtung gleich oder neutral
        if (desiredDirection == 0 || desiredDirection == lastFlipDirection)
        {
            return;
        }
        
        // Prüfe Cooldown
        if (Time.time - lastFlipTime < flipCooldown)
        {
            return; // Noch im Cooldown, nicht flippen
        }
        
        // Flip durchführen
        if (desiredDirection == 1)
        {
            transform.localScale = new Vector3(initialScaleX, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-initialScaleX, transform.localScale.y, transform.localScale.z);
        }
        
        lastFlipDirection = desiredDirection;
        lastFlipTime = Time.time;
    }

    /// <summary>
    /// Findet andere NPCs in der Nähe und gibt eine Ausweichrichtung zurück
    /// </summary>
    private Vector2 GetNPCAvoidanceDirection()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, npcAvoidanceRadius);
        Vector2 avoidanceDirection = Vector2.zero;
        int nearbyNPCCount = 0;
        
        foreach (Collider2D col in nearbyColliders)
        {
            // Ignoriere sich selbst
            if (col.transform == transform) continue;
            
            // Prüfe ob es ein anderer NPC ist (hat auch NPC_Wander)
            NPC_Wander otherNPC = col.GetComponent<NPC_Wander>();
            if (otherNPC != null)
            {
                // Berechne Richtung WEG vom anderen NPC
                Vector2 awayFromOther = ((Vector2)transform.position - (Vector2)col.transform.position).normalized;
                avoidanceDirection += awayFromOther;
                nearbyNPCCount++;
            }
        }
        
        if (nearbyNPCCount > 0)
        {
            return avoidanceDirection.normalized;
        }
        
        return Vector2.zero;
    }


    IEnumerator PauseAndPickNewDestination()
    {
        if (isPaused) yield break;

        isPaused = true;
        
        // Animation Parameter setzen (gleiche wie PlayerMovement2D)
        if (anim != null)
        {
            anim.SetBool("isMoving", false);
        }
        
        // Zufällige Pausendauer für natürlicheres Verhalten
        float randomPause = Random.Range(minPauseDuration, maxPauseDuration);
        yield return new WaitForSeconds(randomPause);

        target = GetRandomTarget();
        isPaused = false;
    }



    public void OnCollisionEnter2D(Collision2D collision)
    {
        // Prüfe ob es ein anderer NPC ist
        NPC_Wander otherNPC = collision.gameObject.GetComponent<NPC_Wander>();
        if (otherNPC != null)
        {
            // Bei NPC-Kollision: Sofort neues Ziel wählen
            StopAllCoroutines();
            StartCoroutine(HandleNPCCollision(collision));
            return;
        }
        
        StartCoroutine(PauseAndPickNewDestination());
    }

    /// <summary>
    /// Spezielle Behandlung für NPC-zu-NPC Kollisionen
    /// </summary>
    private IEnumerator HandleNPCCollision(Collision2D collision)
    {
        isPaused = true;
        rb.linearVelocity = Vector2.zero;
        
        if (anim != null) anim.SetBool("isMoving", false);
        
        // Sehr kurze Pause
        yield return new WaitForSeconds(0.1f);
        
        // Berechne Richtung WEG vom anderen NPC
        Vector2 awayFromOther = ((Vector2)transform.position - collision.GetContact(0).point).normalized;
        
        // Neues Ziel in Ausweichrichtung, aber mit zufälligem Offset
        float randomAngle = Random.Range(-45f, 45f);
        Vector2 rotatedDirection = RotateVector(awayFromOther, randomAngle);
        Vector2 newTarget = (Vector2)transform.position + rotatedDirection * Random.Range(2f, 4f);
        
        // Stelle sicher, dass neues Ziel im Wanderbereich liegt
        target = ClampToWanderArea(newTarget);
        
        lastPositionCheck = transform.position;
        stuckTimer = 0f;
        isPaused = false;
    }

    /// <summary>
    /// Rotiert einen Vector2 um einen Winkel in Grad
    /// </summary>
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }


    /// <summary>
    /// Prüft ob ein Hindernis im Weg ist
    /// </summary>
    private bool CheckForObstacle(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            raycastDistance,
            obstacleLayer
        );
        
        return hit.collider != null;
    }

    /// <summary>
    /// Versucht dem Hindernis auszuweichen indem ein neues Ziel gewählt wird
    /// </summary>
    private bool TryAvoidObstacle()
    {
        obstacleAvoidanceAttempts++;
        
        // Nach mehreren fehlgeschlagenen Versuchen, einfach pausieren
        if (obstacleAvoidanceAttempts >= maxAvoidanceAttempts)
        {
            obstacleAvoidanceAttempts = 0;
            StartCoroutine(PauseAndPickNewDestination());
            return true;
        }
        
        // Versuche ein neues Ziel zu finden das nicht blockiert ist
        for (int i = 0; i < 8; i++)
        {
            Vector2 newTarget = GetRandomTarget();
            Vector2 newDirection = (newTarget - (Vector2)transform.position).normalized;
            
            // Prüfe ob der neue Weg frei ist
            if (!CheckForObstacle(newDirection))
            {
                target = newTarget;
                return false; // Neues Ziel gefunden, weiterlaufen
            }
        }
        
        // Kein freier Weg gefunden, pausieren
        StartCoroutine(PauseAndPickNewDestination());
        return true;
    }


    /// <summary>
    /// Wählt einen zufälligen Punkt im GESAMTEN Wanderbereich (nicht nur Rand)
    /// </summary>
    private Vector2 GetRandomTarget()
    {
        float halfWidth = wanderWidth / 2;
        float halfHeight = wanderHeight / 2;
        
        // Zufälliger Punkt im gesamten Rechteck
        float randomX = Random.Range(startingPosition.x - halfWidth, startingPosition.x + halfWidth);
        float randomY = Random.Range(startingPosition.y - halfHeight, startingPosition.y + halfHeight);
        
        return new Vector2(randomX, randomY);
    }


    private void OnDrawGizmosSelected()
    {
        // Wanderbereich (Gelb)
        Gizmos.color = Color.yellow;
        Vector2 center = startingPosition != Vector2.zero ? startingPosition : (Vector2)transform.position;
        Gizmos.DrawWireCube(center, new Vector3(wanderWidth, wanderHeight, 0));
        
        // Awareness Radius (Cyan)
        if (enablePlayerAwareness)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, awarenessRadius);
        }
        
        // Stop Distance (Rot)
        if (stopWhenPlayerNear)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }
        
        // Aktuelles Ziel (Grün)
        if (Application.isPlaying && target != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target);
            Gizmos.DrawWireSphere(target, 0.2f);
        }
        
        // Raycast Linie (Magenta wenn aktiv)
        if (enableObstacleAvoidance)
        {
            if (Application.isPlaying && currentDirection != Vector2.zero)
            {
                // Prüfe ob Hindernis getroffen wird
                bool hitObstacle = obstacleLayer != 0 && CheckForObstacle(currentDirection);
                Gizmos.color = hitObstacle ? Color.red : Color.magenta;
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + currentDirection * raycastDistance);
            }
            else
            {
                // Im Editor: Zeige Raycast-Distanz nach rechts
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.right * raycastDistance);
            }
        }
    }
}

