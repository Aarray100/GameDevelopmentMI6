using UnityEngine;
using TMPro;
using System.Collections;

public class JournalBootstrap : MonoBehaviour
{
    const string FIRST_START_KEY = "journal_firststart";

    [Header("Popup Settings")]
    [SerializeField] GameObject popupPanel; // Optional: Panel mit Text im Canvas
    [SerializeField] TMP_Text popupText; // Optional: Text Component
    [SerializeField] float popupDisplayTime = 4f;

    static JournalBootstrap instance;
    Coroutine popupCoroutine;

    void Awake()
    {
        // Singleton-Pattern mit DontDestroyOnLoad
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // -002 SOFORT freischalten in Awake (VOR allem anderen!)
        if (JournalProgress.IsFirstStart())
        {
            JournalProgress.Unlock("-002");
            JournalProgress.MarkFirstStartDone();
            Debug.Log("📖 Tutorial-Seite -002 freigeschaltet (Awake - VOR allem anderen)");
        }
        
        // Stelle sicher dass Popup initial aus ist
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    void Start()
    {
        // Popup in Start anzeigen (nach Awake)
        if (popupPanel != null && JournalProgress.IsUnlocked("-002"))
        {
            ShowFirstStartPopup();
        }
    }

    public static void ClosePopup()
    {
        if (instance != null)
        {
            instance.ClosePopupInternal();
        }
    }

    void ClosePopupInternal()
    {
        Debug.Log("🔒 Popup wird geschlossen");
        
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
            popupCoroutine = null;
        }
        
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            Debug.Log("✅ Popup Panel deaktiviert");
        }
    }

    void ShowFirstStartPopup()
    {
        if (popupPanel != null)
        {
            Debug.Log("📢 Zeige Popup an...");
            popupCoroutine = StartCoroutine(ShowPopupRoutine());
        }
        else
        {
            // Fallback: Console Log
            Debug.LogWarning("⚠️ Kein Popup Panel zugewiesen! Verwende Console-Log.");
            Debug.Log("📖 Drücke [J] um das Journal zu öffnen und die Steuerung zu lernen!");
        }
    }

    IEnumerator ShowPopupRoutine()
    {
        popupPanel.SetActive(true);
        
        if (popupText != null)
        {
            popupText.text = "Open your Journal with:\n\"J\"";
        }

        yield return new WaitForSeconds(popupDisplayTime);
        
        popupPanel.SetActive(false);
    }
}
