using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Journal/Entry", fileName = "JournalEntry_")]
public class JournalEntry : ScriptableObject
{
    public string id;              // z.B. "home_01"
    public string title;
    [TextArea(3, 10)] public List<string> pages = new(); // 0=links, 1=rechts, ...
}
