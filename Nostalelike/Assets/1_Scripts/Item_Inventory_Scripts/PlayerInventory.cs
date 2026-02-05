using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerInventory : MonoBehaviour
{
    public Inventory inventory = new Inventory();

    [SerializeField] private int inventorySize = 49;

    [Header("UI References")]
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("UI Toggle Key")]
    public GameObject inventoryPanelObject;
    public GameObject equipmentPanelObject;
    public GameObject statSheetPanelObject;

    private bool isInventoryOpen = false;
    public List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    private void Awake()
    {
        inventory.maxSlots = inventorySize;
        if (inventoryPanelObject != null) inventoryPanelObject.SetActive(false);
        if (equipmentPanelObject != null) equipmentPanelObject.SetActive(false);
        if (statSheetPanelObject != null) statSheetPanelObject.SetActive(false);
    }

    private void Start()
    {
        inventory.OnInventoryChanged += UpdateUISlots;
    }

    private void OnDestroy()
    {
        inventory.OnInventoryChanged -= UpdateUISlots;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    // --- ITEM BENUTZEN ---
    public void UseItem(int index)
    {
        if (index < 0 || index >= inventory.slots.Count) return;
        InventorySlot slot = inventory.slots[index];

        if (slot == null || slot.item == null || slot.quantity <= 0) return;

        ItemData itemToUse = slot.item;
        itemToUse.UseItem(); // Effekt ausführen

        if (itemToUse.itemType == ItemType.Consumable)
        {
            inventory.RemoveItem(itemToUse, 1);
            UpdateUISlots();
        }
    }
    // ---------------------
    
    // --- ITEM DROPPEN ---
    [Header("Drop Settings")]
    public GameObject itemPickupPrefab; // Im Inspector zuweisen!
    public float dropOffset = 4f; // Abstand vom Spieler (größer = weiter weg)
    
    /// <summary>
    /// Droppt ein Item aus einem bestimmten Slot auf den Boden
    /// </summary>
    public void DropItem(int slotIndex, int amount = -1)
    {
        if (slotIndex < 0 || slotIndex >= inventory.slots.Count) return;
        
        InventorySlot slot = inventory.slots[slotIndex];
        if (slot == null || slot.item == null || slot.quantity <= 0) return;
        
        // Wenn amount nicht angegeben, droppe alles
        int dropAmount = (amount <= 0 || amount > slot.quantity) ? slot.quantity : amount;
        
        // Item spawnen
        SpawnDroppedItem(slot.item, dropAmount);
        
        // Aus Inventar entfernen - nutze die vorhandene Methode die das Event triggert
        if (dropAmount >= slot.quantity)
        {
            // Alles droppen
            inventory.RemoveItemAt(slotIndex);
        }
        else
        {
            // Nur teilweise droppen
            inventory.RemoveItem(slot.item, dropAmount);
        }
        
        UpdateUISlots();
    }
    
    /// <summary>
    /// Spawnt ein Item auf dem Boden neben dem Spieler
    /// </summary>
    private void SpawnDroppedItem(ItemData item, int quantity)
    {
        if (item == null) return;
        
        // Position berechnen (vor dem Spieler)
        Vector2 dropPosition = (Vector2)transform.position + Random.insideUnitCircle.normalized * dropOffset;
        
        GameObject droppedItem;
        
        if (itemPickupPrefab != null)
        {
            droppedItem = Instantiate(itemPickupPrefab, dropPosition, Quaternion.identity);
        }
        else
        {
            // Fallback: Einfaches GameObject erstellen
            droppedItem = new GameObject($"DroppedItem_{item.itemName}");
            droppedItem.transform.position = dropPosition;
            droppedItem.AddComponent<SpriteRenderer>();
            droppedItem.AddComponent<CircleCollider2D>().isTrigger = true;
        }
        
        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
        if (pickup == null)
            pickup = droppedItem.AddComponent<ItemPickup>();
            
        pickup.InitializeDroppedItem(item, quantity, 60f); // 60 Sekunden despawn
        
        Debug.Log($"Dropped {quantity}x {item.itemName}");
    }
    // ---------------------

    public void ToggleInventory()
    {
        if (inventoryPanelObject == null) return;
        isInventoryOpen = !isInventoryOpen;
        inventoryPanelObject.SetActive(isInventoryOpen);
        if (equipmentPanelObject != null) equipmentPanelObject.SetActive(isInventoryOpen);
        if (statSheetPanelObject != null) statSheetPanelObject.SetActive(isInventoryOpen);
    }

    public void InitializeInventoryUI()
    {
        if (uiSlots != null && uiSlots.Count > 0)
        {
            uiSlots.RemoveAll(slot => slot == null);
            UpdateUISlots();
            return;
        }
        GenerateUISlots();
        UpdateUISlots();
    }

    private void GenerateUISlots()
    {
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotParent);
            InventorySlotUI slotUI = newSlot.GetComponent<InventorySlotUI>();

            if (slotUI != null)
            {
                slotUI.playerInventory = this;
                slotUI.slotIndex = i; 
                uiSlots.Add(slotUI);
                slotUI.ClearSlot();
            }
        }
    }

    public void UpdateUISlots()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (uiSlots[i] == null) continue;

            if (i < inventory.slots.Count)
            {
                uiSlots[i].UpdateSlot(inventory.slots[i]);
            }
            else
            {
                uiSlots[i].ClearSlot();
            }
        }
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (uiSlots != null && uiSlots.Count > 0) UpdateUISlots();
    }

    // --- SAVE / LOAD SYSTEM (WIEDER EINGEFÜGT) ---
    public List<InventoryItemData> GetSaveData()
    {
        List<InventoryItemData> data = new List<InventoryItemData>();
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            var slot = inventory.slots[i];
            if (slot != null && slot.item != null)
            {
                InventoryItemData itemData = new InventoryItemData
                {
                    itemID = slot.item.itemName,
                    slotIndex = i,
                    stackCount = slot.quantity
                };
                data.Add(itemData);
            }
        }
        return data;
    }

    public void LoadSaveData(List<InventoryItemData> data)
    {
        if (data == null) return;
        inventory.Clear();
        foreach (var itemData in data)
        {
            ItemData item = SaveManager.Instance?.GetItemByName(itemData.itemID);
            if (item != null)
            {
                if (itemData.slotIndex >= 0 && itemData.slotIndex < inventory.slots.Count)
                {
                    inventory.slots[itemData.slotIndex].item = item;
                    inventory.slots[itemData.slotIndex].quantity = itemData.stackCount;
                }
            }
        }
        UpdateUISlots();
    }
}