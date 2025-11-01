using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Slot Info")]
    public EquipmentSlot slotType;  // Im Inspector setzen: Head, Chest, etc.
    
    [Header("UI References")]
    public Image placeholderIcon;     // Das Placeholder-Icon (Head_Icon, Ring_Icon, etc.) - immer sichtbar
    public Image equippedItemIcon;    // Das equipped Item-Icon - nur wenn equipped
    public Image slotBackgroundImage;
    
    [Header("References")]
    public PlayerEquipment playerEquipment;
    
    private CanvasGroup canvasGroup;
    private static GameObject currentlyDraggedIcon;
    private static Canvas mainCanvas;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // DraggedIcon Setup (shared mit Inventar) - suche oder erstelle
        if (currentlyDraggedIcon == null)
        {
            currentlyDraggedIcon = GameObject.Find("DraggedItemIcon");
            
            // Falls nicht gefunden, erstelle es
            if (currentlyDraggedIcon == null)
            {
                if (mainCanvas == null)
                {
                    mainCanvas = FindFirstObjectByType<Canvas>();
                }
                
                currentlyDraggedIcon = new GameObject("DraggedItemIcon");
                Image img = currentlyDraggedIcon.AddComponent<Image>();
                img.raycastTarget = false;
                currentlyDraggedIcon.transform.SetParent(mainCanvas.transform);
                currentlyDraggedIcon.SetActive(false);
            }
        }
    }
    
    private bool isInitialized = false;
    
    private void Start()
    {
        Initialize();
    }
    
    private void OnEnable()
    {
        // Initialize wenn noch nicht geschehen (falls Panel deaktiviert startet)
        if (!isInitialized)
        {
            Initialize();
        }
        
        // Update visual wenn Panel aktiviert wird
        UpdateSlotVisual();
    }
    
    private void Initialize()
    {
        if (isInitialized) return;
        
        // Auto-finde PlayerEquipment falls nicht gesetzt
        if (playerEquipment == null)
        {
            playerEquipment = FindFirstObjectByType<PlayerEquipment>();
            
            if (playerEquipment == null)
            {
                Debug.LogError($"PlayerEquipment not found for {slotType} slot!");
                return;
            }
        }
        
        // Subscribe zu Equipment-Änderungen
        if (playerEquipment != null)
        {
            playerEquipment.OnEquipmentChanged -= UpdateSlotVisual; // Avoid duplicates
            playerEquipment.OnEquipmentChanged += UpdateSlotVisual;
        }
        
        isInitialized = true;
    }
    
    private void OnDestroy()
    {
        if (playerEquipment != null)
        {
            playerEquipment.OnEquipmentChanged -= UpdateSlotVisual;
        }
    }
    
    // Update die visuelle Darstellung des Slots
    public void UpdateSlotVisual()
    {
        if (playerEquipment == null) return;
        
        ItemData equippedItem = playerEquipment.GetEquippedItem(slotType);
        
        // Wenn beide Images das gleiche sind (einfache Variante)
        if (placeholderIcon != null && equippedItemIcon == placeholderIcon)
        {
            if (equippedItem != null)
            {
                placeholderIcon.sprite = equippedItem.itemIcon;
            }
            // Placeholder bleibt immer enabled - zeigt entweder Placeholder oder Item
            placeholderIcon.enabled = true;
        }
        else
        {
            // Separate Images (erweiterte Variante)
            if (equippedItem != null)
            {
                // Item ist equipped - zeige Item-Icon, verstecke Placeholder
                if (equippedItemIcon != null)
                {
                    equippedItemIcon.sprite = equippedItem.itemIcon;
                    equippedItemIcon.enabled = true;
                }
                if (placeholderIcon != null)
                {
                    placeholderIcon.enabled = false;
                }
            }
            else
            {
                // Slot ist leer - zeige Placeholder, verstecke Item-Icon
                if (equippedItemIcon != null)
                {
                    equippedItemIcon.sprite = null;
                    equippedItemIcon.enabled = false;
                }
                if (placeholderIcon != null)
                {
                    placeholderIcon.enabled = true;
                }
            }
        }
    }
    
    // DROP: Item wird auf diesen Slot gezogen
    public void OnDrop(PointerEventData eventData)
    {
        // Hole das gedraggede Item (von Inventar oder anderem Equipment-Slot)
        ItemData draggedItem = GetDraggedItem(eventData);
        
        if (draggedItem == null)
        {
            Debug.Log("No item being dragged");
            return;
        }
        
        // Prüfe ob Item in diesen Slot passt
        if (!playerEquipment.CanEquipInSlot(draggedItem, slotType))
        {
            Debug.Log($"{draggedItem.itemName} cannot be equipped in {slotType} slot!");
            
            // Visual Feedback (optional: rotes Flash, Sound, etc.)
            return;
        }
        
        // Check: Kommt das Item aus dem Inventar?
        InventorySlotUI inventorySlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        
        if (inventorySlot != null)
        {
            // Item kommt aus Inventar → Equipt es (automatischer Swap falls bereits equipped)
            playerEquipment.SwapEquipment(draggedItem, slotType);
        }
        else
        {
            // Equipment-zu-Equipment Drag macht keinen Sinn - ignoriere es
            Debug.Log("Cannot drag equipment items between equipment slots!");
        }
    }
    
    // BEGIN DRAG: Equipment-Item wird gedraggt (zurück zu Inventar oder anderer Slot)
    public void OnBeginDrag(PointerEventData eventData)
    {
        ItemData equippedItem = playerEquipment.GetEquippedItem(slotType);
        
        if (equippedItem == null)
        {
            // Nichts equipped, kann nicht draggen
            return;
        }
        
        // Setup Drag-Icon
        if (currentlyDraggedIcon != null)
        {
            currentlyDraggedIcon.SetActive(true);
            Image dragImage = currentlyDraggedIcon.GetComponent<Image>();
            if (dragImage != null)
            {
                dragImage.sprite = equippedItem.itemIcon;
                dragImage.enabled = true;
            }
            currentlyDraggedIcon.transform.position = Input.mousePosition;
        }
        
        // Mache Slot transparent während Drag
        canvasGroup.blocksRaycasts = false;
        if (equippedItemIcon != null)
        {
            equippedItemIcon.enabled = false;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (currentlyDraggedIcon != null && currentlyDraggedIcon.activeSelf)
        {
            currentlyDraggedIcon.transform.position = Input.mousePosition;
        }
    }
    
    // END DRAG: Drop auf Inventar oder anderen Slot
    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentlyDraggedIcon != null)
        {
            currentlyDraggedIcon.SetActive(false);
        }
        
        canvasGroup.blocksRaycasts = true;
        
        // Check: Wurde auf Inventar-Slot gedroppt?
        InventorySlotUI inventorySlot = eventData.pointerCurrentRaycast.gameObject?.GetComponent<InventorySlotUI>();
        
        if (inventorySlot != null)
        {
            // Item wird ins Inventar gedroppt - OnDrop von InventorySlotUI handelt das
            // Wir machen hier nichts, damit nicht doppelt unequipt wird
        }
        else
        {
            // Nicht auf gültigem Target gedroppt, zurück zur Original-Position
            UpdateSlotVisual();
        }
    }
    
    // Helper: Hole gedraggedes Item
    private ItemData GetDraggedItem(PointerEventData eventData)
    {
        // Von Inventar
        InventorySlotUI inventorySlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (inventorySlot != null && inventorySlot.playerInventory != null)
        {
            int slotIndex = inventorySlot.slotIndex;
            if (slotIndex >= 0 && slotIndex < inventorySlot.playerInventory.inventory.slots.Count)
            {
                return inventorySlot.playerInventory.inventory.slots[slotIndex].item;
            }
        }
        
        // Von anderem Equipment-Slot
        EquipmentSlotUI equipmentSlot = eventData.pointerDrag?.GetComponent<EquipmentSlotUI>();
        if (equipmentSlot != null && equipmentSlot.playerEquipment != null)
        {
            return equipmentSlot.playerEquipment.GetEquippedItem(equipmentSlot.slotType);
        }
        
        return null;
    }
}
