using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Book", fileName = "Book_")]
public class BookData : ItemData
{
    public string bookTitle;

    [TextArea(3, 10)]
    public List<string> pages = new();

    // Optional, falls alter Code storyContent erwartet:
    public string storyContent => (pages != null && pages.Count > 0) ? pages[0] : "";
}
