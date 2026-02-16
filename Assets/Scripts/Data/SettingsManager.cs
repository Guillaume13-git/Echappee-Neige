using UnityEngine;

/// <summary>
/// Je gère les paramètres du jeu (volumes, tutoriel).
/// Je sauvegarde automatiquement chaque modification via le SaveSystem.
/// </summary>
public class SettingsManager : Singleton<SettingsManager>
{
    private GameData _currentSettings; // Je stocke les paramètres actuels du jeu

    // ---------------------------------------------------------
    // ACCESSEURS
    // ---------------------------------------------------------
    
    // Je donne accès au volume de la musique (1.0 par défaut si pas de données)
    public float MusicVolume => _currentSettings != null ? _currentSettings.musicVolume : 1f;
    
    // Je donne accès au volume des effets sonores (1.0 par défaut si pas de données)
    public float SFXVolume => _currentSettings != null ? _currentSettings.sfxVolume : 1f;
    
    // Je donne accès à l'état d'affichage du tutoriel (false par défaut si pas de données)
    public bool ShowTutorial => _currentSettings != null && _currentSettings.showTutorial;

    // ---------------------------------------------------------
    // ÉVÉNEMENTS
    // ---------------------------------------------------------
    
    // J'invoque cet événement quand le volume de la musique change
    // Cela permet à l'AudioManager de réagir automatiquement
    public System.Action<float> OnMusicVolumeChanged;
    
    // J'invoque cet événement quand le volume des SFX change
    // Cela permet à l'AudioManager de réagir automatiquement
    public System.Action<float> OnSFXVolumeChanged;

    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je m'initialise au démarrage
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // J'initialise le Singleton
        
        // ✅ CORRECTION 1 : Je me détache de mon parent pour être à la racine
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        // ✅ CORRECTION 2 : Maintenant je peux me rendre persistant
        DontDestroyOnLoad(gameObject); // Je me rends persistant entre les scènes
        
        // J'initialise mes données de manière préventive pour éviter les erreurs null
        _currentSettings = new GameData(); 
    }

    /// <summary>
    /// Je charge les paramètres sauvegardés au démarrage
    /// </summary>
    private void Start()
    {
        LoadSettings(); // Je charge les paramètres depuis le SaveSystem
    }

    // ---------------------------------------------------------
    // CHARGEMENT
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je charge les paramètres depuis le fichier de sauvegarde
    /// </summary>
    public void LoadSettings()
    {
        // Je vérifie que le SaveSystem existe
        if (SaveSystem.Instance != null)
        {
            // Je récupère les données sauvegardées
            _currentSettings = SaveSystem.Instance.LoadData();
            
            // J'applique les paramètres chargés
            ApplySettings();
        }
        else
        {
            // Si le SaveSystem n'existe pas, j'affiche un avertissement
            Debug.LogWarning("[SettingsManager] SaveSystem non trouvé, chargement des paramètres par défaut.");
        }
    }

    /// <summary>
    /// J'applique les paramètres et j'informe les autres systèmes
    /// </summary>
    private void ApplySettings()
    {
        // Je vérifie que j'ai des paramètres valides
        if (_currentSettings == null) return;

        // J'invoque les événements pour notifier les changements de volume
        // Cela permet à l'AudioManager de mettre à jour ses sources audio
        OnMusicVolumeChanged?.Invoke(_currentSettings.musicVolume);
        OnSFXVolumeChanged?.Invoke(_currentSettings.sfxVolume);

        // J'affiche les paramètres appliqués dans la console
        Debug.Log($"[SettingsManager] Paramètres appliqués : Musique {_currentSettings.musicVolume}, SFX {_currentSettings.sfxVolume}");
    }

    // ---------------------------------------------------------
    // SETTERS (AVEC SAUVEGARDE AUTOMATIQUE)
    // ---------------------------------------------------------

    /// <summary>
    /// Je modifie le volume de la musique et je sauvegarde automatiquement
    /// </summary>
    /// <param name="volume">Le nouveau volume (entre 0.0 et 1.0)</param>
    public void SetMusicVolume(float volume)
    {
        // Je vérifie que j'ai des paramètres valides
        if (_currentSettings == null) return;

        // Je mets à jour le volume en m'assurant qu'il soit entre 0 et 1
        _currentSettings.musicVolume = Mathf.Clamp01(volume);
        
        // J'invoque l'événement pour notifier le changement
        // L'AudioManager recevra cette notification et mettra à jour son volume
        OnMusicVolumeChanged?.Invoke(_currentSettings.musicVolume);
        
        // Je sauvegarde automatiquement le changement
        SaveSettings();
    }

    /// <summary>
    /// Je modifie le volume des effets sonores et je sauvegarde automatiquement
    /// </summary>
    /// <param name="volume">Le nouveau volume (entre 0.0 et 1.0)</param>
    public void SetSFXVolume(float volume)
    {
        // Je vérifie que j'ai des paramètres valides
        if (_currentSettings == null) return;

        // Je mets à jour le volume en m'assurant qu'il soit entre 0 et 1
        _currentSettings.sfxVolume = Mathf.Clamp01(volume);
        
        // J'invoque l'événement pour notifier le changement
        // L'AudioManager recevra cette notification et mettra à jour son volume
        OnSFXVolumeChanged?.Invoke(_currentSettings.sfxVolume);
        
        // Je sauvegarde automatiquement le changement
        SaveSettings();
    }

    /// <summary>
    /// J'active ou désactive l'affichage du tutoriel et je sauvegarde automatiquement
    /// </summary>
    /// <param name="show">True pour afficher le tutoriel, False pour le masquer</param>
    public void SetShowTutorial(bool show)
    {
        // Je vérifie que j'ai des paramètres valides
        if (_currentSettings == null) return;

        // Je mets à jour le paramètre du tutoriel
        _currentSettings.showTutorial = show;
        
        // Je sauvegarde automatiquement le changement
        SaveSettings();
    }

    // ---------------------------------------------------------
    // SAUVEGARDE
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je sauvegarde tous les paramètres actuels dans le fichier
    /// </summary>
    public void SaveSettings()
    {
        // Je vérifie que le SaveSystem et mes paramètres existent
        if (SaveSystem.Instance != null && _currentSettings != null)
        {
            // Je demande au SaveSystem de sauvegarder mes paramètres
            SaveSystem.Instance.SaveData(_currentSettings);
            
            // Je confirme la sauvegarde dans la console
            Debug.Log("[SettingsManager] Paramètres sauvegardés automatiquement.");
        }
        else
        {
            // Si quelque chose manque, j'affiche un avertissement
            Debug.LogWarning("[SettingsManager] Échec de la sauvegarde : SaveSystem ou Data manquants.");
        }
    }

    // ---------------------------------------------------------
    // ACCESSEUR DONNÉES BRUTES
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je retourne une référence aux données actuelles
    /// Utilisé par le HighScoreManager pour sauvegarder les scores
    /// </summary>
    /// <returns>Les données de jeu actuelles</returns>
    public GameData GetCurrentData() => _currentSettings;
}