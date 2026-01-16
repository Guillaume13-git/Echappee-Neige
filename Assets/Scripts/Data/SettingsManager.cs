using UnityEngine;

/// <summary>
/// Gère les paramètres du jeu : volumes, tutoriel, etc.
/// Sauvegarde et charge via SaveSystem.
/// </summary>
public class SettingsManager : Singleton<SettingsManager>
{
    private GameData _currentSettings;

    public float MusicVolume => _currentSettings.musicVolume;
    public float SFXVolume => _currentSettings.sfxVolume;
    public bool ShowTutorial => _currentSettings.showTutorial;

    public System.Action<float> OnMusicVolumeChanged;
    public System.Action<float> OnSFXVolumeChanged;


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    protected override void Awake()
    {
        base.Awake(); // ← Initialise le Singleton
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadSettings();
    }


    // ---------------------------------------------------------
    // LOAD & APPLY
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
            Debug.LogWarning("[SettingsManager] SaveSystem non trouvé, impossible de charger les paramètres.");
        }
    }

    private void ApplySettings()
    {
        OnMusicVolumeChanged?.Invoke(_currentSettings.musicVolume);
        OnSFXVolumeChanged?.Invoke(_currentSettings.sfxVolume);

        Debug.Log($"[SettingsManager] Paramètres appliqués : Music {_currentSettings.musicVolume}, SFX {_currentSettings.sfxVolume}");
    }


    // ---------------------------------------------------------
    // SETTERS
    // ---------------------------------------------------------
    public void SetMusicVolume(float volume)
    {
        _currentSettings.musicVolume = Mathf.Clamp01(volume);
        OnMusicVolumeChanged?.Invoke(_currentSettings.musicVolume);
    }

    public void SetSFXVolume(float volume)
    {
        _currentSettings.sfxVolume = Mathf.Clamp01(volume);
        OnSFXVolumeChanged?.Invoke(_currentSettings.sfxVolume);
    }

    public void SetShowTutorial(bool show)
    {
        _currentSettings.showTutorial = show;
    }


    // ---------------------------------------------------------
    // SAVE
    // ---------------------------------------------------------
    public void SaveSettings()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveData(_currentSettings);
            Debug.Log("[SettingsManager] Paramètres sauvegardés.");
        }
        else
        {
            Debug.LogWarning("[SettingsManager] SaveSystem non trouvé, impossible de sauvegarder.");
        }
    }


    // ---------------------------------------------------------
    // ACCESSOR
    // ---------------------------------------------------------
    public GameData GetCurrentData()
    {
        return _currentSettings;
    }
}