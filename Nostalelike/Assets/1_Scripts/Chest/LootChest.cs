using UnityEngine;

public class LootChest : MonoBehaviour
{
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

    void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

    void Update()
    {
        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
            OpenChest();
    }

    void OpenChest()
{
    isOpened = true;
    if (openChestSprite != null) spriteRenderer.sprite = openChestSprite;

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

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) playerInRange = true; }
    private void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) playerInRange = false; }
}
