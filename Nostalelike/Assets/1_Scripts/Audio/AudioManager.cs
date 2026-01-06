using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Footstep SFX - Je nach Untergrund")]
    public AudioClip stepGrass;
    public AudioClip stepRock;
    public AudioClip stepWood;
    public AudioClip stepWater;

    [Header("Battle SFX")]
    public AudioClip attackSFX;
    public AudioClip hitSFX;
    public AudioClip slashSFX;
    public AudioClip enemyDeathSFX;
    public AudioClip blockSFX;
    public AudioClip missEvadeSFX;

    [Header("UI/Menu SFX")]
    public AudioClip equipSFX;
    public AudioClip unequipSFX;
    public AudioClip buySellSFX;
    public AudioClip useItemSFX;
    public AudioClip hoverSFX;
    public AudioClip confirmSFX;
    public AudioClip declineSFX;
    public AudioClip deniedSFX;

    [Header("Movement SFX")]
    public AudioClip jumpSFX;
    public AudioClip landingSFX;
    public AudioClip teleportSFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadVolumeSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        if (musicSource != null)
            musicSource.volume = musicVolume;
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    #region Volume Control
    
    public float GetMusicVolume()
    {
        return musicSource != null ? musicSource.volume : PlayerPrefs.GetFloat("MusicVolume", 0.5f);
    }
    
    public float GetSFXVolume()
    {
        return sfxSource != null ? sfxSource.volume : PlayerPrefs.GetFloat("SFXVolume", 1f);
    }
    
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(volume);
        
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(volume);
        
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
    
    #endregion

    #region SFX Play Methods

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Footsteps
    public void PlayFootstep(GroundType groundType)
    {
        AudioClip clip = groundType switch
        {
            GroundType.Grass => stepGrass,
            GroundType.Rock => stepRock,
            GroundType.Wood => stepWood,
            GroundType.Water => stepWater,
            _ => stepGrass
        };
        PlaySFX(clip);
    }

    // Battle
    public void PlayAttackSFX() => PlaySFX(attackSFX);
    public void PlayHitSFX() => PlaySFX(hitSFX);
    public void PlaySlashSFX() => PlaySFX(slashSFX);
    public void PlayEnemyDeathSFX() => PlaySFX(enemyDeathSFX);
    public void PlayBlockSFX() => PlaySFX(blockSFX);
    public void PlayMissEvadeSFX() => PlaySFX(missEvadeSFX);

    // UI/Menu
    public void PlayEquipSFX() => PlaySFX(equipSFX);
    public void PlayUnequipSFX() => PlaySFX(unequipSFX);
    public void PlayBuySellSFX() => PlaySFX(buySellSFX);
    public void PlayUseItemSFX() => PlaySFX(useItemSFX);
    public void PlayHoverSFX() => PlaySFX(hoverSFX);
    public void PlayConfirmSFX() => PlaySFX(confirmSFX);
    public void PlayDeclineSFX() => PlaySFX(declineSFX);
    public void PlayDeniedSFX() => PlaySFX(deniedSFX);

    // Movement
    public void PlayJumpSFX() => PlaySFX(jumpSFX);
    public void PlayLandingSFX() => PlaySFX(landingSFX);
    public void PlayTeleportSFX() => PlaySFX(teleportSFX);
    
    #endregion
}

public enum GroundType
{
    Grass,
    Rock,
    Wood,
    Water
}