using UnityEngine;

/// <summary>
/// Tutorial-Kiste: Schaltet Journal-Entry -001 frei beim ersten Öffnen
/// Füge dieses Script zur Kiste hinzu und verbinde es mit einem Trigger/Interaction System
/// </summary>
public class TutorialChest : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] string journalEntryToUnlock = "-001";
    [SerializeField] bool showDebugMessages = true;
    
    [Header("Optional: Visual Feedback")]
    [SerializeField] Animator animator; // Optional: Für Öffnungs-Animation
    [SerializeField] string openTrigger = "Open"; // Animator-Trigger Name
    
    bool isOpened = false;

    void Start()
    {
        // Kiste ist nur während dieser Session geöffnet
        // Wird beim Laden eines Spielstands über dein SaveSystem wiederhergestellt
    }

    /// <summary>
    /// Rufe diese Methode auf wenn der Spieler E drückt (aus deinem Interaction System)
    /// </summary>
    public void OpenChest()
    {
        if (isOpened) return;

        Debug.Log("📦 Kiste wird geöffnet...");
        isOpened = true;
        
        // Journal-Eintrag freischalten (Runtime)
        JournalProgress.Unlock(journalEntryToUnlock);
        
        // Popup schließen (falls noch aktiv)
        Debug.Log("📦 Versuche Popup zu schließen...");
        JournalBootstrap.ClosePopup();
        
        // Animation abspielen
        if (animator != null && !string.IsNullOrEmpty(openTrigger))
        {
            animator.SetTrigger(openTrigger);
        }
        
        SetOpenState();
        
        if (showDebugMessages)
        {
            Debug.Log($"📦 Tutorial-Kiste geöffnet! Journal-Eintrag '{journalEntryToUnlock}' freigeschaltet.");
        }
    }

    void SetOpenState()
    {
        // Hier kannst du visuelle Änderungen machen (z.B. Sprite tauschen)
        // Beispiel: GetComponent<SpriteRenderer>().sprite = openSprite;
    }

    // Optional: Für Trigger-basiertes System (wenn Spieler in Reichweite ist)
    void OnTriggerStay2D(Collider2D other)
    {
        if (isOpened) return;
        
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    // Optional: Zeige "Press E to open" UI wenn in Reichweite
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            // Hier könntest du ein UI-Element einblenden: "Drücke [E] zum Öffnen"
            if (showDebugMessages)
                Debug.Log("📦 Drücke [E] um die Kiste zu öffnen");
        }
    }
}
