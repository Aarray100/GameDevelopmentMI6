using UnityEngine;

public class JournalDatabaseDebug : MonoBehaviour
{
    [SerializeField] JournalDatabase database;

    void Start()
    {
        if (database == null) return;

        Debug.Log($"📚 JOURNAL DATABASE - {database.entries.Count} Einträge\n");
        
        var ordered = database.GetOrderedEntries();
        foreach (var e in ordered)
        {
            if (e == null) continue;
            
            Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log($"📖 ID: '{e.id}' | Title: '{e.title}' | Pages: {e.pages.Count}");
            
            for (int i = 0; i < e.pages.Count; i++)
            {
                Debug.Log($"\n   📄 Seite {i + 1}:");
                Debug.Log($"   {e.pages[i]}\n");
            }
        }
        
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
    }
}
