using UnityEngine;


public class PlayerInventory : MonoBehaviour
{
    public Inventory inventory = new Inventory();

    [SerializeField] private int inventorySize = 30;
    private void Awake()
    {
        inventory.maxSlots = inventorySize;
    }

}
