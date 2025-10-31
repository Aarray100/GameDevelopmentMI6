using UnityEngine;
using System.Collections.Generic;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
public enum ItemType
{
    Consumable,
    Equipment,
    Quest,
    Miscellaneous,
    Currency,
    Tool,
    Weapon,
    Accessory
}
public enum EquipmentType
{
    Head,
    Chest,
    Legs,

    Feet,
    Hands,
    Accessory,
    None
}
public enum ConsumableType
{
    HealthPotion,
    ManaPotion,
    StaminaPotion,
    Food,
    Drink,
    None
}
public enum WeaponType
{
    Sword,
    Bow,
    Staff,
    Dagger,
    Axe,
    None
}
public enum ToolType
{
    Pickaxe,
    Axe,
    Shovel,
    FishingRod,

    None
}
public enum CurrencyType
{
    Gold,
    Silver,
    Bronze,
    None
}
public enum QuestItemType
{
    Mainquest,
    Sidequest,
    Collectible,
    None
}

public enum MiscellaneousType
{
    CraftingMaterial,
    Junk,
    None
}
public enum AccessoryType
{
    Ring,
    Amulet,
    Bracelet,
    None
}


[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("General Item Information")]
    public string itemName;
    public Sprite itemIcon;
    [TextArea(3, 10)]
    public string itemDescription;
    public bool isStackable;
    public int itemValue;
    public ItemRarity itemRarity;

    [Header("Item Type Classification")]
    public ItemType itemType;

    [Header("Equipment Specifics Stats(IF ItemType is Equipment)")]
    public EquipmentType equipmentType;
    public int defense;
    public AccessoryType accessoryType;

    [Header("Weapon Specific Stats (IF ItemType is Weapon)")]
    public WeaponType weaponType;
    public int attackPower;
    public float attackSpeed;
    public float range;
    public float criticalChance;
    public float criticalDamageMultiplier;
    public float knockback;

    [Header("Consumable Specific Stats (IF ItemType is Consumable)")]
    public ConsumableType consumableType;
    public int healAmount;
    public int manaAmount;
    public int staminaAmount;
    public int duration;
    public int cooldown;

    [Header("Tool Specific Stats (IF ItemType is Tool)")]
    public ToolType toolType;
    public int toolPower;
    public float toolSpeed;
    public int toolDamage;

    [Header("Other Item Types (IF ItemType is Currency, Quest, or Miscellaneous)")]
    public CurrencyType currencyType;
    public QuestItemType questItemType;
    public MiscellaneousType miscellaneousType;    
}
