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
public enum EquipmentSlot
{
    None,        // Für nicht-Equipment Items
    Weapon,      // Waffe (in Hotbar)
    Head,        // Helm
    Chest,       // Rüstung
    Hands,       // Handschuhe
    Legs,        // Hose
    Feet,        // Schuhe
    Amulet,      // Halskette
    Ring         // Ring-Slot
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

// Stats-System für Equipment und Waffen
[System.Serializable]
public class ItemStats
{
    [Header("Offensive Stats")]
    public float bonusDamage = 0f;
    public float damageMultiplier = 1f;        // z.B. 1.5 = 150% Damage
    public float bonusCritChance = 0f;         // z.B. 0.05 = +5%
    public float bonusCritDamage = 0f;         // z.B. 0.2 = +20%
    public float bonusAttackSpeed = 0f;
    
    [Header("Defensive Stats")]
    public float bonusDefense = 0f;
    public float bonusResistance = 0f;
    public float bonusEvasion = 0f;
    
    [Header("Resource Stats")]
    public float bonusHealth = 0f;
    public float bonusMana = 0f;
    public float bonusStamina = 0f;
    public float bonusHealthRegen = 0f;
    public float bonusManaRegen = 0f;
    public float bonusStaminaRegen = 0f;
    
    [Header("Magic Stats")]
    public float bonusMagicDamage = 0f;
    public float bonusMagicPower = 0f;
    
    // Helper um Stats zu addieren
    public static ItemStats operator +(ItemStats a, ItemStats b)
    {
        if (a == null) return b;
        if (b == null) return a;
        
        ItemStats result = new ItemStats();
        result.bonusDamage = a.bonusDamage + b.bonusDamage;
        result.damageMultiplier = a.damageMultiplier * b.damageMultiplier;
        result.bonusCritChance = a.bonusCritChance + b.bonusCritChance;
        result.bonusCritDamage = a.bonusCritDamage + b.bonusCritDamage;
        result.bonusAttackSpeed = a.bonusAttackSpeed + b.bonusAttackSpeed;
        result.bonusDefense = a.bonusDefense + b.bonusDefense;
        result.bonusResistance = a.bonusResistance + b.bonusResistance;
        result.bonusEvasion = a.bonusEvasion + b.bonusEvasion;
        result.bonusHealth = a.bonusHealth + b.bonusHealth;
        result.bonusMana = a.bonusMana + b.bonusMana;
        result.bonusStamina = a.bonusStamina + b.bonusStamina;
        result.bonusHealthRegen = a.bonusHealthRegen + b.bonusHealthRegen;
        result.bonusManaRegen = a.bonusManaRegen + b.bonusManaRegen;
        result.bonusStaminaRegen = a.bonusStaminaRegen + b.bonusStaminaRegen;
        result.bonusMagicDamage = a.bonusMagicDamage + b.bonusMagicDamage;
        result.bonusMagicPower = a.bonusMagicPower + b.bonusMagicPower;
        return result;
    }
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
    
    [Header("Equipment & Weapon Info")]
    public EquipmentSlot equipSlot = EquipmentSlot.None;  // Wo wird es equipped?
    public WeaponType weaponType = WeaponType.None;       // Nur für Waffen
    public AccessoryType accessoryType = AccessoryType.None;
    
    [Header("Item Stats (Equipment & Weapons)")]
    public ItemStats stats = new ItemStats();  // Alle Bonus-Stats hier!

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
