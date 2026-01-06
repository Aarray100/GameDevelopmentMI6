using UnityEngine;
using TMPro;

public class BookUIManager : MonoBehaviour
{
    public static BookUIManager Instance;

    [Header("UI Referenzen")]
    public GameObject bookPanel;        // Das Fenster-Objekt
    public TMP_Text titleText;    // Textfeld für den Titel
    public TMP_Text contentText;  // Textfeld für die Story

    private void Awake()
    {
        // Singleton-Pattern, damit wir von überall darauf zugreifen können
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (bookPanel != null) bookPanel.SetActive(false);
    }

    public void OpenBook(BookData book)
{
    // 1. Check: Ist das UI-Fenster überhaupt da?
    if (bookPanel == null) {
        Debug.LogError("BookUIManager: bookPanel ist nicht im Inspector zugewiesen!");
        return;
    }

    // 2. Check: Sind die Textfelder zugewiesen?
    if (titleText == null || contentText == null) {
        Debug.LogError("BookUIManager: Textfelder sind nicht im Inspector zugewiesen!");
        return;
    }

    // 3. Daten zuweisen
    titleText.text = book.bookTitle;
    contentText.text = book.storyContent; // Stelle sicher, dass das in BookData.cs so heißt!
    
    bookPanel.SetActive(true);
}

    public void CloseBook()
    {
        bookPanel.SetActive(false);
        // Time.timeScale = 1f; 
    }
}