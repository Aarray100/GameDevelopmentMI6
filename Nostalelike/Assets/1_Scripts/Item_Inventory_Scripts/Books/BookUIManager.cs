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
        if (bookPanel == null) return;

        titleText.text = book.bookTitle;
        contentText.text = book.storyContent;
        bookPanel.SetActive(true);

        // Optional: Spiel pausieren
        // Time.timeScale = 0f; 
    }

    public void CloseBook()
    {
        bookPanel.SetActive(false);
        // Time.timeScale = 1f; 
    }
}