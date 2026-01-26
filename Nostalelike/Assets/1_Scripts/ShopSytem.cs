using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        // Sucht den Spieler in der aktuellen Szene
        FindeAktivenSpieler();

        // Shop beim Start immer unsichtbar machen
        if (shopPanel != null) shopPanel.SetActive(false);
        if (vorschauText != null) vorschauText.text = "Wähle ein Item...";
    }

    void Update()
    {
        // Gold Anzeige aktualisieren (Dein Stand: 500 Gold) [cite: 2026-01-04]
        if (goldText != null) 
        {
            goldText.text = "Gold: " + PlayerMovement2D.gold;
        }

        // Shop mit G öffnen/schließen
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleShop();
        }
    }

    private void FindeAktivenSpieler()
    {
        // Sucht nach dem Inventar-Skript in der aktuellen Map
        playerInventory = GameObject.FindFirstObjectByType<PlayerInventory>();
    }

    // --- AUSWAHL-FUNKTIONEN FÜR DIE BUTTONS ---
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

        // Sicherstellen, dass wir den Spieler der aktuellen Szene haben
        if (playerInventory == null) FindeAktivenSpieler();

        int besitz = 0;
        if (playerInventory != null && playerInventory.inventory != null)
        {
            var slot = playerInventory.inventory.slots.Find(s => s.item == ausgewähltesItem);
            if (slot != null) besitz = slot.quantity;
        }

        vorschauText.text = "<b>" + ausgewähltesItem.itemName + "</b>\n" +
                          "Preis: " + aktuellerPreis + " Gold\n" +
                          "Im Besitz: " + besitz;
    }

    // --- KAUF-LOGIK (Muss am Kauf-Button hängen!) ---
    public void KaufBestaetigen()
    {
        Debug.Log("Kauf-Versuch gestartet...");

        if (ausgewähltesItem == null)
        {
            Debug.LogWarning("Kauf abgebrochen: Kein Item ausgewählt!");
            return;
        }

        FindeAktivenSpieler();

        if (playerInventory != null && PlayerMovement2D.gold >= aktuellerPreis)
        {
            // 1. Gold abziehen & Speichern [cite: 2026-01-04]
            PlayerMovement2D.gold -= aktuellerPreis;
            PlayerMovement2D.GoldSpeichern();
            
            // 2. Item ins Inventar legen
            playerInventory.inventory.AddItem(ausgewähltesItem, 1);
            
            // 3. UI aktualisieren
            UpdateAnzeige();
            Debug.Log("Kauf erfolgreich! Neues Gold: " + PlayerMovement2D.gold);
        }
        else
        {
            Debug.LogError("Kauf fehlgeschlagen! Zu wenig Gold oder Spieler nicht gefunden.");
        }
    }

    public void ToggleShop() 
    {
        if (shopPanel != null)
        {
            bool status = !shopPanel.activeSelf;
            shopPanel.SetActive(status);
            if (status) FindeAktivenSpieler();
        }
    }
}