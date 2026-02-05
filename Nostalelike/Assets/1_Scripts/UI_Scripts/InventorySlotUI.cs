using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // WICHTIG: Das hat gefehlt!
using TMPro;

// WICHTIG: Jetzt erben wir von den Drag-Interfaces!
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image icon;          
    public TextMeshProUGUI amountText; 
    public Button slotButton;   

    [HideInInspector] public int slotIndex; 
    [HideInInspector] public PlayerInventory playerInventory; 

    private ItemData currentItem;
    private CanvasGroup canvasGroup; // Brauchen wir für Transparenz beim Ziehen
    private bool isMouseOver = false; // Für Drop-Funktion
    
    // Static Variablen für das Drag-Icon (geteilt mit EquipmentSlotUI)
    private static GameObject currentlyDraggedIcon;
    private static Canvas mainCanvas;

    private void Awake()
    {
        // CanvasGroup holen oder erstellen (wichtig damit der Mauszeiger durch das Icon durchklicken kann beim Droppen)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Setup für das Ghost-Icon (genau wie im Equipment Script)
        SetupDragIcon();
    }
    
    private void Update()
    {
        // Q-Taste zum Droppen wenn Maus über Slot ist
        if (isMouseOver && currentItem != null && Input.GetKeyDown(KeyCode.Q))
        {
            if (playerInventory != null)
            {
                playerInventory.DropItem(slotIndex);
            }
        }
    }

    private void SetupDragIcon()
    {
        if (currentlyDraggedIcon == null)
        {
            currentlyDraggedIcon = GameObject.Find("DraggedItemIcon");
            if (currentlyDraggedIcon == null)
            {
                if (mainCanvas == null) mainCanvas = FindFirstObjectByType<Canvas>();
                
                currentlyDraggedIcon = new GameObject("DraggedItemIcon");
                Image img = currentlyDraggedIcon.AddComponent<Image>();
                img.raycastTarget = false; // Ganz wichtig!
                img.preserveAspect = true;
                
                RectTransform rectTransform = currentlyDraggedIcon.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(40, 40); // Größe anpassen
                
                CanvasGroup cg = currentlyDraggedIcon.AddComponent<CanvasGroup>();
                cg.alpha = 0.7f; // Leicht transparent
                
                currentlyDraggedIcon.transform.SetParent(mainCanvas.transform, false);
                currentlyDraggedIcon.SetActive(false);
            }
        }
    }

    public void UpdateSlot(InventorySlot slot)
    {
        if (slot != null && slot.item != null && slot.quantity > 0)
        {
            currentItem = slot.item;
            
            icon.sprite = currentItem.itemIcon;
            icon.enabled = true;
            icon.preserveAspect = true; 

            if (slot.quantity > 1)
            {
                amountText.text = slot.quantity.ToString();
                amountText.enabled = true;
            }
            else
            {
                amountText.enabled = false;
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        icon.sprite = null;
        icon.enabled = false;
        if(amountText != null) amountText.enabled = false;
    }

    public void OnItemClicked()
    {
        if (playerInventory != null)
        {
            playerInventory.UseItem(slotIndex);
        }
    }

    // --- DRAG & DROP LOGIK (NEU!) ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return; // Leere Slots kann man nicht ziehen

        // Icon vorbereiten
        if (currentlyDraggedIcon != null)
        {
            currentlyDraggedIcon.SetActive(true);
            Image dragImage = currentlyDraggedIcon.GetComponent<Image>();
            dragImage.sprite = currentItem.itemIcon;
            
            // Icon an Mausposition setzen
            currentlyDraggedIcon.transform.position = Input.mousePosition;
        }

        // Den originalen Slot unsichtbar/transparent machen
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // WICHTIG: Damit der Raycast durchgeht zum Ziel!
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // Icon bewegt sich mit der Maus
        if (currentlyDraggedIcon != null)
        {
            currentlyDraggedIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Aufräumen
        if (currentlyDraggedIcon != null)
        {
            currentlyDraggedIcon.SetActive(false);
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true; // Wieder klickbar machen
    }

    // Erlaubt das Empfangen von Items (z.B. wenn man Ausrüstung zurück ins Inventar legt)
    public void OnDrop(PointerEventData eventData)
    {
        // === 1. Check: Inventar → Inventar (Slot-Tausch) ===
        InventorySlotUI sourceInventorySlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (sourceInventorySlot != null && sourceInventorySlot != this)
        {
            // Tausche die beiden Slots
            if (playerInventory != null && playerInventory.inventory != null)
            {
                playerInventory.inventory.SwapSlots(sourceInventorySlot.slotIndex, this.slotIndex);
            }
            return;
        }
        
        // === 2. Check: Equipment → Inventar (Unequip) ===
        EquipmentSlotUI sourceEquipmentSlot = eventData.pointerDrag?.GetComponent<EquipmentSlotUI>();
        if (sourceEquipmentSlot != null)
        {
            PlayerEquipment playerEquipment = sourceEquipmentSlot.playerEquipment;
            if (playerEquipment == null) return;
            
            ItemData equippedItem = playerEquipment.GetEquippedItem(sourceEquipmentSlot.slotType);
            if (equippedItem == null) return;
            
            // Prüfe ob dieser Slot leer ist oder ob wir tauschen können
            bool targetSlotEmpty = playerInventory.inventory.IsSlotEmpty(this.slotIndex);
            
            if (targetSlotEmpty)
            {
                // Einfach unequip ins leere Slot
                playerEquipment.UnequipItem(sourceEquipmentSlot.slotType, false); // false = nicht ins Inventar (wir machen das manuell)
                playerInventory.inventory.SetItemAt(this.slotIndex, equippedItem, 1);
            }
            else
            {
                // Zielslot hat bereits ein Item - prüfe ob es equipt werden kann
                ItemData targetItem = playerInventory.inventory.slots[this.slotIndex].item;
                
                if (playerEquipment.CanEquipInSlot(targetItem, sourceEquipmentSlot.slotType))
                {
                    // Tausche: Equip das Item im Inventar, leg das alte ins Inventar
                    playerEquipment.SwapEquipment(targetItem, sourceEquipmentSlot.slotType);
                    playerInventory.inventory.SetItemAt(this.slotIndex, equippedItem, 1);
                }
                else
                {
                    Debug.Log($"{targetItem.itemName} kann nicht in den {sourceEquipmentSlot.slotType} Slot ausgerüstet werden!");
                }
            }
            return;
        }
        
        // === 3. Check: Hotbar → Inventar ===
        HotbarSlotUI sourceHotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlotUI>();
        if (sourceHotbarSlot != null)
        {
            HotbarSlot hotbarSlot = sourceHotbarSlot.GetHotbarSlot();
            if (hotbarSlot == null || hotbarSlot.IsEmpty()) return;
            
            ItemData hotbarItem = hotbarSlot.item;
            int hotbarQuantity = hotbarSlot.quantity;
            
            bool targetSlotEmpty = playerInventory.inventory.IsSlotEmpty(this.slotIndex);
            
            if (targetSlotEmpty)
            {
                // Einfach ins leere Slot verschieben
                playerInventory.inventory.SetItemAt(this.slotIndex, hotbarItem, hotbarQuantity);
                hotbarSlot.ClearSlot();
                sourceHotbarSlot.UpdateUI();
            }
            else
            {
                // Tausche mit dem Item im Inventar
                ItemData targetItem = playerInventory.inventory.slots[this.slotIndex].item;
                int targetQuantity = playerInventory.inventory.slots[this.slotIndex].quantity;
                
                // Inventar bekommt Hotbar-Item
                playerInventory.inventory.SetItemAt(this.slotIndex, hotbarItem, hotbarQuantity);
                
                // Hotbar bekommt Inventar-Item
                hotbarSlot.SetItem(targetItem, targetQuantity);
                sourceHotbarSlot.UpdateUI();
            }
            return;
        }
    }
    
    // --- POINTER ENTER/EXIT für Drop-Funktion ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }
}