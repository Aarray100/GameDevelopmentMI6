using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Loading Screen - wird bei Scene-Wechsel angezeigt.
/// Kann auf jedes GameObject gelegt werden. Erstellt bei Bedarf automatisch einen eigenen
/// fullscreen Canvas als Overlay. Braucht kein manuelles UI-Setup.
/// Optional: loadingPanel, progressBar, loadingText, tipText im Inspector zuweisen für custom UI.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("UI Elements (Optional - wird automatisch erstellt wenn leer)")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float minimumDisplayTime = 1f;

    [Header("Loading Tips")]
    [SerializeField] private string[] loadingTips = new string[]
    {
        "Tipp: Sammle Münzen für bessere Ausrüstung!",
        "Tipp: Vergiss nicht regelmäßig zu speichern!",
        "Tipp: Erkunde jeden Winkel für versteckte Schätze.",
        "Tipp: Truhen bleiben nach dem Öffnen für immer geöffnet.",
        "Tipp: Im Journal findest du wichtige Informationen.",
        "Tipp: Verschiedene Gegner haben verschiedene Schwächen.",
        "Tipp: Tränke können dir im Kampf das Leben retten!"
    };

    private CanvasGroup canvasGroup;
    private bool isLoading = false;
    private float loadStartTime;
    
    // Eigener Canvas der vom Script erstellt wird (DontDestroyOnLoad-sicher)
    private static GameObject persistentLoadingCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); // Nur die Komponente zerstören, NICHT das GameObject
            return;
        }

        Instance = this;
        
        // NICHT SetParent(null) oder DontDestroyOnLoad auf dem Host-GameObject!
        // Stattdessen erstellen wir einen eigenen Canvas wenn nötig.
        EnsureLoadingUI();

        Debug.Log("<color=cyan>LoadingScreen: Initialized</color>");
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Erstellt automatisch einen eigenen Loading-Canvas wenn kein loadingPanel zugewiesen ist.
    /// </summary>
    private void EnsureLoadingUI()
    {
        // Wenn bereits ein persistenter Canvas existiert, nutze ihn
        if (persistentLoadingCanvas != null)
        {
            loadingPanel = persistentLoadingCanvas.transform.GetChild(0).gameObject;
            canvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            
            // Text-Referenzen wiederherstellen
            TextMeshProUGUI[] texts = loadingPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts.Length > 0) loadingText = texts[0];
            if (texts.Length > 1) tipText = texts[1];
            progressBar = loadingPanel.GetComponentInChildren<Slider>(true);
            
            loadingPanel.SetActive(false);
            return;
        }
        
        // Wenn ein loadingPanel manuell zugewiesen ist UND es ein separates Objekt ist
        // (nicht das GameObject auf dem dieses Script liegt), nutze es
        if (loadingPanel != null && loadingPanel != gameObject)
        {
            canvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = loadingPanel.AddComponent<CanvasGroup>();
            loadingPanel.SetActive(false);
            return;
        }
        
        // Erstelle einen eigenen Canvas (DontDestroyOnLoad-sicher)
        CreateOwnLoadingUI();
    }
    
    private void CreateOwnLoadingUI()
    {
        // Root-GameObject für den Loading Canvas
        persistentLoadingCanvas = new GameObject("LoadingScreenCanvas");
        DontDestroyOnLoad(persistentLoadingCanvas);
        
        // Canvas Setup
        Canvas canvas = persistentLoadingCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Immer ganz oben
        
        CanvasScaler scaler = persistentLoadingCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        persistentLoadingCanvas.AddComponent<GraphicRaycaster>();
        
        // Panel (dunkler Hintergrund)
        GameObject panel = new GameObject("LoadingPanel");
        panel.transform.SetParent(persistentLoadingCanvas.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.1f, 0.95f); // Dunkler Hintergrund
        
        canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        
        loadingPanel = panel;
        
        // Loading Text
        GameObject textObj = new GameObject("LoadingText");
        textObj.transform.SetParent(panel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.3f, 0.45f);
        textRect.anchorMax = new Vector2(0.7f, 0.55f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        loadingText = textObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Lade...";
        loadingText.fontSize = 36;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.color = Color.white;
        
        // Tip Text
        GameObject tipObj = new GameObject("TipText");
        tipObj.transform.SetParent(panel.transform, false);
        
        RectTransform tipRect = tipObj.AddComponent<RectTransform>();
        tipRect.anchorMin = new Vector2(0.15f, 0.2f);
        tipRect.anchorMax = new Vector2(0.85f, 0.3f);
        tipRect.offsetMin = Vector2.zero;
        tipRect.offsetMax = Vector2.zero;
        
        tipText = tipObj.AddComponent<TextMeshProUGUI>();
        tipText.text = "";
        tipText.fontSize = 22;
        tipText.alignment = TextAlignmentOptions.Center;
        tipText.color = new Color(0.8f, 0.8f, 0.6f, 1f);
        
        // Progress Bar
        GameObject sliderObj = CreateProgressBar(panel.transform);
        progressBar = sliderObj.GetComponent<Slider>();
        
        // Start: versteckt
        panel.SetActive(false);
        
        Debug.Log("<color=cyan>LoadingScreen: Eigener Canvas automatisch erstellt</color>");
    }
    
    private GameObject CreateProgressBar(Transform parent)
    {
        // Slider Root
        GameObject sliderObj = new GameObject("ProgressBar");
        sliderObj.transform.SetParent(parent, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.25f, 0.35f);
        sliderRect.anchorMax = new Vector2(0.75f, 0.4f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.7f, 1f, 1f); // Blauer Balken
        
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = false;
        
        return sliderObj;
    }

    /// <summary>
    /// Zeigt den Loading Screen an.
    /// </summary>
    public void Show(string message = "Lade...")
    {
        if (isLoading) return;

        isLoading = true;
        loadStartTime = Time.realtimeSinceStartup;
        
        // Spieler-Bewegung sperren
        LockPlayerMovement(true);

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingText != null)
            loadingText.text = message;

        if (tipText != null && loadingTips.Length > 0)
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];

        if (progressBar != null)
            progressBar.value = 0f;

        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Versteckt den Loading Screen mit Fade-Out.
    /// </summary>
    public void Hide()
    {
        if (!isLoading) return;
        StartCoroutine(HideCoroutine());
    }

    /// <summary>
    /// Aktualisiert die Progress Bar und optional den Text.
    /// </summary>
    public void UpdateProgress(float progress, string message = null)
    {
        if (progressBar != null)
            progressBar.value = Mathf.Clamp01(progress);

        if (message != null && loadingText != null)
            loadingText.text = message;
    }

    /// <summary>
    /// Lädt eine Szene per Build-Index asynchron mit Loading Screen.
    /// </summary>
    public void LoadSceneWithScreen(int sceneBuildIndex)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneByIndexCoroutine(sceneBuildIndex));
    }

    /// <summary>
    /// Lädt eine Szene asynchron mit Loading Screen.
    /// Optionaler spawnPointID wird an den SceneTransitionManager weitergegeben.
    /// </summary>
    public void LoadSceneWithScreen(string sceneName, string spawnPointID = "")
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneCoroutine(sceneName, spawnPointID));
    }

    /// <summary>
    /// Zeigt Loading Screen für Teleport innerhalb der gleichen Scene
    /// </summary>
    public void TeleportWithScreen(string spawnPointID)
    {
        if (isLoading) return;
        StartCoroutine(LocalTeleportCoroutine(spawnPointID));
    }

    private IEnumerator LocalTeleportCoroutine(string spawnPointID)
    {
        Debug.Log($"<color=cyan>LoadingScreen: LocalTeleportCoroutine started for '{spawnPointID}'</color>");
        Show("Teleportiere...");

        // Teleport Sound abspielen
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTeleportSFX();

        // Kurze Pause für visuellen Effekt
        yield return new WaitForSecondsRealtime(0.3f);
        UpdateProgress(0.5f, "Bereite Zielort vor...");

        // Finde und teleportiere zum Spawn-Punkt
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"<color=cyan>LoadingScreen: Player gefunden? {player != null}</color>");

        if (player != null)
        {
            // Direkt zum Spawn-Punkt teleportieren
            SceneSpawnPoint[] spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsSortMode.None);
            Debug.Log($"<color=cyan>LoadingScreen: Gefunden {spawnPoints.Length} Spawn-Points</color>");

            bool teleported = false;
            foreach (SceneSpawnPoint sp in spawnPoints)
            {
                if (sp.spawnPointID == spawnPointID)
                {
                    player.transform.position = sp.transform.position;
                    Debug.Log($"<color=green>LoadingScreen: Teleport successful to '{spawnPointID}' at {sp.transform.position}</color>");
                    teleported = true;
                    break;
                }
            }

            if (!teleported)
            {
                Debug.LogError($"<color=red>LoadingScreen: Spawn-Point '{spawnPointID}' not found!</color>");
            }
        }
        else
        {
            Debug.LogError($"<color=red>LoadingScreen: Player GameObject not found! Cannot teleport.</color>");
        }

        UpdateProgress(1f, "Bereit!");
        yield return new WaitForSecondsRealtime(0.2f);

        Debug.Log($"<color=cyan>LoadingScreen: Calling Hide()</color>");
        Hide();

        Debug.Log($"<color=cyan>LoadingScreen: LocalTeleportCoroutine finished</color>");
    }

    private IEnumerator LoadSceneByIndexCoroutine(int buildIndex)
    {
        Show("Lade...");
        yield return new WaitForSecondsRealtime(0.2f);
        UpdateProgress(0.1f, "Lade Szene...");

        AsyncOperation operation = SceneManager.LoadSceneAsync(buildIndex);
        
        // Prüfe ob die Szene geladen werden kann
        if (operation == null)
        {
            Debug.LogError($"<color=red>LoadingScreen: Szene mit Index {buildIndex} konnte nicht geladen werden! Ist sie im Build Profile?</color>");
            UpdateProgress(1f, "Fehler beim Laden!");
            yield return new WaitForSecondsRealtime(1f);
            Hide();
            yield break;
        }
        
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UpdateProgress(progress);

            if (operation.progress >= 0.9f)
            {
                UpdateProgress(1f, "Bereit!");
                yield return new WaitForSecondsRealtime(0.3f);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
        yield return null;
        Hide();
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, string spawnPointID)
    {
        Show("Lade...");

        // SpawnPoint setzen falls angegeben
        if (!string.IsNullOrEmpty(spawnPointID))
        {
            SceneTransitionManager manager = SceneTransitionManager.EnsureInstance();
            if (manager != null)
                manager.targetSpawnPointID = spawnPointID;
        }

        yield return new WaitForSecondsRealtime(0.2f);
        UpdateProgress(0.1f, "Lade Szene...");

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        // Prüfe ob die Szene geladen werden kann
        if (operation == null)
        {
            Debug.LogError($"<color=red>LoadingScreen: Szene '{sceneName}' konnte nicht geladen werden! Ist sie im Build Profile?</color>");
            UpdateProgress(1f, "Fehler beim Laden!");
            yield return new WaitForSecondsRealtime(1f);
            Hide();
            yield break;
        }
        
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UpdateProgress(progress);

            if (operation.progress >= 0.9f)
            {
                UpdateProgress(1f, "Bereit!");
                yield return new WaitForSecondsRealtime(0.3f);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
        yield return null;
        Hide();
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator HideCoroutine()
    {
        float elapsed = Time.realtimeSinceStartup - loadStartTime;
        if (elapsed < minimumDisplayTime)
            yield return new WaitForSecondsRealtime(minimumDisplayTime - elapsed);

        if (progressBar != null)
            progressBar.value = 1f;

        yield return new WaitForSecondsRealtime(0.3f);

        if (canvasGroup != null)
        {
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        isLoading = false;
        
        // Spieler-Bewegung wieder freigeben
        LockPlayerMovement(false);
    }
    
    /// <summary>
    /// Sperrt/entsperrt die Spielerbewegung während des Ladens.
    /// </summary>
    private void LockPlayerMovement(bool locked)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement2D movement = player.GetComponent<PlayerMovement2D>();
            if (movement != null)
            {
                movement.movementLocked = locked;
            }
        }
    }
}
