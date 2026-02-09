using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    // UI Dummies (Damit die Anzeigen voll bleiben)
    [HideInInspector] public float maxMana = 100f;
    [HideInInspector] public float currentMana = 100f;
    [HideInInspector] public float maxStamina = 100f;
    [HideInInspector] public float currentStamina = 100f;

    [Header("Offensive Stats (Calculated)")]
    public float baseDamage = 10f;
    public float totalDamage;
    public float totalAttackSpeed = 1f;
    public float totalCriticalChance = 0f;
    public float totalCriticalDamage = 1.5f;
    
    [Header("Defensive Stats (Calculated)")]
    public float totalDefense = 0f;
    public float totalResistance = 0f;
    public float totalEvasion = 0f;
    
    [Header("Regen Stats (Calculated)")]
    public float totalHealthRegen = 1f;
    public float totalManaRegen = 0f;  
    public float totalStaminaRegen = 0f;

    [Header("Level System")]
    public int currentLevel = 1;
    public int experiencePoints = 0;
    public float experienceRequired = 100f; 
    public int experienceToNextLevel = 100; 

    private float temporaryDamageMultiplier = 1.0f;
    private ItemStats equipmentBonus = new ItemStats();
    private ItemStats activeWeaponBonus = new ItemStats();

    // EVENTS
    public event Action OnStatsChanged;
    public event Action OnHealthChanged;
    public event Action OnManaChanged;    
    public event Action OnStaminaChanged; 
    public event Action OnXPChanged;      
    public event Action<int, int> OnLevelUp; 
    public event Action OnPlayerDeath;
    public event Action OnPlayerRespawn;

    private PlayerMovement2D playerMovement;
    private Animator anim;
    private Coroutine regenCoroutine;
    
    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement2D>();
        anim = GetComponentInChildren<Animator>();
        experienceRequired = experienceToNextLevel;
        RecalculateStats();
        
        // Alles voll machen zum Start
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnStaminaChanged?.Invoke();
        OnXPChanged?.Invoke();
        
        // Starte Health Regeneration
        regenCoroutine = StartCoroutine(HealthRegeneration());
    }

    private IEnumerator HealthRegeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // Alle 2 Sekunden
            
            if (currentHealth < maxHealth && currentHealth > 0)
            {
                currentHealth = Mathf.Min(currentHealth + totalHealthRegen, maxHealth);
                OnHealthChanged?.Invoke();
            }
        }
    }

    public void UpdateEquipmentBonus(ItemStats newBonus)
    {
        equipmentBonus = newBonus;
        RecalculateStats();
    }

    public void SetActiveWeapon(ItemData weapon)
    {
        if (weapon != null && weapon.stats != null)
            activeWeaponBonus = weapon.stats;
        else
            activeWeaponBonus = new ItemStats(); 
        RecalculateStats();
    }

    // --- TRANK LOGIK ---
    public void ForceLevelUp()
    {
        experiencePoints = (int)experienceRequired;
        LevelUp();
    }

    public void ApplyStrengthBuff(float multiplier, float duration)
    {
        StartCoroutine(StrengthBuffCoroutine(multiplier, duration));
    }

    private IEnumerator StrengthBuffCoroutine(float multiplier, float duration)
    {
        temporaryDamageMultiplier = multiplier; 
        RecalculateStats(); 
        yield return new WaitForSeconds(duration);
        temporaryDamageMultiplier = 1.0f; 
        RecalculateStats();
    }

    public void ApplySpeedBuff(float multiplier, float duration)
    {
        // NUTZT JETZT DEINE FUNKTION AUS PlayerMovement2D
        if (playerMovement != null)
        {
            playerMovement.ApplySpeedBoost(multiplier, duration);
        }
    }

    // --- BERECHNUNGEN ---
    public void RecalculateStats()
    {
        float levelDamage = baseDamage + (currentLevel * 2f);
        totalDamage = (levelDamage + equipmentBonus.bonusDamage + activeWeaponBonus.bonusDamage) * temporaryDamageMultiplier;
        
        totalAttackSpeed = 1f + equipmentBonus.bonusAttackSpeed + activeWeaponBonus.bonusAttackSpeed;
        totalCriticalChance = equipmentBonus.bonusCritChance + activeWeaponBonus.bonusCritChance;
        totalCriticalDamage = 1.5f + equipmentBonus.bonusCritDamage + activeWeaponBonus.bonusCritDamage;

        totalDefense = equipmentBonus.bonusDefense;
        totalResistance = equipmentBonus.bonusResistance;
        totalEvasion = equipmentBonus.bonusEvasion;

        maxHealth = 100 + (currentLevel * 10) + equipmentBonus.bonusHealth;
        totalHealthRegen = 1f + equipmentBonus.bonusHealthRegen;

        // Fix für die 0/0 Anzeigen
        maxMana = 100f;
        currentMana = 100f;
        maxStamina = 100f;
        currentStamina = 100f;

        if (currentHealth > maxHealth) currentHealth = maxHealth;
        
        OnStatsChanged?.Invoke();
        OnManaChanged?.Invoke();    
        OnStaminaChanged?.Invoke(); 
    }
    
    private void LevelUp()
    {
        currentLevel++;
        experiencePoints = 0;
        experienceToNextLevel = (int)(experienceToNextLevel * 1.5f);
        experienceRequired = experienceToNextLevel; 

        currentHealth = maxHealth; 
        OnHealthChanged?.Invoke();
        OnLevelUp?.Invoke(currentLevel - 1, currentLevel);
        OnXPChanged?.Invoke(); 
        RecalculateStats();
    }

    public void GainExperience(int amount)
    {
        experiencePoints += amount;
        OnXPChanged?.Invoke(); 
        if (experiencePoints >= experienceRequired)
        {
            LevelUp();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(float damage)
    {
        float damageAfterDef = Mathf.Max(damage - totalDefense, 1); 
        currentHealth -= damageAfterDef;
        OnHealthChanged?.Invoke();
        if (currentHealth <= 0) Die();
    }
    
    private void Die()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
        
        // Trigger Death Animation
        if (anim != null)
        {
            Debug.Log("PlayerStats: Triggering Death animation");
            anim.SetTrigger("Death");
        }
        else
        {
            Debug.LogWarning("PlayerStats: Animator not found!");
        }
        
        // Deaktiviere Movement während Tod
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        // Event ZUERST auslösen (bevor Time.timeScale = 0)
        OnPlayerDeath?.Invoke();
        
        // Warte einen Frame damit UI sich aktualisiert
        yield return null;
        
        // JETZT pausieren
        Time.timeScale = 0f;
        
        // 3 Sekunden warten (Realtime, da Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(3f);
        
        // Spiel fortsetzen
        Time.timeScale = 1f;
        Respawn();
    }

    private void Respawn()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
        OnPlayerRespawn?.Invoke();
        
        // Reaktiviere Movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        // Starte Health Regeneration wieder
        regenCoroutine = StartCoroutine(HealthRegeneration());

        // Prüfe ob wir bereits in 002_HomeScene sind
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != "002_HomeScene")
        {
            // Setze den Spawn-Punkt VOR dem Scene-Load
            SceneTransitionManager manager = SceneTransitionManager.EnsureInstance();
            if (manager != null)
            {
                manager.targetSpawnPointID = "deathSpawn";
                Debug.Log("PlayerStats: Set targetSpawnPointID to 'deathSpawn' before scene load");
            }
            
            // JETZT lade die Scene
            if (LoadingScreen.Instance != null)
                LoadingScreen.Instance.LoadSceneWithScreen("002_HomeScene", "deathSpawn");
            else
                SceneManager.LoadScene("002_HomeScene");
        }
        else
        {
            // Wir sind schon in HomeScene, teleportiere direkt
            PlayerSceneHandler handler = GetComponent<PlayerSceneHandler>();
            if (handler != null)
            {
                handler.TeleportToSpawnPoint("deathSpawn");
            }
        }
        
        Debug.Log("Player respawned at 'deathSpawn' in 002_HomeScene.");
    }

    public void RestoreMana(float amount) { OnManaChanged?.Invoke(); }
    public void RestoreStamina(float amount) { OnStaminaChanged?.Invoke(); }
    public void UseMana(float amount) { } 
    public void UseStamina(float amount) { } 

    public void LoadSaveData(float savedHealth, float savedMaxHealth, float savedMana, float savedMaxMana)
    {
        maxHealth = savedMaxHealth > 0 ? savedMaxHealth : 100f;
        currentHealth = (savedHealth <= 1) ? maxHealth : savedHealth;
        OnHealthChanged?.Invoke();
        RecalculateStats();
    }
}