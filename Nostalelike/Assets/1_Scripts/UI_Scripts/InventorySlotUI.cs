using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image itemIcon;
    public Image slotImage;
    public TextMeshProUGUI itemCountText;

    [Header("Inventory References")]
    public PlayerInventory playerInventory;
    public int slotIndex;
    private bool isHovering = false;
    private CanvasGroup canvasGroup;

    private static InventorySlotUI currentlyDraggedSlot;
    private static GameObject currentlyDraggedIcon;
    private static Canvas mainCanvas;

    // --- GEGENSTAND BENUTZEN (TRÄNKE UND BÜCHER) ---
    public void OnPointerClick(PointerEventData eventData)
    {
        // Nur Linksklick erlauben und Dragging-Check
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (currentlyDraggedSlot != null) return;

        // Slot-Daten validieren
        InventorySlot currentSlot = playerInventory.inventory.slots[slotIndex];
        if (currentSlot == null || currentSlot.item == null) return;

        // 1. Logik für Bücher
        if (currentSlot.item is BookData book)
        {
            BookUIManager.Instance.OpenBook(book);
            return;
        }

        // 2. Logik für Verbrauchsgüter (Tränke)
        if (currentSlot.item.itemType == ItemType.Consumable)
        {
            PlayerStats stats = playerInventory.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // RUFT DIE FUNKTION IN PLAYERSTATS AUF (Muss ItemData akzeptieren!)
                stats.UsePotion(currentSlot.item); 
                
                // Item nach Benutzung entfernen
                playerInventory.inventory.RemoveItem(currentSlot.item, 1);
                playerInventory.UpdateUISlots();
                
                Debug.Log($"<color=blue>Inventory:</color> Trank benutzt: {currentSlot.item.itemName}");
            }
            else
            {
                Debug.LogWarning("PlayerStats Komponente auf dem Spieler nicht gefunden!");
            }
        }
    }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
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
    
    void CreateDraggedItemIcon()
    {
        if (mainCanvas == null)
        {
            mainCanvas = GetComponentInParent<Canvas>();
            if (mainCanvas == null) return;
        }
        
        currentlyDraggedIcon = new GameObject("DraggedItemIcon");
        currentlyDraggedIcon.transform.SetParent(mainCanvas.transform, false);
        
        Image dragImage = currentlyDraggedIcon.AddComponent<Image>();
        dragImage.raycastTarget = false; 
        dragImage.preserveAspect = true; 
        
        RectTransform rectTransform = currentlyDraggedIcon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(40, 40);
        
        Color color = dragImage.color;
        color.a = 0.8f;
        dragImage.color = color;
        
        currentlyDraggedIcon.SetActive(false);
    }

    public void UpdateSlot(InventorySlot slotData)
    {
        if (itemIcon == null || itemCountText == null) return;
        
        if (slotData != null && slotData.item != null)
        {
            itemIcon.sprite = slotData.item.itemIcon;
            itemIcon.enabled = true;

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
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
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

    // --- DRAG AND DROP LOGIK ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (playerInventory == null || playerInventory.inventory.slots[slotIndex].item == null) return;
        
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
        if (currentlyDraggedSlot == null) return;
        currentlyDraggedIcon.transform.position = Input.mousePosition;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (currentlyDraggedSlot == null || currentlyDraggedSlot == this) return;
        
        playerInventory.SwapItems(currentlyDraggedSlot.slotIndex, this.slotIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentlyDraggedSlot == null) return;
        currentlyDraggedIcon.SetActive(false);
        currentlyDraggedSlot = null;
        canvasGroup.blocksRaycasts = true;
        playerInventory.UpdateUISlots();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    void Update()
    {
        if (isHovering && Input.GetKeyDown(KeyCode.E))
        {
            InventorySlot slot = playerInventory.inventory.slots[slotIndex];
            if (slot != null && slot.item != null && slot.item is BookData book)
            {
                BookUIManager.Instance.OpenBook(book);
            }
        }
    }
}