using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Verwaltet den Loot-Drop eines Gegners basierend auf dessen Level.
/// Höhere Gegner-Level = bessere Chancen auf seltene Items.
/// </summary>
public class EnemyLootSystem : MonoBehaviour
{
    [Header("Loot Tables")]
    [Tooltip("Mögliche Items die dieser Gegner droppen kann")]
    public List<EnemyLootEntry> lootTable = new List<EnemyLootEntry>();
    
    [Header("Drop Settings")]
    [Tooltip("Garantierter Drop? (Mindestens 1 Item wird immer gedroppt)")]
    public bool guaranteedDrop = false;
    
    [Tooltip("Maximale Anzahl verschiedener Items die gleichzeitig droppen können")]
    [Range(1, 5)]
    public int maxDrops = 2;
    
    [Tooltip("Abstand zwischen gespawnten Items")]
    public float dropSpreadRadius = 0.5f;
    
    [Header("Level-Based Rarity Bonuses")]
    [Tooltip("Ab welchem Gegner-Level Uncommon Items droppen können")]
    public int uncommonUnlockLevel = 3;
    
    [Tooltip("Ab welchem Gegner-Level Rare Items droppen können")]
    public int rareUnlockLevel = 8;
    
    [Tooltip("Ab welchem Gegner-Level Epic Items droppen können")]
    public int epicUnlockLevel = 15;
    
    [Tooltip("Ab welchem Gegner-Level Legendary Items droppen können")]
    public int legendaryUnlockLevel = 25;
    
    [Header("Drop Chance Multipliers (per Rarity)")]
    [Tooltip("Basis Drop-Chance für Common Items (0-100%)")]
    [Range(0f, 100f)]
    public float commonDropChance = 50f;
    
    [Tooltip("Basis Drop-Chance für Uncommon Items (0-100%)")]
    [Range(0f, 100f)]
    public float uncommonDropChance = 25f;
    
    [Tooltip("Basis Drop-Chance für Rare Items (0-100%)")]
    [Range(0f, 100f)]
    public float rareDropChance = 10f;
    
    [Tooltip("Basis Drop-Chance für Epic Items (0-100%)")]
    [Range(0f, 100f)]
    public float epicDropChance = 3f;
    
    [Tooltip("Basis Drop-Chance für Legendary Items (0-100%)")]
    [Range(0f, 100f)]
    public float legendaryDropChance = 0.5f;
    
    [Header("Level Scaling")]
    [Tooltip("Bonus Drop-Chance pro Level über dem Unlock-Level (in %)")]
    [Range(0f, 5f)]
    public float dropChanceBonusPerLevel = 1f;
    
    // Referenzen
    private EnemyStats enemyStats;
    private int currentLevel = 1;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        if (enemyStats != null)
        {
            enemyStats.OnStatsCalculated += UpdateLevel;
            currentLevel = enemyStats.Level;
        }
    }

    private void OnDestroy()
    {
        if (enemyStats != null)
        {
            enemyStats.OnStatsCalculated -= UpdateLevel;
        }
    }

    private void UpdateLevel()
    {
        if (enemyStats != null)
        {
            currentLevel = enemyStats.Level;
        }
    }

    /// <summary>
    /// Wird von EnemyHealth aufgerufen wenn der Gegner stirbt.
    /// </summary>
    public void DropLoot()
    {
        if (lootTable.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: Keine Items in der Loot-Table!");
            return;
        }

        List<ItemData> droppedItems = new List<ItemData>();
        int dropCount = 0;

        // Gehe durch alle möglichen Loot-Einträge
        foreach (EnemyLootEntry entry in lootTable)
        {
            if (entry.item == null) continue;
            if (dropCount >= maxDrops) break;

            // Prüfe ob Item gedroppt werden kann (Level-Check + Chance)
            if (TryDropItem(entry))
            {
                droppedItems.Add(entry.item);
                dropCount++;
            }
        }

        // Garantierter Drop: Falls nichts gedroppt wurde, droppe ein zufälliges Common Item
        if (guaranteedDrop && droppedItems.Count == 0)
        {
            ItemData fallbackItem = GetRandomCommonItem();
            if (fallbackItem != null)
            {
                droppedItems.Add(fallbackItem);
            }
        }

        // Items spawnen
        SpawnDroppedItems(droppedItems);
    }

    /// <summary>
    /// Prüft ob ein bestimmtes Item gedroppt werden soll.
    /// </summary>
    private bool TryDropItem(EnemyLootEntry entry)
    {
        ItemRarity rarity = entry.item.itemRarity;

        // Level-Check: Kann diese Seltenheit überhaupt droppen?
        if (!CanDropRarity(rarity))
        {
            return false;
        }

        // Berechne finale Drop-Chance
        float baseChance = GetBaseDropChance(rarity);
        float levelBonus = CalculateLevelBonus(rarity);
        float entryModifier = entry.dropChanceModifier;
        
        float finalChance = (baseChance + levelBonus) * entryModifier;
        finalChance = Mathf.Clamp(finalChance, 0f, 100f);

        // Würfeln
        float roll = Random.Range(0f, 100f);
        bool success = roll <= finalChance;

        if (success)
        {
            Debug.Log($"<color=green>LOOT:</color> {entry.item.itemName} ({rarity}) gedroppt! " +
                      $"Chance war {finalChance:F1}% (Roll: {roll:F1})");
        }

        return success;
    }

    /// <summary>
    /// Prüft ob eine bestimmte Seltenheit basierend auf dem Gegner-Level droppen kann.
    /// Gegner droppen NUR die höchste freigeschaltete Seltenheit + eine Stufe darunter.
    /// </summary>
    private bool CanDropRarity(ItemRarity rarity)
    {
        // Bestimme die höchste freigeschaltete Seltenheit für dieses Level
        ItemRarity highestUnlocked = GetHighestUnlockedRarity();
        
        // Erlaubte Seltenheiten: höchste und eine darunter
        ItemRarity oneBelow = GetRarityBelow(highestUnlocked);
        
        // Item muss entweder die höchste oder eine Stufe darunter sein
        if (rarity != highestUnlocked && rarity != oneBelow)
        {
            return false;
        }
        
        // Zusätzlich: Das Item muss auch für dieses Level freigeschaltet sein
        switch (rarity)
        {
            case ItemRarity.Common:
                return true;
            case ItemRarity.Uncommon:
                return currentLevel >= uncommonUnlockLevel;
            case ItemRarity.Rare:
                return currentLevel >= rareUnlockLevel;
            case ItemRarity.Epic:
                return currentLevel >= epicUnlockLevel;
            case ItemRarity.Legendary:
                return currentLevel >= legendaryUnlockLevel;
            default:
                return true;
        }
    }

    /// <summary>
    /// Ermittelt die höchste Seltenheit die bei diesem Level droppen kann.
    /// </summary>
    private ItemRarity GetHighestUnlockedRarity()
    {
        if (currentLevel >= legendaryUnlockLevel)
            return ItemRarity.Legendary;
        if (currentLevel >= epicUnlockLevel)
            return ItemRarity.Epic;
        if (currentLevel >= rareUnlockLevel)
            return ItemRarity.Rare;
        if (currentLevel >= uncommonUnlockLevel)
            return ItemRarity.Uncommon;
        
        return ItemRarity.Common;
    }

    /// <summary>
    /// Gibt die Seltenheitsstufe eine Stufe unter der angegebenen zurück.
    /// </summary>
    private ItemRarity GetRarityBelow(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Legendary:
                return ItemRarity.Epic;
            case ItemRarity.Epic:
                return ItemRarity.Rare;
            case ItemRarity.Rare:
                return ItemRarity.Uncommon;
            case ItemRarity.Uncommon:
                return ItemRarity.Common;
            case ItemRarity.Common:
            default:
                return ItemRarity.Common; // Common ist das Minimum
        }
    }

    /// <summary>
    /// Gibt die Basis-Drop-Chance für eine Seltenheit zurück.
    /// </summary>
    private float GetBaseDropChance(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:     return commonDropChance;
            case ItemRarity.Uncommon:   return uncommonDropChance;
            case ItemRarity.Rare:       return rareDropChance;
            case ItemRarity.Epic:       return epicDropChance;
            case ItemRarity.Legendary:  return legendaryDropChance;
            default:                    return commonDropChance;
        }
    }

    /// <summary>
    /// Berechnet den Level-Bonus für die Drop-Chance.
    /// Je höher das Level über dem Unlock-Level, desto höher die Chance.
    /// </summary>
    private float CalculateLevelBonus(ItemRarity rarity)
    {
        int unlockLevel = GetUnlockLevel(rarity);
        int levelsAboveUnlock = currentLevel - unlockLevel;
        
        if (levelsAboveUnlock <= 0) return 0f;
        
        return levelsAboveUnlock * dropChanceBonusPerLevel;
    }

    /// <summary>
    /// Gibt das Unlock-Level für eine Seltenheit zurück.
    /// </summary>
    private int GetUnlockLevel(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:     return 1;
            case ItemRarity.Uncommon:   return uncommonUnlockLevel;
            case ItemRarity.Rare:       return rareUnlockLevel;
            case ItemRarity.Epic:       return epicUnlockLevel;
            case ItemRarity.Legendary:  return legendaryUnlockLevel;
            default:                    return 1;
        }
    }

    /// <summary>
    /// Findet ein zufälliges Common Item für den garantierten Drop.
    /// </summary>
    private ItemData GetRandomCommonItem()
    {
        List<EnemyLootEntry> commonItems = lootTable.FindAll(e => 
            e.item != null && e.item.itemRarity == ItemRarity.Common);
        
        if (commonItems.Count > 0)
        {
            return commonItems[Random.Range(0, commonItems.Count)].item;
        }
        
        // Fallback: Irgendein Item
        if (lootTable.Count > 0 && lootTable[0].item != null)
        {
            return lootTable[0].item;
        }
        
        return null;
    }

    /// <summary>
    /// Spawnt die gedroppten Items in der Welt.
    /// </summary>
    private void SpawnDroppedItems(List<ItemData> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            ItemData item = items[i];
            
            // Position mit leichtem Offset berechnen
            Vector2 offset = Random.insideUnitCircle * dropSpreadRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0);
            
            // Item-Pickup erstellen
            GameObject droppedItem = new GameObject($"DroppedLoot_{item.itemName}");
            droppedItem.transform.position = spawnPos;
            
            // SpriteRenderer
            SpriteRenderer sr = droppedItem.AddComponent<SpriteRenderer>();
            sr.sprite = item.itemIcon;
            sr.sortingOrder = 99;
            
            // Collider für Pickup
            CircleCollider2D col = droppedItem.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;
            
            // ItemPickup Komponente
            ItemPickup pickup = droppedItem.AddComponent<ItemPickup>();
            pickup.itemToPickup = item;
            pickup.quantity = 1;
            
            Debug.Log($"<color=yellow>SPAWNED:</color> {item.itemName} at {spawnPos}");
        }
    }

    /// <summary>
    /// Debug-Methode: Zeigt die aktuellen Drop-Chancen im Log an.
    /// </summary>
    [ContextMenu("Show Drop Chances")]
    public void ShowDropChances()
    {
        ItemRarity highest = GetHighestUnlockedRarity();
        ItemRarity oneBelow = GetRarityBelow(highest);
        
        Debug.Log($"=== Drop Chances für Level {currentLevel} ===");
        Debug.Log($"Höchste Seltenheit: {highest} | Erlaubt: {highest} + {oneBelow}");
        Debug.Log($"---");
        Debug.Log($"Common: {(CanDropRarity(ItemRarity.Common) ? $"{commonDropChance + CalculateLevelBonus(ItemRarity.Common):F1}% ✓" : "BLOCKED ✗")}");
        Debug.Log($"Uncommon: {(CanDropRarity(ItemRarity.Uncommon) ? $"{uncommonDropChance + CalculateLevelBonus(ItemRarity.Uncommon):F1}% ✓" : "BLOCKED ✗")}");
        Debug.Log($"Rare: {(CanDropRarity(ItemRarity.Rare) ? $"{rareDropChance + CalculateLevelBonus(ItemRarity.Rare):F1}% ✓" : "BLOCKED ✗")}");
        Debug.Log($"Epic: {(CanDropRarity(ItemRarity.Epic) ? $"{epicDropChance + CalculateLevelBonus(ItemRarity.Epic):F1}% ✓" : "BLOCKED ✗")}");
        Debug.Log($"Legendary: {(CanDropRarity(ItemRarity.Legendary) ? $"{legendaryDropChance + CalculateLevelBonus(ItemRarity.Legendary):F1}% ✓" : "BLOCKED ✗")}");
    }
}

/// <summary>
/// Ein einzelner Eintrag in der Loot-Table.
/// </summary>
[System.Serializable]
public class EnemyLootEntry
{
    [Tooltip("Das Item das gedroppt werden kann")]
    public ItemData item;
    
    [Tooltip("Multiplikator für die Drop-Chance (1.0 = normal, 2.0 = doppelt so wahrscheinlich)")]
    [Range(0.1f, 5f)]
    public float dropChanceModifier = 1f;
    
    [Tooltip("Minimale Anzahl die gedroppt wird")]
    [Range(1, 10)]
    public int minQuantity = 1;
    
    [Tooltip("Maximale Anzahl die gedroppt wird")]
    [Range(1, 10)]
    public int maxQuantity = 1;
}
