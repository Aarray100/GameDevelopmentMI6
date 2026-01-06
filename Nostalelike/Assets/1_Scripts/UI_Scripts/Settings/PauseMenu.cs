using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button quitButton;
    
    [Header("Audio Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    
    private bool isPaused = false;
    
    private static PauseMenu instance;
    
    private void Awake()
    {
        // Singleton Pattern (ohne DontDestroyOnLoad - Canvas macht das bereits)
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Panel initial verstecken
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        // Button Listeners
        if (playButton != null)
            playButton.onClick.AddListener(ResumeGame);
        
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveGame);
        
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadGame);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        // Audio Slider Setup - Warte kurz auf AudioManager
        InitializeAudioSliders();
    }
    
    private void InitializeAudioSliders()
    {
        // Slider Range setzen (0 bis 1)
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            
            // Wert vom AudioManager holen (oder Default)
            float musicVol = AudioManager.Instance != null 
                ? AudioManager.Instance.GetMusicVolume() 
                : PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            
            musicSlider.SetValueWithoutNotify(musicVol);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            Debug.Log($"Music Slider initialisiert mit: {musicVol}");
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            
            // Wert vom AudioManager holen (oder Default)
            float sfxVol = AudioManager.Instance != null 
                ? AudioManager.Instance.GetSFXVolume() 
                : PlayerPrefs.GetFloat("SFXVolume", 1f);
            
            sfxSlider.SetValueWithoutNotify(sfxVol);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            
            Debug.Log($"SFX Slider initialisiert mit: {sfxVol}");
        }
    }
    
    private void Update()
    {
        // ESC Taste zum Öffnen/Schließen des Menüs
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    public void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Spiel anhalten
        
        // Aktualisiere Slider-Werte beim Öffnen
        RefreshSliderValues();
        
        // Optional: Cursor sichtbar machen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Spiel pausiert");
    }
    
    private void RefreshSliderValues()
    {
        if (AudioManager.Instance != null)
        {
            if (musicSlider != null)
                musicSlider.SetValueWithoutNotify(AudioManager.Instance.GetMusicVolume());
            
            if (sfxSlider != null)
                sfxSlider.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolume());
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Spiel fortsetzen
        
        Debug.Log("Spiel fortgesetzt");
    }
    
    private void SaveGame()
    {
        Debug.Log("Save Game - wird später implementiert");
        // TODO: Save-System implementieren
        
        // Feedback für User (funktioniert auch bei TimeScale 0)
        PlayUISound();
    }
    
    private void LoadGame()
    {
        Debug.Log("Load Game - wird später implementiert");
        // TODO: Load-System implementieren
        
        // Feedback für User
        PlayUISound();
    }
    
    private void PlayUISound()
    {
        // PlayOneShot funktioniert auch bei TimeScale 0
        AudioManager.Instance?.PlayConfirmSFX();
    }
    
    private void QuitGame()
    {
        Debug.Log("Quit Game");
        
        // Wichtig: Time.timeScale wieder auf 1 setzen bevor wir beenden
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }
    
    private void OnDestroy()
    {
        // Stelle sicher dass Time.timeScale zurückgesetzt wird
        Time.timeScale = 1f;
        
        if (instance == this)
        {
            instance = null;
        }
    }
    
    // Public Getter für isPaused (falls andere Scripts das brauchen)
    public static bool IsPaused => instance != null && instance.isPaused;
}