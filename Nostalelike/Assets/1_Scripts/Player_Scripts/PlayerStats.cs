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

    [Header("Base Combat Stats")]
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
    public float totalCriticalChance;
    public float totalCriticalDamage = 1.5f;

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
    public event Action OnPlayerDeath;
    public event Action OnPlayerRespawn;
    public event Action<int, int> OnLevelUp;
    public event Action OnXPChanged;
    
    [Header("Death Settings")]
    public float deathAnimationDuration = 1.5f;
    public float respawnDelay = 2f;
    public bool isDead = false;
    
    // Referenzen
    private Animator anim;
    private PlayerMovement2D playerMovement;
    
    private void Awake()
    {
        movement = GetComponent<PlayerMovement2D>();
        playerMovement = movement;
        anim = GetComponentInChildren<Animator>();
        
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
        int previousLevel = currentLevel;
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
        
        OnLevelUp?.Invoke(previousLevel, currentLevel);
    }
    
    // --- XP SYSTEM ---
    
    /// <summary>
    /// Berechnet benötigte XP für nächstes Level (sanfte Kurve)
    /// </summary>
    private int CalculateExperienceRequired()
    {
        return Mathf.RoundToInt(experienceToNextLevel * Mathf.Pow(currentLevel, 1.2f));
    }
    
    /// <summary>
    /// Fügt Experience Points hinzu und prüft auf Level Up
    /// </summary>
    public void GainExperience(int amount)
    {
        experiencePoints += amount;
        int experienceRequired = CalculateExperienceRequired();
        Debug.Log($"Gained {amount} EXP. Total: {experiencePoints}/{experienceRequired}");
        
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
        experiencePoints = 0;
        experienceToNextLevel = CalculateExperienceRequired();
        RecalculateStats();
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStaminaChanged?.Invoke();
        
        Debug.Log($"LEVEL UP! Level {previousLevel} -> Level {currentLevel}. HP: {currentHealth}/{maxHealth}");
        
        OnLevelUp?.Invoke(previousLevel, currentLevel);
    }
    
    // --- COMBAT SYSTEM ---
    
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
        if (isDead) return;
        
        float finalDamage = Mathf.Max(damage - totalDefense, 0);
        currentHealth = Mathf.Max(currentHealth - finalDamage, 0);
        OnHealthChanged?.Invoke();
        
        if (currentHealth <= 0)
        {
            Die();
        }
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
    
    // --- DEATH & RESPAWN SYSTEM ---
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("<color=red>Player died!</color>");
        
        if (playerMovement != null)
        {
            playerMovement.ForceStop();
            playerMovement.movementLocked = true;
        }
        
        AudioManager.Instance?.PlayEnemyDeathSFX();
        
        if (anim != null)
        {
            anim.SetTrigger("Death");
        }
        
        OnPlayerDeath?.Invoke();
        
        StartCoroutine(RespawnCoroutine());
    }
    
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        yield return new WaitForSeconds(respawnDelay);
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
        
        if (playerMovement != null)
        {
            playerMovement.movementLocked = false;
        }
        
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStaminaChanged?.Invoke();
        OnPlayerRespawn?.Invoke();
        
        Debug.Log("<color=green>Player respawned!</color>");
    }
    
    public void CancelAutoRespawn()
    {
        StopAllCoroutines();
    }

    // --- STAT CALCULATION ---

    public void RecalculateStats()
    {
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

    public void LoadSaveData(float savedHealth, float savedMaxHealth, float savedMana, float savedMaxMana)
    {
        maxHealth = savedMaxHealth;
        maxMana = savedMaxMana;
        
        currentHealth = Mathf.Clamp(savedHealth, 0f, maxHealth);
        currentMana = Mathf.Clamp(savedMana, 0f, maxMana);
        
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStatsChanged?.Invoke();
        
        Debug.Log($"PlayerStats loaded: HP {currentHealth}/{maxHealth}, Mana {currentMana}/{maxMana}");
    }

    #endregion
}
