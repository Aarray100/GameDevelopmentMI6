using UnityEngine;

[CreateAssetMenu(fileName = "NewBook", menuName = "Inventory/Items/Book")]
public class BookData : ItemData 
{
    [Header("Story Content")]
    public string bookTitle;
    [TextArea(15, 20)] // Makes the text box big in the Inspector
    public string storyContent;

    private void OnValidate()
    {
        itemType = ItemType.Book; // Ensure you added 'Book' to your ItemType Enum
        isStackable = false;
    }
}