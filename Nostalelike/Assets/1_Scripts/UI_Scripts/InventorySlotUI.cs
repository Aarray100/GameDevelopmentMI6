using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image itemIcon;
    public Image slotImage;
    public TextMeshProUGUI itemCountText;

    // Drag-and-Drop
    public PlayerInventory playerInventory;
    public int slotIndex;
    private CanvasGroup canvasGroup;

    private static InventorySlotUI currentlyDraggedSlot;
    private static GameObject currentlyDraggedIcon;
    private static Canvas mainCanvas;


    void Awake()
    {
        // CanvasGroup prüfen und ggf. hinzufügen
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.Log("CanvasGroup wurde automatisch zu " + gameObject.name + " hinzugefügt.");
        }
        
        if (currentlyDraggedIcon == null)
        {
            // Versuche zuerst das GameObject zu finden
            currentlyDraggedIcon = GameObject.Find("DraggedItemIcon");
            
            // Falls nicht gefunden, erstelle es dynamisch
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
    
    void CreateDraggedItemIcon()
    {
        // Finde das Canvas
        if (mainCanvas == null)
        {
            mainCanvas = GetComponentInParent<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("Kein Canvas gefunden! Drag-Icon kann nicht erstellt werden.");
                return;
            }
        }
        
        // Erstelle das Drag-Icon GameObject
        currentlyDraggedIcon = new GameObject("DraggedItemIcon");
        currentlyDraggedIcon.transform.SetParent(mainCanvas.transform, false);
        
        // Füge Image-Komponente hinzu
        Image dragImage = currentlyDraggedIcon.AddComponent<Image>();
        dragImage.raycastTarget = false; // Wichtig! Damit es keine Raycasts blockiert
        dragImage.preserveAspect = true; // Behalte das Seitenverhältnis
        
        // Setze FESTE Größe für Drag-Icon (unabhängig vom Slot)
        RectTransform rectTransform = currentlyDraggedIcon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(10, 10); // Feste Größe für alle Drag-Operations
        
        // Mache es etwas transparent
        Color color = dragImage.color;
        color.a = 0.8f;
        dragImage.color = color;
        
        currentlyDraggedIcon.SetActive(false);
    }

    // Aktualisiert den Slot, um einen Gegenstand anzuzeigen
    public void UpdateSlot(InventorySlot slotData)
    {
        // Null-Checks für UI-Komponenten
        if (itemIcon == null || itemCountText == null)
        {
            Debug.LogWarning("UI components are null in InventorySlotUI - slot might be destroyed");
            return;
        }
        
        if (slotData != null && slotData.item != null)
        {
            itemIcon.sprite = slotData.item.itemIcon;
            itemIcon.enabled = true; // Icon anzeigen

            // Zeige die Anzahl nur an, wenn stapelbar und mehr als 1
            if (slotData.item.isStackable && slotData.quantity > 1)
            {
                itemCountText.text = slotData.quantity.ToString();
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


    // Leert den Slot
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Nur ziehen erlauben, wenn ein Item im Slot vorhanden ist
        if (playerInventory == null || playerInventory.inventory.slots[slotIndex].item == null)
        {
            return;
        }
        
        if (currentlyDraggedIcon == null)
        {
            Debug.LogError("DraggedItemIcon ist null! Kann nicht gedragged werden.");
            return;
        }
        
        currentlyDraggedSlot = this;
        currentlyDraggedIcon.SetActive(true);
        
        // Setze das Sprite
        Image dragImage = currentlyDraggedIcon.GetComponent<Image>();
        if (dragImage != null)
        {
            dragImage.sprite = itemIcon.sprite;
            dragImage.enabled = true;
        }
        
        // Setze die Position direkt auf die Mausposition
        currentlyDraggedIcon.transform.position = Input.mousePosition;

        canvasGroup.blocksRaycasts = false;
        itemIcon.enabled = false;
        itemCountText.enabled = false;
        
        Debug.Log("Drag gestartet für Item: " + playerInventory.inventory.slots[slotIndex].item.itemName);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentlyDraggedSlot == null)
        {
            return;
        }
        currentlyDraggedIcon.transform.position = Input.mousePosition;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Check 1: Kommt das Item von einem Equipment-Slot?
        EquipmentSlotUI equipmentSlot = eventData.pointerDrag?.GetComponent<EquipmentSlotUI>();
        
        if (equipmentSlot != null)
        {
            // Item von Equipment zu Inventar - unequip es und lege ins target slot
            PlayerEquipment playerEquipment = FindFirstObjectByType<PlayerEquipment>();
            if (playerEquipment != null)
            {
                ItemData equippedItem = playerEquipment.GetEquippedItem(equipmentSlot.slotType);
                if (equippedItem != null)
                {
                    // Unequip das Item (OHNE es ins Inventar zu legen - wir machen das manuell)
                    playerEquipment.UnequipItem(equipmentSlot.slotType, addToInventory: false);
                    
                    // Lege es in diesen spezifischen Inventory-Slot
                    Inventory inventory = playerInventory.inventory;
                    InventorySlot targetSlot = inventory.slots[this.slotIndex];
                    
                    if (targetSlot.item != null)
                    {
                        // Target-Slot ist belegt - swap
                        ItemData currentItem = targetSlot.item;
                        
                        // Entferne aktuelles Item aus diesem Slot und füge equipped item hinzu
                        inventory.RemoveItemAt(this.slotIndex);
                        inventory.AddItemAt(equippedItem, this.slotIndex);
                        
                        // Füge das verdrängte Item wieder hinzu (geht in ersten freien Slot)
                        inventory.AddItem(currentItem, 1);
                    }
                    else
                    {
                        // Target-Slot ist leer - einfach hinlegen
                        inventory.AddItemAt(equippedItem, this.slotIndex);
                    }
                    
                    playerInventory.UpdateUISlots();
                    Debug.Log($"Unequipped {equippedItem.itemName} to slot {this.slotIndex}");
                }
            }
            return;
        }
        
        // Check 2: Kommt das Item von der Hotbar?
        HotbarSlotUI hotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlotUI>();
        
        if (hotbarSlot != null)
        {
            HotbarSlot sourceSlot = hotbarSlot.GetHotbarSlot();
            
            if (sourceSlot != null && sourceSlot.item != null)
            {
                // Hole das Item von der Hotbar
                ItemData hotbarItem = sourceSlot.item;
                int hotbarQuantity = sourceSlot.quantity;
                
                // Füge es ins Inventar hinzu
                Inventory inventory = playerInventory.inventory;
                InventorySlot targetSlot = inventory.slots[this.slotIndex];
                
                if (targetSlot.item != null)
                {
                    // Target-Slot ist belegt - füge in ersten freien Slot
                    inventory.AddItem(hotbarItem, hotbarQuantity);
                    
                    // Entferne von Hotbar
                    Hotbar hotbar = hotbarSlot.GetComponentInParent<Hotbar>();
                    if (hotbar != null)
                    {
                        hotbar.RemoveItemFromSlot(hotbarSlot.GetSlotIndex());
                    }
                    Debug.Log($"Item {hotbarItem.itemName} von Hotbar zu Inventar verschoben");
                }
                else
                {
                    // Target-Slot ist leer - direkt hinlegen
                    inventory.AddItemAt(hotbarItem, this.slotIndex);
                    targetSlot.quantity = hotbarQuantity;
                    
                    // Entferne von Hotbar
                    Hotbar hotbar = hotbarSlot.GetComponentInParent<Hotbar>();
                    if (hotbar != null)
                    {
                        hotbar.RemoveItemFromSlot(hotbarSlot.GetSlotIndex());
                    }
                    Debug.Log($"Item {hotbarItem.itemName} von Hotbar zu Inventar Slot {this.slotIndex} verschoben");
                }
                
                playerInventory.UpdateUISlots();
            }
            return;
        }
        
        // Check 3: Normaler Swap zwischen Inventory-Slots
        if (currentlyDraggedSlot == null || currentlyDraggedSlot == this)
        {
            return;
        }
        int sourceIndex = currentlyDraggedSlot.slotIndex;
        int targetIndex = this.slotIndex;

        playerInventory.SwapItems(sourceIndex, targetIndex);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentlyDraggedSlot == null)
        {
            return;
        }
        
        // Check: Wurde auf Equipment-Slot gedroppt?
        EquipmentSlotUI equipmentSlot = eventData.pointerCurrentRaycast.gameObject?.GetComponent<EquipmentSlotUI>();
        
        if (equipmentSlot != null)
        {
            // Item wird zu Equipment gezogen, EquipmentSlotUI.OnDrop() handelt das
            Debug.Log("Dropped on equipment slot");
        }
        
        currentlyDraggedIcon.SetActive(false);
        currentlyDraggedSlot = null;
        canvasGroup.blocksRaycasts = true;
        playerInventory.UpdateUISlots();
    }
    
    public void OnPointerClick(PointerEventData eventData)
{
    // Wir prüfen auf Linksklick (PointerId -1 ist meistens links)
    if (eventData.button == PointerEventData.InputButton.Left)
    {
        // Sicherstellen, dass ein Item im Slot liegt
        InventorySlot slot = playerInventory.inventory.slots[slotIndex];

        if (slot != null && slot.item != null)
        {
            // PRÜFUNG: Ist das Item ein Buch?
            if (slot.item is BookData book)
            {
                Debug.Log("Buch wird geöffnet: " + book.bookTitle);
                BookUIManager.Instance.OpenBook(book);
            }
            else
            {
                Debug.Log("Dies ist ein normales Item: " + slot.item.itemName);
            }
        }
    }
}
    
}
