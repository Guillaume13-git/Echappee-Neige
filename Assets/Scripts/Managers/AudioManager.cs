using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SFXClip
{
    public string name;
    public AudioClip clip;
}

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

    protected override void Awake()
    {
        // SÉCURITÉ : Si doublon, se détruire immédiatement
        if (_instance != null && _instance != this)
        {
            Debug.Log("[AudioManager] Doublon détecté, destruction immédiate");
            DestroyImmediate(gameObject);
            return;
        }

        base.Awake();
        DontDestroyOnLoad(gameObject); // ✅ IMPORTANT
        
        InitializeSFXDictionary();

        // Configuration des AudioSources
        if (_musicSource != null)
        {
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
        }
        
        if (_sfxSource != null)
        {
            _sfxSource.playOnAwake = false;
        }
        
        Debug.Log("[AudioManager] Initialisé avec succès");
    }
    
    // ✅ AJOUT CRITIQUE : Start() pour s'abonner aux événements
    private void Start()
    {
        Debug.Log("[AudioManager] Start() - Abonnement aux événements");
        
        // Attendre que SettingsManager soit prêt
        if (SettingsManager.Instance != null)
        {
            // S'abonner aux changements de volume
            SettingsManager.Instance.OnMusicVolumeChanged += SetMusicVolume;
            SettingsManager.Instance.OnSFXVolumeChanged += SetSFXVolume;
            
            // Appliquer les volumes sauvegardés
            SetMusicVolume(SettingsManager.Instance.MusicVolume);
            SetSFXVolume(SettingsManager.Instance.SFXVolume);
            
            Debug.Log($"[AudioManager] Volumes appliqués - Music: {SettingsManager.Instance.MusicVolume:F2}, SFX: {SettingsManager.Instance.SFXVolume:F2}");
        }
        else
        {
            Debug.LogWarning("[AudioManager] SettingsManager introuvable au Start !");
        }
    }
    
    // ✅ Nettoyage lors de la destruction
    protected override void OnDestroy()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= SetMusicVolume;
            SettingsManager.Instance.OnSFXVolumeChanged -= SetSFXVolume;
        }
    }

    private void InitializeSFXDictionary()
    {
        _sfxDictionary.Clear();
        
        foreach (SFXClip sfx in _sfxClips)
        {
            if (sfx != null && !string.IsNullOrEmpty(sfx.name) && sfx.clip != null)
            {
                if (!_sfxDictionary.ContainsKey(sfx.name))
                {
                    _sfxDictionary.Add(sfx.name, sfx.clip);
                }
            }
        }
        
        Debug.Log($"[AudioManager] {_sfxDictionary.Count} SFX chargés");
    }

    /// <summary>
    /// Joue une musique de fond.
    /// Ne relance pas si la même musique est déjà en cours.
    /// </summary>
    public void PlayMusic(AudioClip music, bool loop = true)
    {
        if (_musicSource == null || music == null) return;

        // Vérifier si la même musique joue déjà
        if (_musicSource.isPlaying && _musicSource.clip != null)
        {
            if (_musicSource.clip.name == music.name)
            {
                Debug.Log($"[AudioManager] Musique '{music.name}' déjà en cours");
                return;
            }
        }

        Debug.Log($"[AudioManager] Lecture de la musique : {music.name}");
        _musicSource.clip = music;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    public void PlayMenuMusic() => PlayMusic(_menuMusic);
    public void PlayGameplayMusic() => PlayMusic(_gameplayMusic);

    /// <summary>
    /// Joue un effet sonore.
    /// </summary>
    public void PlaySFX(string sfxName, float volumeScale = 1f)
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

    /// <summary>
    /// Modifie le volume de la musique.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (_musicSource != null)
        {
            _musicSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] Volume Musique appliqué : {_musicSource.volume:F2}");
        }
        else
        {
            Debug.LogError("[AudioManager] MusicSource est NULL !");
        }
    }

    /// <summary>
    /// Modifie le volume des SFX.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (_sfxSource != null)
        {
            _sfxSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] Volume SFX appliqué : {_sfxSource.volume:F2}");
        }
        else
        {
            Debug.LogError("[AudioManager] SFXSource est NULL !");
        }
    }
    
    // Méthodes utilitaires
    public AudioSource GetMusicSource() => _musicSource;
    public AudioClip GetMenuMusicClip() => _menuMusic;
    public AudioClip GetGameplayMusicClip() => _gameplayMusic;
}