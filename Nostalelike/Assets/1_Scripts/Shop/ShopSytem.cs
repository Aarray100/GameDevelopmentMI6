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

    [Header("Audio Clips")]
    public AudioClip clickSound;
    public AudioClip buySound;
    public AudioClip errorSound;
    public AudioClip openShopSound;

    [Header("Verbrauchs-Items")]
    public ItemData healPotion;
    public ItemData strengthPotion;
    public ItemData speedPotion;
    public ItemData omniPotion;

    [Header("Ausrüstung (Iron Set)")]
    // Wir haben die Namen angepasst, damit sie Sinn ergeben!
    public ItemData ironSword;      // Vorher schwert
    public ItemData ironHelm;       // Vorher helm1
    public ItemData ironChestplate; // Vorher helm2
    public ItemData ironAmulet;     // Vorher kette
    public ItemData ironRing;       // Neu dazu (falls du den Ring auch verkaufen willst)

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
    }

    private void FindeAktivenSpieler()
    {
        playerInventory = GameObject.FindFirstObjectByType<PlayerInventory>();
    }

    // --- BUTTON FUNKTIONEN ---
    // Diese Funktionen musst du im Button "On Click" neu zuweisen!

    public void WaehleHeiltrank() { SetzeVorschau(healPotion, 20); }
    public void WaehleStaerke() { SetzeVorschau(strengthPotion, 20); }
    public void WaehleSpeed() { SetzeVorschau(speedPotion, 20); }
    public void WaehleOmni() { SetzeVorschau(omniPotion, 50); }

    public void WaehleIronSword() { SetzeVorschau(ironSword, 50); }
    public void WaehleIronHelm() { SetzeVorschau(ironHelm, 100); }
    public void WaehleIronChestplate() { SetzeVorschau(ironChestplate, 100); } // Teurer, da Rüstung
    public void WaehleIronAmulet() { SetzeVorschau(ironAmulet, 60); }
    public void WaehleIronRing() { SetzeVorschau(ironRing, 60); }

    // -------------------------

    private void SetzeVorschau(ItemData data, int preis)
    {
        if (data == null) return;
        ausgewähltesItem = data;
        aktuellerPreis = preis;
        UpdateAnzeige(); 
        
        if (AudioManager.Instance != null && clickSound != null) 
            AudioManager.Instance.PlaySFX(clickSound); 
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
        if (ausgewähltesItem == null || GoldManager.Instance == null || playerInventory == null) return;
        
        if (GoldManager.Instance.GoldAbziehen(aktuellerPreis))
        {
            if (playerInventory.inventory != null)
            {
                playerInventory.inventory.AddItem(ausgewähltesItem, 1);
                
                if (AudioManager.Instance != null && buySound != null) 
                    AudioManager.Instance.PlaySFX(buySound); 
            }
            UpdateAnzeige(); 
        }
        else
        {
            if (AudioManager.Instance != null && errorSound != null) 
                AudioManager.Instance.PlaySFX(errorSound);
        }
    }

    public void ToggleShop() 
    {
        if (shopPanel != null)
        {
            bool istAktiv = !shopPanel.activeSelf;
            shopPanel.SetActive(istAktiv);
            
            if (istAktiv)
            {
                UpdateAnzeige();
                if (AudioManager.Instance != null && openShopSound != null) 
                    AudioManager.Instance.PlaySFX(openShopSound);
            }
        }
    }

    public void OpenShop() 
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            UpdateAnzeige();
        }
    }

    public void CloseShop() 
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }
}