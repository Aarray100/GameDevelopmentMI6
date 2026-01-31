using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopSystem : MonoBehaviour
{
    [Header("UI Referenzen")]
    public PlayerInventory playerInventory; 
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI vorschauText;
    public GameObject shopPanel;

    [Header("Item Daten (Assets hier reinziehen)")]
    public ItemData healPotion;
    public ItemData strengthPotion;
    public ItemData speedPotion;
    public ItemData omniPotion;
    public ItemData helm1, helm2, schwert, kette;

    private ItemData ausgewähltesItem;
    private int aktuellerPreis;

    void Start()
    {
        FindeAktivenSpieler();
        if (shopPanel != null) shopPanel.SetActive(false);
        if (vorschauText != null) vorschauText.text = "Wähle ein Item...";
    }

    void Update()
    {
        if (goldText != null && GoldManager.Instance != null) 
        {
            goldText.text = "Gold: " + GoldManager.Instance.aktuellesGold;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleShop();
        }
    }

    private void FindeAktivenSpieler()
    {
        playerInventory = GameObject.FindFirstObjectByType<PlayerInventory>();
    }

    public void WaehleHeiltrank() { SetzeVorschau(healPotion, 20); }
    public void WaehleStaerke() { SetzeVorschau(strengthPotion, 20); }
    public void WaehleSpeed() { SetzeVorschau(speedPotion, 20); }
    public void WaehleOmni() { SetzeVorschau(omniPotion, 50); }
    public void WaehleSchwert() { SetzeVorschau(schwert, 50); }
    public void WaehleHelm1() { SetzeVorschau(helm1, 100); }
    public void WaehleHelm2() { SetzeVorschau(helm2, 100); }
    public void WaehleKette() { SetzeVorschau(kette, 80); }

    private void SetzeVorschau(ItemData data, int preis)
    {
        if (data == null) return;
        ausgewähltesItem = data;
        aktuellerPreis = preis;
        UpdateAnzeige(); 
        
        // Optional: Kleiner Klick-Sound beim Auswählen
        if (AudioManager.Instance != null) AudioManager.Instance.PlayHoverSFX();
    }

    public void UpdateAnzeige()
    {
        if (vorschauText == null || ausgewähltesItem == null) return;
        if (playerInventory == null) FindeAktivenSpieler();

        int besitz = 0;
        if (playerInventory != null && playerInventory.inventory != null)
        {
            foreach (var slot in playerInventory.inventory.slots)
            {
                if (slot.item != null && slot.item.itemName == ausgewähltesItem.itemName)
                {
                    besitz += slot.quantity;
                }
            }
        }

        vorschauText.text = "<b>" + ausgewähltesItem.itemName + "</b>\n" +
                          "Preis: " + aktuellerPreis + " Gold\n" +
                          "Im Besitz: " + besitz;
    }

    public void KaufBestaetigen()
    {
        if (ausgewähltesItem == null) return;
        if (playerInventory == null) FindeAktivenSpieler();

        // Erst prüfen ob Inventar da ist
        if (playerInventory == null)
        {
            Debug.LogError("Kauf abgebrochen: Kein PlayerInventory in der Szene gefunden!");
            return;
        }

        // Gold prüfen und abziehen
        if (GoldManager.Instance.GoldAbziehen(aktuellerPreis))
        {
            playerInventory.inventory.AddItem(ausgewähltesItem, 1);
            UpdateAnzeige(); 
            
            // Erfolg: Nutzt jetzt den zentralen AudioManager
            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlayItemSoldSFX();

            Debug.Log("Kauf erfolgreich! " + ausgewähltesItem.itemName + " wurde hinzugefügt.");
        }
        else
        {
            // Fehler: Nutzt jetzt den zentralen AudioManager
            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlayInsufficientGoldSFX();

            Debug.LogWarning("Kauf fehlgeschlagen: Zu wenig Gold!");
        }
    }

    public void ToggleShop() 
    {
        if (shopPanel != null)
        {
            bool status = !shopPanel.activeSelf;
            shopPanel.SetActive(status);
            
            if (status) 
            {
                FindeAktivenSpieler();
                UpdateAnzeige();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayShopOpenSFX();
            }
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            FindeAktivenSpieler();
            UpdateAnzeige();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayShopOpenSFX();
        }
    }
}