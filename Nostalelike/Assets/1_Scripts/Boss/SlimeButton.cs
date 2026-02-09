using UnityEngine;

public class SlimeButton : MonoBehaviour
{
    [Header("Slime Settings")]
    [SerializeField] private GameObject slimePrefab; // Welche Farbe spawnt hier
    [SerializeField] private int slimesToSpawn = 6;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("UI")]
    [SerializeField] private GameObject uiPrompt; // "Press E" UI
    
    [Header("Visuals (optional)")]
    [SerializeField] private SpriteRenderer buttonRenderer;
    [SerializeField] private Sprite pressedSprite;

    [Header("Behavior")]
    [SerializeField] private bool deactivateAfterPress = false;
    [SerializeField] private float deactivateDelay = 0.5f;
    [SerializeField] private float cooldownTime = 3f; // Zeit bis Button wieder drückbar ist
    [SerializeField] private bool startActive = true; // Button ist direkt aktiv beim Start
    
    private SlimeWaveController controller;
    private bool isActive = false;
    private bool isOnCooldown = false;
    private bool playerInRange = false;
    
    void Start()
    {
        // Auto-Finde SpriteRenderer wenn nicht zugewiesen
        if (buttonRenderer == null)
        {
            buttonRenderer = GetComponent<SpriteRenderer>();
            if (buttonRenderer != null)
                Debug.Log($"<color=green>SlimeButton '{gameObject.name}': SpriteRenderer automatisch gefunden!</color>");
            else
                Debug.LogWarning($"SlimeButton '{gameObject.name}': Kein SpriteRenderer gefunden! Sprite wird nicht geändert.");
        }
        
        if (uiPrompt != null)
            uiPrompt.SetActive(false);
        
        // Testing: Sofort aktivieren wenn gewünscht
        if (startActive)
        {
            isActive = true;
            Debug.Log($"<color=yellow>SlimeButton '{gameObject.name}': Sofort aktiviert (startActive=true)</color>");
        }
        
        // Status-Check
        Debug.Log($"<color=cyan>SlimeButton '{gameObject.name}' Setup:</color>\n" +
                  $"  - SpriteRenderer: {(buttonRenderer != null ? "✓" : "✗")}\n" +
                  $"  - Pressed Sprite: {(pressedSprite != null ? "✓" : "✗")}\n" +
                  $"  - Slime Prefab: {(slimePrefab != null ? "✓" : "✗")}\n" +
                  $"  - Spawn Points: {spawnPoints?.Length ?? 0}\n" +
                  $"  - Is Active: {isActive}");
    }
    
    public void Activate(SlimeWaveController waveController)
    {
        controller = waveController;
        isActive = true;
        Debug.Log($"<color=green>SlimeButton '{gameObject.name}': Aktiviert vom WaveController!</color>");
    }
    
    void Update()
    {
        // Debug-Info wenn E gedrückt wird aber Button nicht reagiert
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isActive)
                Debug.LogWarning($"SlimeButton '{gameObject.name}': E gedrückt aber Button ist NICHT aktiv! Warte auf Aktivierung.");
            else if (!playerInRange)
                Debug.Log($"SlimeButton '{gameObject.name}': E gedrückt aber Spieler NICHT in Range!");
            else if (isOnCooldown)
                Debug.Log($"SlimeButton '{gameObject.name}': E gedrückt aber Button auf Cooldown!");
        }
        
        if (!playerInRange || !isActive || isOnCooldown) return;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            PressButton();
        }
    }
    
    void PressButton()
    {
        isOnCooldown = true;
        
        if (uiPrompt != null)
            uiPrompt.SetActive(false);
        
        // Visuelles Feedback
        if (buttonRenderer != null && pressedSprite != null)
            buttonRenderer.sprite = pressedSprite;
        
        // Spawne Welle von Slimes
        if (slimePrefab != null && spawnPoints.Length > 0)
        {
            // Finde Spieler-Level für +3 Skalierung
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            PlayerStats playerStats = player?.GetComponent<PlayerStats>();
            
            for (int i = 0; i < slimesToSpawn; i++)
            {
                Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject slime = Instantiate(slimePrefab, randomSpawn.position, Quaternion.identity);
                
                // Setze Level auf Spieler-Level +3
                if (playerStats != null)
                {
                    EnemyStats enemyStats = slime.GetComponent<EnemyStats>();
                    if (enemyStats != null)
                    {
                        enemyStats.SetLevel(playerStats.currentLevel + 3);
                    }
                }
                
                // Registriere beim Controller
                var slimeEnemy = slime.GetComponent<EnemyHealth>();
                if (slimeEnemy != null)
                {
                    slime.AddComponent<SlimeDeathNotifier>().Initialize(controller);
                }
            }
        }
        
        // Informiere Controller
        if (controller != null)
            controller.OnButtonPressed();
        
        Debug.Log($"Button gedrückt! {slimesToSpawn} Slimes spawnen.");
        
        // Cooldown starten
        Invoke(nameof(ResetCooldown), cooldownTime);
        
        // Button nach kurzer Zeit deaktivieren (optional)
        if (deactivateAfterPress)
        {
            Invoke(nameof(DeactivateButton), deactivateDelay);
        }
    }
    
    void ResetCooldown()
    {
        isOnCooldown = false;
        Debug.Log($"Button wieder bereit!");
    }
    
    void DeactivateButton()
    {
        gameObject.SetActive(false);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"SlimeButton '{gameObject.name}': Trigger Enter von '{other.name}' (Tag: {other.tag})");
        
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"<color=cyan>SlimeButton '{gameObject.name}': Spieler in Range! isActive={isActive}</color>");
            
            if (isActive)
            {
                if (uiPrompt != null && !isOnCooldown)
                    uiPrompt.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"SlimeButton '{gameObject.name}': Spieler in Range aber Button noch nicht aktiviert!");
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log($"SlimeButton '{gameObject.name}': Spieler verlässt Range");
            
            if (uiPrompt != null)
                uiPrompt.SetActive(false);
        }
    }
}
