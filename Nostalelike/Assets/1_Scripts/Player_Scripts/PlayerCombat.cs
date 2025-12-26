using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 1.0f;
    public LayerMask enemyLayers; // Bitmaske für Filterung
    public float attackDamage = 20f; // Später aus PlayerStats holen

    [Header("Rate Limiter")]
    public float attackRate = 2f; 
    private float nextAttackTime = 0f;

    private Animator anim;
    private PlayerMovement2D playerMovement;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement2D>();
    }

    public void MeleeAttack()
    {
        // 0. Cooldown Check (Rate Limiting)
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + 1f / attackRate;

        Debug.Log("PlayerCombat: MeleeAttack called");

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>(); // Fallback: Suche irgendeine Kamera
            if (cam == null)
            {
                Debug.LogError("PlayerCombat: KEINE KAMERA GEFUNDEN! Bitte tagge deine Kamera als 'MainCamera'.");
                return;
            }
        }

        // 1. Mausposition in Weltkoordinaten holen
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // Z auf 0 setzen für 2D

        // 2. Richtung vom Spieler zur Maus berechnen
        Vector2 direction = (mousePos - transform.position).normalized;

        // 3. Charakter in die Richtung drehen
        if (playerMovement != null)
        {
            playerMovement.FaceDirection(direction);
        }
        else
        {
            Debug.LogWarning("PlayerCombat: PlayerMovement is null");
        }

        // 4. Animator Parameter setzen
        if (anim != null)
        {
            Debug.Log($"PlayerCombat: Setting Trigger 'Attack'. Dir: {direction}");
            anim.SetFloat("AttackX", direction.x);
            anim.SetFloat("AttackY", direction.y);
            anim.SetTrigger("Attack");
        }
        else
        {
            Debug.LogError("PlayerCombat: Animator is null!");
        }

        // 5. Treffererkennung (Physics Overlap)
        DetectAndDamageEnemies(transform.position, direction);
    }

    void DetectAndDamageEnemies(Vector2 origin, Vector2 direction)
    {
        // Wir berechnen den Mittelpunkt des Angriffskreises
        // Der Kreis ist um 'attackRange' in Richtung der Maus verschoben
        Vector2 attackPoint = origin + direction * 0.5f; 

        // Physics2D.OverlapCircleAll:
        // Fragt die Physics Engine (Box2D) nach allen Collidern in diesem Radius.
        // Nutzt intern einen Spatial Partitioning Algorithmus (Dynamic AABB Tree).
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            // GetComponent: Sucht im Speicherbereich des GameObjects nach der Komponente.
            // Das ist ein O(n) Lookup, aber bei wenigen Komponenten vernachlässigbar.
            EnemyHealth healthScript = enemy.GetComponent<EnemyHealth>();
            
            if (healthScript != null)
            {
                healthScript.TakeDamage(attackDamage);
            }
        }
    }

    // Visualisierung im Editor (Gizmos)
    void OnDrawGizmosSelected()
    {
        if (playerMovement == null) return;
        
        // Da wir im Editor keine Mausposition haben, zeichnen wir einfach einen Kreis um den Spieler
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
