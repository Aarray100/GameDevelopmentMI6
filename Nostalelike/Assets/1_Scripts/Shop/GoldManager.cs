using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance; // Erlaubt Zugriff von überall

    [Header("Einstellungen")]
    public int startGold = 500;
    public int aktuellesGold;
    public TextMeshProUGUI goldAnzeigeUI;

    void Awake()
    {
        // Singleton-Muster: Es darf nur einen GoldManager geben
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        aktuellesGold = startGold;
        UpdateGoldAnzeige();
    }

    public void GoldHinzufuegen(int menge)
    {
        aktuellesGold += menge;
        UpdateGoldAnzeige();
        Debug.Log(menge + " Gold erhalten! Kontostand: " + aktuellesGold);
    }

    public bool GoldAbziehen(int menge)
    {
        if (aktuellesGold >= menge)
        {
            aktuellesGold -= menge;
            UpdateGoldAnzeige();
            return true; // Kauf erfolgreich
        }
        return false; // Zu wenig Gold
    }

    public void UpdateGoldAnzeige()
    {
        if (goldAnzeigeUI != null)
        {
            goldAnzeigeUI.text = "Gold: " + aktuellesGold;
        }
    }
}