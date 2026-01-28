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
    public event Action OnPlayerDeath;
    public event Action OnPlayerRespawn;
    public event Action<int, int> OnLevelUp; // (fromLevel, toLevel)
    public event Action OnXPChanged; // NEU: Für XP Bar Updates
    
    [Header("Death Settings")]
    public float deathAnimationDuration = 1.5f;
    public float respawnDelay = 2f;
    public bool isDead = false;
    
    // Referenzen
    private Animator anim;
    private PlayerMovement2D playerMovement;
    
    private void Awake()
    {
        experienceRequired = CalculateEXPForNextLevel();
        RecalculateStats();
        
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        
        anim = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement2D>();
    }
    
    private void Update()
    {
        RegenerateResources();
    }

    // --- NEU: DIESE METHODE BEHEBT DEN FEHLER CS1061 ---
    /// <summary>
    /// Wird von der InventorySlotUI aufgerufen.
    /// Wendet den Effekt eines verbrauchbaren Gegenstands (Trank) an.
    /// </summary>
    public void UsePotion(ItemData item)
    {
        if (item == null) return;

        Debug.Log($"<color=green>PlayerStats:</color> Benutze Gegenstand {item.itemName}");

        // Beispiel-Logik für Heilung:
        // Wir suchen im Item-Namen nach "Health" oder "Mana"
        if (item.itemName.Contains("Health"))
        {
            Heal(20f); // Heilt 20 Punkte
        }
        else if (item.itemName.Contains("Mana"))
        {
            RestoreMana(15f); // Stellt 15 Mana wieder her
        }

        OnStatsChanged?.Invoke();
    }
    // --------------------------------------------------

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
    
    public void SetActiveWeapon(ItemData weapon)
    {
        if (weapon != null && weapon.itemType == ItemType.Weapon && weapon.stats != null)
        {
            activeWeaponBonus = weapon.stats;
            Debug.Log($"Active weapon set: {weapon.itemName}");
        }
        else
        {
            activeWeaponBonus = new ItemStats(); 
            Debug.Log("No weapon active");
        }
        RecalculateStats();
    }
    
    public void UpdateEquipmentBonus(ItemStats newEquipmentBonus)
    {
        equipmentBonus = newEquipmentBonus;
        RecalculateStats();
    }
    
    public void RecalculateStats()
    {
        float levelMaxHealth = baseMaxHealth + (currentLevel * healthPerLevel);
        float levelMaxMana = baseMaxMana + (currentLevel * manaPerLevel);
        float levelMaxStamina = baseMaxStamina + (currentLevel * staminaPerLevel);
        float levelDamage = baseDamage + (currentLevel * damagePerLevel);
        float levelDefense = baseDefense + (currentLevel * defensePerLevel);
        float levelCritChance = baseCriticalChance + (currentLevel * criticalChancePerLevel);
        
        maxHealth = levelMaxHealth + equipmentBonus.bonusHealth;
        maxMana = levelMaxMana + equipmentBonus.bonusMana;
        maxStamina = levelMaxStamina + equipmentBonus.bonusStamina;
        totalDefense = levelDefense + equipmentBonus.bonusDefense;
        totalResistance = baseResistance + equipmentBonus.bonusResistance;
        totalEvasion = baseEvasion + equipmentBonus.bonusEvasion;
        
        totalDamage = levelDamage + equipmentBonus.bonusDamage + activeWeaponBonus.bonusDamage;
        totalAttackSpeed = baseAttackSpeed + equipmentBonus.bonusAttackSpeed + activeWeaponBonus.bonusAttackSpeed;
        totalCriticalChance = levelCritChance + equipmentBonus.bonusCritChance + activeWeaponBonus.bonusCritChance;
        totalCriticalDamage = baseCriticalDamage + equipmentBonus.bonusCritDamage + activeWeaponBonus.bonusCritDamage;
        
        totalHealthRegen = baseHealthRegenRate + equipmentBonus.bonusHealthRegen + activeWeaponBonus.bonusHealthRegen;
        totalManaRegen = baseManaRegenRate + equipmentBonus.bonusManaRegen + activeWeaponBonus.bonusManaRegen;
        totalStaminaRegen = baseStaminaRegenRate + equipmentBonus.bonusStaminaRegen + activeWeaponBonus.bonusStaminaRegen;
        
        if (activeWeaponBonus.damageMultiplier > 1f)
        {
            totalDamage *= activeWeaponBonus.damageMultiplier;
        }
        
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentMana = Mathf.Min(currentMana, maxMana);
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        
        OnStatsChanged?.Invoke();
    }
    
    private float CalculateEXPForNextLevel()
    {
        return Mathf.Round(experienceToNextLevel * Mathf.Pow(currentLevel, 1.2f));
    }
    
    public void GainExperience(int amount)
    {
        experiencePoints += amount;
        OnXPChanged?.Invoke();
        while (experiencePoints >= experienceRequired)
        {
            LevelUp();
        }
    }
    
    private void LevelUp()
    {
        int previousLevel = currentLevel;
        currentLevel++;
        experiencePoints -= (int)experienceRequired;
        experienceRequired = CalculateEXPForNextLevel();
        
        RecalculateStats();
        
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStaminaChanged?.Invoke();
        
        OnLevelUp?.Invoke(previousLevel, currentLevel);
    }
    
    public float GetFinalDamage() => totalDamage;
    public bool IsCriticalHit() => UnityEngine.Random.value < totalCriticalChance;
    public float GetCriticalDamage() => totalDamage * totalCriticalDamage;
    
    public void TakeDamage(float damage)
    {
        float finalDamage = Mathf.Max(damage - totalDefense, 0);
        currentHealth = Mathf.Max(currentHealth - finalDamage, 0);
        OnHealthChanged?.Invoke();
        
        if (currentHealth <= 0) Die();
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
        if (isDead) return;
        isDead = true;
        if (playerMovement != null)
        {
            playerMovement.ForceStop();
            playerMovement.movementLocked = true;
        }
        if (anim != null) anim.SetTrigger("Death");
        OnPlayerDeath?.Invoke();
        StartCoroutine(RespawnCoroutine());
    }
    
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(deathAnimationDuration + respawnDelay);
        Respawn();
    }
    
    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        if (anim != null)
        {
            anim.SetTrigger("Respawn");
            anim.ResetTrigger("Death");
        }
        if (playerMovement != null) playerMovement.movementLocked = false;
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStaminaChanged?.Invoke();
        OnPlayerRespawn?.Invoke();
    }

    public void LoadSaveData(float savedHealth, float savedMaxHealth, float savedMana, float savedMaxMana)
    {
        maxHealth = savedMaxHealth;
        maxMana = savedMaxMana;
        currentHealth = Mathf.Clamp(savedHealth, 0f, maxHealth);
        currentMana = Mathf.Clamp(savedMana, 0f, maxMana);
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStatsChanged?.Invoke();
    }
}