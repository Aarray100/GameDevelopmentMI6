using UnityEngine;
using System.Collections.Generic;
using System;

public class Inventory
{
    public event Action OnInventoryChanged;
    public List<InventorySlot> slots = new List<InventorySlot>();
    private int _maxSlots = 49;
    public int maxSlots 
    { 
        get => _maxSlots; 
        set 
        { 
            _maxSlots = value;
            InitializeSlots();
        } 
    }

    public Inventory()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        slots.Clear();
        for (int i = 0; i < _maxSlots; i++)
        {
            slots.Add(new InventorySlot(null, 0));
        }
    }

    public void AddItem(ItemData item, int quantity)
    {
        if (item.isStackable)
        {
            InventorySlot existingSlot = slots.Find(s => s.item == item);
            if (existingSlot != null)
            {
                existingSlot.quantity += quantity;
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        if (item.isStackable)
        {
            InventorySlot emptySlot = slots.Find(s => s.item == null);
            if (emptySlot != null)
            {
                emptySlot.item = item;
                emptySlot.quantity = quantity;
                OnInventoryChanged?.Invoke();
            }
            else { Debug.Log("Inventory is full!"); }
        }
        else
        {
            for (int i = 0; i < quantity; i++)
            {
                InventorySlot emptySlot = slots.Find(s => s.item == null);
                if (emptySlot != null)
                {
                    emptySlot.item = item;
                    emptySlot.quantity = 1;
                }
                else
                {
                    Debug.Log("Inventory is full!");
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }
            OnInventoryChanged?.Invoke();
        }
    }

    public void RemoveItem(ItemData item, int quantity)
    {
        InventorySlot slot = slots.Find(s => s.item == item);
        if (slot != null)
        {
            slot.quantity -= quantity;
            if (slot.quantity <= 0)
            {
                slot.item = null;
                slot.quantity = 0;
            }
            OnInventoryChanged?.Invoke();
        }
    }

    // --- HIER IST DIE FUNKTION, DIE DEINE HOTBAR VERMISST HAT ---
    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= slots.Count) return;
        InventorySlot slot = slots[index];
        slot.item = null;
        slot.quantity = 0;
        OnInventoryChanged?.Invoke();
    }
    // -------------------------------------------------------------

    public void Clear()
    {
        foreach (var slot in slots)
        {
            slot.item = null;
            slot.quantity = 0;
        }
        OnInventoryChanged?.Invoke();
    }
}

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}