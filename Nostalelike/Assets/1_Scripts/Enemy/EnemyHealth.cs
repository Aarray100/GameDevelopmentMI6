using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Debug - Zum Testen!")]
    public bool debugTakeDamage = false;  // Im Inspector anklicken = 20 Schaden
    public bool debugKill = false;         // Im Inspector anklicken = Sofort töten

    private Animator anim;
    
    // Speichert die aktuelle Blickrichtung für Animationen (Up, Down, Left)
    private Vector2 facingDirection = Vector2.down;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // DEBUG: Im Play-Mode im Inspector anklicken zum Testen!
        if (debugTakeDamage)
        {
            debugTakeDamage = false;
            TakeDamage(20f);
        }
        if (debugKill)
        {
            debugKill = false;
            TakeDamage(currentHealth + 10f);
        }
    }

    /// <summary>
    /// Wird vom Movement-Script aufgerufen, um die Blickrichtung zu aktualisieren.
    /// </summary>
    public void SetFacingDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            facingDirection = direction.normalized;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Hit Sound abspielen
        AudioManager.Instance?.PlayHitSFX();

        if (anim != null)
        {
            SetAnimationDirection();
            // Reset trigger first to avoid stuck animations
            anim.ResetTrigger("Hurt");
            anim.SetTrigger("Hurt");
        }

        Debug.Log($"<color=red>{gameObject.name} took {damage} damage. Current HP: {currentHealth}</color>");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        
        // Death Sound abspielen
        AudioManager.Instance?.PlayEnemyDeathSFX();
        
        if (anim != null)
        {
            SetAnimationDirection();
            anim.SetTrigger("Death");
        }
        
        // Deaktiviere Kollision sofort, damit der Gegner nicht mehr getroffen werden kann
        GetComponent<Collider2D>().enabled = false;
        
        // Warte auf Death-Animation, dann zerstöre das Objekt
        Destroy(gameObject, 1f);
    }
    
    /// <summary>
    /// Setzt die Animator-Parameter für Richtungs-Animationen.
    /// Blend Tree nutzt 4 Animationen (Down, Up, Left für links, Left nochmal für rechts).
    /// </summary>
    private void SetAnimationDirection()
    {
        // Direkte Übergabe der Richtung an den Blend Tree
        anim.SetFloat("FaceX", facingDirection.x);
        anim.SetFloat("FaceY", facingDirection.y);
    }
}