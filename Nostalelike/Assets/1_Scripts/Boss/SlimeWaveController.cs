using UnityEngine;
using System.Collections;

public class SlimeWaveController : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int totalSlimesToDefeat = 60; // Gesamt-Ziel
    private int slimesDefeated = 0;
    
    [Header("Phase Thresholds")]
    [SerializeField] private float phase2Threshold = 0.33f; // Bei 20 Slimes (33%)
    [SerializeField] private float phase3Threshold = 0.66f; // Bei 40 Slimes (66%)
    
    [Header("Slime Prefabs")]
    [SerializeField] private GameObject blueSlimePrefab;
    [SerializeField] private GameObject greenSlimePrefab;
    [SerializeField] private GameObject redSlimePrefab;
    [SerializeField] private GameObject yellowSlimePrefab;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints; // Spawn-Positionen
    
    [Header("Phase 2 Buttons")]
    [SerializeField] private SlimeButton[] colorButtons; // 4 Buttons in den Ecken
    
    [Header("Victory")]
    [SerializeField] private GameObject bridgeToPortal;
    [SerializeField] private GameObject bridgeAntiFall; // Unsichtbare Wand die deaktiviert wird
    
    [Header("Camera Cinematic")]
    [SerializeField] private Transform cameraTarget; // Das CamTarget für die Brücke
    [SerializeField] private float cinematicDuration = 3f; // Wie lange die Kamerafahrt dauert
    
    [Header("Journal")]
    [SerializeField] private JournalDatabase journalDb;
    
    private int currentPhase = 1;
    private bool phase2Started = false;
    private bool phase3Started = false;
    private int buttonsActivated = 0;
    
    void Start()
    {
        bridgeToPortal?.SetActive(false);
        
        // Buttons sichtbar machen aber inaktiv halten
        foreach (var button in colorButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
                // Button ist noch nicht interaktiv (wird in Phase 2 aktiviert)
            }
        }
        
        StartPhase1();
    }
    
    void StartPhase1()
    {
        currentPhase = 1;
        Debug.Log("🟦 PHASE 1: Erste Welle - Besiege Slimes!");
        
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowNotification("Slime-Angriff!");
        
        StartCoroutine(SpawnPhase1Slimes());
    }
    
    IEnumerator SpawnPhase1Slimes()
    {
        while (currentPhase == 1)
        {
            // Spawne abwechselnd blaue & grüne Slimes
            SpawnSlime(blueSlimePrefab);
            yield return new WaitForSeconds(5f);
            SpawnSlime(greenSlimePrefab);
            yield return new WaitForSeconds(5f);
        }
    }
    
    void SpawnSlime(GameObject slimePrefab)
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
                // Registriere Slime-Tod beim EnemyHealth-Script
        var slimeEnemy = slime.GetComponent<EnemyHealth>();
        if (slimeEnemy != null)
        {
            // Füge temporären Listener hinzu
            slimeEnemy.gameObject.AddComponent<SlimeDeathNotifier>().Initialize(this);
        }
    }
    
    public void OnSlimeDefeated()
    {
        slimesDefeated++;
        float progress = (float)slimesDefeated / totalSlimesToDefeat;
        
        Debug.Log($"Slimes besiegt: {slimesDefeated}/{totalSlimesToDefeat} ({progress:P0})");
        
        // Phase-Übergänge
        if (!phase2Started && progress >= phase2Threshold)
        {
            StartPhase2();
        }
        else if (!phase3Started && progress >= phase3Threshold)
        {
            StartPhase3();
        }
        
        // Victory
        if (slimesDefeated >= totalSlimesToDefeat)
        {
            Victory();
        }
    }
    
    void StartPhase2()
    {
        phase2Started = true;
        currentPhase = 2;
        StopAllCoroutines();
        
        Debug.Log("🟨 PHASE 2: Die Prüfung - Aktiviere die 4 Buttons!");
        
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowNotification("Die Buttons erscheinen!");
        
        // Journal-Eintrag
        JournalProgress.Unlock("010");
        JournalToast.Enqueue("📖 Kampfprotokoll aktualisiert");
        
        // Buttons aktivieren
        foreach (var button in colorButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
                button.Activate(this);
            }
        }
        
        buttonsActivated = 0;
    }
    
    public void OnButtonPressed()
    {
        buttonsActivated++;
        Debug.Log($"Button aktiviert! ({buttonsActivated}/4)");
        
        if (buttonsActivated >= 4)
        {
            Debug.Log("✅ Alle Buttons aktiviert! Alle Farben spawnen jetzt!");
            // Spawne jetzt alle 4 Farben
            StartCoroutine(SpawnAllColors());
        }
    }
    
    IEnumerator SpawnAllColors()
    {
        while (currentPhase == 2)
        {
            SpawnSlime(blueSlimePrefab);
            SpawnSlime(greenSlimePrefab);
            SpawnSlime(redSlimePrefab);
            SpawnSlime(yellowSlimePrefab);
            yield return new WaitForSeconds(10f);
        }
    }
    
    void StartPhase3()
    {
        phase3Started = true;
        currentPhase = 3;
        StopAllCoroutines();
        
        Debug.Log("🔴 PHASE 3: Finale Welle!");
        
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowNotification("FINALE WELLE!");
        
        // Journal-Eintrag
        JournalProgress.Unlock("011");
        JournalToast.Enqueue("📖 Protokoll: Omnis' Entschluss");
        
        StartCoroutine(SpawnPhase3Slimes());
    }
    
    IEnumerator SpawnPhase3Slimes()
    {
        while (currentPhase == 3)
        {
            // SCHNELLER spawnen!
            SpawnSlime(blueSlimePrefab);
            SpawnSlime(greenSlimePrefab);
            yield return new WaitForSeconds(3f);
            SpawnSlime(redSlimePrefab);
            SpawnSlime(yellowSlimePrefab);
            yield return new WaitForSeconds(4f);
        }
    }
    
    void Victory()
    {
        StopAllCoroutines();
        currentPhase = 0;
        
        Debug.Log("🎉 SIEG! Alle Slimes besiegt!");
        
        StartCoroutine(VictorySequence());
    }
    
    IEnumerator VictorySequence()
    {
        // Kurze Stille
        yield return new WaitForSeconds(1f);

        // Brücke spawnen bevor die Kamera rüberschwenkt
        if (bridgeAntiFall != null)
            bridgeAntiFall.SetActive(false);

        if (bridgeToPortal != null)
            bridgeToPortal.SetActive(true);
        
        // Spiel pausieren & Kamerafahrt starten
        if (cameraTarget != null && Camera.main != null)
        {
            // Pausiere Spieler-Input
            Time.timeScale = 0f;
            
            // Starte Kamerafahrt zur Brücke (mit unscaled time)
            yield return StartCoroutine(CameraFocusCinematic());
            
            // Spiel fortsetzen
            Time.timeScale = 1f;
        }
        
        // Journal-Eintrag
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
        
        // Kamera zur Brücke bewegen
        while (elapsed < cinematicDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / cinematicDuration;
            
            // Smooth interpolation zur target position
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
        
        // Kurz auf der Brücke verweilen
        yield return new WaitForSecondsRealtime(1.5f);
        
        // Zurück zum Spieler
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
        // Wenn Slime stirbt, informiere Controller
        if (controller != null)
        {
            controller.OnSlimeDefeated();
        }
    }
}
