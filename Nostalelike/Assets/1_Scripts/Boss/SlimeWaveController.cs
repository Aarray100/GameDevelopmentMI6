using UnityEngine;
using System.Collections;

public class SlimeWaveController : MonoBehaviour
{
    [Header("Kill Target")]
    [SerializeField] private int totalSlimesToDefeat = 60;
    private int slimesDefeated = 0;

    [Header("Slime Prefabs")]
    [SerializeField] private GameObject blueSlimePrefab;
    [SerializeField] private GameObject greenSlimePrefab;
    [SerializeField] private GameObject redSlimePrefab;
    [SerializeField] private GameObject yellowSlimePrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Buttons")]
    [SerializeField] private SlimeButton[] colorButtons;

    [Header("Victory")]
    [SerializeField] private GameObject bridgeToPortal;
    [SerializeField] private GameObject bridgeAntiFall;

    [Header("Camera Cinematic")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float cinematicDuration = 3f;

    [Header("Journal")]
    [SerializeField] private JournalDatabase journalDb;

    private bool victoryTriggered = false;
    private bool journal010Unlocked = false;
    private bool journal011Unlocked = false;

    void Start()
    {
        bridgeToPortal?.SetActive(false);

        // Buttons sofort aktivieren - Spieler steuert das Spawning
        foreach (var button in colorButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
                button.Activate(this);
            }
        }

        // Langsames automatisches Spawning nebenbei
        StartCoroutine(AutoSpawnSlimes());
    }

    IEnumerator AutoSpawnSlimes()
    {
        GameObject[] prefabs = { blueSlimePrefab, greenSlimePrefab, redSlimePrefab, yellowSlimePrefab };

        while (!victoryTriggered)
        {
            yield return new WaitForSeconds(60f);
            if (victoryTriggered) break;

            // 2 zufällige Slimes spawnen
            for (int i = 0; i < 2; i++)
            {
                GameObject randomPrefab = prefabs[Random.Range(0, prefabs.Length)];
                SpawnSlime(randomPrefab);
            }
        }
    }

    public void SpawnSlime(GameObject slimePrefab)
    {
        if (slimePrefab == null || spawnPoints.Length == 0) return;

        Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject slime = Instantiate(slimePrefab, randomSpawn.position, Quaternion.identity);

        // Setze Level auf Spieler-Level +3
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                EnemyStats enemyStats = slime.GetComponent<EnemyStats>();
                if (enemyStats != null)
                {
                    enemyStats.SetLevel(playerStats.currentLevel + 3);
                }
            }
        }

        // Registriere Slime-Tod
        var slimeEnemy = slime.GetComponent<EnemyHealth>();
        if (slimeEnemy != null)
        {
            slimeEnemy.gameObject.AddComponent<SlimeDeathNotifier>().Initialize(this);
        }
    }

    public void OnSlimeDefeated()
    {
        slimesDefeated++;
        Debug.Log($"Slimes besiegt: {slimesDefeated}/{totalSlimesToDefeat}");

        // Journal-Einträge bei Meilensteinen
        if (slimesDefeated >= 20 && !journal010Unlocked)
        {
            journal010Unlocked = true;
            JournalProgress.Unlock("010");
            JournalToast.Enqueue("📖 Kampfprotokoll aktualisiert");
        }

        if (slimesDefeated >= 40 && !journal011Unlocked)
        {
            journal011Unlocked = true;
            JournalProgress.Unlock("011");
            JournalToast.Enqueue("📖 Protokoll: Omnis' Entschluss");
        }

        // Victory bei 60 Kills
        if (slimesDefeated >= totalSlimesToDefeat && !victoryTriggered)
        {
            Victory();
        }
    }

    void Victory()
    {
        victoryTriggered = true;
        Debug.Log("🎉 SIEG! Alle Slimes besiegt!");
        StartCoroutine(VictorySequence());
    }

    IEnumerator VictorySequence()
    {
        yield return new WaitForSeconds(1f);

        if (bridgeAntiFall != null)
            bridgeAntiFall.SetActive(false);

        if (bridgeToPortal != null)
            bridgeToPortal.SetActive(true);

        if (cameraTarget != null && Camera.main != null)
        {
            Time.timeScale = 0f;
            yield return StartCoroutine(CameraFocusCinematic());
            Time.timeScale = 1f;
        }

        JournalProgress.Unlock("012");
        JournalToast.Enqueue("📖 Der Guide: Liturgie der Ordnung");

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowNotification("SIEG! Portal freigeschaltet!");
    }

    IEnumerator CameraFocusCinematic()
    {
        Camera mainCam = Camera.main;
        Vector3 originalPosition = mainCam.transform.position;
        float elapsed = 0f;

        while (elapsed < cinematicDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / cinematicDuration;

            Vector3 targetPos = new Vector3(
                cameraTarget.position.x,
                cameraTarget.position.y,
                originalPosition.z
            );

            mainCam.transform.position = Vector3.Lerp(
                originalPosition,
                targetPos,
                t
            );

            yield return null;
        }

        yield return new WaitForSecondsRealtime(1.5f);

        elapsed = 0f;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            while (elapsed < cinematicDuration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (cinematicDuration * 0.5f);

                Vector3 playerPos = new Vector3(
                    player.transform.position.x,
                    player.transform.position.y,
                    originalPosition.z
                );

                mainCam.transform.position = Vector3.Lerp(
                    mainCam.transform.position,
                    playerPos,
                    t
                );

                yield return null;
            }
        }
    }
}

// Helper-Klasse um Slime-Tod zu registrieren
public class SlimeDeathNotifier : MonoBehaviour
{
    private SlimeWaveController controller;

    public void Initialize(SlimeWaveController waveController)
    {
        controller = waveController;
    }

    void OnDestroy()
    {
        if (controller != null)
        {
            controller.OnSlimeDefeated();
        }
    }
}
