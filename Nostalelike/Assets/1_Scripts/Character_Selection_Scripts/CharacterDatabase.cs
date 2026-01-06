using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu]

public class CharacterDatabase : ScriptableObject
{
    public Character[] characters;

     public int CharacterCount
    {
        get
        {
            return characters.Length;
        }
    }

   
    public Character GetCharacterByIndex(int index)
    {
        if (index >= 0 && index < characters.Length)
        {
            return characters[index];
        }
        else
        {
            Debug.LogError("Index out of range: " + index);
            return null;
        }
    }

}
