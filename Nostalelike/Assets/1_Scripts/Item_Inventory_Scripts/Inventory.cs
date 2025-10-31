using UnityEngine;
using System.Collections.Generic;
using System;

public class Inventory
{

    public event Action OnInventoryChanged;
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int maxSlots = 49;

    public void AddItem(ItemData item, int quantity)
    {
        if (item.isStackable)
        {
            InventorySlot slot = slots.Find(s => s.item == item);
            if (slot != null)
            {
                slot.quantity += quantity;
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        if (slots.Count >= maxSlots)
        {
            Debug.Log("Inventory is full!");
            return;
        }

        if (item.isStackable)
        {
            slots.Add(new InventorySlot(item, quantity));
        }
        else
        {
            for (int i = 0; i < quantity; i++)
            {
                if (slots.Count >= maxSlots)
                {
                    Debug.Log("Inventory is full!");
                    OnInventoryChanged?.Invoke();
                    return;
                }
                slots.Add(new InventorySlot(item, 1));
            }
        }
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemData item, int quantity)
    {
        InventorySlot slot = slots.Find(s => s.item == item);
        if (slot != null)
        {
            slot.quantity -= quantity;
            if (slot.quantity <= 0)
            {
                slots.Remove(slot);
            }
            OnInventoryChanged?.Invoke();
        }
    }
    public bool HasItem(ItemData item, int quantity)
    {
        InventorySlot slot = slots.Find(s => s.item == item);
        if (slot != null)
        {
            return slot.quantity >= quantity;
        }
        else
        {
            return false;
        }
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
