using UnityEngine;
using System;
using System.Collections.Generic;

public class Hotbar : MonoBehaviour
{
    [Header("Hotbar Settings")]
    [SerializeField] private int hotbarSize = 10;
    public HotbarSlot[] slots;
    public int activeSlotIndex = 0;
    
    [Header("UI References")]
    [SerializeField] private HotbarSlotUI[] slotUIElements; // Drag deine 10 Slot-Prefabs hier rein
    
    [Header("References (werden automatisch gefunden)")]
    private PlayerStats playerStats;
    private PlayerInventory playerInventory;
    private PlayerCombat playerCombat;
    
    // Events
    public event Action<int> OnSlotChanged;  // Slot wurde gewechselt
    public event Action OnHotbarUpdated;     // Hotbar content changed
    
    private static Hotbar instance;
    
    private void Awake()
    {
        // DontDestroyOnLoad Pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("Hotbar: DontDestroyOnLoad gesetzt");
        }
        else
        {
            Debug.Log("Hotbar: Duplikat gefunden, wird zerstört");
            Destroy(gameObject);
            return;
        }
        
        // Initialisiere Slots
        slots = new HotbarSlot[hotbarSize];
        for (int i = 0; i < hotbarSize; i++)
        {
            slots[i] = new HotbarSlot();
        }
        
        // Initialisiere UI-Slots
        InitializeUI();
    }
    
    private void Start()
    {
        // Finde Player-Referenzen (nach Character-Spawn)
        FindPlayerReferences();
        
        // Setze initial aktive Waffe und wähle ersten Slot aus
        UpdateAllSlotsUI();
        SelectSlot(0); // Wähle ersten Slot beim Start
        UpdateActiveWeapon();
    }
    
    private void OnDestroy()
    {
        // Reset der Instanz wenn diese Hotbar zerstört wird
        if (instance == this)
        {
            instance = null;
        }
    }
    
    /// <summary>
    /// Findet automatisch die Player-Referenzen (nach Character-Spawn)
    /// </summary>
    private void FindPlayerReferences()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log("Hotbar: PlayerStats gefunden");
            }
            else
            {
                Debug.LogWarning("Hotbar: PlayerStats nicht gefunden! Wird später gesucht.");
            }
        }
        
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
            if (playerInventory != null)
            {
                Debug.Log("Hotbar: PlayerInventory gefunden");
            }
            else
            {
                Debug.LogWarning("Hotbar: PlayerInventory nicht gefunden! Wird später gesucht.");
            }
        }

        if (playerCombat == null)
        {
            playerCombat = FindFirstObjectByType<PlayerCombat>();
            if (playerCombat != null)
            {
                Debug.Log("Hotbar: PlayerCombat gefunden");
            }
            else
            {
                Debug.LogWarning("Hotbar: PlayerCombat nicht gefunden! Wird später gesucht.");
            }
        }
    }
    
    /// <summary>
    /// Wird nach Scene-Load aufgerufen um Player-Referenzen neu zu finden
    /// </summary>
    private void OnEnable()
    {
        // Versuche Player-Referenzen zu finden falls noch nicht vorhanden
        if (playerStats == null || playerInventory == null)
        {
            Invoke(nameof(FindPlayerReferences), 0.1f); // Kurze Verzögerung für Spawn
        }
    }
    
    private void InitializeUI()
    {
        if (slotUIElements == null || slotUIElements.Length != hotbarSize)
        {
            Debug.LogError($"Hotbar: slotUIElements muss genau {hotbarSize} Elemente haben!");
            return;
        }
        
        // Verbinde UI-Slots mit Data-Slots
        for (int i = 0; i < hotbarSize; i++)
        {
            if (slotUIElements[i] != null)
            {
                slotUIElements[i].Initialize(i, slots[i]);
            }
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // Slot-Auswahl mit Tasten 1-9 und 0 für den 10. Slot
        // Tasten 1-9 (Alpha1 bis Alpha9)
        for (int i = 0; i < Mathf.Min(hotbarSize, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
        
        // Taste 0 für den 10. Slot (falls hotbarSize >= 10)
        if (hotbarSize >= 10 && Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelectSlot(9);
        }
        
        // Nutze aktiven Slot (Linksklick)
        if (Input.GetMouseButtonDown(0))
        {
            UseActiveSlot();
        }
        
        // Cycle durch Abilities (Rechtsklick oder nochmal gleiche Taste drücken)
        if (Input.GetMouseButtonDown(1))
        {
            CycleAbilityIfStaff();
        }
        
        // Optional: Mausrad für Slot-Wechsel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SelectSlot((activeSlotIndex + 1) % hotbarSize);
        }
        else if (scroll < 0f)
        {
            SelectSlot((activeSlotIndex - 1 + hotbarSize) % hotbarSize);
        }
    }
    
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= hotbarSize) return;
        
        // Deselect vorherigen Slot in UI
        if (slotUIElements != null && activeSlotIndex < slotUIElements.Length && slotUIElements[activeSlotIndex] != null)
        {
            slotUIElements[activeSlotIndex].SetSelected(false);
        }
        
        activeSlotIndex = index;
        
        // Select neuen Slot in UI
        if (slotUIElements != null && index < slotUIElements.Length && slotUIElements[index] != null)
        {
            slotUIElements[index].SetSelected(true);
        }
        
        UpdateActiveWeapon();
        OnSlotChanged?.Invoke(index);
    }
    
    private void UpdateActiveWeapon()
    {
        // Prüfe ob PlayerStats vorhanden ist
        if (playerStats == null)
        {
            FindPlayerReferences();
            if (playerStats == null)
            {
                Debug.LogWarning("Hotbar: Kann Waffe nicht updaten, PlayerStats fehlt");
                return;
            }
        }
        
        HotbarSlot slot = slots[activeSlotIndex];
        
        if (slot.item != null && slot.item.itemType == ItemType.Weapon)
        {
            // Diese Waffe ist jetzt aktiv und gibt Stats!
            playerStats.SetActiveWeapon(slot.item);
        }
        else
        {
            // Kein Weapon aktiv = keine Weapon-Stats
            playerStats.SetActiveWeapon(null);
        }
    }
    
    private void UseActiveSlot()
    {
        HotbarSlot slot = slots[activeSlotIndex];
        
        if (slot.IsEmpty()) return;
        
        switch (slot.item.itemType)
        {
            case ItemType.Weapon:
                UseWeapon(slot);
                break;
                
            case ItemType.Consumable:
                UseConsumable(slot);
                break;
                
            default:
                Debug.Log($"Cannot use item type: {slot.item.itemType}");
                break;
        }
    }
    
    private void UseWeapon(HotbarSlot slot)
    {
        if (slot.item.weaponType == WeaponType.Sword)
        {
            // Trigger Melee Attack
            Debug.Log("Melee attack with sword!");
            
            // Sicherheits-Check: Wenn Referenz fehlt, versuche sie JETZT zu finden
            if (playerCombat == null)
            {
                playerCombat = FindFirstObjectByType<PlayerCombat>();
            }

            if (playerCombat != null)
            {
                Debug.Log($"Hotbar: Found PlayerCombat on GameObject '{playerCombat.gameObject.name}'. Calling Attack...");
                playerCombat.MeleeAttack();
            }
            else
            {
                Debug.LogError("Hotbar: CRITICAL - PlayerCombat Script not found on Player! Did you attach it?");
            }
        }
        else if (slot.item.weaponType == WeaponType.Staff)
        {
            // Cast aktuell gewählte Magic Ability
            MagicAbility ability = slot.GetCurrentAbility();
            if (ability != null)
            {
                Debug.Log($"Casting {ability} magic!");
                // Später: playerCombat.CastMagic(ability);
            }
            else
            {
                Debug.Log("No magic ability selected!");
            }
        }
        else if (slot.item.weaponType == WeaponType.Bow)
        {
            // Shoot Arrow
            Debug.Log("Shoot arrow!");
            // Später: playerCombat.ShootArrow();
        }
    }
    
    private void UseConsumable(HotbarSlot slot)
    {
        bool used = false;
        
        switch (slot.item.consumableType)
        {
            case ConsumableType.HealthPotion:
                playerStats.Heal(slot.item.healAmount);
                used = true;
                Debug.Log($"Used Health Potion! Healed {slot.item.healAmount} HP");
                break;
                
            case ConsumableType.ManaPotion:
                playerStats.RestoreMana(slot.item.manaAmount);
                used = true;
                Debug.Log($"Used Mana Potion! Restored {slot.item.manaAmount} Mana");
                break;
                
            case ConsumableType.StaminaPotion:
                playerStats.RestoreStamina(slot.item.staminaAmount);
                used = true;
                Debug.Log($"Used Stamina Potion! Restored {slot.item.staminaAmount} Stamina");
                break;
        }
        
        if (used)
        {
            // Reduziere Quantity
            slot.quantity--;
            
            if (slot.quantity <= 0)
            {
                slot.ClearSlot();
            }
            
            UpdateSlotUI(activeSlotIndex);
            OnHotbarUpdated?.Invoke();
        }
    }
    
    private void CycleAbilityIfStaff()
    {
        HotbarSlot slot = slots[activeSlotIndex];
        
        if (slot.item != null && slot.item.weaponType == WeaponType.Staff)
        {
            slot.CycleAbility();
            OnHotbarUpdated?.Invoke();
            Debug.Log("Cycled to next magic ability");
        }
    }
    
    // Füge Item zu Hotbar hinzu (von Inventar)
    public bool AddItemToSlot(ItemData item, int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSize) return false;
        
        HotbarSlot slot = slots[slotIndex];
        
        // Wenn Slot leer ist
        if (slot.IsEmpty())
        {
            slot.SetItem(item, quantity);
            UpdateSlotUI(slotIndex);
            OnHotbarUpdated?.Invoke();
            
            // Wenn dieser Slot aktiv ist, update Waffe
            if (slotIndex == activeSlotIndex)
            {
                UpdateActiveWeapon();
            }
            
            return true;
        }
        // Wenn gleicher Item-Typ und stapelbar
        else if (slot.item == item && item.isStackable)
        {
            slot.quantity += quantity;
            UpdateSlotUI(slotIndex);
            OnHotbarUpdated?.Invoke();
            return true;
        }
        
        return false;
    }
    
    // Entferne Item von Hotbar
    public ItemData RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSize) return null;
        
        HotbarSlot slot = slots[slotIndex];
        ItemData removedItem = slot.item;
        
        slot.ClearSlot();
        UpdateSlotUI(slotIndex);
        OnHotbarUpdated?.Invoke();
        
        // Wenn aktiver Slot geleert wurde, update Waffe
        if (slotIndex == activeSlotIndex)
        {
            UpdateActiveWeapon();
        }
        
        return removedItem;
    }
    
    // Swap zwei Hotbar-Slots
    public void SwapSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= hotbarSize || indexB < 0 || indexB >= hotbarSize) return;
        
        HotbarSlot temp = slots[indexA];
        slots[indexA] = slots[indexB];
        slots[indexB] = temp;
        
        // WICHTIG: UI-Referenzen aktualisieren, damit sie auf die richtigen Slots zeigen
        if (slotUIElements != null)
        {
            if (indexA < slotUIElements.Length && slotUIElements[indexA] != null)
            {
                slotUIElements[indexA].Initialize(indexA, slots[indexA]);
            }
            if (indexB < slotUIElements.Length && slotUIElements[indexB] != null)
            {
                slotUIElements[indexB].Initialize(indexB, slots[indexB]);
            }
        }
        
        OnHotbarUpdated?.Invoke();
        
        // Update Waffe wenn aktiver Slot betroffen
        if (indexA == activeSlotIndex || indexB == activeSlotIndex)
        {
            UpdateActiveWeapon();
        }
    }
    
    // === UI Update Methoden ===
    
    /// <summary>
    /// Update einen einzelnen UI-Slot
    /// </summary>
    private void UpdateSlotUI(int index)
    {
        if (slotUIElements != null && index >= 0 && index < slotUIElements.Length && slotUIElements[index] != null)
        {
            slotUIElements[index].UpdateUI();
        }
    }
    
    /// <summary>
    /// Update alle UI-Slots (z.B. nach Load oder Initialization)
    /// </summary>
    private void UpdateAllSlotsUI()
    {
        if (slotUIElements == null) return;
        
        for (int i = 0; i < slotUIElements.Length; i++)
        {
            if (slotUIElements[i] != null)
            {
                slotUIElements[i].UpdateUI();
                
                // Setze Selection-Highlight für aktiven Slot
                slotUIElements[i].SetSelected(i == activeSlotIndex);
            }
        }
    }

    // ...existing code...

public List<HotbarSlotData> GetSaveData()
{
    List<HotbarSlotData> data = new List<HotbarSlotData>();
    
    for (int i = 0; i < slots.Length; i++)
    {
        HotbarSlotData slotData = new HotbarSlotData
        {
            slotIndex = i,
            itemID = slots[i].item?.itemName ?? "",
            quantity = slots[i].quantity,
            isEmpty = slots[i].IsEmpty()
        };
        data.Add(slotData);
    }
    
    Debug.Log($"Hotbar: {data.Count} Slots zum Speichern gesammelt");
    return data;
}

public void LoadSaveData(List<HotbarSlotData> data)
{
    if (data == null) return;
    
    foreach (var slotData in data)
    {
        if (slotData.slotIndex >= 0 && slotData.slotIndex < slots.Length)
        {
            if (!slotData.isEmpty && !string.IsNullOrEmpty(slotData.itemID))
            {
                ItemData item = SaveManager.Instance?.GetItemByName(slotData.itemID);
                if (item != null)
                {
                    slots[slotData.slotIndex].SetItem(item, slotData.quantity);
                }
            }
            else
            {
                slots[slotData.slotIndex].ClearSlot();
            }
        }
    }
    
    UpdateAllSlotsUI();
    Debug.Log("Hotbar data loaded");
}
}
