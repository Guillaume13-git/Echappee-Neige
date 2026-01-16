using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contrôle l'écran des options.
/// Réglages audio, tutoriel, affichage des contrôles.
/// </summary>
public class OptionsController : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI _musicVolumeText;
    [SerializeField] private TextMeshProUGUI _sfxVolumeText;
    
    [Header("Tutorial Toggle")]
    [SerializeField] private Toggle _tutorialToggle;
    
    [Header("Buttons")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _showControlsButton;
    
    [Header("Controls Panel")]
    [SerializeField] private GameObject _controlsPanel;
    
    private void Start()
    {
        // Charger les valeurs sauvegardées
        LoadCurrentSettings();
        
        // Lier les événements
        _musicVolumeSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
        _sfxVolumeSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
        _tutorialToggle?.onValueChanged.AddListener(OnTutorialToggleChanged);
        
        _saveButton?.onClick.AddListener(OnSaveClicked);
        _backButton?.onClick.AddListener(OnBackClicked);
        _showControlsButton?.onClick.AddListener(OnShowControlsClicked);
        
        // Masquer le panneau de contrôles
        _controlsPanel?.SetActive(false);
    }
    
    /// <summary>
    /// Charge les paramètres actuels dans l'UI.
    /// </summary>
    private void LoadCurrentSettings()
    {
        if (_musicVolumeSlider != null)
        {
            _musicVolumeSlider.value = SettingsManager.Instance.MusicVolume;
            UpdateMusicVolumeText(SettingsManager.Instance.MusicVolume);
        }
        
        if (_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.value = SettingsManager.Instance.SFXVolume;
            UpdateSFXVolumeText(SettingsManager.Instance.SFXVolume);
        }
        
        if (_tutorialToggle != null)
        {
            _tutorialToggle.isOn = SettingsManager.Instance.ShowTutorial;
        }
    }
    
    /// <summary>
    /// Callback du slider de volume musique.
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
        UpdateMusicVolumeText(value);
    }
    
    /// <summary>
    /// Callback du slider de volume SFX.
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        SettingsManager.Instance.SetSFXVolume(value);
        UpdateSFXVolumeText(value);
        
        // Test du nouveau volume
        AudioManager.Instance?.PlaySFX("Blip");
    }
    
    /// <summary>
    /// Callback du toggle tutoriel.
    /// </summary>
    private void OnTutorialToggleChanged(bool isOn)
    {
        SettingsManager.Instance.SetShowTutorial(isOn);
    }
    
    /// <summary>
    /// Met à jour le texte du volume musique.
    /// </summary>
    private void UpdateMusicVolumeText(float value)
    {
        if (_musicVolumeText != null)
        {
            _musicVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
    
    /// <summary>
    /// Met à jour le texte du volume SFX.
    /// </summary>
    private void UpdateSFXVolumeText(float value)
    {
        if (_sfxVolumeText != null)
        {
            _sfxVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }
    
    /// <summary>
    /// Bouton Sauvegarder : sauvegarde et retourne au menu.
    /// </summary>
    private void OnSaveClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        SettingsManager.Instance.SaveSettings();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Bouton Retour : retourne au menu sans sauvegarder.
    /// </summary>
    private void OnBackClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Bouton Afficher les contrôles.
    /// </summary>
    private void OnShowControlsClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        _controlsPanel?.SetActive(!_controlsPanel.activeSelf);
    }
}