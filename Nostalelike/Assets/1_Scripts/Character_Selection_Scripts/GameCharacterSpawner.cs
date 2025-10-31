using UnityEngine;

public class GameCharacterSpawner : MonoBehaviour
{


    public CharacterDatabase characterDatabase;
    public Transform spawnPoint;

    [Header("UI References")]
    public Transform slotParent;
    public GameObject inventoryPanelObject;
    public GameObject slotPrefab;
    public static GameCharacterSpawner instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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

            if (playerInventory != null)
            {
                playerInventory.slotParent = slotParent;
                playerInventory.inventoryPanelObject = inventoryPanelObject;
                playerInventory.slotPrefab = slotPrefab;

                playerInventory.InitializeInventoryUI();
            }
            else
            {
                Debug.LogError("Selected character or prefab is null.");
            }
            
            DontDestroyOnLoad(characterInstance);
            DontDestroyOnLoad(inventoryPanelObject.transform.root.gameObject);
            Destroy(this.gameObject);
            
        }
        else
        {
            Debug.LogError("Selected character or prefab is null.");
        }
    }
}