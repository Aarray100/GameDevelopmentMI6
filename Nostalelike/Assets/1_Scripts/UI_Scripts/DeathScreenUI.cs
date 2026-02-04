using UnityEngine;

public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject deathScreenPanel;
    private PlayerStats playerStats;

    private void Start()
    {
        // Stelle sicher, dass das Panel versteckt ist
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }

        // Finde den Player und abonniere die Events
        FindAndSubscribeToPlayer();
    }

    private void FindAndSubscribeToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.OnPlayerDeath += ShowDeathScreen;
                playerStats.OnPlayerRespawn += HideDeathScreen;
                Debug.Log("DeathScreenUI: Subscribed to player events");
            }
        }
        else
        {
            Debug.LogWarning("DeathScreenUI: Player nicht gefunden, versuche erneut in 0.5s");
            Invoke(nameof(FindAndSubscribeToPlayer), 0.5f);
        }
    }

    private void ShowDeathScreen()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
            Debug.Log("Death Screen angezeigt");
        }
    }

    private void HideDeathScreen()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
            Debug.Log("Death Screen versteckt");
        }
    }

    private void OnDestroy()
    {
        // Events wieder abmelden
        if (playerStats != null)
        {
            playerStats.OnPlayerDeath -= ShowDeathScreen;
            playerStats.OnPlayerRespawn -= HideDeathScreen;
        }
    }
}
