using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    [Header("Einstellungen")]
    public ItemData goldItemAsset;
    public int startGold = 11000; 
    public int aktuellesGold;

    [Header("Testing")]
    // HAKEN REIN = Startet immer bei 0 (gut zum Testen)
    // HAKEN RAUS = Merkt sich das Gold für immer (gut für das fertige Spiel)
    public bool resetOnStart = true; 
    
    [Header("UI Verknüpfung")]
    public TextMeshProUGUI goldAnzeigeUI;

    void Awake()
    {
        // Wenn der Haken im Inspector an ist, löschen wir den Speicherstand sofort
        if (resetOnStart)
        {
            PlayerPrefs.DeleteKey("GespeichertesGold");
            Debug.Log("<color=yellow>TEST-MODUS: Gold wurde automatisch auf 0 gesetzt!</color>");
        }

        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
            
            // Gold laden (oder 0 nehmen, falls gerade gelöscht wurde)
            LadeGold();
        }
        else 
        { 
            Destroy(gameObject); 
        }
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sucht das Textfeld in der neuen Szene
        if (goldAnzeigeUI == null)
        {
            GameObject textObj = GameObject.Find("GoldAnzeige"); 
            if (textObj != null)
                goldAnzeigeUI = textObj.GetComponent<TextMeshProUGUI>();
        }
        UpdateGoldAnzeige();
    }

    public void GoldHinzufuegen(int menge)
    {
        aktuellesGold += menge;
        UpdateGoldAnzeige();
        SpeichereGold(); 
    }

    // Gibt 'true' zurück, wenn genug Gold da war (wichtig für Shop!)
    public bool GoldAbziehen(int menge)
    {
        if (aktuellesGold >= menge)
        {
            aktuellesGold -= menge;
            UpdateGoldAnzeige();
            SpeichereGold(); 
            return true; 
        }
        return false; 
    }

    public void UpdateGoldAnzeige()
    {
        if (goldAnzeigeUI != null)
        {
            goldAnzeigeUI.text = "Gold: " + aktuellesGold;
        }
    }

    private void SpeichereGold()
    {
        PlayerPrefs.SetInt("GespeichertesGold", aktuellesGold);
        PlayerPrefs.Save();
    }

    private void LadeGold()
    {
        aktuellesGold = PlayerPrefs.GetInt("GespeichertesGold", startGold);
        UpdateGoldAnzeige();
    }

    // Kleiner Zusatz: Du kannst auch während des Spiels im Inspector "Reset Gold" klicken
    [ContextMenu("Reset Gold")]
    public void ResetGold()
    {
        PlayerPrefs.DeleteKey("GespeichertesGold");
        aktuellesGold = startGold;
        UpdateGoldAnzeige();
        Debug.Log("Gold manuell zurückgesetzt!");
    }

    // Debug-Methoden zum Erhöhen von Gold im Inspector
    [ContextMenu("DEBUG: +100 Gold")]
    public void AddGold100()
    {
        GoldHinzufuegen(100);
        Debug.Log("<color=green>+100 Gold hinzugefügt! Aktuell: " + aktuellesGold + "</color>");
    }

    [ContextMenu("DEBUG: +1000 Gold")]
    public void AddGold1000()
    {
        GoldHinzufuegen(1000);
        Debug.Log("<color=green>+1000 Gold hinzugefügt! Aktuell: " + aktuellesGold + "</color>");
    }

    [ContextMenu("DEBUG: +10000 Gold")]
    public void AddGold10000()
    {
        GoldHinzufuegen(10000);
        Debug.Log("<color=green>+10000 Gold hinzugefügt! Aktuell: " + aktuellesGold + "</color>");
    }
}