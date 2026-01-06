using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Journal/Database", fileName = "JournalDatabase")]
public class JournalDatabase : ScriptableObject
{
    public List<JournalEntry> entries = new();

    public JournalEntry GetById(string id)
    {
        return entries.Find(e => e != null && e.id == id);
    }
}
