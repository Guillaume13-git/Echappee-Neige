using UnityEngine;

/// <summary>
/// Gère les paramètres du jeu (volumes, tutoriel).
/// Sauvegarde automatiquement chaque modification via le SaveSystem.
/// </summary>
public class SettingsManager : Singleton<SettingsManager>
{
    private GameData _currentSettings;

    // Accesseurs
    public float MusicVolume => _currentSettings != null ? _currentSettings.musicVolume : 1f;
    public float SFXVolume => _currentSettings != null ? _currentSettings.sfxVolume : 1f;
    public bool ShowTutorial => _currentSettings != null && _currentSettings.showTutorial;

    // Événements pour informer les autres systèmes (ex: AudioManager)
    public System.Action<float> OnMusicVolumeChanged;
    public System.Action<float> OnSFXVolumeChanged;

    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    protected override void Awake()
    {
        base.Awake(); // Initialise le Singleton
        DontDestroyOnLoad(gameObject);
        
        // Initialisation préventive des données
        _currentSettings = new GameData(); 
    }

    private void Start()
    {
        LoadSettings();
    }

    // ---------------------------------------------------------
    // CHARGEMENT
    // ---------------------------------------------------------
    public void LoadSettings()
    {
        if (SaveSystem.Instance != null)
        {
            _currentSettings = SaveSystem.Instance.LoadData();
            ApplySettings();
        }
        else
        {
            Debug.LogWarning("[SettingsManager] SaveSystem non trouvé, chargement des paramètres par défaut.");
        }
    }

    private void ApplySettings()
    {
        if (_currentSettings == null) return;

        OnMusicVolumeChanged?.Invoke(_currentSettings.musicVolume);
        OnSFXVolumeChanged?.Invoke(_currentSettings.sfxVolume);

        Debug.Log($"[SettingsManager] Paramètres appliqués : Musique {_currentSettings.musicVolume}, SFX {_currentSettings.sfxVolume}");
    }

    // ---------------------------------------------------------
    // SETTERS (AVEC SAUVEGARDE AUTO)
    // ---------------------------------------------------------

    /// <summary>
    /// Modifie le volume de la musique et sauvegarde.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (_currentSettings == null) return;

        _currentSettings.musicVolume = Mathf.Clamp01(volume);
        OnMusicVolumeChanged?.Invoke(_currentSettings.musicVolume);
        
        SaveSettings();
    }

    /// <summary>
    /// Modifie le volume des SFX et sauvegarde.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (_currentSettings == null) return;

        _currentSettings.sfxVolume = Mathf.Clamp01(volume);
        OnSFXVolumeChanged?.Invoke(_currentSettings.sfxVolume);
        
        SaveSettings();
    }

    /// <summary>
    /// Active/désactive le tutoriel et sauvegarde.
    /// </summary>
    public void SetShowTutorial(bool show)
    {
        if (_currentSettings == null) return;

        _currentSettings.showTutorial = show;
        
        SaveSettings();
    }

    // ---------------------------------------------------------
    // SAUVEGARDE
    // ---------------------------------------------------------
    public void SaveSettings()
    {
        if (SaveSystem.Instance != null && _currentSettings != null)
        {
            SaveSystem.Instance.SaveData(_currentSettings);
            Debug.Log("[SettingsManager] Paramètres sauvegardés automatiquement.");
        }
        else
        {
            Debug.LogWarning("[SettingsManager] Échec de la sauvegarde : SaveSystem ou Data manquants.");
        }
    }

    // Accesseur pour obtenir une copie brute des données
    public GameData GetCurrentData() => _currentSettings;
}