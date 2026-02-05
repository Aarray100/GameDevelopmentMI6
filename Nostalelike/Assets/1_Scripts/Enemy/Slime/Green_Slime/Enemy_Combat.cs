using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    [Header("Contact Damage - Wird von EnemyStats überschrieben wenn vorhanden")]
    public float contactDamage = 5f;         // Schaden bei Berührung
    public float contactCooldown = 1f;       // Cooldown zwischen Kontaktschaden
    
    private float nextContactDamageTime = 0f;
    private EnemyStats enemyStats;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        // Wenn EnemyStats vorhanden, nutze skalierten Schaden
        if (enemyStats != null)
        {
            enemyStats.OnStatsCalculated += UpdateDamageFromStats;
            UpdateDamageFromStats();
        }
    }

    private void OnDestroy()
    {
        if (enemyStats != null)
        {
            enemyStats.OnStatsCalculated -= UpdateDamageFromStats;
        }
    }

    private void UpdateDamageFromStats()
    {
        if (enemyStats != null)
        {
            contactDamage = enemyStats.Damage;
        }
    }

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
