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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip kaufSound;   // Das "Katsching"
    public AudioClip fehlerSound; // Der tiefe Ton bei zu wenig Gold

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
    public void WaehleStaerke() { SetzeVorschau(strengthPotion, 30); }
    public void WaehleSpeed() { SetzeVorschau(speedPotion, 25); }
    public void WaehleOmni() { SetzeVorschau(omniPotion, 50); }
    public void WaehleSchwert() { SetzeVorschau(schwert, 200); }
    public void WaehleHelm1() { SetzeVorschau(helm1, 100); }
    public void WaehleHelm2() { SetzeVorschau(helm2, 150); }
    public void WaehleKette() { SetzeVorschau(kette, 80); }

    private void SetzeVorschau(ItemData data, int preis)
    {
        if (data == null) return;
        ausgewähltesItem = data;
        aktuellerPreis = preis;
        UpdateAnzeige(); 
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

        if (playerInventory != null && GoldManager.Instance.GoldAbziehen(aktuellerPreis))
        {
            playerInventory.inventory.AddItem(ausgewähltesItem, 1);
            UpdateAnzeige(); 
            
            // Erfolg: Katsching!
            if (audioSource != null && kaufSound != null) 
                audioSource.PlayOneShot(kaufSound);

            Debug.Log("Kauf erfolgreich! " + ausgewähltesItem.itemName + " wurde hinzugefügt.");
        }
        else
        {
            // Fehler: Tiefer Ton
            if (audioSource != null && fehlerSound != null) 
                audioSource.PlayOneShot(fehlerSound);

            Debug.LogWarning("Kauf fehlgeschlagen: Zu wenig Gold oder Inventar fehlt.");
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
        }
    }
}