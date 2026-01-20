using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsController : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider _sonSlider;
    [SerializeField] private Slider _musiqueSlider;
    
    [Header("Buttons")]
    [SerializeField] private Button _afficherControlesButton;
    [SerializeField] private Button _rejouerTutorielButton;
    [SerializeField] private Button _retourButton;
    
    [Header("Controles Panel")]
    [SerializeField] private GameObject _controlesPanel;
    [SerializeField] private Button _fermerControlesButton;
    
    private void Start()
    {
        // 1. Charger les valeurs
        LoadCurrentSettings();
        
        // 2. Lier les Sliders
        _sonSlider?.onValueChanged.AddListener(OnSonVolumeChanged);
        _musiqueSlider?.onValueChanged.AddListener(OnMusiqueVolumeChanged);
        
        // 3. Lier les Boutons
        _afficherControlesButton?.onClick.AddListener(OnAfficherControlesClicked);
        _rejouerTutorielButton?.onClick.AddListener(OnRejouerTutorielClicked);
        _retourButton?.onClick.AddListener(OnRetourClicked);
        _fermerControlesButton?.onClick.AddListener(OnFermerControlesClicked);

        // 4. État initial
        if (_controlesPanel != null) _controlesPanel.SetActive(false);
        
        // 5. Musique (L'AudioManager gère déjà de ne pas couper si c'est la même)
        AudioManager.Instance?.PlayMenuMusic();
    }
    
    private void LoadCurrentSettings()
    {
        if (SettingsManager.Instance == null) return;
        
        if (_sonSlider != null) _sonSlider.value = SettingsManager.Instance.SFXVolume;
        if (_musiqueSlider != null) _musiqueSlider.value = SettingsManager.Instance.MusicVolume;
    }
    
    private void OnSonVolumeChanged(float value)
    {
        SettingsManager.Instance?.SetSFXVolume(value);
        // On ne joue le son de test que si l'utilisateur ne fait pas que glisser rapidement
        // ou on accepte que ça "spam" le blip pour le retour sonore.
    }

    private void OnMusiqueVolumeChanged(float value)
    {
        SettingsManager.Instance?.SetMusicVolume(value);
    }
    
    private void OnAfficherControlesClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        _controlesPanel?.SetActive(true);
    }

    private void OnFermerControlesClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        _controlesPanel?.SetActive(false);
    }

    private void OnRejouerTutorielClicked()
    {
        AudioManager.Instance?.PlaySFX("LetsGo");
        SettingsManager.Instance?.SetShowTutorial(true);
        SceneManager.LoadScene("Tutorial");
    }
    
    private void OnRetourClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        SettingsManager.Instance?.SaveSettings();
        SceneManager.LoadScene("MainMenu");
    }
}