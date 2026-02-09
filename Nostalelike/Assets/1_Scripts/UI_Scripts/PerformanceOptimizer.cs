using UnityEngine;

/// <summary>
/// Performance-Einstellungen für das Spiel.
/// Auf ein leeres GameObject in der ersten Szene legen.
/// Wird persistent (DontDestroyOnLoad).
/// </summary>
public class PerformanceOptimizer : MonoBehaviour
{
    public static PerformanceOptimizer Instance { get; private set; }
    
    [Header("Frame Rate")]
    [SerializeField] private int targetFrameRate = 60;
    
    [Header("Physics")]
    [Tooltip("Physics Update Rate (0.02 = 50Hz, 0.03 = 33Hz)")]
    [SerializeField] private float fixedTimestep = 0.02f;
    
    [Header("VSync")]
    [Tooltip("0 = aus (Target FPS), 1 = an (Monitor Refresh Rate)")]
    [SerializeField] private int vSyncCount = 0;
    
    [Header("GC Management")]
    [Tooltip("Automatische GC-Bereinigung bei Szenenwechsel")]
    [SerializeField] private bool cleanupOnSceneLoad = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Frame Rate setzen
        Application.targetFrameRate = targetFrameRate;
        
        // VSync
        QualitySettings.vSyncCount = vSyncCount;
        
        // Physics Timestep
        Time.fixedDeltaTime = fixedTimestep;
        
        // Low Memory Handler
        Application.lowMemory += OnLowMemory;
        
        // Scene Load Cleanup
        if (cleanupOnSceneLoad)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        Debug.Log($"<color=green>PerformanceOptimizer: FPS={targetFrameRate}, VSync={vSyncCount}, Physics={fixedTimestep}</color>");
    }

    private void OnDestroy()
    {
        Application.lowMemory -= OnLowMemory;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Nach Szenenwechsel aufräumen — reduziert Speicher-Spikes
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Debug.Log($"<color=green>PerformanceOptimizer: Cleanup nach Scene '{scene.name}'</color>");
    }

    private void OnLowMemory()
    {
        // Notfall-Cleanup bei wenig Speicher
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Debug.LogWarning("PerformanceOptimizer: Low memory! Cleanup durchgeführt.");
    }
}
