using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    
    void Start()
    {
        // Lade gespeicherte Werte
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }
    
    public void OnMusicSliderChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }
    
    public void OnSFXSliderChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
    }
}