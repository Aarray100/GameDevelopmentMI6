using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    private Animator anim;
    
    // Speichert die aktuelle Blickrichtung für Animationen (Up, Down, Left)
    private Vector2 facingDirection = Vector2.down;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
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

        if (anim != null)
        {
            SetAnimationDirection();
            anim.SetTrigger("Hurt");
        }

        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        
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
