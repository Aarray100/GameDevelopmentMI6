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
    
    private SlimeWaveController controller;
    private bool isActive = false;
    private bool isOnCooldown = false;
    private bool playerInRange = false;
    
    void Start()
    {
        if (uiPrompt != null)
            uiPrompt.SetActive(false);
    }
    
    public void Activate(SlimeWaveController waveController)
    {
        controller = waveController;
        isActive = true;
    }
    
    void Update()
    {
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
            for (int i = 0; i < slimesToSpawn; i++)
            {
                Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject slime = Instantiate(slimePrefab, randomSpawn.position, Quaternion.identity);
                
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
        if (other.CompareTag("Player") && isActive)
        {
            playerInRange = true;
            
            if (uiPrompt != null && !isOnCooldown)
                uiPrompt.SetActive(true);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (uiPrompt != null)
                uiPrompt.SetActive(false);
        }
    }
}
