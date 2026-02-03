using UnityEngine;

public class LootChest : MonoBehaviour
{
    [Header("Unique Identifier")]
    [Tooltip("Eindeutige ID für diese Truhe. Wird automatisch generiert wenn leer.")]
    [SerializeField] private string chestID;
    
    [Header("Content")]
    public ItemData itemInside;
    public int quantity = 1;
    
    [Header("Journal (optional)")]
    [SerializeField] bool unlockJournalEntry = false;
    [SerializeField] string journalEntryId = "";
    [SerializeField] JournalDatabase journalDb;
    
    [Header("Visuals")]
    public Sprite openChestSprite;
    
    [Header("UI")]
    public GameObject uiPopup; // Hier ziehst du dein "Press 'G'" UI rein
    
    private bool isOpened = false;
    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Auto-generate ID wenn nicht gesetzt
        if (string.IsNullOrEmpty(chestID))
        {
            chestID = $"{gameObject.scene.name}_Chest_{transform.position.x:F2}_{transform.position.y:F2}";
        }
    }
    
    void Start()
    {
        // UI am Start ausblenden
        if(uiPopup != null) 
            uiPopup.SetActive(false);
            
        // Prüfe ob diese Truhe bereits geöffnet wurde
        CheckAndApplyOpenedState();
    }
    
    public void CheckAndApplyOpenedState()
    {
        if (ChestManager.Instance != null && ChestManager.Instance.IsChestOpened(chestID))
        {
            SetOpenedVisual();
        }
    }
    
    public string GetChestID() => chestID;
    
    void Update()
    {
        // Du kannst hier G statt E verwenden, wenn du das willst
        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
            OpenChest();
    }
    
    void OpenChest()
    {
        isOpened = true;
        SetOpenedVisual();
        
        // UI ausblenden nach dem Öffnen
        if(uiPopup != null) 
            uiPopup.SetActive(false);
        
        // Markiere Truhe als geöffnet im ChestManager
        ChestManager.Instance?.MarkChestOpened(chestID);
        
        PlayerInventory playerInv = Object.FindFirstObjectByType<PlayerInventory>();
        if (playerInv != null && itemInside != null)
        {
            playerInv.inventory.AddItem(itemInside, quantity);
            JournalToast.Enqueue($"+{quantity} {itemInside.itemName}");
        }
        
        if (unlockJournalEntry && !string.IsNullOrEmpty(journalEntryId))
        {
            if (journalDb == null) return;
            var entry = journalDb.GetById(journalEntryId);
            
            if (entry != null)
            {
                JournalProgress.Unlock(journalEntryId);
                JournalToast.Enqueue($"Journal aktualisiert: {journalEntryId}");
                NotificationManager.Instance?.ShowNotification($"Neuer Eintrag: {entry.title}");
            }
        }
    }
    
    private void SetOpenedVisual()
    {
        isOpened = true;
        if (openChestSprite != null) 
            spriteRenderer.sprite = openChestSprite;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // UI anzeigen wenn Truhe noch nicht geöffnet
            if (!isOpened && uiPopup != null)
                uiPopup.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // UI ausblenden
            if(uiPopup != null)
                uiPopup.SetActive(false);
        }
    }
} 