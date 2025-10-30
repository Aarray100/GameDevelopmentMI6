using UnityEngine;

public class ItemPickup : MonoBehaviour
{
   
    public ItemData itemToPickup;
    public int quantity = 1;

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
