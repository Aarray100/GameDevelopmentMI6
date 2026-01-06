using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    [Header("Contact Damage")]
    public float contactDamage = 5f;         // Schaden bei Berührung
    public float contactCooldown = 1f;       // Cooldown zwischen Kontaktschaden
    
    private float nextContactDamageTime = 0f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Kontaktschaden bei Berührung mit dem Player
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextContactDamageTime)
            {
                PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(contactDamage);
                    Debug.Log($"{gameObject.name} dealt {contactDamage} contact damage!");
                }
                nextContactDamageTime = Time.time + contactCooldown;
            }
        }
    }
}
