using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class craftingIngredient
{

    public ItemData item;
    public int quantity;


}


[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Inventory/Crafting Recipe")]
public class Recipe : ScriptableObject
{
    public List<craftingIngredient> ingredients;

    [Header("Result")]
    public ItemData resultItem;
    public int resultQuantity = 1;
}
