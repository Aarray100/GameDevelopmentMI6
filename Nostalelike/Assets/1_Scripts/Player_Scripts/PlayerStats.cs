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
    
    [Header("Death Settings")]
    public float deathAnimationDuration = 1.5f;
    public float respawnDelay = 2f;
    public bool isDead = false;
    
    // Referenzen
    private Animator anim;
    private PlayerMovement2D playerMovement;
    
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
        
        // Hole Referenzen
        anim = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement2D>();
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
        int previousLevel = currentLevel;
        currentLevel++;
        experiencePoints -= (int)experienceRequired;
        experienceRequired = CalculateEXPForNextLevel();
        
        // Stats neu berechnen (basierend auf neuem Level)
        RecalculateStats();
        
        // Full Heal beim Level Up
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        
        Debug.Log($"LEVEL UP! Level {previousLevel} -> Level {currentLevel}");
        
        // Event für UI/Effekte triggern
        OnLevelUp?.Invoke(previousLevel, currentLevel);
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
        if (isDead) return; // Verhindere mehrfaches Sterben
        
        isDead = true;
        Debug.Log("<color=red>Player died!</color>");
        
        // Bewegung stoppen und sperren
        if (playerMovement != null)
        {
            playerMovement.ForceStop();
            playerMovement.movementLocked = true;
        }
        
        // Death Sound abspielen (optional: eigener Player Death Sound)
        AudioManager.Instance?.PlayEnemyDeathSFX(); // TODO: Eigener PlayerDeathSFX
        
        // Death Animation abspielen
        if (anim != null)
        {
            anim.SetTrigger("Death");
        }
        
        // Event für UI/andere Systeme (z.B. Game Over Screen)
        OnPlayerDeath?.Invoke();
        
        // Starte Respawn Coroutine
        StartCoroutine(RespawnCoroutine());
    }
    
    private IEnumerator RespawnCoroutine()
    {
        // Warte auf Death-Animation
        yield return new WaitForSeconds(deathAnimationDuration);
        
        // Warte zusätzliche Zeit (für Game Over Screen etc.)
        yield return new WaitForSeconds(respawnDelay);
        
        // Respawn durchführen
        Respawn();
    }
    
    /// <summary>
    /// Respawnt den Spieler mit vollen HP/Mana.
    /// Kann auch extern aufgerufen werden (z.B. von UI Button).
    /// </summary>
    public void Respawn()
    {
        isDead = false;
        
        // Volle Ressourcen wiederherstellen
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        
        // Respawn Animation abspielen (falls vorhanden)
        if (anim != null)
        {
            anim.SetTrigger("Respawn"); // Optional: Respawn Animation
            anim.ResetTrigger("Death");
        }
        
        // Bewegung wieder freigeben
        if (playerMovement != null)
        {
            playerMovement.movementLocked = false;
        }
        
        // Events triggern
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStaminaChanged?.Invoke();
        OnPlayerRespawn?.Invoke();
        
        Debug.Log("<color=green>Player respawned!</color>");
    }
    
    /// <summary>
    /// Stoppt automatischen Respawn und wartet auf manuellen Aufruf.
    /// Nützlich für Game Over Screen mit "Restart" Button.
    /// </summary>
    public void CancelAutoRespawn()
    {
        StopAllCoroutines();
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
