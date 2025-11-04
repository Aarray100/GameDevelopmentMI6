using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCharacterSpawner : MonoBehaviour
{


    public CharacterDatabase characterDatabase;
    public Transform spawnPoint;

    [Header("UI References")]
    public Transform slotParent;
    public GameObject inventoryPanelObject;
    public GameObject equipmentPanelObject;  // Equipment-Panel Referenz
    public GameObject slotPrefab;
    public static GameCharacterSpawner instance;
    private static bool hasSpawnedCharacter = false;  // Flag um mehrfaches Spawnen zu verhindern
    
    // Statische Referenzen zu den persistenten UI-Objekten
    private static GameObject persistentInventoryPanel;
    private static GameObject persistentEquipmentPanel;
    private static Transform persistentSlotParent;
    private static GameObject persistentSlotPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Stelle sicher, dass das GameObject ein Root-Objekt ist
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
            
            // Speichere die UI-Referenzen statisch (nur beim ersten Mal)
            if (persistentInventoryPanel == null && inventoryPanelObject != null)
            {
                persistentInventoryPanel = inventoryPanelObject;
                Debug.Log("GameCharacterSpawner: Persistent Inventory Panel gespeichert");
            }
            if (persistentEquipmentPanel == null && equipmentPanelObject != null)
            {
                persistentEquipmentPanel = equipmentPanelObject;
                Debug.Log("GameCharacterSpawner: Persistent Equipment Panel gespeichert");
            }
            if (persistentSlotParent == null && slotParent != null)
            {
                persistentSlotParent = slotParent;
                Debug.Log("GameCharacterSpawner: Persistent SlotParent gespeichert");
            }
            if (persistentSlotPrefab == null && slotPrefab != null)
            {
                persistentSlotPrefab = slotPrefab;
                Debug.Log("GameCharacterSpawner: Persistent SlotPrefab gespeichert");
            }
        }
        else
        {
            // Duplikat gefunden - prüfe ob es UI-Referenzen hat die NICHT die persistenten sind
            if (inventoryPanelObject != null && inventoryPanelObject != persistentInventoryPanel)
            {
                Debug.LogWarning("ACHTUNG: Duplikat GameCharacterSpawner hat ein ANDERES Inventory Panel! " +
                    "Dies könnte das Problem verursachen. Bitte entferne die UI-Referenzen aus diesem Spawner in der Szene.");
            }
            
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Spawne nur, wenn noch kein Character gespawnt wurde
        if (!hasSpawnedCharacter)
        {
            SpawnSelectedCharacter();
            hasSpawnedCharacter = true;
        }
    }


    void SpawnSelectedCharacter()
    {
        if (characterDatabase == null)
        {
            Debug.LogError("CharacterDatabase is not assigned in GameCharacterSpawner.");
            return;
        }

        int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        Debug.Log("Selected Character Index: " + selectedCharacterIndex);

        Character characterToSpawn = characterDatabase.GetCharacterByIndex(selectedCharacterIndex);

        if (characterToSpawn != null && characterToSpawn.characterPrefab != null)
        {
            GameObject characterInstance = Instantiate(characterToSpawn.characterPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Spawned Character: " + selectedCharacterIndex);

            PlayerInventory playerInventory = characterInstance.GetComponent<PlayerInventory>();
            PlayerEquipment playerEquipment = characterInstance.GetComponent<PlayerEquipment>();

            if (playerInventory != null)
            {
                // Verwende die statischen (persistenten) Referenzen
                playerInventory.slotParent = persistentSlotParent;
                playerInventory.inventoryPanelObject = persistentInventoryPanel;
                playerInventory.equipmentPanelObject = persistentEquipmentPanel;
                playerInventory.slotPrefab = persistentSlotPrefab;

                playerInventory.InitializeInventoryUI();
            }
            else
            {
                Debug.LogError("PlayerInventory component not found on character.");
            }
            
            if (playerEquipment != null)
            {
                // PlayerEquipment braucht Referenz zu PlayerInventory (ist im Awake schon gesetzt via GetComponent)
                Debug.Log("PlayerEquipment component found and ready.");
            }
            else
            {
                Debug.LogWarning("PlayerEquipment component not found on character. Equipment system will not work!");
            }
            
            DontDestroyOnLoad(characterInstance);
            
            // UI-Panels persistent machen - prüfe ob sie nicht bereits persistent sind
            // Verwende die statischen Referenzen
            if (persistentInventoryPanel != null)
            {
                GameObject inventoryRoot = persistentInventoryPanel.transform.root.gameObject;
                if (inventoryRoot.scene.name != "DontDestroyOnLoad")
                {
                    DontDestroyOnLoad(inventoryRoot);
                    Debug.Log("Canvas (Inventory) set to DontDestroyOnLoad");
                }
                else
                {
                    Debug.Log("Canvas (Inventory) is already DontDestroyOnLoad");
                }
            }
            
            // Equipment-Panel ebenfalls persistent machen (falls separates Root-GameObject)
            if (persistentEquipmentPanel != null)
            {
                GameObject equipmentRoot = persistentEquipmentPanel.transform.root.gameObject;
                if (equipmentRoot.scene.name != "DontDestroyOnLoad" && equipmentRoot != persistentInventoryPanel.transform.root.gameObject)
                {
                    DontDestroyOnLoad(equipmentRoot);
                    Debug.Log("Canvas (Equipment) set to DontDestroyOnLoad");
                }
            }
            
            // Spawner hat seine Aufgabe erfüllt - kann gelöscht werden
            Destroy(this.gameObject);
            
        }
        else
        {
            Debug.LogError("Selected character or prefab is null.");
        }
    }
}