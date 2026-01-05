using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalViewer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text leftText;
    [SerializeField] TMP_Text rightText;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;

    [Header("Data")]
    [SerializeField] JournalDatabase database;
    [SerializeField] string currentEntryId = ""; // optional: Start-Entry

    int leftPageIndex = 0;

   void OnEnable()
{
    // Sicherheitsnetz: Wenn keine ID da ist, nimm die erste aus der Datenbank
    if (string.IsNullOrEmpty(currentEntryId) && database != null && database.entries.Count > 0)
    {
        currentEntryId = database.entries[0].id;
        Debug.Log($"JournalViewer: Keine ID gesetzt, verwende Fallback: {currentEntryId}");
    }

    Refresh();
}

    public void SetEntryById(string id)
    {
        currentEntryId = id;
        leftPageIndex = 0;
        Refresh();
    }

    JournalEntry CurrentEntry =>
        (database != null && !string.IsNullOrEmpty(currentEntryId))
            ? database.GetById(currentEntryId)
            : null;

    public void Next()
{
    var entry = CurrentEntry;
    if (entry == null || database == null) return;

    // 1. Gibt es noch mehr Seiten im AKTUELLEN Eintrag?
    if (leftPageIndex + 2 < entry.pages.Count)
    {
        leftPageIndex += 2;
    }
    else 
    {
        // 2. Wenn nicht: Gibt es einen NÄCHSTEN Eintrag in der Database?
        int currentIndex = database.entries.IndexOf(entry);
        if (currentIndex < database.entries.Count - 1)
        {
            // Springe zum nächsten Eintrag, starte bei Seite 0
            currentEntryId = database.entries[currentIndex + 1].id;
            leftPageIndex = 0;
        }
    }
    Refresh();
}

public void Prev()
{
    var entry = CurrentEntry;
    if (entry == null || database == null) return;

    // 1. Können wir im AKTUELLEN Eintrag zurückblättern?
    if (leftPageIndex > 0)
    {
        leftPageIndex -= 2;
    }
    else 
    {
        // 2. Wenn nicht: Gibt es einen VORHERIGEN Eintrag?
        int currentIndex = database.entries.IndexOf(entry);
        if (currentIndex > 0)
        {
            var prevEntry = database.entries[currentIndex - 1];
            currentEntryId = prevEntry.id;
            
            // Wichtig: Wir müssen am Ende des vorherigen Eintrags landen
            // Wenn der z.B. 4 Seiten hat, müssen wir auf Seite 2 starten (für Doppelseite 2&3)
            int lastPage = prevEntry.pages.Count - 1;
            leftPageIndex = (lastPage / 2) * 2; 
        }
    }
    Refresh();
}

public void Refresh()
{
    var entry = CurrentEntry;
    if (database == null || entry == null) return;

    int total = entry.pages.Count;
    int currentIndex = database.entries.IndexOf(entry);

    leftText.text  = GetPageText(entry, leftPageIndex);
    rightText.text = GetPageText(entry, leftPageIndex + 1);

    // Button-Logik: Wann darf man klicken?
    if (prevButton != null)
    {
        // Aktiv, wenn wir nicht auf Seite 0 des ALLERERSTEN Eintrags sind
        prevButton.interactable = (leftPageIndex > 0) || (currentIndex > 0);
    }

    if (nextButton != null)
    {
        // Aktiv, wenn der aktuelle Eintrag noch Seiten hat ODER ein weiterer Eintrag folgt
        nextButton.interactable = (leftPageIndex + 2 < total) || (currentIndex < database.entries.Count - 1);
    }
}
    string GetPageText(JournalEntry entry, int index)
    {
        if (entry == null) return "";

        if (index < 0 || index >= entry.pages.Count) return "";

        if (!JournalProgress.IsUnlocked(entry.id)) return "???";

        return entry.pages[index];
    }
}