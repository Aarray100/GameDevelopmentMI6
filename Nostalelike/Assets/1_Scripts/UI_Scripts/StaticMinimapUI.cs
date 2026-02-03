using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Verwaltet die statische Minimap UI und zeigt die Position des Spielers auf der Karte an.
/// Dieses Script kommt auf den 'MinimapContainer' im Canvas.
/// </summary>
public class StaticMinimapUI : MonoBehaviour
{
    [Header("UI Referenzen")]
    [Tooltip("Das Image, das die Karte anzeigt (muss mittigen Pivot haben: 0.5, 0.5!)")]
    public Image mapDisplayImage;

    [Tooltip("Das kleine Icon für den Spieler")]
    public RectTransform playerIconRect;

    [Header("Spieler Referenz")]
    [Tooltip("Wird automatisch gesucht, wenn leer (über Tag 'Player')")]
    public Transform playerTransform;

    [Header("Einstellungen")]
    [Tooltip("Taste zum Wechseln der Zoom-Stufe")]
    public KeyCode toggleKey = KeyCode.M;

    [Tooltip("Minimap beim Start anzeigen")]
    public bool showOnStart = true;

    [Tooltip("Player Icon rotieren basierend auf Spieler-Rotation")]
    public bool rotatePlayerIcon = true;

    [Header("Zoom-Einstellungen")]
    [Tooltip("Verschiedene Zoom-Stufen (0.5 = 50% der Karte sichtbar, 1.0 = ganze Karte)")]
    public float[] zoomLevels = new float[] { 0.5f, 1.0f };
    
    private int currentZoomIndex = 0; // Startet bei 50%

    private RectTransform mapRectTransform;
    private RectTransform containerRectTransform; // Das MinimapContainer selbst
    private bool isVisible;

    private void OnEnable()
    {
        // Registriert das Event: "Immer wenn eine Szene geladen wird, führe RefreshMapInfo aus"
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Sauber aufräumen
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Bei jedem Szenenwechsel die Map-Info aktualisieren
        RefreshMapInfo();

        // Spieler-Referenz erneut suchen (falls Spieler neu instanziiert wurde)
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                Debug.Log("StaticMinimapUI: Spieler nach Szenenwechsel gefunden: " + playerObj.name);
            }
        }
    }

    private void Start()
    {
        mapRectTransform = mapDisplayImage.GetComponent<RectTransform>();
        containerRectTransform = GetComponent<RectTransform>();

        // Sicherstellen, dass eine Mask vorhanden ist, damit die Karte nicht über die Grenzen hinausragt
        EnsureMaskComponent();

        // Spieler suchen, falls nicht zugewiesen
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                Debug.Log("StaticMinimapUI: Spieler gefunden: " + playerObj.name);
            }
            else
            {
                Debug.LogWarning("StaticMinimapUI: Kein Spieler mit Tag 'Player' gefunden!");
            }
        }

        // Map laden
        RefreshMapInfo();

        // Sichtbarkeit setzen
        isVisible = showOnStart;
        SetMinimapVisible(isVisible);
    }

    /// <summary>
    /// Wird aufgerufen, wenn eine neue Szene geladen wurde.
    /// Lädt die Map-Informationen für die aktuelle Szene.
    /// </summary>
    public void RefreshMapInfo()
    {
        if (MapSceneInfo.Current != null && MapSceneInfo.Current.mapSprite != null)
        {
            mapDisplayImage.sprite = MapSceneInfo.Current.mapSprite;
            mapDisplayImage.enabled = true;

            if (playerIconRect != null)
            {
                playerIconRect.gameObject.SetActive(true);
            }

            Debug.Log("StaticMinimapUI: Map-Sprite geladen für Szene: " + MapSceneInfo.Current.gameObject.scene.name);
        }
        else
        {
            // Keine Map Info in dieser Szene gefunden (z.B. Hauptmenü)
            Debug.Log("StaticMinimapUI: Diese Szene hat keine Map Info. Minimap wird ausgeblendet.");
            mapDisplayImage.enabled = false;

            if (playerIconRect != null)
            {
                playerIconRect.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // Toggle zwischen verschiedenen Zoom-Stufen
        if (Input.GetKeyDown(toggleKey))
        {
            CycleZoomLevel();
        }
    }

    /// <summary>
    /// Wechselt zur nächsten Zoom-Stufe
    /// </summary>
    private void CycleZoomLevel()
    {
        currentZoomIndex = (currentZoomIndex + 1) % zoomLevels.Length;
        Debug.Log($"Zoom-Stufe: {zoomLevels[currentZoomIndex] * 100}% der Karte sichtbar");
    }

    private void LateUpdate()
    {
        // Abbrechen, wenn wichtige Referenzen fehlen oder Map ausgeblendet ist
        if (playerTransform == null || MapSceneInfo.Current == null || !mapDisplayImage.enabled || !isVisible)
            return;

        UpdatePlayerIcon();
    }

    /// <summary>
    /// Aktualisiert die Position und Rotation des Spieler-Icons auf der Minimap
    /// </summary>
    private void UpdatePlayerIcon()
    {
        if (playerIconRect == null) return;

        Bounds worldBounds = MapSceneInfo.Current.WorldBounds;
        Vector3 playerPos = playerTransform.position;

        // 1. Relative Position (0.0 bis 1.0) in der Welt berechnen
        // Für 2D Top-Down: X (horizontal) und Y (vertikal) verwenden!
        float normalizedX = Mathf.InverseLerp(worldBounds.min.x, worldBounds.max.x, playerPos.x);
        float normalizedY = Mathf.InverseLerp(worldBounds.min.y, worldBounds.max.y, playerPos.y);

        float uiWidth = mapRectTransform.rect.width;
        float uiHeight = mapRectTransform.rect.height;

        // Hole aktuellen Zoom-Faktor
        float currentZoom = zoomLevels[currentZoomIndex];
        float scale = 1f / currentZoom;
        
        // Berechne wo der Spieler auf der skalierten Karte wäre (in UI-Koordinaten)
        // -0.5 bis +0.5 -> von -width/2 bis +width/2
        float playerOnMapX = (normalizedX - 0.5f) * uiWidth * scale;
        float playerOnMapY = (normalizedY - 0.5f) * uiHeight * scale;
        
        // Karte so verschieben, dass Spieler in der Mitte ist (invertiert)
        float desiredMapOffsetX = -playerOnMapX;
        float desiredMapOffsetY = -playerOnMapY;
        
        // Verhindere, dass die Karte über die Ränder hinausscrollt
        float maxOffsetX = (uiWidth * scale - uiWidth) / 2f;
        float maxOffsetY = (uiHeight * scale - uiHeight) / 2f;
        
        float actualMapOffsetX = Mathf.Clamp(desiredMapOffsetX, -maxOffsetX, maxOffsetX);
        float actualMapOffsetY = Mathf.Clamp(desiredMapOffsetY, -maxOffsetY, maxOffsetY);
        
        mapDisplayImage.rectTransform.anchoredPosition = new Vector2(actualMapOffsetX, actualMapOffsetY);
        mapDisplayImage.rectTransform.localScale = new Vector3(scale, scale, 1);
        
        // Spieler-Position relativ zum Container (nicht zur Karte!)
        // Spieler ist auf der Karte bei playerOnMapX/Y, Karte ist verschoben um actualMapOffset
        // Also ist Spieler im Container bei: playerOnMap + actualMapOffset
        float playerInContainerX = playerOnMapX + actualMapOffsetX;
        float playerInContainerY = playerOnMapY + actualMapOffsetY;
        
        playerIconRect.anchoredPosition = new Vector2(playerInContainerX, playerInContainerY);

        // Rotation des Spieler-Icons anpassen (optional)
        if (rotatePlayerIcon)
        {
            // Wir nehmen die Y-Rotation des Spielers und drehen das Icon negativ auf Z.
            // Für 2D Top-Down: Nutze Z-Rotation des Spielers direkt
            float playerRotY = playerTransform.eulerAngles.y;
            playerIconRect.localEulerAngles = new Vector3(0, 0, -playerRotY);
        }

        // Icon sicherheitshalber aktivieren und sichtbar machen
        if (!playerIconRect.gameObject.activeSelf)
        {
            playerIconRect.gameObject.SetActive(true);
        }
        
        // Stelle sicher, dass das Icon sichtbar ist (über der Karte)
        playerIconRect.SetAsLastSibling(); // Bringt es nach vorne
    }

    /// <summary>
    /// Schaltet die Minimap-Sichtbarkeit um
    /// </summary>
    public void ToggleMinimap()
    {
        isVisible = !isVisible;
        SetMinimapVisible(isVisible);
    }

    /// <summary>
    /// Setzt die Minimap-Sichtbarkeit
    /// </summary>
    public void SetMinimapVisible(bool visible)
    {
        isVisible = visible;

        if (mapDisplayImage != null)
        {
            mapDisplayImage.gameObject.SetActive(visible);
        }

        if (playerIconRect != null)
        {
            playerIconRect.gameObject.SetActive(visible && MapSceneInfo.Current != null);
        }
    }

    private void OnValidate()
    {
        // Überprüfe, ob das Map Display Image den richtigen Pivot hat
        if (mapDisplayImage != null)
        {
            RectTransform rect = mapDisplayImage.GetComponent<RectTransform>();
            if (rect != null && rect.pivot != new Vector2(0.5f, 0.5f))
            {
                Debug.LogWarning("StaticMinimapUI: Das Map Display Image sollte einen Pivot von (0.5, 0.5) haben!");
            }
        }
        
        // Prüfe, ob eine Mask vorhanden ist
        if (Application.isPlaying && GetComponent<UnityEngine.UI.Mask>() == null)
        {
            Debug.LogWarning("StaticMinimapUI: Keine Mask-Komponente gefunden! Die Karte wird über die Grenzen hinausragen.");
        }
    }

    /// <summary>
    /// Stellt sicher, dass eine Mask-Komponente vorhanden ist, damit die Karte nicht überläuft
    /// </summary>
    private void EnsureMaskComponent()
    {
        UnityEngine.UI.Mask mask = GetComponent<UnityEngine.UI.Mask>();
        
        if (mask == null)
        {
            mask = gameObject.AddComponent<UnityEngine.UI.Mask>();
            mask.showMaskGraphic = true; // Zeigt das Hintergrundbild (weiße Box)
            Debug.Log("StaticMinimapUI: Mask-Komponente automatisch hinzugefügt!");
        }

        // Sicherstellen, dass der Container ein Image hat (wird für die Mask benötigt)
        Image containerImage = GetComponent<Image>();
        if (containerImage == null)
        {
            containerImage = gameObject.AddComponent<Image>();
            containerImage.color = new Color(1, 1, 1, 0.3f); // Leicht transparentes Weiß als Hintergrund
            Debug.Log("StaticMinimapUI: Image-Komponente für Mask automatisch hinzugefügt!");
        }
        
        // Prüfe, ob das Spieler-Icon vorhanden ist
        if (playerIconRect == null)
        {
            Debug.LogError("StaticMinimapUI: Player Icon Rect ist nicht zugewiesen! Bitte im Inspector zuweisen.");
        }
        else if (playerIconRect.GetComponent<Image>() == null)
        {
            Debug.LogWarning("StaticMinimapUI: Player Icon hat kein Image! Füge ein Image mit einem Pfeil-Sprite hinzu.");
        }
    }
}
