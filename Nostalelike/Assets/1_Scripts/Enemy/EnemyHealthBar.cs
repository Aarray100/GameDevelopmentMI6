using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Synchronisiert die Health Bar UI mit den Enemy Stats.
/// Füge dieses Script auf das Enemy-Prefab (Root) hinzu, nicht auf das Canvas!
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Der Slider für die Health Bar")]
    public Slider healthSlider;
    
    [Tooltip("Text für die Level-Anzeige (optional)")]
    public TextMeshProUGUI levelText;
    
    [Header("Settings")]
    [Tooltip("Soll die Health Bar immer zur Kamera schauen?")]
    public bool billboardEffect = true;
    
    [Tooltip("Health Bar erst anzeigen wenn Schaden genommen wurde?")]
    public bool hideWhenFull = false;
    
    [Tooltip("Health Bar nach X Sekunden ohne Schaden ausblenden (0 = nie)")]
    public float hideDelay = 0f;
    
    // Referenzen
    private EnemyHealth enemyHealth;
    private EnemyStats enemyStats;
    private Canvas canvas;
    private Camera mainCamera;
    
    // Interne Variablen
    private float maxHealth;
    private float lastDamageTime;
    private bool hasTakenDamage = false;

    private void Awake()
    {
        // Komponenten finden
        enemyHealth = GetComponent<EnemyHealth>();
        enemyStats = GetComponent<EnemyStats>();
        canvas = GetComponentInChildren<Canvas>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        // Events abonnieren
        if (enemyStats != null)
        {
            enemyStats.OnStatsCalculated += OnStatsUpdated;
            // Falls Stats bereits berechnet wurden
            OnStatsUpdated();
        }
        else if (enemyHealth != null)
        {
            // Fallback: Nutze EnemyHealth direkt
            maxHealth = enemyHealth.maxHealth;
            UpdateHealthBar(maxHealth);
        }
        
        // Initial verstecken wenn gewünscht
        if (hideWhenFull && canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (enemyStats != null)
        {
            enemyStats.OnStatsCalculated -= OnStatsUpdated;
        }
    }

    private void Update()
    {
        // Billboard-Effekt: Canvas schaut immer zur Kamera
        if (billboardEffect && canvas != null && mainCamera != null)
        {
            canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.forward);
        }
        
        // Auto-Hide nach Delay
        if (hideDelay > 0 && hasTakenDamage && canvas != null)
        {
            if (Time.time - lastDamageTime > hideDelay)
            {
                canvas.gameObject.SetActive(false);
            }
        }
    }

    private void LateUpdate()
    {
        // Health Bar aktualisieren
        if (enemyHealth != null)
        {
            UpdateHealthBar(GetCurrentHealth());
        }
    }

    /// <summary>
    /// Wird aufgerufen wenn EnemyStats die Werte berechnet hat.
    /// </summary>
    private void OnStatsUpdated()
    {
        if (enemyStats != null)
        {
            maxHealth = enemyStats.MaxHealth;
            
            // Level Text aktualisieren
            if (levelText != null)
            {
                levelText.text = $"Lv.{enemyStats.Level}";
            }
        }
        
        UpdateHealthBar(maxHealth);
    }

    /// <summary>
    /// Aktualisiert die Health Bar Anzeige.
    /// </summary>
    private void UpdateHealthBar(float currentHealth)
    {
        if (healthSlider == null || maxHealth <= 0) return;
        
        float healthPercent = currentHealth / maxHealth;
        healthSlider.value = healthPercent;
        
        // Zeige Health Bar wenn Schaden genommen wurde
        if (healthPercent < 1f && !hasTakenDamage)
        {
            hasTakenDamage = true;
            lastDamageTime = Time.time;
            
            if (hideWhenFull && canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }
        }
        
        // Update last damage time wenn HP sich ändert
        if (healthPercent < 1f)
        {
            lastDamageTime = Time.time;
        }
    }

    /// <summary>
    /// Holt die aktuelle HP vom Enemy.
    /// </summary>
    private float GetCurrentHealth()
    {
        if (enemyHealth != null)
        {
            return enemyHealth.CurrentHealth;
        }
        return maxHealth;
    }
}
