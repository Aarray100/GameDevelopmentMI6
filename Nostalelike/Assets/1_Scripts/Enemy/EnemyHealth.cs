using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    private Animator anim;

    void Start()
    {
        // Initialisierung: Wir laden den Wert aus dem RAM in den Cache
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(float damage)
    {
        // Subtraktion auf der ALU (Arithmetic Logic Unit)
        currentHealth -= damage;

        // Animation Trigger setzen
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {currentHealth}");

        // Branch Prediction: Die CPU versucht zu raten, ob dieser Block ausgeführt wird
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        
        // Animation Trigger könnte hier hin
        // Loot Drop Logik könnte hier hin

        // Markiert das Objekt für den Garbage Collector bzw. entfernt es aus der Szenen-Hierarchie
        Destroy(gameObject);
    }
}
