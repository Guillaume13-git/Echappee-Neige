using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Contrôle le menu principal.
/// Affichage du meilleur score, navigation vers les scènes.
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _bestScoreText;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _scoresButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _quitButton;
    
    [Header("Effects (Optional)")]
    [SerializeField] private ParticleSystem _snowParticles;
    
    private void Start()
    {
        Debug.Log("[MenuController] Initialisation du menu principal");
        
        // Afficher le meilleur score
        DisplayBestScore();
        
        // Lier les boutons
        SetupButtons();
        
        // Lancer la musique du menu
        StartMenuMusic();
        
        // Particules de neige
        if (_snowParticles != null)
        {
            _snowParticles.Play();
        }
    }
    
    /// <summary>
    /// Configure les listeners des boutons.
    /// </summary>
    private void SetupButtons()
    {
        if (_playButton != null)
            _playButton.onClick.AddListener(OnPlayClicked);
        else
            Debug.LogWarning("[MenuController] PlayButton non assigné !");
        
        if (_scoresButton != null)
            _scoresButton.onClick.AddListener(OnScoresClicked);
        
        if (_optionsButton != null)
            _optionsButton.onClick.AddListener(OnOptionsClicked);
        
        if (_quitButton != null)
            _quitButton.onClick.AddListener(OnQuitClicked);
    }
    
    /// <summary>
    /// Affiche le meilleur score sauvegardé.
    /// </summary>
    private void DisplayBestScore()
    {
        if (_bestScoreText == null)
        {
            Debug.LogWarning("[MenuController] BestScoreText non assigné !");
            return;
        }
        
        // Vérifier que HighScoreManager existe
        if (HighScoreManager.Instance == null)
        {
            _bestScoreText.text = "Meilleur Score : ---";
            Debug.LogWarning("[MenuController] HighScoreManager introuvable !");
            return;
        }
        
        int[] highScores = HighScoreManager.Instance.HighScores;
        int bestScore = highScores[0];
        
        if (bestScore > 0)
        {
            _bestScoreText.text = $"Meilleur Score : {bestScore:N0}";
        }
        else
        {
            _bestScoreText.text = "Aucun score enregistré";
        }
    }
    
    /// <summary>
    /// Démarre la musique du menu.
    /// </summary>
    private void StartMenuMusic()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[MenuController] AudioManager.Instance est NULL ! Vérifier que AudioManager existe dans la scène.");
            return;
        }
        
        Debug.Log("[MenuController] Lancement de la musique du menu");
        AudioManager.Instance.PlayMenuMusic();
    }
    
    /// <summary>
    /// Bouton Jouer : vérifie si le tutoriel doit être affiché.
    /// </summary>
    private void OnPlayClicked()
    {
        Debug.Log("[MenuController] Bouton JOUER cliqué");
        
        // Son de clic
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("LetsGo");
        }
        
        // Vérifier si le tutoriel doit être affiché
        if (SettingsManager.Instance != null && SettingsManager.Instance.ShowTutorial)
        {
            Debug.Log("[MenuController] Chargement du tutoriel");
            SceneManager.LoadScene("Tutorial");
        }
        else
        {
            Debug.Log("[MenuController] Lancement direct du jeu");
            SceneManager.LoadScene("Gameplay");
        }
    }
    
    /// <summary>
    /// Bouton Scores : charge la scène des meilleurs scores.
    /// </summary>
    private void OnScoresClicked()
    {
        Debug.Log("[MenuController] Bouton SCORES cliqué");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Blip");
        }
        
        SceneManager.LoadScene("Scores");
    }
    
    /// <summary>
    /// Bouton Options : charge la scène des options.
    /// </summary>
    private void OnOptionsClicked()
    {
        Debug.Log("[MenuController] Bouton OPTIONS cliqué");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Blip");
        }
        
        SceneManager.LoadScene("Options");
    }
    
    /// <summary>
    /// Bouton Quitter : ferme l'application.
    /// </summary>
    private void OnQuitClicked()
    {
        Debug.Log("[MenuController] Bouton QUITTER cliqué");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Blip");
        }
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}