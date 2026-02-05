using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Je contrôle le menu principal du jeu.
/// Je gère la navigation entre les scènes avec des délais pour laisser les sons se jouer.
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _bestScoreText;  // Je stocke le texte d'affichage du meilleur score
    [SerializeField] private Button _playButton;              // Je stocke le bouton Jouer
    [SerializeField] private Button _scoresButton;            // Je stocke le bouton Scores
    [SerializeField] private Button _optionsButton;           // Je stocke le bouton Options
    [SerializeField] private Button _quitButton;              // Je stocke le bouton Quitter

    private bool _isNavigating = false;  // Je stocke si une navigation est en cours (pour éviter les doubles clics)

    /// <summary>
    /// Je m'initialise au démarrage du menu
    /// </summary>
    private void Start()
    {
        // Je configure les boutons
        SetupButtons();
        
        // J'affiche le meilleur score
        DisplayBestScore();
        
        // Je lance la musique du menu
        CheckAndPlayMenuMusic();
    }

    /// <summary>
    /// Je vérifie que l'AudioManager existe et je lance la musique du menu
    /// </summary>
    private void CheckAndPlayMenuMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    /// <summary>
    /// Je configure tous les boutons du menu
    /// </summary>
    private void SetupButtons()
    {
        // Je retire tous les listeners existants pour éviter les doublons
        _playButton?.onClick.RemoveAllListeners();
        _optionsButton?.onClick.RemoveAllListeners();
        _scoresButton?.onClick.RemoveAllListeners();
        _quitButton?.onClick.RemoveAllListeners();

        // ---------------------------------------------------------
        // BOUTON PLAY (CORRECTION CRITIQUE)
        // ---------------------------------------------------------
        
        // ✅ CORRECTION : Le bouton Play passe maintenant par GameManager
        // Avant, je chargeais la scène directement sans changer l'état du jeu.
        // Maintenant, GameManager.StartNewGame() gère l'état ET choisit la bonne scène.
        _playButton?.onClick.AddListener(OnPlayClicked);

        // ---------------------------------------------------------
        // BOUTONS DE NAVIGATION SIMPLES
        // ---------------------------------------------------------
        
        // Ces deux sont des navigations simples vers des menus
        // Pas de changement d'état nécessaire
        _optionsButton?.onClick.AddListener(() => OnClickNavigation("Options", "Blip"));
        _scoresButton?.onClick.AddListener(() => OnClickNavigation("Scores", "Blip"));
        
        // Bouton Quitter
        _quitButton?.onClick.AddListener(QuitGame);
    }

    /// <summary>
    /// Je gère le clic sur le bouton Play
    /// Je joue un son puis je délègue le démarrage du jeu au GameManager
    /// </summary>
    private void OnPlayClicked()
    {
        // Si une navigation est déjà en cours, je ne fais rien (évite les doubles clics)
        if (_isNavigating) return;
        
        // Je démarre la coroutine de démarrage avec délai
        StartCoroutine(DelayedStartGame());
    }

    /// <summary>
    /// J'attends que le son "LetsGo" se termine avant de lancer le jeu via GameManager
    /// </summary>
    private IEnumerator DelayedStartGame()
    {
        // Je marque qu'une navigation est en cours
        _isNavigating = true;

        // Je joue le son "Let's Go!"
        AudioManager.Instance?.PlaySFX("LetsGo");

        // J'attends 0.7 seconde pour laisser le son se jouer
        yield return new WaitForSecondsRealtime(0.7f);

        // ✅ Je passe par GameManager qui gère l'état ET choisit la bonne scène
        // GameManager décide si on va en Tutorial ou en Gameplay selon les paramètres
        GameManager.Instance?.StartNewGame();
    }

    /// <summary>
    /// Je gère la navigation générique pour les menus (Options, Scores)
    /// Je ne change pas l'état du jeu, c'est juste un changement de scène menu
    /// </summary>
    /// <param name="sceneName">Le nom de la scène à charger</param>
    /// <param name="sfx">Le nom du son à jouer</param>
    private void OnClickNavigation(string sceneName, string sfx)
    {
        // Si une navigation est déjà en cours, je ne fais rien
        if (_isNavigating) return;
        
        // Je démarre la coroutine de navigation avec délai
        StartCoroutine(NavWithDelay(sceneName, sfx));
    }

    /// <summary>
    /// Je joue le son, j'attends, puis je change de scène
    /// </summary>
    /// <param name="sceneName">Le nom de la scène à charger</param>
    /// <param name="sfx">Le nom du son à jouer</param>
    private IEnumerator NavWithDelay(string sceneName, string sfx)
    {
        // Je marque qu'une navigation est en cours
        _isNavigating = true;

        // Je joue le son
        AudioManager.Instance?.PlaySFX(sfx);

        // J'attends 0.7 seconde pour laisser le son se jouer
        yield return new WaitForSecondsRealtime(0.7f);

        // Je charge la scène demandée
        Debug.Log($"[MenuController] Chargement de la scène : {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// J'affiche le meilleur score sur le menu principal
    /// </summary>
    private void DisplayBestScore()
    {
        // Je vérifie que j'ai le texte de meilleur score
        if (_bestScoreText == null) return;

        // Je vérifie que le HighScoreManager existe et qu'il a des scores
        if (HighScoreManager.Instance != null && HighScoreManager.Instance.HighScores.Length > 0)
        {
            // Je récupère le premier score (le meilleur)
            int score = HighScoreManager.Instance.HighScores[0];
            
            // Si le score est supérieur à 0, je l'affiche formaté
            // Sinon, j'affiche "Aucun score enregistré"
            _bestScoreText.text = score > 0 
                ? $"Meilleur Score : {score:N0}"  // N0 = format avec espaces de milliers
                : "Aucun score enregistré";
        }
        else
        {
            // Si le HighScoreManager n'existe pas ou n'a pas de scores
            _bestScoreText.text = "Meilleur Score : ---";
        }
    }

    /// <summary>
    /// Je gère le clic sur le bouton Quitter
    /// </summary>
    private void QuitGame()
    {
        // Si une navigation est déjà en cours, je ne fais rien
        if (_isNavigating) return;

        Debug.Log("[MenuController] Quitter le jeu");
        
        // Je joue un son de confirmation
        AudioManager.Instance?.PlaySFX("Blip");

#if UNITY_EDITOR
        // Si je suis dans l'éditeur Unity, j'arrête le mode Play
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Sinon, je quitte l'application
        Application.Quit();
#endif
    }
}