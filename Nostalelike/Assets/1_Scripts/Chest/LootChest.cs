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
    
    [Header("Auto-Unlock bei allen Fragmenten")]
    [SerializeField] bool checkForAllFragments = false;
    [Tooltip("Entry-ID die freigeschaltet wird wenn alle Fragmente da sind (z.B. 008)")]
    [SerializeField] string autoUnlockEntryIfAllFragments = "008";
    [Tooltip("Boss-Fragment IDs die gesammelt werden müssen (004, 005, 006, 007)")]
    [SerializeField] string[] requiredFragmentIDs = { "004", "005", "006", "007" };
    
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
        
        // Prüfe ob jetzt alle Fragmente gesammelt wurden
        if (checkForAllFragments)
        {
            CheckAndUnlockOmnis();
        }
    }
    
    private void SetOpenedVisual()
    {
        isOpened = true;
        if (openChestSprite != null) 
            spriteRenderer.sprite = openChestSprite;
    }
    
    /// <summary>
    /// Prüft ob alle Boss-Fragmente (004-007) freigeschaltet sind.
    /// Wenn ja: Schaltet automatisch Entry 008 (Das Erwachen von Omnis) frei.
    /// </summary>
    void CheckAndUnlockOmnis()
    {
        if (AreAllFragmentsUnlocked())
        {
            // Nur freischalten wenn noch nicht geschehen
            if (!JournalProgress.IsUnlocked(autoUnlockEntryIfAllFragments))
            {
                JournalProgress.Unlock(autoUnlockEntryIfAllFragments);
                JournalToast.Enqueue($"🌟 ALLE FRAGMENTE GESAMMELT!");
                
                if (journalDb != null)
                {
                    var entry = journalDb.GetById(autoUnlockEntryIfAllFragments);
                    if (entry != null)
                    {
                        NotificationManager.Instance?.ShowNotification($"Neuer Eintrag: {entry.title}");
                    }
                }
                
                Debug.Log($"🌟 ALLE FRAGMENTE GESAMMELT! '{autoUnlockEntryIfAllFragments}' wurde freigeschaltet!");
            }
        }
    }
    
    /// <summary>
    /// Prüft ob alle angegebenen Fragment-IDs freigeschaltet sind.
    /// </summary>
    bool AreAllFragmentsUnlocked()
    {
        foreach (string fragmentID in requiredFragmentIDs)
        {
            if (!JournalProgress.IsUnlocked(fragmentID))
            {
                return false;
            }
        }
        return true; // Alle Fragmente sind da!
    }
    
    /// <summary>
    /// Öffentliche Methode: Prüft einzelnes Fragment.
    /// </summary>
    public static bool IsFragmentUnlocked(string fragmentID)
    {
        return JournalProgress.IsUnlocked(fragmentID);
    }
    
    /// <summary>
    /// Öffentliche Methode: Gibt Anzahl gesammelter Fragmente zurück.
    /// </summary>
    public static int GetUnlockedFragmentCount(string[] fragmentIDs)
    {
        int count = 0;
        foreach (string id in fragmentIDs)
        {
            if (JournalProgress.IsUnlocked(id))
                count++;
        }
        return count;
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