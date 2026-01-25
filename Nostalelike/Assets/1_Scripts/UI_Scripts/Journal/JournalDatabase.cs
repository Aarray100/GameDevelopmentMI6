using System;
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
    
    // Returns a new list of entries sorted by alphanumeric id (numeric prefix then suffix)
    public List<JournalEntry> GetOrderedEntries()
    {
        var list = new List<JournalEntry>(entries);
        list.Sort(CompareById);
        return list;
    }

    static int CompareById(JournalEntry a, JournalEntry b)
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a == null) return -1;
        if (b == null) return 1;

        var (an, asuf) = SplitId(a.id);
        var (bn, bsuf) = SplitId(b.id);

        int c = an.CompareTo(bn);
        if (c != 0) return c;

        return string.Compare(asuf, bsuf, StringComparison.Ordinal);
    }

    static (long, string) SplitId(string id)
    {
        if (string.IsNullOrEmpty(id)) return (0, "");

        int i = 0;
        while (i < id.Length && char.IsDigit(id[i])) i++;

        var num = id.Substring(0, i);
        var suf = id.Substring(i);

        long n = 0;
        if (!string.IsNullOrEmpty(num)) long.TryParse(num, out n);
        return (n, suf ?? "");
    }
}
