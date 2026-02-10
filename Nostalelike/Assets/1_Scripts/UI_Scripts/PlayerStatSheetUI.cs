using UnityEngine;
using TMPro;

/// <summary>
/// UI Panel das alle aktuellen Player Stats anzeigt (inkl. Equipment Boni).
/// Platziere dieses Script auf einem Panel rechts vom Inventar.
/// 
/// === UNITY SETUP ===
/// 
/// StatSheet (Panel mit diesem Script)
/// ├── Header (TextMeshPro) - "STATS"
/// ├── OffensiveStats (Empty)
/// │   ├── DamageText (TextMeshPro)
/// │   ├── AttackSpeedText (TextMeshPro)
/// │   ├── CritChanceText (TextMeshPro)
/// │   └── CritDamageText (TextMeshPro)
/// ├── DefensiveStats (Empty)
/// │   ├── DefenseText (TextMeshPro)
/// │   ├── ResistanceText (TextMeshPro)
/// │   └── EvasionText (TextMeshPro)
/// └── RegenStats (Empty)
///     ├── HealthRegenText (TextMeshPro)
///     ├── ManaRegenText (TextMeshPro)
///     └── StaminaRegenText (TextMeshPro)
/// </summary>
public class PlayerStatSheetUI : MonoBehaviour
{
    [Header("Offensive Stats")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI critChanceText;
    [SerializeField] private TextMeshProUGUI critDamageText;
    
    [Header("Defensive Stats")]
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI resistanceText;
    [SerializeField] private TextMeshProUGUI evasionText;
    
    [Header("Regeneration Stats")]
    [SerializeField] private TextMeshProUGUI healthRegenText;
    [SerializeField] private TextMeshProUGUI manaRegenText;
    
    [Header("Resource Stats")]
    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private TextMeshProUGUI maxManaText;
    
    [Header("Settings")]
    [SerializeField] private bool showLabels = true;
    
    private PlayerStats playerStats;
    private bool isSubscribed = false;
    
    private void Start()
    {
        FindAndSubscribeToPlayer();
    }
    
    private void Update()
    {
        if (!isSubscribed || playerStats == null)
        {
            FindAndSubscribeToPlayer();
        }
    }
    
    private void OnEnable()
    {
        if (playerStats != null && !isSubscribed)
        {
            SubscribeToEvents();
            UpdateAllStats();
        }
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void FindAndSubscribeToPlayer()
    {
        if (isSubscribed && playerStats != null) return;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                SubscribeToEvents();
                UpdateAllStats();
                Debug.Log("PlayerStatSheetUI: Mit PlayerStats verbunden!");
            }
        }
    }
    
    private void SubscribeToEvents()
    {
        if (playerStats == null || isSubscribed) return;
        
        playerStats.OnStatsChanged += UpdateAllStats;
        isSubscribed = true;
    }
    
    private void UnsubscribeFromEvents()
    {
        if (playerStats == null || !isSubscribed) return;
        
        playerStats.OnStatsChanged -= UpdateAllStats;
        isSubscribed = false;
    }
    
    private void UpdateAllStats()
    {
        if (playerStats == null) return;
        
        // Offensive Stats
        SetStatText(damageText, "DMG", playerStats.totalDamage, 1);
        SetStatText(attackSpeedText, "ATK SPD", playerStats.totalAttackSpeed, 2);
        SetStatText(critChanceText, "CRIT %", playerStats.totalCriticalChance * 100f, 1, "%");
        SetStatText(critDamageText, "CRIT DMG", playerStats.totalCriticalDamage * 100f, 0, "%");
        
        // Defensive Stats
        SetStatText(defenseText, "DEF", playerStats.totalDefense, 1);
        SetStatText(resistanceText, "RES", playerStats.totalResistance, 1);
        SetStatText(evasionText, "EVA", playerStats.totalEvasion * 100f, 1, "%");
        
        // Regeneration Stats
        SetStatText(healthRegenText, "HP/s", playerStats.totalHealthRegen, 1);
        SetStatText(manaRegenText, "MP/s", playerStats.totalManaRegen, 1);
        
        // Resource Stats
        SetStatText(maxHealthText, "MAX HP", playerStats.maxHealth, 0);
        SetStatText(maxManaText, "MAX MP", playerStats.maxMana, 0);
    }
    
    private void SetStatText(TextMeshProUGUI textField, string label, float value, int decimals, string suffix = "")
    {
        if (textField == null) return;
        
        string valueStr = decimals > 0 ? value.ToString($"F{decimals}") : Mathf.RoundToInt(value).ToString();
        
        if (showLabels)
        {
            textField.text = $"{label}: {valueStr}{suffix}";
        }
        else
        {
            textField.text = $"{valueStr}{suffix}";
        }
    }
    
    /// <summary>
    /// Manuelles Update erzwingen (z.B. nach Equipment-Wechsel)
    /// </summary>
    public void ForceUpdate()
    {
        UpdateAllStats();
    }
    
    /// <summary>
    /// Reconnect nach Szenenwechsel
    /// </summary>
    public void Reconnect()
    {
        UnsubscribeFromEvents();
        playerStats = null;
        FindAndSubscribeToPlayer();
    }
}
