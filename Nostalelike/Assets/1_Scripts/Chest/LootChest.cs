using UnityEngine;

public class LootChest : MonoBehaviour
{
    [Header("Content")]
    public ItemData itemInside;
    public int quantity = 1;

    [Header("Visuals")]
    public Sprite openChestSprite; // Assign your 'Open' asset here
    
    private bool isOpened = false;
    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;

    void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

    void Update()
    {
        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpened = true;
        if (openChestSprite != null) spriteRenderer.sprite = openChestSprite;

        // Find the player and add the item
        PlayerInventory playerInv = Object.FindFirstObjectByType<PlayerInventory>();
        if (playerInv != null && itemInside != null)
        {
            playerInv.inventory.AddItem(itemInside, quantity);
            Debug.Log(itemInside.itemName + " found in chest!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) playerInRange = true; }
    private void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) playerInRange = false; }
}