using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Equipment Slots")]
    public Dictionary<EquipmentSlot, ItemData> equippedItems = new Dictionary<EquipmentSlot, ItemData>();
    
    [Header("References")]
    public PlayerStats playerStats;
    public PlayerInventory playerInventory;
    
    // Events
    public event Action OnEquipmentChanged;
    
    private void Awake()
    {
        // Auto-hole Referenzen falls nicht gesetzt
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }
        
        // Initialisiere alle Equipment-Slots als leer
        InitializeEquipmentSlots();
    }
    
    private void InitializeEquipmentSlots()
    {
        equippedItems.Clear();
        
        // Erstelle leere Slots für alle Equipment-Positionen
        equippedItems[EquipmentSlot.Head] = null;
        equippedItems[EquipmentSlot.Chest] = null;
        equippedItems[EquipmentSlot.Hands] = null;
        equippedItems[EquipmentSlot.Legs] = null;
        equippedItems[EquipmentSlot.Feet] = null;
        equippedItems[EquipmentSlot.Amulet] = null;
        equippedItems[EquipmentSlot.Ring] = null;
    }
    
    // Equipt ein Item
    public bool EquipItem(ItemData item)
    {
        if (item == null) return false;
        
        // Prüfe ob es ein Equipment-Item ist
        if (item.equipSlot == EquipmentSlot.None || item.equipSlot == EquipmentSlot.Weapon)
        {
            Debug.Log($"{item.itemName} cannot be equipped here! Use Hotbar for weapons.");
            return false;
        }
        
        EquipmentSlot targetSlot = item.equipSlot;
        
        // Falls schon ein Item im Slot ist, unequip es zuerst
        if (equippedItems[targetSlot] != null)
        {
            UnequipItem(targetSlot);
        }
        
        // Equipt das Item
        equippedItems[targetSlot] = item;
        
        // Entferne aus Inventar
        playerInventory.inventory.RemoveItem(item, 1);
        
        Debug.Log($"Equipped {item.itemName} in {targetSlot} slot");
        
        // Update Stats
        RecalculateEquipmentStats();
        OnEquipmentChanged?.Invoke();
        
        return true;
    }
    
    // Unequipt ein Item
    public bool UnequipItem(EquipmentSlot slot, bool addToInventory = true)
    {
        if (!equippedItems.ContainsKey(slot) || equippedItems[slot] == null)
        {
            Debug.Log($"No item equipped in {slot} slot");
            return false;
        }
        
        ItemData item = equippedItems[slot];
        
        // Füge zurück zum Inventar (nur wenn gewünscht)
        if (addToInventory)
        {
            playerInventory.inventory.AddItem(item, 1);
        }
        
        // Entferne aus Equipment
        equippedItems[slot] = null;
        
        Debug.Log($"Unequipped {item.itemName} from {slot} slot");
        
        // Update Stats
        RecalculateEquipmentStats();
        OnEquipmentChanged?.Invoke();
        
        return true;
    }
    
    // Hole equipped Item aus Slot
    public ItemData GetEquippedItem(EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot))
        {
            return equippedItems[slot];
        }
        return null;
    }
    
    // Check ob ein Slot leer ist
    public bool IsSlotEmpty(EquipmentSlot slot)
    {
        return !equippedItems.ContainsKey(slot) || equippedItems[slot] == null;
    }
    
    // Swap Equipment (wenn man ein neues Item auf ein belegtes dropped)
    public bool SwapEquipment(ItemData newItem, EquipmentSlot slot)
    {
        if (newItem == null) return false;
        
        // Check ob Item in diesen Slot passt
        if (!CanEquipInSlot(newItem, slot))
        {
            Debug.Log($"{newItem.itemName} cannot be equipped in {slot} slot!");
            return false;
        }
        
        ItemData oldItem = equippedItems[slot];
        
        // Unequip altes Item (geht zurück ins Inventar)
        if (oldItem != null)
        {
            playerInventory.inventory.AddItem(oldItem, 1);
        }
        
        // Entferne neues Item aus Inventar
        playerInventory.inventory.RemoveItem(newItem, 1);
        
        // Equipt neues Item
        equippedItems[slot] = newItem;
        
        Debug.Log($"Swapped {oldItem?.itemName ?? "empty"} with {newItem.itemName} in {slot}");
        
        RecalculateEquipmentStats();
        OnEquipmentChanged?.Invoke();
        
        return true;
    }
    
    // Prüft ob ein Item in einen bestimmten Slot passt
    public bool CanEquipInSlot(ItemData item, EquipmentSlot slot)
    {
        if (item == null) return false;
        
        // Waffen gehen nicht ins Equipment-Panel
        if (item.itemType == ItemType.Weapon) return false;
        
        // Equipment-Items müssen zum Slot passen
        return item.equipSlot == slot;
    }
    
    // Berechnet alle Equipment-Stats und sendet an PlayerStats
    private void RecalculateEquipmentStats()
    {
        ItemStats totalEquipmentStats = new ItemStats();
        
        // Addiere Stats von allen equipped Items
        foreach (var kvp in equippedItems)
        {
            if (kvp.Value != null && kvp.Value.stats != null)
            {
                totalEquipmentStats = totalEquipmentStats + kvp.Value.stats;
            }
        }
        
        // Sende an PlayerStats
        if (playerStats != null)
        {
            playerStats.UpdateEquipmentBonus(totalEquipmentStats);
        }
        
        Debug.Log($"Equipment stats updated! Total Defense: +{totalEquipmentStats.bonusDefense}, Total HP: +{totalEquipmentStats.bonusHealth}");
    }
    
    // Helper: Hole alle equipped Items
    public List<ItemData> GetAllEquippedItems()
    {
        List<ItemData> items = new List<ItemData>();
        foreach (var kvp in equippedItems)
        {
            if (kvp.Value != null)
            {
                items.Add(kvp.Value);
            }
        }
        return items;
    }
    
    // Öffentliche Methode um Equipment-Update zu triggern (für UI)
    public void TriggerEquipmentUpdate()
    {
        RecalculateEquipmentStats();
        OnEquipmentChanged?.Invoke();
    }
}
