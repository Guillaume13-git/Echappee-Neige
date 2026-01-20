using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SFClip {
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
    [SerializeField] private List<SFClip> _sfxClips = new List<SFClip>();
    
    private Dictionary<string, AudioClip> _sfxDictionary = new Dictionary<string, AudioClip>();

    protected override void Awake()
    {
        // SÉCURITÉ ABSOLUE : Si une instance existe déjà, on se détruit AVANT de toucher à quoi que ce soit
        if (_instance != null && _instance != this)
        {
            Debug.Log("[AudioManager] Doublon supprimé immédiatement pour éviter la coupure.");
            DestroyImmediate(gameObject); // DestroyImmediate est plus radical que Destroy
            return;
        }

        base.Awake();
        InitializeSFXDictionary();

        // On force les réglages pour éviter les bugs Unity
        if (_musicSource != null) {
            _musicSource.playOnAwake = false;
            _musicSource.ignoreListenerPause = true; // La musique continue même si le jeu est en pause
        }
    }

    private void InitializeSFXDictionary()
    {
        _sfxDictionary.Clear();
        foreach (SFClip sfx in _sfxClips)
        {
            if (sfx != null && !string.IsNullOrEmpty(sfx.name))
                _sfxDictionary[sfx.name] = sfx.clip;
        }
    }

    public void PlayMusic(AudioClip music, bool loop = true)
    {
        if (_musicSource == null || music == null) return;

        // Comparaison stricte par nom
        if (_musicSource.isPlaying && _musicSource.clip != null)
        {
            if (_musicSource.clip.name == music.name) return;
        }

        _musicSource.clip = music;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    public void PlayMenuMusic() => PlayMusic(_menuMusic);
    public void PlaySFX(string sfxName, float vol = 1f)
    {
        if (_sfxDictionary.TryGetValue(sfxName, out AudioClip c))
            _sfxSource.PlayOneShot(c, vol);
    }

    // Gestion des volumes sans passer par le Start (plus rapide)
    public void SetMusicVolume(float v) => _musicSource.volume = v;
    public void SetSFXVolume(float v) => _sfxSource.volume = v;
}