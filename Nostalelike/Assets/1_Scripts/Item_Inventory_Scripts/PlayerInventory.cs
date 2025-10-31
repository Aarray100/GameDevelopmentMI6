using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PlayerInventory : MonoBehaviour
{
    public Inventory inventory = new Inventory();

    [SerializeField] private int inventorySize = 49;

    [Header("UI References")]
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("UI Toggle Key")]
    public GameObject inventoryPanelObject;

    private bool isInventoryOpen = false;

    public List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private void Awake()
    {
        inventory.maxSlots = inventorySize;
        if (inventoryPanelObject != null)
        {
            inventoryPanelObject.SetActive(false);
            isInventoryOpen = false;
        }
    }
    private void Start()
    {
        //GenerateUISlots();
        inventory.OnInventoryChanged += UpdateUISlots;
    }
    private void OnDestroy()
    {
        inventory.OnInventoryChanged -= UpdateUISlots;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Pressed I key to toggle inventory.", this.gameObject);
            ToggleInventory();
        }
    }
    public void ToggleInventory()
    {
        Debug.Log("Toggling Inventory UI.", this.gameObject);
        if (inventoryPanelObject != null)
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanelObject.SetActive(isInventoryOpen);
        }
        else
        {
            Debug.LogError("Inventory Panel Object is not assigned.", this.gameObject);
            return;
        }

    }

    public void InitializeInventoryUI()
    {
        GenerateUISlots();
        UpdateUISlots();
    }
    private void GenerateUISlots()
    {
        Debug.Log("Generating UI Slots" + inventory.maxSlots + " slots.");

        //Hier UI-Slots generieren basierend auf inventory.maxSlots
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            Debug.Log("Generating UI Slot " + i);
            GameObject newSlot = Instantiate(slotPrefab, slotParent); //UI-Slot prefab instanziieren und in der UI anordnen
            InventorySlotUI slotUI = newSlot.GetComponent<InventorySlotUI>();

            if (slotUI == null)
            {
                Debug.LogError("Slot Prefab is not assigned in PlayerInventory script.");
                return;
            }

            // WICHTIG: Setze die Referenzen für Drag-and-Drop
            slotUI.playerInventory = this;
            slotUI.slotIndex = i;

            uiSlots.Add(slotUI);
            slotUI.ClearSlot(); //Slot initial leeren
        }
        Debug.Log("Finished generating UI Slots." + uiSlots.Count);

    }
    public void SwapItems(int indexA, int indexB)
    {
        // Sicherheitsüberprüfung
        if (indexA < 0 || indexA >= inventory.slots.Count || indexB < 0 || indexB >= inventory.slots.Count)
        {
            Debug.LogError($"SwapItems: Ungültiger Index! indexA={indexA}, indexB={indexB}, slots.Count={inventory.slots.Count}");
            return;
        }

        InventorySlot slotA = inventory.slots[indexA];      //wird gezogen
        InventorySlot slotB = inventory.slots[indexB];      //wird hierauf abgelegt

        if (slotB.item != null && slotA.item != null && slotA.item == slotB.item && slotA.item.isStackable)
        {
            // Stapeln, wenn beide Slots denselben stapelbaren Gegenstand enthalten
            slotB.quantity += slotA.quantity;
            slotA.item = null;
            slotA.quantity = 0;

        }
        else
        {
            // Tausche die Slot-Inhalte
            ItemData tempItem = slotA.item;
            int tempQuantity = slotA.quantity;
            
            slotA.item = slotB.item;
            slotA.quantity = slotB.quantity;
            
            slotB.item = tempItem;
            slotB.quantity = tempQuantity;
        }
        
        UpdateUISlots();
    }

    public void UpdateUISlots()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < inventory.slots.Count)
            {
                uiSlots[i].UpdateSlot(inventory.slots[i]);
            }
            else
            {
                uiSlots[i].ClearSlot();
            }
        }
    }

    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update the inventory UI when a new scene is loaded
        string targetID = SceneTransitionManager.instance.targetSpawnPointID;
        Debug.Log("Scene loaded: " + scene.name + ", Target Spawn Point ID: " + targetID);

        if (!string.IsNullOrEmpty(targetID))
        {
            // Here you can implement logic to position the player at the target spawn point
            SceneSpawnPoint[] allSpawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (SceneSpawnPoint spawnPoint in allSpawnPoints)
            {
                if (spawnPoint.spawnPointID == targetID)
                {
                    transform.position = spawnPoint.transform.position;
                    Debug.Log("Player spawned at: " + spawnPoint.spawnPointID);
                    break;
                }
                Debug.Log("Player should spawn at: " + targetID);
            }
        }
        UpdateUISlots();
    }

}
