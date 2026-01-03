using UnityEngine;

[CreateAssetMenu(fileName = "NewBook", menuName = "Inventory/Book")]
public class BookData : ItemData
{
    [Header("Book Specifics")]
    public string bookTitle;
    [TextArea(15, 20)]
    public string storyContent;

    private void OnValidate()
    {
        itemType = ItemType.Book; // Setzt den Typ automatisch auf Book
        isStackable = false;      // Bücher sollten meist nicht stapelbar sein
    }
}