using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemPickup : MonoBehaviour
{
    public ItemData itemToPickup;
    public int quantity = 1;
    
    [Header("Despawn Settings")]
    [Tooltip("Zeit in Sekunden bis das Item despawnt. 0 = nie despawnen.")]
    public float despawnTime = 60f;
    
    [Tooltip("Wurde dieses Item vom Spieler gedroppt? (Dann despawnt es)")]
    public bool wasDroppedByPlayer = false;

    private SpriteRenderer spriteRenderer;
    private float spawnTime;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (itemToPickup != null && itemToPickup.itemIcon != null)
        {
            spriteRenderer.sprite = itemToPickup.itemIcon;
        }
    }
    
    private void Start()
    {
        spawnTime = Time.time;
    }
    
    private void Update()
    {
        // Nur despawnen wenn vom Spieler gedroppt und despawnTime > 0
        if (wasDroppedByPlayer && despawnTime > 0)
        {
            if (Time.time - spawnTime >= despawnTime)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();

            if (playerInventory != null)
            {
                // Prüfe ob das Inventar Platz hat
                if (playerInventory.inventory.CanAddItem(itemToPickup, quantity))
                {
                    playerInventory.inventory.AddItem(itemToPickup, quantity);
                    Destroy(gameObject);
                }
                // Wenn kein Platz: Item bleibt liegen (nichts passiert)
            }
        }
    }
    
    /// <summary>
    /// Initialisiert ein gedropptes Item
    /// </summary>
    public void InitializeDroppedItem(ItemData item, int qty, float customDespawnTime = 60f)
    {
        itemToPickup = item;
        quantity = qty;
        wasDroppedByPlayer = true;
        despawnTime = customDespawnTime;
        spawnTime = Time.time;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        if (item != null && item.itemIcon != null)
        {
            spriteRenderer.sprite = item.itemIcon;
        }
    }















}
