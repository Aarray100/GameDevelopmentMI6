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
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }
        
        InitializeEquipmentSlots();
    }
    
    private void InitializeEquipmentSlots()
    {
        equippedItems.Clear();
        
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
        
        if (item.equipSlot == EquipmentSlot.None || item.equipSlot == EquipmentSlot.Weapon)
        {
            Debug.Log($"{item.itemName} cannot be equipped here!");
            return false;
        }
        
        EquipmentSlot targetSlot = item.equipSlot;
        
        // Falls schon ein Item im Slot ist, unequip es (ohne Sound abzuspielen)
        if (equippedItems[targetSlot] != null)
        {
            UnequipItem(targetSlot, true, false); 
        }
        
        // Equipt das Item
        equippedItems[targetSlot] = item;
        
        // Entferne aus Inventar
        playerInventory.inventory.RemoveItem(item, 1);

        // Sound abspielen
        AudioManager.Instance?.PlayEquipSFX();
        
        Debug.Log($"Equipped {item.itemName} in {targetSlot} slot");
        
        RecalculateEquipmentStats();
        OnEquipmentChanged?.Invoke();
        
        return true;
    }
    
    // Unequipt ein Item (playAudio Parameter hinzugefügt)
    public bool UnequipItem(EquipmentSlot slot, bool addToInventory = true, bool playAudio = true)
    {
        if (!equippedItems.ContainsKey(slot) || equippedItems[slot] == null)
        {
            return false;
        }
        
        ItemData item = equippedItems[slot];
        
        if (addToInventory)
        {
            playerInventory.inventory.AddItem(item, 1);
        }
        
        equippedItems[slot] = null;

        // Nur Sound spielen, wenn nicht von EquipItem unterdrückt
        if (playAudio)
        {
            AudioManager.Instance?.PlayUnequipSFX();
        }
        
        Debug.Log($"Unequipped {item.itemName} from {slot} slot");
        
        RecalculateEquipmentStats();
        OnEquipmentChanged?.Invoke();
        
        return true;
    }
    
    public ItemData GetEquippedItem(EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot))
        {
            return equippedItems[slot];
        }
        return null;
    }
    
    public bool IsSlotEmpty(EquipmentSlot slot)
    {
        return !equippedItems.ContainsKey(slot) || equippedItems[slot] == null;
    }
    
    // Swap Equipment (mit Sound-Fix)
    public bool SwapEquipment(ItemData newItem, EquipmentSlot slot)
    {
        if (newItem == null) return false;
        
        if (!CanEquipInSlot(newItem, slot))
        {
            return false;
        }
        
        ItemData oldItem = equippedItems[slot];
        
        if (oldItem != null)
        {
            playerInventory.inventory.AddItem(oldItem, 1);
        }
        
        playerInventory.inventory.RemoveItem(newItem, 1);
        equippedItems[slot] = newItem;
        
        // Sound hinzugefügt
        AudioManager.Instance?.PlayEquipSFX();
        
        Debug.Log($"Swapped {oldItem?.itemName ?? "empty"} with {newItem.itemName} in {slot}");
        
        RecalculateEquipmentStats();
        OnEquipmentChanged?.Invoke();
        
        return true;
    }
    
    public bool CanEquipInSlot(ItemData item, EquipmentSlot slot)
    {
        if (item == null) return false;
        if (item.itemType == ItemType.Weapon) return false;
        return item.equipSlot == slot;
    }
    
    private void RecalculateEquipmentStats()
    {
        ItemStats totalEquipmentStats = new ItemStats();
        
        foreach (var kvp in equippedItems)
        {
            if (kvp.Value != null && kvp.Value.stats != null)
            {
                totalEquipmentStats = totalEquipmentStats + kvp.Value.stats;
            }
        }
        
        if (playerStats != null)
        {
            playerStats.UpdateEquipmentBonus(totalEquipmentStats);
        }
    }
    
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
    
    public void TriggerEquipmentUpdate()
    {
        RecalculateEquipmentStats();
        OnEquipmentChanged?.Invoke();
    }

// ...existing code...

#region Save/Load System

public List<EquipmentItemData> GetSaveData()
{
    List<EquipmentItemData> data = new List<EquipmentItemData>();
    
    foreach (var kvp in equippedItems)
    {
        if (kvp.Value != null)
        {
            EquipmentItemData itemData = new EquipmentItemData
            {
                itemID = kvp.Value.itemName,
                equipSlotType = (int)kvp.Key
            };
            data.Add(itemData);
        }
    }
    
    Debug.Log($"PlayerEquipment: {data.Count} Items zum Speichern gesammelt");
    return data;
}

public void LoadSaveData(List<EquipmentItemData> data)
{
    if (data == null) return;
    
    // Zuerst alles unequippen
    foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
    {
        if (slot != EquipmentSlot.None && slot != EquipmentSlot.Weapon)
        {
            equippedItems[slot] = null;
        }
    }
    
    // Dann gespeicherte Items equippen
    foreach (var itemData in data)
    {
        ItemData item = SaveManager.Instance?.GetItemByName(itemData.itemID);
        
        if (item != null)
        {
            EquipmentSlot slot = (EquipmentSlot)itemData.equipSlotType;
            equippedItems[slot] = item;
        }
        else
        {
            Debug.LogWarning($"Equipment Item nicht gefunden: {itemData.itemID}");
        }
    }
    
    RecalculateEquipmentStats();
    OnEquipmentChanged?.Invoke();
    
    Debug.Log($"PlayerEquipment: {data.Count} Items geladen");
}

#endregion
}