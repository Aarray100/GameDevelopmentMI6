using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
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
        
        // Setze die Größe - passe diese an deine Item-Icon-Größe an
        RectTransform rectTransform = currentlyDraggedIcon.GetComponent<RectTransform>();
        
        // Hole die Größe vom aktuellen itemIcon als Referenz
        if (itemIcon != null)
        {
            RectTransform itemIconRect = itemIcon.GetComponent<RectTransform>();
            rectTransform.sizeDelta = itemIconRect.sizeDelta; // Gleiche Größe wie die Slot-Icons
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(64, 64); // Fallback-Größe
        }
        
        // Mache es etwas transparent
        Color color = dragImage.color;
        color.a = 0.8f;
        dragImage.color = color;
        
        currentlyDraggedIcon.SetActive(false);
        
        Debug.Log("DraggedItemIcon wurde dynamisch erstellt mit Größe: " + rectTransform.sizeDelta);
    }

    // Aktualisiert den Slot, um einen Gegenstand anzuzeigen
    public void UpdateSlot(InventorySlot slotData)
    {
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
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        itemCountText.text = "";
        itemCountText.enabled = false;
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
        // Check: Kommt das Item von einem Equipment-Slot?
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
        
        // Normaler Swap zwischen Inventory-Slots
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
    
    
}