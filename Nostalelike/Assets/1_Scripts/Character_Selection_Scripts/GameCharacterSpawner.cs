using UnityEngine;

public class GameCharacterSpawner : MonoBehaviour
{


    public CharacterDatabase characterDatabase;
    public Transform spawnPoint;
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
            Instantiate(characterToSpawn.characterPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Spawned Character: " + selectedCharacterIndex);
        }
        else
        {
            Debug.LogError("Selected character or prefab is null.");
        }
    }

}
