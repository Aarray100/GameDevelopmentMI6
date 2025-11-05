using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Resource Stats")]
    public float baseMaxHealth = 100f;
    public float baseMaxMana = 50f;
    public float baseMaxStamina = 75f;
    
    [Header("Current Resources")]
    public float currentHealth;
    public float currentMana;
    public float currentStamina;
    
    [Header("Base Regeneration Rates (per second)")]
    public float baseHealthRegenRate = 5f;
    public float baseManaRegenRate = 3f;
    public float baseStaminaRegenRate = 4f;
    
    [Header("Base Offensive Stats")]
    public float baseDamage = 10f;
    public float baseAttackSpeed = 1f;
    public float baseCriticalChance = 0.1f;
    public float baseCriticalDamage = 1.5f;

    [Header("Base Defensive Stats")]
    public float baseDefense = 5f;
    public float baseResistance = 3f;
    public float baseEvasion = 0.05f;
    
    [Header("Calculated Total Stats (Base + Level + Equipment)")]
    public float maxHealth;
    public float maxMana;
    public float maxStamina;
    public float totalDamage;
    public float totalAttackSpeed;
    public float totalCriticalChance;
    public float totalCriticalDamage;
    public float totalDefense;
    public float totalResistance;
    public float totalEvasion;
    public float totalHealthRegen;
    public float totalManaRegen;
    public float totalStaminaRegen;
    [Header("Level System")]
    public int currentLevel = 1;
    public int experiencePoints = 0;
    public int experienceToNextLevel = 100;
    public float experienceRequired; // Wird in Awake() berechnet
    
    [Header("Stat Growth per Level")]
    public float healthPerLevel = 10f;
    public float manaPerLevel = 8f;
    public float staminaPerLevel = 5f;
    public float damagePerLevel = 2f;
    public float defensePerLevel = 1f;
    public float criticalChancePerLevel = 0.005f; // 0.5%
    
    // Equipment Bonuses
    private ItemStats equipmentBonus = new ItemStats();  // Armor + Accessories
    private ItemStats activeWeaponBonus = new ItemStats(); // Nur aktive Waffe
    
    // Events für UI Updates
    public event Action OnStatsChanged;
    public event Action OnHealthChanged;
    public event Action OnManaChanged;
    public event Action OnStaminaChanged;
    
    private void Awake()
    {
        // Initialisiere Werte, die von anderen Feldern abhängen
        experienceRequired = CalculateEXPForNextLevel();
        
        // Berechne initiale Stats
        RecalculateStats();
        
        // Setze Current Values auf Max
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
    }
    
    private void Update()
    {
        // Regeneration
        RegenerateResources();
    }
    
    private void RegenerateResources()
    {
        // Health Regeneration
        if (currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + totalHealthRegen * Time.deltaTime, maxHealth);
            OnHealthChanged?.Invoke();
        }
        
        // Mana Regeneration
        if (currentMana < maxMana)
        {
            currentMana = Mathf.Min(currentMana + totalManaRegen * Time.deltaTime, maxMana);
            OnManaChanged?.Invoke();
        }
        
        // Stamina Regeneration
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + totalStaminaRegen * Time.deltaTime, maxStamina);
            OnStaminaChanged?.Invoke();
        }
    }
    
    // Wird von Hotbar aufgerufen wenn Waffe gewechselt wird
    public void SetActiveWeapon(ItemData weapon)
    {
        if (weapon != null && weapon.itemType == ItemType.Weapon && weapon.stats != null)
        {
            activeWeaponBonus = weapon.stats;
            Debug.Log($"Active weapon set: {weapon.itemName} (+{weapon.stats.bonusDamage} damage)");
        }
        else
        {
            activeWeaponBonus = new ItemStats(); // Reset
            Debug.Log("No weapon active");
        }
        
        RecalculateStats();
    }
    
    // Wird von Equipment-System aufgerufen
    public void UpdateEquipmentBonus(ItemStats newEquipmentBonus)
    {
        equipmentBonus = newEquipmentBonus;
        RecalculateStats();
    }
    
    // Berechnet alle Stats basierend auf Level + Equipment + aktive Waffe
    public void RecalculateStats()
    {
        // Base Stats vom Level
        float levelMaxHealth = baseMaxHealth + (currentLevel * healthPerLevel);
        float levelMaxMana = baseMaxMana + (currentLevel * manaPerLevel);
        float levelMaxStamina = baseMaxStamina + (currentLevel * staminaPerLevel);
        float levelDamage = baseDamage + (currentLevel * damagePerLevel);
        float levelDefense = baseDefense + (currentLevel * defensePerLevel);
        float levelCritChance = baseCriticalChance + (currentLevel * criticalChancePerLevel);
        
        // + Equipment Bonus (Armor + Accessories, immer aktiv)
        maxHealth = levelMaxHealth + equipmentBonus.bonusHealth;
        maxMana = levelMaxMana + equipmentBonus.bonusMana;
        maxStamina = levelMaxStamina + equipmentBonus.bonusStamina;
        totalDefense = levelDefense + equipmentBonus.bonusDefense;
        totalResistance = baseResistance + equipmentBonus.bonusResistance;
        totalEvasion = baseEvasion + equipmentBonus.bonusEvasion;
        
        // + Aktive Waffe Bonus (nur wenn Waffe in Hotbar aktiv)
        totalDamage = levelDamage + equipmentBonus.bonusDamage + activeWeaponBonus.bonusDamage;
        totalAttackSpeed = baseAttackSpeed + equipmentBonus.bonusAttackSpeed + activeWeaponBonus.bonusAttackSpeed;
        totalCriticalChance = levelCritChance + equipmentBonus.bonusCritChance + activeWeaponBonus.bonusCritChance;
        totalCriticalDamage = baseCriticalDamage + equipmentBonus.bonusCritDamage + activeWeaponBonus.bonusCritDamage;
        
        // Regeneration
        totalHealthRegen = baseHealthRegenRate + equipmentBonus.bonusHealthRegen + activeWeaponBonus.bonusHealthRegen;
        totalManaRegen = baseManaRegenRate + equipmentBonus.bonusManaRegen + activeWeaponBonus.bonusManaRegen;
        totalStaminaRegen = baseStaminaRegenRate + equipmentBonus.bonusStaminaRegen + activeWeaponBonus.bonusStaminaRegen;
        
        // Damage Multiplier anwenden (von Waffe)
        if (activeWeaponBonus.damageMultiplier > 1f)
        {
            totalDamage *= activeWeaponBonus.damageMultiplier;
        }
        
        // Clamp current values wenn max sich ändert
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentMana = Mathf.Min(currentMana, maxMana);
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        
        OnStatsChanged?.Invoke();
        
        Debug.Log($"Stats recalculated! Total Damage: {totalDamage}, Total Defense: {totalDefense}");
    }
    
    private float CalculateEXPForNextLevel()
    {
        // Exponentielle Kurve: wird mit jedem Level schwerer
        return Mathf.Pow(experienceToNextLevel * currentLevel, 1.5f);
    }
    
    public void GainExperience(int amount)
    {
        experiencePoints += amount;
        Debug.Log($"Gained {amount} EXP. Total: {experiencePoints}/{experienceRequired}");
        
        // Check für Level Up
        while (experiencePoints >= experienceRequired)
        {
            LevelUp();
        }
    }
    
    private void LevelUp()
    {
        currentLevel++;
        experiencePoints -= (int)experienceRequired;
        experienceRequired = CalculateEXPForNextLevel();
        
        // Stats neu berechnen (basierend auf neuem Level)
        RecalculateStats();
        
        // Full Heal beim Level Up
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        
        Debug.Log($"LEVEL UP! Now Level {currentLevel}");
        // Hier könntest du ein Event triggern für UI/Effekte
    }
    
    // Helper Methods für Combat System
    public float GetFinalDamage()
    {
        return totalDamage;
    }
    
    public bool IsCriticalHit()
    {
        return UnityEngine.Random.value < totalCriticalChance;
    }
    
    public float GetCriticalDamage()
    {
        return totalDamage * totalCriticalDamage;
    }
    
    public void TakeDamage(float damage)
    {
        float finalDamage = Mathf.Max(damage - totalDefense, 0);
        currentHealth = Mathf.Max(currentHealth - finalDamage, 0);
        OnHealthChanged?.Invoke();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke();
    }
    
    public void RestoreMana(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        OnManaChanged?.Invoke();
    }
    
    public void RestoreStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        OnStaminaChanged?.Invoke();
    }
    
    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            OnManaChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            OnStaminaChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    private void Die()
    {
        Debug.Log("Player died!");
        // Hier Death-Logik
    }
}
