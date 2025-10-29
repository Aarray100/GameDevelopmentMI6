using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


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

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
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
        GenerateUISlots();
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
            ToggleInventory();
        }
    }
    public void ToggleInventory()
    {
        if (inventoryPanelObject != null)
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanelObject.SetActive(isInventoryOpen);
        }
    }
    void GenerateUISlots()
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
            
            uiSlots.Add(slotUI);
            slotUI.ClearSlot(); //Slot initial leeren
        }
        Debug.Log("Finished generating UI Slots." + uiSlots.Count);

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



}
