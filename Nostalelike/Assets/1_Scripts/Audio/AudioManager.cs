using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Tracks")]
    public AudioClip peacefulMusic;
    public AudioClip combatMusic;
    
    [Header("Music Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public float musicFadeDuration = 1f;
    public float combatMusicDelay = 2f;  // Wartezeit bevor wieder peaceful

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

    [Header("Shop Specific SFX")]
    public AudioClip shopOpenSFX;
    public AudioClip itemSoldSFX;
    public AudioClip insufficientGoldSFX; // Der Blipp-Sound bei zu wenig Geld

    [Header("Movement SFX")]
    public AudioClip jumpSFX;
    public AudioClip landingSFX;
    public AudioClip teleportSFX;

    // Combat Music State
    private bool isInCombat = false;
    private float lastCombatTime = 0f;
    private Coroutine musicFadeCoroutine;
    private Coroutine combatCheckCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadVolumeSettings();
        ApplyVolumes();
        PlayMusic(peacefulMusic);
    }

    #region Volume Control
    
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
    }
    
    #endregion

    #region Music System
    
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void EnterCombat()
    {
        lastCombatTime = Time.time;
        if (!isInCombat)
        {
            isInCombat = true;
            SwitchToCombatMusic();
        }
    }

    private void SwitchToCombatMusic()
    {
        if (combatMusic == null) return;
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(CrossfadeMusic(combatMusic));
        if (combatCheckCoroutine != null) StopCoroutine(combatCheckCoroutine);
        combatCheckCoroutine = StartCoroutine(CheckCombatEnd());
    }

    private IEnumerator CheckCombatEnd()
    {
        while (isInCombat)
        {
            if (Time.time - lastCombatTime > combatMusicDelay) ExitCombat();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ExitCombat()
    {
        if (!isInCombat) return;
        isInCombat = false;
        if (peacefulMusic == null) return;
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(CrossfadeMusic(peacefulMusic));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;
        while (timer < musicFadeDuration / 2f)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / (musicFadeDuration / 2f));
            yield return null;
        }
        musicSource.clip = newClip;
        musicSource.Play();
        timer = 0f;
        while (timer < musicFadeDuration / 2f)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, timer / (musicFadeDuration / 2f));
            yield return null;
        }
        musicSource.volume = musicVolume;
    }
    
    #endregion

    #region SFX
    
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

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

    public void PlayAttackSFX() => PlaySFX(attackSFX);
    public void PlayHitSFX() => PlaySFX(hitSFX);
    public void PlaySlashSFX() => PlaySFX(slashSFX);
    public void PlayEnemyDeathSFX() => PlaySFX(enemyDeathSFX);
    public void PlayBlockSFX() => PlaySFX(blockSFX);
    public void PlayMissEvadeSFX() => PlaySFX(missEvadeSFX);

    public void PlayEquipSFX() => PlaySFX(equipSFX);
    public void PlayUnequipSFX() => PlaySFX(unequipSFX);
    public void PlayBuySellSFX() => PlaySFX(buySellSFX);
    public void PlayUseItemSFX() => PlaySFX(useItemSFX);
    public void PlayHoverSFX() => PlaySFX(hoverSFX);
    public void PlayConfirmSFX() => PlaySFX(confirmSFX);
    public void PlayDeclineSFX() => PlaySFX(declineSFX);
    public void PlayDeniedSFX() => PlaySFX(deniedSFX);

    // Shop Specific
    public void PlayShopOpenSFX() => PlaySFX(shopOpenSFX);
    public void PlayItemSoldSFX() => PlaySFX(itemSoldSFX);
    public void PlayInsufficientGoldSFX() => PlaySFX(insufficientGoldSFX);

    public void PlayJumpSFX() => PlaySFX(jumpSFX);
    public void PlayLandingSFX() => PlaySFX(landingSFX);
    public void PlayTeleportSFX() => PlaySFX(teleportSFX);
    
    #endregion
}

public enum GroundType { Grass, Rock, Wood, Water }