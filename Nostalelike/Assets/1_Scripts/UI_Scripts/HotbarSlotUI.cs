using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HotbarSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI References")]
    public Image itemIcon;                             // Nur sichtbar wenn Item vorhanden
    public Image hotbarSlotImageIfSelected;            // Nur sichtbar wenn ausgewählt (Selection Highlight)
    public Image hotbarSlotImage;                      // Immer sichtbar (Der Slot selbst)
    public Image hotbarPanelImage;                     // Immer sichtbar (Rahmen)
    public TextMeshProUGUI itemCountText;              // Optional: Anzahl anzeigen (number_of_items)
    
    [Header("Settings")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    
    private int slotIndex;
    private HotbarSlot hotbarSlot;
    
    // Drag and Drop
    private CanvasGroup canvasGroup;
    private static HotbarSlotUI currentlyDraggedSlot;
    private static GameObject currentlyDraggedIcon;
    private static Canvas mainCanvas;
    
    private void Awake()
    {
        // CanvasGroup für Drag & Drop
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Erstelle Drag-Icon falls nicht vorhanden
        if (currentlyDraggedIcon == null)
        {
            currentlyDraggedIcon = GameObject.Find("DraggedItemIcon");
            if (currentlyDraggedIcon == null)
            {
                CreateDraggedItemIcon();
            }
            else
            {
                currentlyDraggedIcon.SetActive(false);
            }
        }
    }
    
    private void CreateDraggedItemIcon()
    {
        if (mainCanvas == null)
        {
            mainCanvas = GetComponentInParent<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("Kein Canvas gefunden!");
                return;
            }
        }
        
        currentlyDraggedIcon = new GameObject("DraggedItemIcon");
        currentlyDraggedIcon.transform.SetParent(mainCanvas.transform, false);
        
        Image dragImage = currentlyDraggedIcon.AddComponent<Image>();
        dragImage.raycastTarget = false;
        dragImage.preserveAspect = true;
        
        // Feste Größe für Drag-Icon (wie im Inventory)
        RectTransform rectTransform = currentlyDraggedIcon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(10, 10);
        
        Color color = dragImage.color;
        color.a = 0.8f;
        dragImage.color = color;
        
        currentlyDraggedIcon.SetActive(false);
    }
    
    public void Initialize(int index, HotbarSlot slot)
    {
        slotIndex = index;
        hotbarSlot = slot;
        
        // Initial: Selection-Highlight ausblenden
        if (hotbarSlotImageIfSelected != null)
        {
            hotbarSlotImageIfSelected.enabled = false;
        }
        
        // Slot und Rahmen immer sichtbar
        if (hotbarSlotImage != null)
        {
            hotbarSlotImage.enabled = true;
        }
        if (hotbarPanelImage != null)
        {
            hotbarPanelImage.enabled = true;
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// Aktualisiert die UI basierend auf dem Slot-Zustand (wie InventorySlotUI)
    /// </summary>
    public void UpdateUI()
    {
        if (hotbarSlot == null) return;
        
        // Null-Checks für UI-Komponenten
        if (itemIcon == null || itemCountText == null)
        {
            Debug.LogWarning("UI components are null in HotbarSlotUI - slot might be destroyed");
            return;
        }
        
        // Item Icon: Nur sichtbar wenn Item vorhanden
        if (hotbarSlot.item != null)
        {
            itemIcon.sprite = hotbarSlot.item.itemIcon;
            itemIcon.enabled = true;
            
            // Zeige die Anzahl nur an, wenn stapelbar und mehr als 1
            if (hotbarSlot.item.isStackable && hotbarSlot.quantity > 1)
            {
                itemCountText.text = hotbarSlot.quantity.ToString();
                itemCountText.enabled = true;
            }
            else
            {
                itemCountText.enabled = false;
            }
        }
        else
        {
            // Slot ist leer
            ClearSlot();
        }
    }
    
    /// <summary>
    /// Leert den Slot (wie InventorySlotUI)
    /// </summary>
    public void ClearSlot()
    {
        // Null-Checks um Fehler bei zerstörten UI-Elementen zu vermeiden
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
        
        if (itemCountText != null)
        {
            itemCountText.text = "";
            itemCountText.enabled = false;
        }
    }
    
    /// <summary>
    /// Zeigt den Slot als ausgewählt an
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (hotbarSlotImageIfSelected != null)
        {
            hotbarSlotImageIfSelected.enabled = isSelected;
            
            // Optional: Farbe ändern
            if (isSelected)
            {
                hotbarSlotImageIfSelected.color = selectedColor;
            }
        }
    }
    
    // === Drag & Drop Handlers ===
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Nur ziehen wenn ein Item vorhanden ist
        if (hotbarSlot == null || hotbarSlot.IsEmpty())
        {
            return;
        }
        
        if (currentlyDraggedIcon == null)
        {
            Debug.LogError("DraggedItemIcon ist null!");
            return;
        }
        
        currentlyDraggedSlot = this;
        currentlyDraggedIcon.SetActive(true);
        
        Image dragImage = currentlyDraggedIcon.GetComponent<Image>();
        if (dragImage != null)
        {
            dragImage.sprite = itemIcon.sprite;
            dragImage.enabled = true;
        }
        
        currentlyDraggedIcon.transform.position = Input.mousePosition;
        
        canvasGroup.blocksRaycasts = false;
        itemIcon.enabled = false;
        itemCountText.enabled = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (currentlyDraggedSlot == null)
        {
            return;
        }
        currentlyDraggedIcon.transform.position = Input.mousePosition;
    }
    
    /// <summary>
    /// Drop-Handler: Kann von Inventory ODER von anderer Hotbar kommen
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // Check 1: Kommt von Inventory?
        InventorySlotUI inventorySlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (inventorySlot != null && inventorySlot.playerInventory != null)
        {
            Inventory inventory = inventorySlot.playerInventory.inventory;
            if (inventory == null || inventory.slots == null || inventorySlot.slotIndex >= inventory.slots.Count)
            {
                return;
            }
            
            InventorySlot sourceSlot = inventory.slots[inventorySlot.slotIndex];
            
            if (sourceSlot != null && sourceSlot.item != null)
            {
                Hotbar hotbar = GetComponentInParent<Hotbar>();
                if (hotbar != null)
                {
                    ItemData itemToMove = sourceSlot.item;
                    int quantityToMove = sourceSlot.quantity;
                    
                    bool success = hotbar.AddItemToSlot(itemToMove, slotIndex, quantityToMove);
                    if (success)
                    {
                        // WICHTIG: Entferne das Item aus dem Inventory (sonst Duplikat!)
                        inventory.RemoveItemAt(inventorySlot.slotIndex);
                        
                        if (inventorySlot.playerInventory != null)
                        {
                            inventorySlot.playerInventory.UpdateUISlots();
                        }
                    }
                }
            }
            return;
        }
        
        // Check 2: Kommt von anderem Hotbar-Slot? (Swap)
        HotbarSlotUI otherHotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlotUI>();
        if (otherHotbarSlot != null && otherHotbarSlot != this)
        {
            Hotbar hotbar = GetComponentInParent<Hotbar>();
            if (hotbar != null)
            {
                hotbar.SwapSlots(otherHotbarSlot.slotIndex, this.slotIndex);
            }
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentlyDraggedSlot == null)
        {
            return;
        }
        
        currentlyDraggedIcon.SetActive(false);
        currentlyDraggedSlot = null;
        canvasGroup.blocksRaycasts = true;
        
        // Update UI
        UpdateUI();
    }
    
    /// <summary>
    /// Setzt ein neues Item in den Slot
    /// </summary>
    public void SetItem(ItemData item, int quantity = 1)
    {
        if (hotbarSlot != null)
        {
            hotbarSlot.SetItem(item, quantity);
            UpdateUI();
        }
    }
    
    public int GetSlotIndex() => slotIndex;
    public HotbarSlot GetHotbarSlot() => hotbarSlot;
}
