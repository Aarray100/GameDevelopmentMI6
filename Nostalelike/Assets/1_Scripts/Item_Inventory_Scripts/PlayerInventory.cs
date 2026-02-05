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
    public GameObject itemPickupPrefab; // Das Prefab, das gespawnt wird wenn ein Item gedroppt wird
    public float dropDistance = 1.5f;   // Wie weit vor dem Spieler das Item gespawnt wird

    public void DropItem(int index, int dropQuantity = -1)
    {
        if (index < 0 || index >= inventory.slots.Count) return;
        
        var (item, quantity) = inventory.DropItemAt(index, dropQuantity);
        
        if (item != null && quantity > 0)
        {
            SpawnDroppedItem(item, quantity);
            UpdateUISlots();
        }
    }

    private void SpawnDroppedItem(ItemData item, int quantity)
    {
        if (itemPickupPrefab == null)
        {
            // Fallback: Erstelle ein einfaches GameObject wenn kein Prefab zugewiesen ist
            GameObject droppedItem = new GameObject($"DroppedItem_{item.itemName}");
            droppedItem.transform.position = GetDropPosition();
            
            SpriteRenderer sr = droppedItem.AddComponent<SpriteRenderer>();
            sr.sprite = item.itemIcon;
            sr.sortingOrder = 5;
            
            CircleCollider2D col = droppedItem.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            
            ItemPickup pickup = droppedItem.AddComponent<ItemPickup>();
            pickup.itemToPickup = item;
            pickup.quantity = quantity;
            
            Debug.Log($"Item gedroppt: {item.itemName} x{quantity}");
        }
        else
        {
            // Benutze das zugewiesene Prefab
            GameObject droppedItem = Instantiate(itemPickupPrefab, GetDropPosition(), Quaternion.identity);
            
            ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.itemToPickup = item;
                pickup.quantity = quantity;
            }
            
            // Sprite aktualisieren
            SpriteRenderer sr = droppedItem.GetComponent<SpriteRenderer>();
            if (sr != null && item.itemIcon != null)
            {
                sr.sprite = item.itemIcon;
            }
            
            Debug.Log($"Item gedroppt: {item.itemName} x{quantity}");
        }
    }

    private Vector3 GetDropPosition()
    {
        // Position vor dem Spieler berechnen (basierend auf Blickrichtung)
        Vector3 dropPos = transform.position;
        
        // Versuche die Blickrichtung zu ermitteln
        PlayerMovement2D movement = GetComponent<PlayerMovement2D>();
        if (movement != null)
        {
            // Hier könntest du die lastDirection vom Movement Script nutzen
            // Fallback: Random Offset
            Vector2 randomOffset = Random.insideUnitCircle.normalized * dropDistance;
            dropPos += new Vector3(randomOffset.x, randomOffset.y, 0);
        }
        else
        {
            // Fallback: Vor dem Spieler (nach unten)
            dropPos += Vector3.down * dropDistance;
        }
        
        return dropPos;
    }
    // --------------------

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