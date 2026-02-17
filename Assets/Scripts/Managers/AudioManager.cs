using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Je représente un clip sonore avec son nom et son fichier audio
/// </summary>
[System.Serializable]
public class SFXClip
{
    public string name; // Je stocke le nom de l'effet sonore
    public AudioClip clip; // Je stocke le fichier audio de l'effet sonore
}

/// <summary>
/// Je suis le gestionnaire audio du jeu.
/// Je m'occupe de la musique de fond et des effets sonores.
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource; // Je stocke la source audio pour la musique
    [SerializeField] private AudioSource _sfxSource;   // Je stocke la source audio pour les effets sonores
    
    [Header("Music Clips")]
    [SerializeField] private AudioClip _menuMusic;     // Je stocke la musique du menu
    [SerializeField] private AudioClip _gameplayMusic; // Je stocke la musique du gameplay
    
    [Header("SFX Clips")]
    [SerializeField] private List<SFXClip> _sfxClips = new List<SFXClip>(); // Je stocke la liste de tous mes effets sonores
    
    // Je crée un dictionnaire pour accéder rapidement aux effets sonores par leur nom
    private Dictionary<string, AudioClip> _sfxDictionary = new Dictionary<string, AudioClip>();
    
    // ✅ NOUVEAU : Je crée une source audio dédiée pour l'alarme en boucle
    private AudioSource _alarmSource;

    /// <summary>
    /// Je m'initialise au démarrage du jeu
    /// </summary>
    protected override void Awake()
    {
        // SÉCURITÉ : Je vérifie qu'il n'y a pas déjà une instance de moi
        if (_instance != null && _instance != this)
        {
            Debug.Log("[AudioManager] Doublon détecté, je me détruis immédiatement");
            DestroyImmediate(gameObject); // Je me détruis pour éviter les doublons
            return;
        }

        base.Awake(); // J'initialise le Singleton
        
        // ✅ CORRECTION 1 : Je me détache de mon parent pour être à la racine
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        // ✅ CORRECTION 2 : Maintenant je peux me rendre persistant
        DontDestroyOnLoad(gameObject);
        
        // J'initialise mon dictionnaire d'effets sonores
        InitializeSFXDictionary();

        // Je configure mes sources audio
        if (_musicSource != null)
        {
            _musicSource.playOnAwake = false; // Je désactive le démarrage automatique
            _musicSource.loop = true;         // J'active la répétition en boucle
        }
        
        if (_sfxSource != null)
        {
            _sfxSource.playOnAwake = false; // Je désactive le démarrage automatique
        }
        
        Debug.Log("[AudioManager] Initialisé avec succès");
    }
    
    /// <summary>
    /// Je m'abonne aux événements du SettingsManager pour recevoir les changements de volume
    /// </summary>
    private void Start()
    {
        Debug.Log("[AudioManager] Start() - Abonnement aux événements");
        
        // J'attends que le SettingsManager soit prêt
        if (SettingsManager.Instance != null)
        {
            // Je m'abonne aux événements de changement de volume
            SettingsManager.Instance.OnMusicVolumeChanged += SetMusicVolume;
            SettingsManager.Instance.OnSFXVolumeChanged += SetSFXVolume;
            
            // J'applique immédiatement les volumes sauvegardés
            SetMusicVolume(SettingsManager.Instance.MusicVolume);
            SetSFXVolume(SettingsManager.Instance.SFXVolume);
            
            Debug.Log($"[AudioManager] Volumes appliqués - Music: {SettingsManager.Instance.MusicVolume:F2}, SFX: {SettingsManager.Instance.SFXVolume:F2}");
        }
        else
        {
            Debug.LogWarning("[AudioManager] SettingsManager introuvable au Start !");
        }
    }
    
    /// <summary>
    /// Je me désabonne des événements quand je suis détruit
    /// </summary>
    protected override void OnDestroy()
    {
        // Si le SettingsManager existe encore, je me désabonne de ses événements
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= SetMusicVolume;
            SettingsManager.Instance.OnSFXVolumeChanged -= SetSFXVolume;
        }
    }

    /// <summary>
    /// J'initialise mon dictionnaire d'effets sonores pour un accès rapide
    /// </summary>
    private void InitializeSFXDictionary()
    {
        // Je vide le dictionnaire au cas où
        _sfxDictionary.Clear();
        
        // Je parcours tous mes clips sonores
        foreach (SFXClip sfx in _sfxClips)
        {
            // Je vérifie que le clip est valide
            if (sfx != null && !string.IsNullOrEmpty(sfx.name) && sfx.clip != null)
            {
                // Je vérifie qu'il n'y a pas de doublon
                if (!_sfxDictionary.ContainsKey(sfx.name))
                {
                    // J'ajoute le clip au dictionnaire
                    _sfxDictionary.Add(sfx.name, sfx.clip);
                }
            }
        }
        
        // J'affiche le nombre d'effets sonores chargés
        Debug.Log($"[AudioManager] {_sfxDictionary.Count} SFX chargés");
    }

    /// <summary>
    /// Je joue une musique de fond.
    /// Je ne relance pas la musique si elle est déjà en cours.
    /// </summary>
    /// <param name="music">Le clip musical à jouer</param>
    /// <param name="loop">Si je dois répéter la musique en boucle</param>
    public void PlayMusic(AudioClip music, bool loop = true)
    {
        // Je vérifie que j'ai une source audio et un clip valide
        if (_musicSource == null || music == null) return;

        // Je vérifie si la même musique est déjà en train de jouer
        if (_musicSource.isPlaying && _musicSource.clip != null)
        {
            if (_musicSource.clip.name == music.name)
            {
                // Si c'est la même musique, je ne fais rien
                Debug.Log($"[AudioManager] Musique '{music.name}' déjà en cours");
                return;
            }
        }

        // Je lance la nouvelle musique
        Debug.Log($"[AudioManager] Lecture de la musique : {music.name}");
        _musicSource.clip = music;      // Je définis le clip à jouer
        _musicSource.loop = loop;       // Je configure la répétition
        _musicSource.Play();            // Je lance la lecture
    }

    /// <summary>
    /// Je joue la musique du menu
    /// </summary>
    public void PlayMenuMusic() => PlayMusic(_menuMusic);
    
    /// <summary>
    /// Je joue la musique du gameplay
    /// </summary>
    public void PlayGameplayMusic() => PlayMusic(_gameplayMusic);

    /// <summary>
    /// Je joue un effet sonore
    /// </summary>
    /// <param name="sfxName">Le nom de l'effet sonore à jouer</param>
    /// <param name="volumeScale">Le multiplicateur de volume (1.0 = volume normal)</param>
    public void PlaySFX(string sfxName, float volumeScale = 1f)
    {
        // Je vérifie que j'ai une source audio
        if (_sfxSource == null) return;
        
        // Je cherche l'effet sonore dans mon dictionnaire
        if (_sfxDictionary.TryGetValue(sfxName, out AudioClip clip))
        {
            // Je joue l'effet sonore avec PlayOneShot (permet de jouer plusieurs sons simultanément)
            _sfxSource.PlayOneShot(clip, volumeScale);
        }
        else
        {
            // Si je ne trouve pas l'effet sonore, j'affiche un avertissement
            Debug.LogWarning($"[AudioManager] SFX '{sfxName}' introuvable !");
        }
    }

    // ---------------------------------------------------------
    // ✅ NOUVELLES MÉTHODES POUR L'ALARME
    // ---------------------------------------------------------
    
    /// <summary>
    /// ✅ NOUVEAU : Je joue l'alarme en boucle
    /// Mon rôle : Créer une source audio dédiée pour l'alarme et la jouer en continu
    /// </summary>
    public void PlayAlarm()
    {
        // Si la source d'alarme n'existe pas encore, je la crée
        if (_alarmSource == null)
        {
            // Je crée une nouvelle source audio dédiée pour l'alarme
            _alarmSource = gameObject.AddComponent<AudioSource>();
            _alarmSource.loop = true;          // Je l'active en mode boucle
            _alarmSource.playOnAwake = false;  // Je désactive le démarrage automatique
            
            Debug.Log("[AudioManager] Source audio d'alarme créée");
        }
        
        // Je cherche le clip d'alarme dans mon dictionnaire
        // IMPORTANT : Assurez-vous d'avoir un SFX nommé "Alarm" dans votre liste !
        if (_sfxDictionary.TryGetValue("Alarm", out AudioClip alarmClip))
        {
            // Si l'alarme n'est pas déjà en train de jouer
            if (!_alarmSource.isPlaying)
            {
                _alarmSource.clip = alarmClip;
                
                // Je synchronise le volume avec celui des SFX
                if (_sfxSource != null)
                {
                    _alarmSource.volume = _sfxSource.volume;
                }
                
                _alarmSource.Play();
                Debug.Log("[AudioManager] 🚨 Alarme lancée en boucle");
            }
        }
        else
        {
            Debug.LogWarning("[AudioManager] ⚠️ SFX 'Alarm' introuvable ! Ajoutez un clip nommé 'Alarm' dans votre liste de SFX.");
        }
    }
    
    /// <summary>
    /// ✅ NOUVEAU : J'arrête l'alarme
    /// Mon rôle : Arrêter proprement la lecture de l'alarme
    /// </summary>
    public void StopAlarm()
    {
        // Si la source d'alarme existe et qu'elle joue
        if (_alarmSource != null && _alarmSource.isPlaying)
        {
            _alarmSource.Stop();
            Debug.Log("[AudioManager] ✅ Alarme arrêtée");
        }
    }

    /// <summary>
    /// Je modifie le volume de la musique
    /// </summary>
    /// <param name="volume">Le nouveau volume (0.0 à 1.0)</param>
    public void SetMusicVolume(float volume)
    {
        // Je vérifie que ma source audio de musique existe
        if (_musicSource != null)
        {
            // J'applique le volume en m'assurant qu'il est entre 0 et 1
            _musicSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] Volume Musique appliqué : {_musicSource.volume:F2}");
        }
        else
        {
            Debug.LogError("[AudioManager] MusicSource est NULL !");
        }
    }

    /// <summary>
    /// Je modifie le volume des effets sonores
    /// </summary>
    /// <param name="volume">Le nouveau volume (0.0 à 1.0)</param>
    public void SetSFXVolume(float volume)
    {
        // Je vérifie que ma source audio d'effets sonores existe
        if (_sfxSource != null)
        {
            // J'applique le volume en m'assurant qu'il est entre 0 et 1
            _sfxSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] Volume SFX appliqué : {_sfxSource.volume:F2}");
        }
        else
        {
            Debug.LogError("[AudioManager] SFXSource est NULL !");
        }
        
        // ✅ NOUVEAU : Je synchronise aussi le volume de l'alarme si elle existe
        if (_alarmSource != null)
        {
            _alarmSource.volume = Mathf.Clamp01(volume);
        }
    }
    
    // ---------------------------------------------------------
    // MÉTHODES UTILITAIRES
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je retourne ma source audio de musique
    /// </summary>
    public AudioSource GetMusicSource() => _musicSource;
    
    /// <summary>
    /// Je retourne le clip de musique du menu
    /// </summary>
    public AudioClip GetMenuMusicClip() => _menuMusic;
    
    /// <summary>
    /// Je retourne le clip de musique du gameplay
    /// </summary>
    public AudioClip GetGameplayMusicClip() => _gameplayMusic;
}