using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // WICHTIG: Das hat gefehlt!
using TMPro;

// WICHTIG: Jetzt erben wir von den Drag-Interfaces!
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("UI Components")]
    public Image icon;          
    public TextMeshProUGUI amountText; 
    public Button slotButton;   

    [HideInInspector] public int slotIndex; 
    [HideInInspector] public PlayerInventory playerInventory; 

    private ItemData currentItem;
    private CanvasGroup canvasGroup; // Brauchen wir für Transparenz beim Ziehen
    
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
        
        // --- DROP DETECTION ---
        // Prüfe ob das Item außerhalb eines gültigen UI-Elements losgelassen wurde
        if (currentItem != null && eventData.pointerCurrentRaycast.gameObject == null)
        {
            // Item wurde ins "Nichts" gezogen -> Droppen!
            if (playerInventory != null)
            {
                playerInventory.DropItem(slotIndex);
                Debug.Log($"Item {currentItem.itemName} aus Slot {slotIndex} gedroppt!");
            }
        }
        // ----------------------
    }

    // Erlaubt das Empfangen von Items (z.B. wenn man Ausrüstung zurück ins Inventar legt)
    public void OnDrop(PointerEventData eventData)
    {
        // Hier könnte man Logik einbauen, um Items innerhalb des Inventars zu tauschen.
        // Fürs Erste reicht es, wenn der EquipmentSlotUI das handled.
        // Wenn du Equipment ZURÜCK ins Inventar ziehst, kümmert sich EquipmentSlotUI darum.
    }

    // --- RECHTSKLICK ZUM DROPPEN ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Rechtsklick -> Item droppen
            if (currentItem != null && playerInventory != null)
            {
                // Bei gedrückter Shift-Taste nur 1 Item droppen, sonst alle
                int dropAmount = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) 
                    ? 1 
                    : -1; // -1 bedeutet alle
                
                playerInventory.DropItem(slotIndex, dropAmount);
                Debug.Log($"Item per Rechtsklick gedroppt!");
            }
        }
    }
    // -------------------------------
}