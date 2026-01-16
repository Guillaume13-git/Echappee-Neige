using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire audio central.
/// Gère la musique, les SFX, les volumes, et les transitions.
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _gameplayMusic;

    [Header("SFX Clips")]
    [SerializeField] private List<SFXClip> _sfxClips = new List<SFXClip>();

    private Dictionary<string, AudioClip> _sfxDictionary = new Dictionary<string, AudioClip>();


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    protected override void Awake()
    {
        base.Awake(); // ← Initialise le Singleton
        DontDestroyOnLoad(gameObject);

        InitializeSFXDictionary();
    }

    private void Start()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged += SetMusicVolume;
            SettingsManager.Instance.OnSFXVolumeChanged += SetSFXVolume;

            SetMusicVolume(SettingsManager.Instance.MusicVolume);
            SetSFXVolume(SettingsManager.Instance.SFXVolume);
        }
    }

    private void OnDestroy()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= SetMusicVolume;
            SettingsManager.Instance.OnSFXVolumeChanged -= SetSFXVolume;
        }
    }


    // ---------------------------------------------------------
    // INITIALISATION DES SFX
    // ---------------------------------------------------------
    private void InitializeSFXDictionary()
    {
        _sfxDictionary.Clear();

        foreach (SFXClip sfx in _sfxClips)
        {
            if (!_sfxDictionary.ContainsKey(sfx.name))
            {
                _sfxDictionary.Add(sfx.name, sfx.clip);
            }
        }

        Debug.Log($"[AudioManager] {_sfxDictionary.Count} SFX chargés.");
    }


    // ---------------------------------------------------------
    // MUSIQUE
    // ---------------------------------------------------------
    public void PlayMusic(AudioClip music, bool loop = true)
    {
        if (_musicSource == null || music == null) return;

        _musicSource.clip = music;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    public void PlayMenuMusic() => PlayMusic(_menuMusic);
    public void PlayGameplayMusic() => PlayMusic(_gameplayMusic);

    public void StopMusic()
    {
        if (_musicSource != null)
            _musicSource.Stop();
    }


    // ---------------------------------------------------------
    // SFX
    // ---------------------------------------------------------
    public void PlaySFX(string sfxName)
    {
        if (_sfxSource == null) return;

        if (_sfxDictionary.TryGetValue(sfxName, out AudioClip clip))
        {
            _sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX '{sfxName}' introuvable !");
        }
    }

    public void PlaySFX(string sfxName, float volumeScale)
    {
        if (_sfxSource == null) return;

        if (_sfxDictionary.TryGetValue(sfxName, out AudioClip clip))
        {
            _sfxSource.PlayOneShot(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX '{sfxName}' introuvable !");
        }
    }


    // ---------------------------------------------------------
    // VOLUMES
    // ---------------------------------------------------------
    public void SetMusicVolume(float volume)
    {
        if (_musicSource != null)
            _musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (_sfxSource != null)
            _sfxSource.volume = Mathf.Clamp01(volume);
    }


    // ---------------------------------------------------------
    // FADE OUT
    // ---------------------------------------------------------
    public void FadeOutMusic(float duration)
    {
        if (_musicSource != null)
            StartCoroutine(FadeOutCoroutine(duration));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = _musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        _musicSource.Stop();
        _musicSource.volume = startVolume;
    }
}


// ---------------------------------------------------------
// STRUCTURE SFX
// ---------------------------------------------------------
[System.Serializable]
public class SFXClip
{
    public string name;
    public AudioClip clip;
}