using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemPickup : MonoBehaviour
{

    public ItemData itemToPickup;
    public int quantity = 1;

    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (itemToPickup != null && itemToPickup.itemIcon != null)
        {
            spriteRenderer.sprite = itemToPickup.itemIcon;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();

            if (playerInventory != null)
            {
                playerInventory.inventory.AddItem(itemToPickup, quantity);
                Destroy(gameObject);
            }
        }
    }















}
