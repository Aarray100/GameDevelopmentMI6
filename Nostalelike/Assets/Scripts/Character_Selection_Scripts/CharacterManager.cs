using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{

    public CharacterDatabase characterDatabase;

    public SpriteRenderer characterRenderer;
    private int currentCharacterIndex = 0;
    void Start()
    {
        Debug.Log("Character Manager Initialized");
        UpdateCharacterDisplay(currentCharacterIndex);
    }

    public void NextCharacter()
    {
        currentCharacterIndex = (currentCharacterIndex + 1) % characterDatabase.CharacterCount;
        
        UpdateCharacterDisplay(currentCharacterIndex);
    }

    public void PreviousCharacter()
    {
        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = characterDatabase.CharacterCount - 1;
        }
        
        UpdateCharacterDisplay(currentCharacterIndex);
    }


    
    

    private void UpdateCharacterDisplay(int currentCharacterIndex)
    {
        Character currentCharacter = characterDatabase.GetCharacterByIndex(currentCharacterIndex);
        if (currentCharacter != null && characterRenderer != null)
        {
            characterRenderer.sprite = currentCharacter.characterSprite;
            //Debug.Log("Displaying Character: " + currentCharacter.characterName);
        }
        else
        {
            Debug.LogError("Character or Renderer is null");
        }
    }
}
