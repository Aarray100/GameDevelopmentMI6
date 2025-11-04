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
        }
        else
        {
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
                playerInventory.slotParent = slotParent;
                playerInventory.inventoryPanelObject = inventoryPanelObject;
                playerInventory.equipmentPanelObject = equipmentPanelObject;  // Equipment-Panel zuweisen
                playerInventory.slotPrefab = slotPrefab;

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
            if (inventoryPanelObject != null)
            {
                GameObject inventoryRoot = inventoryPanelObject.transform.root.gameObject;
                if (inventoryRoot.scene.name != "DontDestroyOnLoad")
                {
                    DontDestroyOnLoad(inventoryRoot);
                }
            }
            
            // Equipment-Panel ebenfalls persistent machen (falls separates Root-GameObject)
            if (equipmentPanelObject != null)
            {
                GameObject equipmentRoot = equipmentPanelObject.transform.root.gameObject;
                if (equipmentRoot.scene.name != "DontDestroyOnLoad")
                {
                    DontDestroyOnLoad(equipmentRoot);
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