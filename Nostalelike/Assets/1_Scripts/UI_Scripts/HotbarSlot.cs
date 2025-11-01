using UnityEngine;

[System.Serializable]
public class HotbarSlot
{
    public ItemData item;
    public int quantity = 1;
    
    // Für Magic Cycling (nur relevant wenn Zauberstab)
    public MagicAbility[] magicAbilities;
    public int currentAbilityIndex = 0;
    
    public HotbarSlot()
    {
        item = null;
        quantity = 0;
    }
    
    public void SetItem(ItemData newItem, int newQuantity = 1)
    {
        item = newItem;
        quantity = newQuantity;
        
        // Wenn es ein Zauberstab ist, initialisiere Magic Abilities
        if (item != null && item.weaponType == WeaponType.Staff && magicAbilities == null)
        {
            // Hier werden später die 4 Magic Abilities zugewiesen
            magicAbilities = new MagicAbility[4];
        }
    }
    
    public void ClearSlot()
    {
        item = null;
        quantity = 0;
        currentAbilityIndex = 0;
    }
    
    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }
    
    // Cycle durch Magic Abilities (für Zauberstab)
    public void CycleAbility()
    {
        if (item != null && item.weaponType == WeaponType.Staff && magicAbilities != null && magicAbilities.Length > 0)
        {
            currentAbilityIndex = (currentAbilityIndex + 1) % magicAbilities.Length;
            Debug.Log($"Switched to ability: {currentAbilityIndex}");
        }
    }
    
    public MagicAbility GetCurrentAbility()
    {
        if (magicAbilities != null && currentAbilityIndex < magicAbilities.Length)
        {
            return magicAbilities[currentAbilityIndex];
        }
        return null;
    }
}
