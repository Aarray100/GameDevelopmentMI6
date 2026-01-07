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
    [SerializeField] bool unlockJournalEntry = false;          // Haken im Inspector
    [SerializeField] string journalEntryId = "";               // z.B. "id000"
    [SerializeField] JournalDatabase journalDb;                // JournalDatabase Asset reinziehen

    [Header("Visuals")]
    public Sprite openChestSprite;

    private bool isOpened = false;
    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Auto-generate ID wenn nicht gesetzt (Scene + Position für Einzigartigkeit)
        if (string.IsNullOrEmpty(chestID))
        {
            chestID = $"{gameObject.scene.name}_Chest_{transform.position.x:F2}_{transform.position.y:F2}";
        }
    }
    
    void Start()
    {
        // Prüfe ob diese Truhe bereits geöffnet wurde
        CheckAndApplyOpenedState();
    }
    
    /// <summary>
    /// Prüft und wendet den geöffneten Zustand an (wird auch vom ChestManager aufgerufen)
    /// </summary>
    public void CheckAndApplyOpenedState()
    {
        if (ChestManager.Instance != null && ChestManager.Instance.IsChestOpened(chestID))
        {
            SetOpenedVisual();
        }
    }
    
    /// <summary>
    /// Gibt die eindeutige ID dieser Truhe zurück
    /// </summary>
    public string GetChestID() => chestID;

    void Update()
    {
        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
            OpenChest();
    }

    void OpenChest()
    {
        isOpened = true;
        SetOpenedVisual();
        
        // Markiere Truhe als geöffnet im ChestManager
        ChestManager.Instance?.MarkChestOpened(chestID);

        PlayerInventory playerInv = Object.FindFirstObjectByType<PlayerInventory>();
        if (playerInv != null && itemInside != null)
        {
            playerInv.inventory.AddItem(itemInside, quantity);
            // NEU: Benachrichtigung für Item
            NotificationManager.Instance?.ShowNotification($"{itemInside.itemName} x{quantity} erhalten!");
        }

        if (unlockJournalEntry && !string.IsNullOrEmpty(journalEntryId))
        {
            if (journalDb == null) return;
            var entry = journalDb.GetById(journalEntryId);
            
            if (entry != null)
            {
                JournalProgress.Unlock(journalEntryId);
                // NEU: Benachrichtigung für Journal
                NotificationManager.Instance?.ShowNotification($"Neuer Eintrag: {entry.title}");
            }
        }
    }
    
    /// <summary>
    /// Setzt das visuelle Erscheinungsbild auf "geöffnet"
    /// </summary>
    private void SetOpenedVisual()
    {
        isOpened = true;
        if (openChestSprite != null) 
            spriteRenderer.sprite = openChestSprite;
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) playerInRange = true; }
    private void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) playerInRange = false; }
}
