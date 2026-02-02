using UnityEngine;

// --- 1. ENUMS ---
public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
public enum ItemType { Consumable, Equipment, Quest, Miscellaneous, Currency, Tool, Weapon, Accessory, Book }
public enum EquipmentSlot { None, Weapon, Head, Chest, Hands, Legs, Feet, Amulet, Ring }
public enum ConsumableType { HealthPotion, ManaPotion, StaminaPotion, StrengthPotion, SpeedPotion, OmniPotion, Food, Drink, None }
public enum WeaponType { Sword, Bow, Staff, None }
public enum ToolType { Pickaxe, Axe, Shovel, FishingRod, None }
public enum CurrencyType { Gold, Silver, Bronze, None }
public enum QuestItemType { Mainquest, Sidequest, Collectible, None }
public enum MiscellaneousType { CraftingMaterial, Junk, None }
public enum AccessoryType { Ring, Amulet, Bracelet, None }

// --- 2. ITEM STATS KLASSE ---
[System.Serializable]
public class ItemStats
{
    [Header("Offensive Stats")]
    public float bonusDamage = 0f;
    public float damageMultiplier = 1f;
    public float bonusCritChance = 0f;
    public float bonusCritDamage = 0f;
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

// --- 3. ITEM DATA SCRIPTABLE OBJECT ---
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
    
    [Header("Consumable Stats")]
    public ConsumableType consumableType;
    public int healAmount;     
    public int manaAmount;     
    public int staminaAmount;  
    public int duration;       
    public int cooldown;

    [Header("Equipment & Weapon Info")]
    public EquipmentSlot equipSlot = EquipmentSlot.None;
    public WeaponType weaponType = WeaponType.None;
    public AccessoryType accessoryType = AccessoryType.None;
    public ItemStats stats = new ItemStats();
    
    [Header("Other Types")]
    public ToolType toolType;
    public CurrencyType currencyType;
    public QuestItemType questItemType;
    public MiscellaneousType miscellaneousType;

    // --- LOGIK ---
    public void UseItem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) 
        {
            Debug.LogWarning("ItemData: Kein Spieler mit Tag 'Player' gefunden!");
            return;
        }

        switch (itemType)
        {
            case ItemType.Consumable:
                HandleConsumable(player);
                break;
            case ItemType.Equipment:
            case ItemType.Weapon:
            case ItemType.Accessory:
                Debug.Log($"{itemName} ausgerüstet!");
                break;
            default:
                Debug.Log("Item kann nicht direkt benutzt werden.");
                break;
        }
    }

    private void HandleConsumable(GameObject player)
    {
        // FIX: Audio Code entfernt, da 'drinkSound' in deinem AudioManager fehlt.
        // Falls du Sound willst, füge im AudioManager 'public AudioClip drinkSound;' hinzu und entferne hier die Kommentare.
        /*
        if (AudioManager.Instance != null) 
        {
            // AudioManager.Instance.PlaySFX("Drink"); 
        }
        */

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null) return;

        switch (consumableType)
        {
            case ConsumableType.HealthPotion:
                playerStats.Heal((float)healAmount);
                Debug.Log($"<color=green>HP geheilt: +{healAmount}</color>");
                break;

            case ConsumableType.ManaPotion:
                playerStats.RestoreMana((float)manaAmount);
                Debug.Log($"<color=blue>Mana wiederhergestellt: +{manaAmount}</color>");
                break;

            case ConsumableType.StaminaPotion:
                playerStats.RestoreStamina((float)staminaAmount);
                Debug.Log($"<color=orange>Stamina wiederhergestellt: +{staminaAmount}</color>");
                break;

            case ConsumableType.StrengthPotion:
                playerStats.ApplyStrengthBuff(1.3f, (float)duration);
                Debug.Log("<color=red>Stärke Buff (+30%) aktiviert!</color>");
                if (BuffManager.Instance != null) BuffManager.Instance.AddBuff(itemIcon, duration);
                break;

            case ConsumableType.SpeedPotion:
                playerStats.ApplySpeedBuff(1.3f, (float)duration);
                Debug.Log("<color=cyan>Speed Buff (+30%) aktiviert!</color>");
                if (BuffManager.Instance != null) BuffManager.Instance.AddBuff(itemIcon, duration);
                break;
            
            case ConsumableType.OmniPotion:
                playerStats.ForceLevelUp(); 
                Debug.Log("<color=yellow>OMNI POTION: LEVEL UP!</color>");
                break;

            case ConsumableType.Food:
            case ConsumableType.Drink:
                playerStats.Heal((float)healAmount);
                break;
        }
    }
}