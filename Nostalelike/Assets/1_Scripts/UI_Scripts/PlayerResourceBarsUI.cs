using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller für Health, Mana und XP Balken.
/// Nutzt Image.fillAmount für Pixel-Art kompatible Darstellung.
/// Dieses GameObject sollte ein Child des PersistentCanvas sein.
/// 
/// === UNITY SETUP ANLEITUNG ===
/// 
/// Hierarchie unter PersistentCanvas:
/// 
/// PlayerResourceBars (Empty GameObject + dieses Script)
/// ├── HealthBar (Empty GameObject)
/// │   ├── Background (Image) - Sprite: leerer Rahmen (z.B. Barsforhealthbar_26)
/// │   │   └── Image Type: Simple
/// │   ├── Fill (Image) - Sprite: gefüllter Balken (z.B. Barsforhealthbar_21)
/// │   │   └── Image Type: FILLED, Fill Method: Horizontal, Fill Origin: Left
/// │   └── HealthText (TextMeshPro - Text) - optional
/// ├── ManaBar (Empty GameObject)
/// │   ├── Background (Image)
/// │   ├── Fill (Image) - Image Type: FILLED
/// │   └── ManaText (TextMeshPro - Text) - optional
/// └── XPBar (Empty GameObject)
///     ├── Background (Image)
///     ├── Fill (Image) - Image Type: FILLED
///     ├── XPText (TextMeshPro - Text) - optional
///     └── LevelText (TextMeshPro - Text) - optional
/// 
/// WICHTIG für jedes Fill-Image:
/// 1. Image Type = FILLED
/// 2. Fill Method = Horizontal
/// 3. Fill Origin = Left
/// 4. Fill Amount wird vom Script gesteuert (0-1)
/// 
/// Komponenten-Typen:
/// - healthFill, manaFill, xpFill = Image (mit Image Type = Filled)
/// - healthText, manaText, xpText, levelText = TextMeshProUGUI
/// </summary>
public class PlayerResourceBarsUI : MonoBehaviour
{
    [Header("Health Bar")]
    [Tooltip("Typ: Image mit Image Type = Filled")]
    [SerializeField] private Image healthFill;
    [Tooltip("Typ: TextMeshProUGUI - Optional für '100/100' Anzeige")]
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Mana Bar")]
    [Tooltip("Typ: Image mit Image Type = Filled")]
    [SerializeField] private Image manaFill;
    [Tooltip("Typ: TextMeshProUGUI - Optional für '50/50' Anzeige")]
    [SerializeField] private TextMeshProUGUI manaText;
    
    [Header("XP Bar")]
    [Tooltip("Typ: Image mit Image Type = Filled")]
    [SerializeField] private Image xpFill;
    [Tooltip("Typ: TextMeshProUGUI - Optional für 'XP: 50/150' Anzeige")]
    [SerializeField] private TextMeshProUGUI xpText;
    [Tooltip("Typ: TextMeshProUGUI - Optional für 'Lvl 5' Anzeige")]
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Settings")]
    [SerializeField] private bool showText = true;
    [SerializeField] private float lowHealthThreshold = 0.25f; // 25% für Warnung
    
    [Header("Low Health Warning")]
    [SerializeField] private bool enableLowHealthPulse = true;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private Color normalHealthColor = Color.white;
    [SerializeField] private Color lowHealthColor = new Color(1f, 0.3f, 0.3f); // Rötlich
    private bool isLowHealth = false;
    
    [Header("Smooth Animation")]
    [SerializeField] private bool smoothFill = true;
    [SerializeField] private float fillSpeed = 5f;
    
    // Target values für smooth animation
    private float targetHealthFill = 1f;
    private float targetManaFill = 1f;
    private float targetXPFill = 0f;
    
    // Referenz zum Spieler
    private PlayerStats playerStats;
    private bool isSubscribed = false;
    
    private void Start()
    {
        // Finde PlayerStats
        FindAndSubscribeToPlayer();
    }
    
    private void Update()
    {
        // Versuche erneut zu subscriben wenn noch nicht verbunden
        if (!isSubscribed || playerStats == null)
        {
            FindAndSubscribeToPlayer();
        }
        
        // Smooth Fill Animation
        if (smoothFill)
        {
            SmoothUpdateFills();
        }
        
        // Low Health Puls-Effekt
        if (enableLowHealthPulse && isLowHealth && healthFill != null)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // 0-1
            healthFill.color = Color.Lerp(lowHealthColor, Color.white, pulse * 0.5f);
        }
        else if (healthFill != null && !isLowHealth)
        {
            healthFill.color = normalHealthColor;
        }
    }
    
    private void SmoothUpdateFills()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetHealthFill, Time.deltaTime * fillSpeed);
        }
        if (manaFill != null)
        {
            manaFill.fillAmount = Mathf.Lerp(manaFill.fillAmount, targetManaFill, Time.deltaTime * fillSpeed);
        }
        if (xpFill != null)
        {
            xpFill.fillAmount = Mathf.Lerp(xpFill.fillAmount, targetXPFill, Time.deltaTime * fillSpeed);
        }
    }
    
    private void OnEnable()
    {
        if (playerStats != null && !isSubscribed)
        {
            SubscribeToEvents();
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
                InitializeBars();
                Debug.Log("PlayerResourceBarsUI: Mit PlayerStats verbunden!");
            }
        }
    }
    
    private void SubscribeToEvents()
    {
        if (playerStats == null || isSubscribed) return;
        
        playerStats.OnHealthChanged += UpdateHealthBar;
        playerStats.OnManaChanged += UpdateManaBar;
        playerStats.OnXPChanged += UpdateXPBar;
        playerStats.OnStatsChanged += InitializeBars;
        playerStats.OnPlayerDeath += OnPlayerDeath;
        playerStats.OnPlayerRespawn += OnPlayerRespawn;
        playerStats.OnLevelUp += OnLevelUp;
        
        isSubscribed = true;
    }
    
    private void UnsubscribeFromEvents()
    {
        if (playerStats == null || !isSubscribed) return;
        
        playerStats.OnHealthChanged -= UpdateHealthBar;
        playerStats.OnManaChanged -= UpdateManaBar;
        playerStats.OnXPChanged -= UpdateXPBar;
        playerStats.OnStatsChanged -= InitializeBars;
        playerStats.OnPlayerDeath -= OnPlayerDeath;
        playerStats.OnPlayerRespawn -= OnPlayerRespawn;
        playerStats.OnLevelUp -= OnLevelUp;
        
        isSubscribed = false;
    }
    
    private void InitializeBars()
    {
        if (playerStats == null) return;
        
        UpdateHealthBar();
        UpdateManaBar();
        UpdateXPBar();
    }
    
    private void UpdateHealthBar()
    {
        if (playerStats == null) return;
        
        float healthPercent = playerStats.currentHealth / playerStats.maxHealth;
        targetHealthFill = healthPercent;
        
        // Sofort setzen wenn smooth deaktiviert
        if (!smoothFill && healthFill != null)
        {
            healthFill.fillAmount = healthPercent;
        }
        
        // Low Health Check
        isLowHealth = healthPercent <= lowHealthThreshold;
        
        // Text Update
        if (showText && healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(playerStats.currentHealth)}/{Mathf.CeilToInt(playerStats.maxHealth)}";
        }
    }
    
    private void UpdateManaBar()
    {
        if (playerStats == null) return;
        
        float manaPercent = playerStats.currentMana / playerStats.maxMana;
        targetManaFill = manaPercent;
        
        if (!smoothFill && manaFill != null)
        {
            manaFill.fillAmount = manaPercent;
        }
        
        if (showText && manaText != null)
        {
            manaText.text = $"{Mathf.CeilToInt(playerStats.currentMana)}/{Mathf.CeilToInt(playerStats.maxMana)}";
        }
    }
    
    private void UpdateXPBar()
    {
        if (playerStats == null) return;
        
        float xpPercent = playerStats.experiencePoints / playerStats.experienceRequired;
        targetXPFill = xpPercent;
        
        if (!smoothFill && xpFill != null)
        {
            xpFill.fillAmount = xpPercent;
        }
        
        if (showText && xpText != null)
        {
            xpText.text = $"{playerStats.experiencePoints}/{Mathf.CeilToInt(playerStats.experienceRequired)}";
        }
        
        if (levelText != null)
        {
            levelText.text = $"Lvl {playerStats.currentLevel}";
        }
    }
    
    private void OnLevelUp(int fromLevel, int toLevel)
    {
        // XP Bar zurücksetzen - sofort auf 0 für visuellen Reset
        if (xpFill != null)
        {
            xpFill.fillAmount = 0f;
        }
        targetXPFill = 0f;
        
        UpdateXPBar();
    }
    
    private void OnPlayerDeath()
    {
        Debug.Log("PlayerResourceBarsUI: Spieler ist gestorben!");
    }
    
    private void OnPlayerRespawn()
    {
        InitializeBars();
        isLowHealth = false;
        Debug.Log("PlayerResourceBarsUI: Spieler respawnt - Bars aktualisiert!");
    }
    
    public void Reconnect()
    {
        UnsubscribeFromEvents();
        playerStats = null;
        FindAndSubscribeToPlayer();
    }
}
