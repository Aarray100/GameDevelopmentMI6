using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 1.5f;          // Reichweite des Schwerts
    public float attackAngle = 90f;           // Winkel des Angriffs-Kegels (90° = halber Kreis)
    public LayerMask enemyLayers;             // Bitmaske für Filterung
    public float attackDamage = 20f;          // Schaden pro Treffer

    [Header("Rate Limiter")]
    public float attackRate = 2f; 
    private float nextAttackTime = 0f;
    
    // Für Gizmo-Visualisierung
    private Vector2 lastAttackDirection = Vector2.right;
    private float gizmoDisplayTime = 0f;

    private Animator anim;
    private PlayerMovement2D playerMovement;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement2D>();
    }

    public void MeleeAttack()
    {
        // Nicht angreifen wenn Spiel pausiert oder UI offen ist
        if (PauseMenu.IsPaused) return;
        if (JournalOverlay.IsOpen) return;
        
        // 0. Cooldown Check (Rate Limiting)
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + 1f / attackRate;

        Debug.Log("PlayerCombat: MeleeAttack called");

        // Slash Sound abspielen (immer bei Angriff)
        AudioManager.Instance?.PlaySlashSFX();

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                Debug.LogError("PlayerCombat: KEINE KAMERA GEFUNDEN!");
                return;
            }
        }

        // 1. Mausposition in Weltkoordinaten holen
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // 2. Richtung vom Spieler zur Maus berechnen
        Vector2 direction = (mousePos - transform.position).normalized;
        lastAttackDirection = direction;
        gizmoDisplayTime = Time.time + 0.3f; // Zeige Gizmo für 0.3 Sekunden

        // 3. Charakter in die Richtung drehen
        if (playerMovement != null)
        {
            playerMovement.FaceDirection(direction);
        }

        // 4. Animator Parameter setzen
        if (anim != null)
        {
            Debug.Log($"PlayerCombat: Attack in Richtung {direction}");
            anim.SetFloat("AttackX", direction.x);
            anim.SetFloat("AttackY", direction.y);
            anim.SetTrigger("Attack");
        }

        // 5. KEGEL-BASIERTE Treffererkennung
        DetectAndDamageEnemiesInArc(transform.position, direction);
    }

    /// <summary>
    /// Erkennt alle Gegner in einem Kegel vor dem Spieler.
    /// Kombiniert Distanz-Check mit Winkel-Check für realistische Schwertangriffe.
    /// </summary>
    void DetectAndDamageEnemiesInArc(Vector2 origin, Vector2 attackDirection)
    {
        // DEBUG: Zeige was wir suchen
        Debug.Log($"PlayerCombat: Suche auf Layer {enemyLayers.value} mit Range {attackRange}");
        
        // Alle Gegner in der maximalen Reichweite finden (Kreis als Vorfilter)
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(origin, attackRange, enemyLayers);
        
        // FALLBACK: Wenn keine Targets gefunden, suche ALLE Collider (Debug)
        if (potentialTargets.Length == 0)
        {
            Collider2D[] allNearby = Physics2D.OverlapCircleAll(origin, attackRange);
            Debug.LogWarning($"PlayerCombat: Keine Enemies auf Layer! Aber {allNearby.Length} andere Collider in Range.");
            foreach (var col in allNearby)
            {
                Debug.LogWarning($"  - {col.name} auf Layer {col.gameObject.layer} ({LayerMask.LayerToName(col.gameObject.layer)})");
            }
        }
        
        int hitCount = 0;
        
        foreach (Collider2D target in potentialTargets)
        {
            // Richtung zum Gegner berechnen
            Vector2 directionToEnemy = ((Vector2)target.transform.position - origin).normalized;
            float distanceToEnemy = Vector2.Distance(origin, target.transform.position);
            
            // Winkel zwischen Angriffsrichtung und Richtung zum Gegner
            float angleToEnemy = Vector2.Angle(attackDirection, directionToEnemy);
            
            Debug.Log($"PlayerCombat: Prüfe {target.name} - Dist: {distanceToEnemy:F2}, Winkel: {angleToEnemy:F1}° (Max: {attackAngle/2f}°)");
            
            // Ist der Gegner innerhalb des Angriffs-Kegels?
            if (angleToEnemy <= attackAngle / 2f)
            {
                // TREFFER!
                EnemyHealth healthScript = target.GetComponent<EnemyHealth>();
                if (healthScript != null)
                {
                    healthScript.TakeDamage(attackDamage);
                    hitCount++;
                    Debug.Log($"<color=green>PlayerCombat: {target.name} GETROFFEN!</color>");
                }
                else
                {
                    Debug.LogWarning($"PlayerCombat: {target.name} hat kein EnemyHealth Script!");
                }
            }
        }
        
        // Hit Sound nur wenn mindestens ein Treffer
        if (hitCount > 0)
        {
            AudioManager.Instance?.PlayHitSFX();
        }
        else if (hitCount == 0 && potentialTargets.Length > 0)
        {
            // Verfehlt - optional: Miss Sound
            AudioManager.Instance?.PlayMissEvadeSFX();
            Debug.Log($"PlayerCombat: {potentialTargets.Length} Gegner in Reichweite, aber keiner im Kegel!");
        }
        else if (potentialTargets.Length == 0)
        {
            Debug.Log("PlayerCombat: Keine Gegner in Reichweite (Layer-Problem?)");
        }
    }

    // ...existing code...
    // Visualisierung im Editor und während Play-Mode
    void OnDrawGizmos()
    {
        // Zeige Attack-Kegel nur kurz nach einem Angriff
        if (Time.time < gizmoDisplayTime)
        {
            DrawAttackArc(transform.position, lastAttackDirection, attackRange, attackAngle, Color.red);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Wenn selektiert, zeige immer den potenziellen Angriffs-Kegel
        Vector2 direction = lastAttackDirection;
        if (direction == Vector2.zero) direction = Vector2.right;
        
        DrawAttackArc(transform.position, direction, attackRange, attackAngle, Color.yellow);
        
        // Zeige auch die Reichweite als Kreis
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    /// <summary>
    /// Zeichnet einen Kegel/Arc zur Visualisierung des Angriffsbereichs.
    /// </summary>
    void DrawAttackArc(Vector2 origin, Vector2 direction, float radius, float angle, Color color)
    {
        Gizmos.color = color;
        
        int segments = 20;
        float halfAngle = angle / 2f;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        Vector3 prevPoint = origin;
        
        // Zeichne den Kegel
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = baseAngle - halfAngle + (angle * i / segments);
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 point = origin + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            
            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }
            
            // Linien vom Ursprung zu den Eckpunkten
            if (i == 0 || i == segments)
            {
                Gizmos.DrawLine(origin, point);
            }
            
            prevPoint = point;
        }
    }
}