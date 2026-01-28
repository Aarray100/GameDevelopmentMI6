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
                // WICHTIG: Ruft UsePotion mit dem Item als Parameter auf
                // Dies behebt den Fehler CS1061, wenn PlayerStats die Methode bereitstellt
                stats.UsePotion(currentSlot.item); 
                
                // Item nach Benutzung um 1 verringern
                playerInventory.inventory.RemoveItem(currentSlot.item, 1);
                playerInventory.UpdateUISlots();
                
                Debug.Log($"<color=green>UI Log:</color> Benutze {currentSlot.item.itemName}");
            }
            else
            {
                Debug.LogError("PlayerStats-Skript wurde auf dem Spieler-Objekt nicht gefunden!");
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
        
        if (mainCanvas == null)
        {
            mainCanvas = GetComponentInParent<Canvas>();
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
                // WICHTIG: Auch ein gefundenes Icon muss korrekt konfiguriert werden!
                ConfigureDraggedIcon(currentlyDraggedIcon);
                currentlyDraggedIcon.SetActive(false);
            }
        }
    }
    
    void ConfigureDraggedIcon(GameObject icon)
    {
        // Scale explizit auf (1,1,1) setzen
        icon.transform.localScale = Vector3.one;
        
        RectTransform rectTransform = icon.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // WICHTIG: Größe vom itemIcon übernehmen (wie im Original)
            if (itemIcon != null)
            {
                RectTransform itemIconRect = itemIcon.GetComponent<RectTransform>();
                rectTransform.sizeDelta = itemIconRect.sizeDelta;
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(40, 40); // Fallback
            }
            
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        Image dragImage = icon.GetComponent<Image>();
        if (dragImage != null)
        {
            dragImage.raycastTarget = false;
            dragImage.preserveAspect = true;
            Color color = dragImage.color;
            color.a = 0.8f;
            dragImage.color = color;
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
        
        // Image-Komponente hinzufügen
        currentlyDraggedIcon.AddComponent<Image>();
        
        // Konfiguration über gemeinsame Methode
        ConfigureDraggedIcon(currentlyDraggedIcon);
        
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
        
        // Sicherstellen, dass das Icon korrekt konfiguriert ist
        ConfigureDraggedIcon(currentlyDraggedIcon);
        
        // Debug: Zeige die Größe an
        RectTransform dragRect = currentlyDraggedIcon.GetComponent<RectTransform>();
        Debug.Log($"<color=yellow>Drag Icon Size:</color> sizeDelta={dragRect.sizeDelta}, localScale={currentlyDraggedIcon.transform.localScale}");
        
        currentlyDraggedIcon.SetActive(true);
        
        Image dragImage = currentlyDraggedIcon.GetComponent<Image>();
        if (dragImage != null)
        {
            dragImage.sprite = itemIcon.sprite;
            dragImage.enabled = true;
        }
        
        // Position setzen mit Canvas-Konvertierung
        SetDragIconPosition(eventData);
        canvasGroup.blocksRaycasts = false;
        itemIcon.enabled = false;
        itemCountText.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentlyDraggedSlot == null) return;
        SetDragIconPosition(eventData);
    }
    
    private void SetDragIconPosition(PointerEventData eventData)
    {
        if (currentlyDraggedIcon == null || mainCanvas == null) return;
        
        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        
        // Korrekte Positionsberechnung für alle Canvas-Modi
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            eventData.position, 
            mainCanvas.worldCamera, 
            out localPoint))
        {
            currentlyDraggedIcon.GetComponent<RectTransform>().anchoredPosition = localPoint;
        }
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