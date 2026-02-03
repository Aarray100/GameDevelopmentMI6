using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    
    [Header("Slider Labels")]
    [SerializeField] private TextMeshProUGUI musicLabel;
    [SerializeField] private TextMeshProUGUI sfxLabel;
    
    private bool isPaused = false;
    
    private static PauseMenu instance;
    
    private void Awake()
    {
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
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        if (playButton != null)
            playButton.onClick.AddListener(ResumeGame);
        
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveGame);
        
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadGame);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        InitializeAudioSliders();
    }
    
    private void InitializeAudioSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            
            float musicVol = AudioManager.Instance != null 
                ? AudioManager.Instance.GetMusicVolume() 
                : PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            
            musicSlider.SetValueWithoutNotify(musicVol);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            UpdateMusicLabel(musicVol);
            
            Debug.Log($"Music Slider initialisiert mit: {musicVol}");
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            
            float sfxVol = AudioManager.Instance != null 
                ? AudioManager.Instance.GetSFXVolume() 
                : PlayerPrefs.GetFloat("SFXVolume", 1f);
            
            sfxSlider.SetValueWithoutNotify(sfxVol);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            UpdateSFXLabel(sfxVol);
            
            Debug.Log($"SFX Slider initialisiert mit: {sfxVol}");
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    public void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        
        RefreshSliderValues();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Spiel pausiert");
    }
    
    private void RefreshSliderValues()
    {
        if (AudioManager.Instance != null)
        {
            if (musicSlider != null)
            {
                float musicVol = AudioManager.Instance.GetMusicVolume();
                musicSlider.SetValueWithoutNotify(musicVol);
                UpdateMusicLabel(musicVol);
            }
            
            if (sfxSlider != null)
            {
                float sfxVol = AudioManager.Instance.GetSFXVolume();
                sfxSlider.SetValueWithoutNotify(sfxVol);
                UpdateSFXLabel(sfxVol);
            }
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        
        Debug.Log("Spiel fortgesetzt");
    }
    
    private void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            Debug.Log("Game Saved!");
        }
        else
        {
            Debug.LogWarning("SaveManager not found!");
        }
        
        PlayUISound();
    }
    
    private void LoadGame()
    {
        if (SaveManager.Instance != null)
        {
            Time.timeScale = 1f;
            isPaused = false;
            
            SaveManager.Instance.LoadGame();
            Debug.Log("Loading Game...");
        }
        else
        {
            Debug.LogWarning("SaveManager not found!");
        }
        
        PlayUISound();
    }
    
    private void PlayUISound()
    {
        AudioManager.Instance?.PlayConfirmSFX();
    }
    
    private void QuitGame()
    {
        Debug.Log("Quit Game");
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
        UpdateMusicLabel(value);
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        UpdateSFXLabel(value);
    }
    
    private void UpdateMusicLabel(float value)
    {
        if (musicLabel != null)
            musicLabel.text = $"Musik: {Mathf.RoundToInt(value * 100)}%";
    }
    
    private void UpdateSFXLabel(float value)
    {
        if (sfxLabel != null)
            sfxLabel.text = $"Effekte: {Mathf.RoundToInt(value * 100)}%";
    }
    
    private void OnDestroy()
    {
        Time.timeScale = 1f;
        
        if (instance == this)
        {
            instance = null;
        }
    }
    
    public static bool IsPaused => instance != null && instance.isPaused;
}