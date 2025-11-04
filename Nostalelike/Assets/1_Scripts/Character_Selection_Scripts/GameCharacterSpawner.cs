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
        SpawnSelectedCharacter();
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
            DontDestroyOnLoad(inventoryPanelObject.transform.root.gameObject);
            
            // Equipment-Panel ebenfalls persistent machen (falls separates Root-GameObject)
            if (equipmentPanelObject != null)
            {
                DontDestroyOnLoad(equipmentPanelObject.transform.root.gameObject);
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