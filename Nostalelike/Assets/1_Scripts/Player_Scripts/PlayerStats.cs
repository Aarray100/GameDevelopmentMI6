using UnityEngine;
using System;
using System.Collections;

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
    
    [Header("Base Regeneration Rates")]
    public float baseHealthRegenRate = 5f;
    public float baseManaRegenRate = 3f;
    public float baseStaminaRegenRate = 4f;

    [Header("Base Combat Stats")] // Diese Variablen haben gefehlt!
    public float baseDamage = 10f;
    public float baseDefense = 5f;
    
    [Header("Calculated Total Stats")]
    public float maxHealth;
    public float maxMana;
    public float maxStamina;
    public float totalDamage;
    public float totalDefense;
    public float totalHealthRegen;
    public float totalManaRegen;
    public float totalStaminaRegen;

    [Header("Level System")]
    public int currentLevel = 1;
    public int experiencePoints = 0;
    public int experienceToNextLevel = 100;
    
    [Header("Temporäre Buffs")]
    private float strengthBuffMultiplier = 1f;
    private PlayerMovement2D movement;

    private ItemStats equipmentBonus = new ItemStats();
    private ItemStats activeWeaponBonus = new ItemStats();
    
    public event Action OnStatsChanged;
    public event Action OnHealthChanged;
    public event Action OnManaChanged;
    public event Action OnStaminaChanged;
    
    private void Awake()
    {
        movement = GetComponent<PlayerMovement2D>();
        RecalculateStats();
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
    }
    
    private void Update()
    {
        RegenerateResources();
    }

    private void RegenerateResources()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + totalHealthRegen * Time.deltaTime, maxHealth);
            OnHealthChanged?.Invoke();
        }
        if (currentMana < maxMana)
        {
            currentMana = Mathf.Min(currentMana + totalManaRegen * Time.deltaTime, maxMana);
            OnManaChanged?.Invoke();
        }
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + totalStaminaRegen * Time.deltaTime, maxStamina);
            OnStaminaChanged?.Invoke();
        }
    }

    // --- INTERFACES FÜR ANDERE SKRIPTE (Hotbar & Equipment) ---

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

    public void SetActiveWeapon(ItemData weapon)
    {
        activeWeaponBonus = (weapon != null && weapon.stats != null) ? weapon.stats : new ItemStats();
        RecalculateStats();
    }

    public void UpdateEquipmentBonus(ItemStats newEquipmentBonus)
    {
        equipmentBonus = newEquipmentBonus;
        RecalculateStats();
    }

    // --- TRANK LOGIK (3 MINUTEN BUFFS) ---

    public void UsePotion(ItemData potion)
    {
        if (potion.itemName.Contains("Healing")) Heal(potion.healAmount);
        else if (potion.itemName.Contains("Mana")) RestoreMana(potion.manaAmount);
        else if (potion.itemName.Contains("Strength")) StartCoroutine(StrengthBuffRoutine(180f));
        else if (potion.itemName.Contains("Speed")) StartCoroutine(SpeedBuffRoutine(180f));
        else if (potion.itemName.Contains("Omni")) ApplyOmniLevelUp();
    }

    private IEnumerator StrengthBuffRoutine(float duration)
    {
        strengthBuffMultiplier = 1.2f;
        RecalculateStats();
        yield return new WaitForSeconds(duration);
        strengthBuffMultiplier = 1f;
        RecalculateStats();
    }

    private IEnumerator SpeedBuffRoutine(float duration)
    {
        if (movement == null) movement = GetComponent<PlayerMovement2D>();
        float boost = movement.walkSpeed * 0.2f;
        movement.walkSpeed += boost;
        movement.runSpeed += boost;
        yield return new WaitForSeconds(duration);
        movement.walkSpeed -= boost;
        movement.runSpeed -= boost;
    }

    private void ApplyOmniLevelUp()
    {
        currentLevel++;
        experiencePoints = 0;
        experienceToNextLevel = (int)(experienceToNextLevel * 1.5f);
        RecalculateStats();
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        OnStatsChanged?.Invoke();
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStaminaChanged?.Invoke();
    }

    // --- BERECHNUNG ---

    public void RecalculateStats()
    {
        // Hier werden baseDamage und baseDefense jetzt korrekt gefunden
        maxHealth = (baseMaxHealth + (currentLevel * 10f)) + equipmentBonus.bonusHealth;
        maxMana = (baseMaxMana + (currentLevel * 8f)) + equipmentBonus.bonusMana;
        maxStamina = (baseMaxStamina + (currentLevel * 5f)) + equipmentBonus.bonusStamina;
        
        totalDamage = (baseDamage + (currentLevel * 2f) + equipmentBonus.bonusDamage + activeWeaponBonus.bonusDamage) * strengthBuffMultiplier;
        totalDefense = (baseDefense + (currentLevel * 1f)) + equipmentBonus.bonusDefense;

        totalHealthRegen = baseHealthRegenRate + equipmentBonus.bonusHealthRegen;
        totalManaRegen = baseManaRegenRate + equipmentBonus.bonusManaRegen;
        totalStaminaRegen = baseStaminaRegenRate + equipmentBonus.bonusStaminaRegen;

        OnStatsChanged?.Invoke();
    }

    #region Save/Load System

    /// <summary>
    /// Lädt die gespeicherten Stats
    /// </summary>
    public void LoadSaveData(float savedHealth, float savedMaxHealth, float savedMana, float savedMaxMana)
    {
        // Max-Werte setzen
        maxHealth = savedMaxHealth;
        maxMana = savedMaxMana;
        
        // Current-Werte setzen mit Clamp
        currentHealth = Mathf.Clamp(savedHealth, 0f, maxHealth);
        currentMana = Mathf.Clamp(savedMana, 0f, maxMana);
        
        // Events triggern für UI Updates
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStatsChanged?.Invoke();
        
        Debug.Log($"PlayerStats loaded: HP {currentHealth}/{maxHealth}, Mana {currentMana}/{maxMana}");
    }

    #endregion
}
