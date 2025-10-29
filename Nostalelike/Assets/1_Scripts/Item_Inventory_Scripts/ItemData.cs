using UnityEngine;
using System.Collections.Generic;

public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }


[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public string itemDescription;


    public bool isStackable;
    public int itemValue;
    public ItemRarity itemRarity;
}
