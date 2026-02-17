using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Je contrôle le menu des options du jeu.
/// Je gère les paramètres audio, l'affichage des contrôles et le tutoriel.
/// </summary>
public class OptionsController : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider _sonSlider;       // Je stocke le slider du volume des effets sonores
    [SerializeField] private Slider _musiqueSlider;   // Je stocke le slider du volume de la musique
    
    [Header("Buttons")]
    [SerializeField] private Button _afficherControlesButton;  // Je stocke le bouton "Afficher les contrôles"
    [SerializeField] private Button _rejouerTutorielButton;    // Je stocke le bouton "Rejouer le tutoriel"
    [SerializeField] private Button _retourButton;             // Je stocke le bouton "Retour"
    
    [Header("Controles Panel")]
    [SerializeField] private GameObject _controlesPanel;       // Je stocke le panneau d'affichage des contrôles
    [SerializeField] private Button _fermerControlesButton;    // Je stocke le bouton "Fermer" du panneau contrôles
    
    /// <summary>
    /// Je m'initialise au démarrage du menu Options
    /// </summary>
    private void Start()
    {
        // ---------------------------------------------------------
        // 1. CHARGEMENT DES VALEURS ACTUELLES
        // ---------------------------------------------------------
        
        // Je charge les paramètres actuels depuis le SettingsManager
        LoadCurrentSettings();
        
        // ---------------------------------------------------------
        // 2. LIAISON DES SLIDERS
        // ---------------------------------------------------------
        
        // Je m'abonne aux changements du slider de volume son
        _sonSlider?.onValueChanged.AddListener(OnSonVolumeChanged);
        
        // Je m'abonne aux changements du slider de volume musique
        _musiqueSlider?.onValueChanged.AddListener(OnMusiqueVolumeChanged);
        
        // ---------------------------------------------------------
        // 3. LIAISON DES BOUTONS
        // ---------------------------------------------------------
        
        // Je lie le bouton "Afficher les contrôles"
        _afficherControlesButton?.onClick.AddListener(OnAfficherControlesClicked);
        
        // Je lie le bouton "Rejouer le tutoriel"
        _rejouerTutorielButton?.onClick.AddListener(OnRejouerTutorielClicked);
        
        // Je lie le bouton "Retour"
        _retourButton?.onClick.AddListener(OnRetourClicked);
        
        // Je lie le bouton "Fermer" du panneau contrôles
        _fermerControlesButton?.onClick.AddListener(OnFermerControlesClicked);

        // ---------------------------------------------------------
        // 4. ÉTAT INITIAL DE L'UI
        // ---------------------------------------------------------
        
        // Je cache le panneau des contrôles au démarrage
        if (_controlesPanel != null) 
            _controlesPanel.SetActive(false);
        
        // ---------------------------------------------------------
        // 5. MUSIQUE DU MENU
        // ---------------------------------------------------------
        
        // Je joue la musique du menu
        // L'AudioManager gère déjà de ne pas couper si c'est la même musique qui joue
        AudioManager.Instance?.PlayMenuMusic();
    }
    
    /// <summary>
    /// Je charge les paramètres actuels depuis le SettingsManager
    /// </summary>
    private void LoadCurrentSettings()
    {
        // Je vérifie que le SettingsManager existe
        if (SettingsManager.Instance == null) return;
        
        // Je charge la valeur du volume son dans le slider
        if (_sonSlider != null) 
            _sonSlider.value = SettingsManager.Instance.SFXVolume;
        
        // Je charge la valeur du volume musique dans le slider
        if (_musiqueSlider != null) 
            _musiqueSlider.value = SettingsManager.Instance.MusicVolume;
    }
    
    /// <summary>
    /// Je gère le changement du volume des effets sonores
    /// </summary>
    /// <param name="value">La nouvelle valeur du slider (0 à 1)</param>
    private void OnSonVolumeChanged(float value)
    {
        // Je mets à jour le volume son via le SettingsManager
        // Le SettingsManager notifiera l'AudioManager automatiquement
        SettingsManager.Instance?.SetSFXVolume(value);
        
        // Note : Je ne joue pas de son de test ici pour éviter le spam
        // Le joueur entendra le changement avec les sons du menu
    }

    /// <summary>
    /// Je gère le changement du volume de la musique
    /// </summary>
    /// <param name="value">La nouvelle valeur du slider (0 à 1)</param>
    private void OnMusiqueVolumeChanged(float value)
    {
        // Je mets à jour le volume musique via le SettingsManager
        // Le SettingsManager notifiera l'AudioManager automatiquement
        SettingsManager.Instance?.SetMusicVolume(value);
    }
    
    /// <summary>
    /// Je gère le clic sur le bouton "Afficher les contrôles"
    /// </summary>
    private void OnAfficherControlesClicked()
    {
        // Je joue un son de clic
        AudioManager.Instance?.PlaySFX("Blip");
        
        // J'affiche le panneau des contrôles
        _controlesPanel?.SetActive(true);
    }

    /// <summary>
    /// Je gère le clic sur le bouton "Fermer" du panneau contrôles
    /// </summary>
    private void OnFermerControlesClicked()
    {
        // Je joue un son de clic
        AudioManager.Instance?.PlaySFX("Blip");
        
        // Je cache le panneau des contrôles
        _controlesPanel?.SetActive(false);
    }

    /// <summary>
    /// Je gère le clic sur le bouton "Rejouer le tutoriel"
    /// </summary>
    private void OnRejouerTutorielClicked()
    {
        // Je joue le son "Let's Go!"
        AudioManager.Instance?.PlaySFX("LetsGo");
        
        // J'active l'option pour afficher le tutoriel
        SettingsManager.Instance?.SetShowTutorial(true);
        
        // Je charge la scène du tutoriel
        SceneManager.LoadScene("Tutorial");
    }
    
    /// <summary>
    /// Je gère le clic sur le bouton "Retour"
    /// </summary>
    private void OnRetourClicked()
    {
        // Je joue un son de clic
        AudioManager.Instance?.PlaySFX("Blip");
        
        // Je sauvegarde tous les paramètres avant de quitter
        SettingsManager.Instance?.SaveSettings();
        
        // Je retourne au menu principal
        SceneManager.LoadScene("MainMenu");
    }
}