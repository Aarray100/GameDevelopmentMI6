using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Book", fileName = "Book_")]
public class BookData : ScriptableObject
{
    public string bookId;          // z.B. "book_intro_01"
    public string title;
    public Sprite icon;

    [TextArea(3, 10)]
    public List<string> pages = new();
}
