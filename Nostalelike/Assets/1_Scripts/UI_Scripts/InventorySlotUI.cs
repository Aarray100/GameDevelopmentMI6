using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    // Diese im Inspector des Prefabs zuweisen
    public Image itemIcon;
    public TextMeshProUGUI itemCountText;


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
        //itemIcon.sprite = null;
        //itemIcon.enabled = false;
        itemCountText.text = "";
        itemCountText.enabled = false;
    }
}