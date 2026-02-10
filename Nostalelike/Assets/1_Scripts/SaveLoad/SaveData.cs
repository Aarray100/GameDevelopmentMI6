using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // Meta-Daten
    public string saveDate;
    public string saveName;
    
    // Character
    public int selectedCharacterIndex;
    public Vector3Serializable playerPosition;
    public string currentSceneName;
    
    // Level System
    public int playerLevel;
    public float currentXP;
    public float xpToNextLevel;
    
    // Player Stats
    public float currentHealth;
    public float maxHealth;
    public float currentMana;
    public float maxMana;
    
    // Inventory
    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    
    // Equipment
    public List<EquipmentItemData> equippedItems = new List<EquipmentItemData>();
    
    // Hotbar
    public List<HotbarSlotData> hotbarSlots = new List<HotbarSlotData>();
    
    // Chests
    public List<ChestStateData> openedChests = new List<ChestStateData>();
    
    // Audio Settings
    public float musicVolume;
    public float sfxVolume;
    
    // Gold
    public int playerGold;
    
    // Journal
    public JournalSaveData journalData;
    
    public SaveData()
    {
        saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        saveName = "Save_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
    }
}

// Vector3 ist nicht serialisierbar, daher Wrapper-Klasse
[Serializable]
public struct Vector3Serializable
{
    public float x, y, z;
    
    public Vector3Serializable(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }
    
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[Serializable]
public class InventoryItemData
{
    public string itemID;
    public int slotIndex;
    public int stackCount;
}

[Serializable]
public class EquipmentItemData
{
    public string itemID;
    public int equipSlotType;
}

[Serializable]
public class HotbarSlotData
{
    public int slotIndex;
    public string itemID;
    public int quantity;
    public bool isEmpty;
}

[Serializable]
public class ChestStateData
{
    public string chestID;
    public bool isOpened;
}