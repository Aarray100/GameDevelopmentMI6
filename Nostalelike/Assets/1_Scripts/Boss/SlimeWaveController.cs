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
    
    [Header("Journal")]
    [SerializeField] private JournalDatabase journalDb;
    
    private int currentPhase = 1;
    private bool phase2Started = false;
    private bool phase3Started = false;
    private int buttonsActivated = 0;
    
    void Start()
    {
        bridgeToPortal?.SetActive(false);
        
        // Buttons am Anfang deaktivieren
        foreach (var button in colorButtons)
        {
            if (button != null)
                button.gameObject.SetActive(false);
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
        yield return new WaitForSeconds(2f);
        
        // Journal-Eintrag
        JournalProgress.Unlock("012");
        JournalToast.Enqueue("📖 Der Guide: Liturgie der Ordnung");
        
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowNotification("SIEG! Portal freigeschaltet!");
        
        // Unsichtbare Wand deaktivieren
        if (bridgeAntiFall != null)
            bridgeAntiFall.SetActive(false);
        
        // Brücke spawnen
        if (bridgeToPortal != null)
            bridgeToPortal.SetActive(true);
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
