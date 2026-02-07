using UnityEngine;

public class SlimeButton : MonoBehaviour
{
    [Header("Slime Settings")]
    [SerializeField] private GameObject slimePrefab; // Welche Farbe spawnt hier
    [SerializeField] private int slimesToSpawn = 3;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("UI")]
    [SerializeField] private GameObject uiPrompt; // "Press E" UI
    
    [Header("Visuals (optional)")]
    [SerializeField] private SpriteRenderer buttonRenderer;
    [SerializeField] private Sprite pressedSprite;
    
    private SlimeWaveController controller;
    private bool isActive = false;
    private bool isCompleted = false;
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
        if (playerInRange && isActive && !isCompleted && Input.GetKeyDown(KeyCode.E))
        {
            PressButton();
        }
    }
    
    void PressButton()
    {
        isCompleted = true;
        
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
        
        // Button nach kurzer Zeit deaktivieren
        Invoke(nameof(DeactivateButton), 0.5f);
    }
    
    void DeactivateButton()
    {
        gameObject.SetActive(false);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isActive && !isCompleted)
        {
            playerInRange = true;
            
            if (uiPrompt != null)
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
