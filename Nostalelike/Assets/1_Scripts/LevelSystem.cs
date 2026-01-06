using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSystem : MonoBehaviour
{
    [Header("Level Werte")]
    public int level = 1;
    public float currentXP = 0;
    public float xpToNextLevel = 100;

    [Header("UI Anzeige")]
    public Slider xpSlider;
    public TextMeshProUGUI levelText;

    void Start()
    {
        LoadData(); // Lädt die gespeicherten Werte beim Start
        UpdateUI();
    }

    public void AddXP(float amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
        SaveData(); // Speichert sofort, wenn man EP bekommt
        UpdateUI();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;
        xpToNextLevel = Mathf.Round(xpToNextLevel * 1.15f);
        Debug.Log("Level Up! Neues Level: " + level);
    }

    void UpdateUI()
    {
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpToNextLevel;
            xpSlider.value = currentXP;
        }
        if (levelText != null)
        {
            levelText.text = "Level: " + level;
        }
    }

    // --- SPEICHER-LOGIK ---
    public void SaveData()
    {
        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.SetFloat("PlayerXP", currentXP);
        PlayerPrefs.SetFloat("XPToNext", xpToNextLevel);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        // Lädt die Werte. Falls keine Datei existiert, werden die Standardwerte (1, 0, 100) genutzt.
        level = PlayerPrefs.GetInt("PlayerLevel", 1);
        currentXP = PlayerPrefs.GetFloat("PlayerXP", 0);
        xpToNextLevel = PlayerPrefs.GetFloat("XPToNext", 100);
    }

    // Nur zum Testen: Drücke 'R' zum Zurücksetzen des Spielstands
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) AddXP(20);
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Spielstand gelöscht! Starte das Spiel neu.");
        }
    }

    // Neue Methode für SaveSystem
    public void LoadSaveData(int savedLevel, float savedXP, float savedXPToNext)
    {
        level = savedLevel;
        currentXP = savedXP;
        xpToNextLevel = savedXPToNext;
        UpdateUI();
        Debug.Log($"LevelSystem loaded: Level {level}, XP {currentXP}/{xpToNextLevel}");
    }
}