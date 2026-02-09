using UnityEngine;
using System.Collections;

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
        // Tod kostet Gold!
        if (GoldManager.Instance != null)
        {
            int deathCost = 200;
            int currentGold = GoldManager.Instance.aktuellesGold;
            int actualCost = Mathf.Min(deathCost, currentGold); // Maximal das was der Spieler hat
            
            if (actualCost > 0)
            {
                GoldManager.Instance.GoldAbziehen(actualCost);
                Debug.Log($"<color=red>Tod: {actualCost} Gold verloren! (Verbleibend: {GoldManager.Instance.aktuellesGold})</color>");
            }
            else
            {
                Debug.Log("<color=red>Tod: Kein Gold zum Verlieren!</color>");
            }
        }
        
        if (deathScreenPanel != null)
        {
            StartCoroutine(ShowDeathScreenCoroutine());
        }
    }

    private IEnumerator ShowDeathScreenCoroutine()
    {
        deathScreenPanel.SetActive(true);
        
        // Force Canvas Update für Time.timeScale = 0
        Canvas.ForceUpdateCanvases();
        
        // Warte einen Frame (mit unscaled time)
        yield return null;
        
        Debug.Log($"Death Screen angezeigt - Panel aktiv: {deathScreenPanel.activeSelf}");
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
