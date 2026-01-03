using UnityEngine;

public class LootChest : MonoBehaviour
{
    [Header("Chest Content")]
    public ItemData itemInside;
    public int quantity = 1;
    
    [Header("Visuals")]
    public Sprite openSprite;
    
    private bool isOpened = false;
    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Prüfen, ob Spieler da ist, Truhe zu ist und 'E' gedrückt wird
        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (itemInside == null)
        {
            Debug.LogWarning("Truhe ist leer! ItemInside fehlt im Inspector.");
            return;
        }

        isOpened = true;

        // Bild zu "offen" wechseln
        if (openSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = openSprite;
        }

        // Spieler suchen und Item ins Inventar legen
        PlayerInventory playerInv = Object.FindFirstObjectByType<PlayerInventory>();
        if (playerInv != null)
        {
            playerInv.inventory.AddItem(itemInside, quantity);
            Debug.Log($"{itemInside.itemName} wurde dem Inventar hinzugefügt!");
        }
    }

    // Trigger-Methoden (jetzt im richtigen Format)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}