using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Zeigt "LEVEL UP!" Animation über dem Spieler an.
/// Kann entweder als World Space (am Player) oder Screen Space (Persistent Canvas) verwendet werden.
/// </summary>
public class LevelUpPopup : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private float floatUpDistance = 1.5f;
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private float fadeStartTime = 1f;
    
    [Header("Text Settings")]
    [SerializeField] private string levelUpText = "LEVEL UP!";
    [SerializeField] private Color textColor = Color.yellow;
    [SerializeField] private float fontSize = 36f;
    
    [Header("Mode")]
    [SerializeField] private PopupMode mode = PopupMode.WorldSpace;
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.5f, 0);
    
    // Für Screen Space Mode
    private Canvas persistentCanvas;
    private Camera mainCamera;
    
    // Referenz zum Spieler
    private PlayerStats playerStats;
    private Transform playerTransform;
    
    public enum PopupMode
    {
        WorldSpace,     // Text schwebt in der Spielwelt über dem Player
        ScreenSpace     // Text erscheint auf dem UI Canvas
    }
    
    private void Start()
    {
        // Finde Player
        FindPlayer();
        
        // Finde Persistent Canvas für Screen Space Mode
        if (mode == PopupMode.ScreenSpace)
        {
            persistentCanvas = GetComponentInParent<Canvas>();
            if (persistentCanvas == null)
            {
                persistentCanvas = FindFirstObjectByType<Canvas>();
            }
        }
        
        mainCamera = Camera.main;
    }
    
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            playerTransform = player.transform;
            
            if (playerStats != null)
            {
                // Subscribe zum Level Up Event
                playerStats.OnLevelUp += OnPlayerLevelUp;
            }
        }
    }
    
    private void OnPlayerLevelUp(int fromLevel, int toLevel)
    {
        ShowLevelUpPopup(fromLevel, toLevel);
    }
    
    /// <summary>
    /// Zeigt das Level Up Popup an.
    /// </summary>
    public void ShowLevelUpPopup(int fromLevel, int toLevel)
    {
        if (popupPrefab != null)
        {
            // Nutze Prefab
            SpawnPrefabPopup(fromLevel, toLevel);
        }
        else
        {
            // Erstelle dynamisch
            StartCoroutine(CreateAndAnimatePopup(fromLevel, toLevel));
        }
    }
    
    private void SpawnPrefabPopup(int fromLevel, int toLevel)
    {
        GameObject popup;
        
        if (mode == PopupMode.WorldSpace)
        {
            popup = Instantiate(popupPrefab, playerTransform.position + worldOffset, Quaternion.identity);
        }
        else
        {
            popup = Instantiate(popupPrefab, persistentCanvas.transform);
        }
        
        // Setze Text
        TextMeshProUGUI tmp = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = $"{levelUpText}\nLvl {fromLevel} → Lvl {toLevel}";
        }
        
        // Starte Animation
        StartCoroutine(AnimatePopup(popup, mode == PopupMode.WorldSpace));
    }
    
    private IEnumerator CreateAndAnimatePopup(int fromLevel, int toLevel)
    {
        // Erstelle GameObject
        GameObject popup = new GameObject("LevelUpPopup");
        
        if (mode == PopupMode.WorldSpace)
        {
            // World Space Setup
            popup.transform.position = playerTransform.position + worldOffset;
            
            // Canvas für World Space
            Canvas canvas = popup.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            
            // Skalierung für World Space
            popup.transform.localScale = Vector3.one * 0.02f;
        }
        else
        {
            // Screen Space Setup - als Child des Canvas
            popup.transform.SetParent(persistentCanvas.transform, false);
            
            // Position zur Spielerposition
            if (mainCamera != null && playerTransform != null)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(playerTransform.position + worldOffset);
                popup.GetComponent<RectTransform>().position = screenPos;
            }
        }
        
        // TextMeshPro hinzufügen
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(popup.transform, false);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = $"{levelUpText}\nLvl {fromLevel} → Lvl {toLevel}";
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        
        // RectTransform für Text
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(300, 100);
        textRect.anchoredPosition = Vector2.zero;
        
        // Animation starten
        yield return StartCoroutine(AnimatePopup(popup, mode == PopupMode.WorldSpace));
    }
    
    private IEnumerator AnimatePopup(GameObject popup, bool isWorldSpace)
    {
        if (popup == null) yield break;
        
        Vector3 startPos = popup.transform.position;
        TextMeshProUGUI tmp = popup.GetComponentInChildren<TextMeshProUGUI>();
        Color startColor = tmp != null ? tmp.color : Color.white;
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            
            // Float Up Animation
            if (isWorldSpace)
            {
                popup.transform.position = startPos + Vector3.up * (floatUpDistance * progress);
                
                // Billboard - immer zur Kamera schauen
                if (mainCamera != null)
                {
                    popup.transform.rotation = mainCamera.transform.rotation;
                }
            }
            else
            {
                // Screen Space - einfach nach oben bewegen
                RectTransform rect = popup.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition += Vector2.up * (floatUpDistance * 50f * Time.deltaTime);
                }
            }
            
            // Fade Out nach fadeStartTime
            if (elapsed > fadeStartTime && tmp != null)
            {
                float fadeProgress = (elapsed - fadeStartTime) / (animationDuration - fadeStartTime);
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - fadeProgress);
            }
            
            // Scale Punch am Anfang
            if (progress < 0.2f)
            {
                float scaleProgress = progress / 0.2f;
                float scale = 1f + Mathf.Sin(scaleProgress * Mathf.PI) * 0.3f;
                
                if (isWorldSpace)
                    popup.transform.localScale = Vector3.one * 0.02f * scale;
                else
                    popup.transform.localScale = Vector3.one * scale;
            }
            
            yield return null;
        }
        
        // Cleanup
        Destroy(popup);
    }
    
    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnLevelUp -= OnPlayerLevelUp;
        }
    }
    
    /// <summary>
    /// Für manuellen Test - zeigt Popup an.
    /// </summary>
    [ContextMenu("Test Level Up Popup")]
    public void TestPopup()
    {
        ShowLevelUpPopup(1, 2);
    }
}
