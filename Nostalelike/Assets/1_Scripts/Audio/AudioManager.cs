using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Tracks")]
    public AudioClip peacefulMusic;
    public AudioClip combatMusic;

    [Header("Scene Music")]
    [Tooltip("Musik für bestimmte Szenen - wenn leer, wird peacefulMusic verwendet")]
    public SceneMusic[] sceneMusicList;
    
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
        Debug.Log($"[AudioManager] Awake called. Instance exists: {Instance != null}");
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("[AudioManager] Created and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("[AudioManager] Duplicate found, destroying this one");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Beim Szenenwechsel: Passende Musik abspielen
        PlaySceneMusic(scene.name);
    }

    private void Start()
    {
        LoadVolumeSettings();
        ApplyVolumes();
        // Spiele Musik für aktuelle Szene
        PlaySceneMusic(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Spielt die Musik für eine bestimmte Szene ab.
    /// </summary>
    public void PlaySceneMusic(string sceneName)
    {
        if (isInCombat) return; // Nicht wechseln während Kampf
        
        AudioClip musicForScene = GetMusicForScene(sceneName);
        
        // Nur wechseln wenn andere Musik
        if (musicSource.clip != musicForScene)
        {
            if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(CrossfadeMusic(musicForScene));
        }
    }

    /// <summary>
    /// Findet die passende Musik für eine Szene.
    /// </summary>
    private AudioClip GetMusicForScene(string sceneName)
    {
        if (sceneMusicList != null)
        {
            foreach (var sceneMusic in sceneMusicList)
            {
                if (sceneMusic.sceneName == sceneName && sceneMusic.music != null)
                {
                    return sceneMusic.music;
                }
            }
        }
        return peacefulMusic; // Fallback
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
        
        // Spiele die korrekte Szenen-Musik, nicht einfach peacefulMusic!
        string currentSceneName = SceneManager.GetActiveScene().name;
        AudioClip sceneMusic = GetMusicForScene(currentSceneName);
        
        if (sceneMusic == null) return;
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(CrossfadeMusic(sceneMusic));
        
        Debug.Log($"[AudioManager] Kampf beendet - spiele Musik für Szene: {currentSceneName}");
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

/// <summary>
/// Verknüpft eine Szene mit einem Musik-Clip.
/// </summary>
[Serializable]
public class SceneMusic
{
    [Tooltip("Exakter Name der Szene (wie im Build Settings)")]
    public string sceneName;
    
    [Tooltip("Musik für diese Szene")]
    public AudioClip music;
}

public enum GroundType { Grass, Rock, Wood, Water }