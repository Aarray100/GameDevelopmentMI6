using UnityEngine;
using System;

public class Hotbar : MonoBehaviour
{
    [Header("Hotbar Settings")]
    [SerializeField] private int hotbarSize = 6;
    public HotbarSlot[] slots;
    public int activeSlotIndex = 0;
    
    [Header("References")]
    public PlayerStats playerStats;
    public PlayerInventory playerInventory;
    
    // Events
    public event Action<int> OnSlotChanged;  // Slot wurde gewechselt
    public event Action OnHotbarUpdated;     // Hotbar content changed
    
    private void Awake()
    {
        // Initialisiere Slots
        slots = new HotbarSlot[hotbarSize];
        for (int i = 0; i < hotbarSize; i++)
        {
            slots[i] = new HotbarSlot();
        }
    }
    
    private void Start()
    {
        // Setze initial aktive Waffe
        UpdateActiveWeapon();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // Slot-Auswahl mit Tasten 1-6 (oder mehr)
        for (int i = 0; i < Mathf.Min(hotbarSize, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
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
        
        activeSlotIndex = index;
        UpdateActiveWeapon();
        OnSlotChanged?.Invoke(index);
        
        Debug.Log($"Selected hotbar slot {index + 1}");
    }
    
    private void UpdateActiveWeapon()
    {
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
            // Später: playerCombat.MeleeAttack();
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
        
        OnHotbarUpdated?.Invoke();
        
        // Update Waffe wenn aktiver Slot betroffen
        if (indexA == activeSlotIndex || indexB == activeSlotIndex)
        {
            UpdateActiveWeapon();
        }
    }
}
