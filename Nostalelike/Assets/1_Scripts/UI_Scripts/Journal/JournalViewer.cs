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
    [SerializeField] string currentEntryId = "-002"; // Start mit Tutorial-Seite

    int leftPageIndex = 0;

   void OnEnable()
{
    // Debug: Zeige alle Einträge in der Database
    if (database != null)
    {
        Debug.Log($"JournalViewer: Database hat {database.entries.Count} Einträge");
        var ordered = database.GetOrderedEntries();
        foreach (var e in ordered)
        {
            bool unlocked = JournalProgress.IsUnlocked(e.id);
            Debug.Log($"  → Entry '{e.id}' | Unlocked: {unlocked} | Pages: {e.pages.Count}");
        }
    }

    // Fallback: Wenn currentEntryId leer oder nicht freigeschaltet, nimm erste freigeschaltete Entry
    if (database != null)
    {
        var entry = database.GetById(currentEntryId);
        if (entry == null || !JournalProgress.IsUnlocked(currentEntryId))
        {
            // Finde erste freigeschaltete Entry
            var ordered = database.GetOrderedEntries();
            foreach (var e in ordered)
            {
                if (JournalProgress.IsUnlocked(e.id))
                {
                    currentEntryId = e.id;
                    leftPageIndex = 0; // Reset page index
                    Debug.Log($"JournalViewer: Wechsle zu erster freigeschalteter Entry: {currentEntryId}");
                    break;
                }
            }
        }
    }

    Debug.Log($"JournalViewer: Starte mit Entry ID '{currentEntryId}'");
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
            // 2. Wenn nicht: Gibt es einen NÄCHSTEN Eintrag in der geordneten Liste?
            var ordered = database.GetOrderedEntries();
            int currentIndex = ordered.IndexOf(entry);
            if (currentIndex < ordered.Count - 1)
            {
                currentEntryId = ordered[currentIndex + 1].id;
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
        // 2. Wenn nicht: Gibt es einen VORHERIGEN Eintrag in der geordneten Liste?
        var ordered = database.GetOrderedEntries();
        int currentIndex = ordered.IndexOf(entry);
        if (currentIndex > 0)
        {
            var prevEntry = ordered[currentIndex - 1];
            currentEntryId = prevEntry.id;

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
    var ordered = database.GetOrderedEntries();
    int currentIndex = ordered.IndexOf(entry);

    leftText.text  = GetPageText(entry, leftPageIndex);
    rightText.text = GetPageText(entry, leftPageIndex + 1);

    // Button-Logik: Wann darf man klicken?
    if (prevButton != null)
    {
        prevButton.interactable = (leftPageIndex > 0) || (currentIndex > 0);
    }

    if (nextButton != null)
    {
        nextButton.interactable = (leftPageIndex + 2 < total) || (currentIndex < ordered.Count - 1);
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