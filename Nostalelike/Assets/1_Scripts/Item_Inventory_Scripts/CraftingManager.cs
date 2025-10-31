using UnityEngine;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    //im Inspector ALLE Rezept-ScriptableObjects reinziehen
    public List<Recipe> knownRecipes;

    public PlayerInventory playerInventory;

    public bool CraftItem(Recipe recipe)
    {
        //Überprüfen, ob alle Zutaten im Inventar sind
        foreach (craftingIngredient ingredient in recipe.ingredients)
        {
            InventorySlot slot = playerInventory.inventory.slots.Find(s => s.item == ingredient.item);
            if (slot == null || slot.quantity < ingredient.quantity)
            {
                Debug.Log("Nicht genügend Zutaten für das Rezept: " + recipe.resultItem.itemName);
                return false; //Zutat fehlt oder nicht genug davon
            }
        }

        //Zutaten entfernen
        foreach (craftingIngredient ingredient in recipe.ingredients)
        {
            playerInventory.inventory.RemoveItem(ingredient.item, ingredient.quantity);
        }

        //Ergebnis zum Inventar hinzufügen
        playerInventory.inventory.AddItem(recipe.resultItem, recipe.resultQuantity);
        Debug.Log("Gegenstand hergestellt: " + recipe.resultItem.itemName);
        return true;
    }
  
    
}
