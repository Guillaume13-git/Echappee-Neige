using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Je contrôle le menu Game Over.
/// Je gère l'affichage du score final et la navigation vers les autres scènes.
/// </summary>
public class GameOverMenuController : MonoBehaviour
{
    [Header("UI Elements - Score Display")]
    [SerializeField] private TextMeshProUGUI _finalScoreText;      // Je stocke le texte du score final
    [SerializeField] private TextMeshProUGUI _bestScoreText;       // Je stocke le texte du meilleur score (optionnel)
    [SerializeField] private TextMeshProUGUI _gameOverTitleText;   // Je stocke le titre "GAME OVER" (optionnel)

    [Header("UI Elements - Buttons")]
    [SerializeField] private Button _restartButton;      // Je stocke le bouton Recommencer
    [SerializeField] private Button _mainMenuButton;     // Je stocke le bouton Menu Principal
    [SerializeField] private Button _quitButton;         // Je stocke le bouton Quitter

    [Header("Animation Settings (Optionnel)")]
    [SerializeField] private bool _animateScoreAppearance = true;   // J'anime l'apparition du score
    [SerializeField] private float _scoreAnimationDuration = 1f;    // Je stocke la durée de l'animation du score

    private bool _isNavigating = false;  // Je stocke si une navigation est en cours (pour éviter les doubles clics)
    private int _finalScore = 0;         // Je stocke le score final de la partie

    /// <summary>
    /// Je m'initialise au démarrage du menu Game Over
    /// </summary>
    private void Start()
    {
        // Je configure les boutons
        SetupButtons();
        
        // J'affiche le score final
        DisplayFinalScore();
        
        // J'affiche le meilleur score
        DisplayBestScore();
        
        // Je lance la musique du Game Over
        CheckAndPlayGameOverMusic();
    }

    /// <summary>
    /// Je vérifie que l'AudioManager existe et je gère la musique du Game Over
    /// </summary>
    private void CheckAndPlayGameOverMusic()
    {
        // Je vérifie que l'AudioManager existe
        if (AudioManager.Instance != null)
        {
            // --- OPTION A : Si vous avez une musique de Game Over ---
            // AudioManager.Instance.PlayMusic("GameOverTheme"); 

            // --- OPTION B : Si vous voulez juste arrêter la musique actuelle ---
            // Si l'erreur persiste, c'est que StopMusic() n'est pas le nom de votre méthode.
            // Vérifiez si votre AudioManager utilise : StopBackgroundMusic(), StopAll(), etc.
            
            // AudioManager.Instance.StopMusic(); // Décommentez si vous renommez la méthode dans AudioManager
        }
    }

    /// <summary>
    /// Je configure tous les boutons du menu Game Over
    /// </summary>
    private void SetupButtons()
    {
        // Je retire tous les listeners existants pour éviter les doublons
        _restartButton?.onClick.RemoveAllListeners();
        _mainMenuButton?.onClick.RemoveAllListeners();
        _quitButton?.onClick.RemoveAllListeners();

        // J'assigne les callbacks aux boutons
        _restartButton?.onClick.AddListener(OnRestartClicked);
        _mainMenuButton?.onClick.AddListener(OnMainMenuClicked);
        _quitButton?.onClick.AddListener(OnQuitClicked);
    }

    /// <summary>
    /// Je gère le clic sur le bouton Recommencer
    /// </summary>
    private void OnRestartClicked()
    {
        // Si une navigation est déjà en cours, je ne fais rien (évite les doubles clics)
        if (_isNavigating) return;
        
        // Je démarre la coroutine de redémarrage avec délai
        StartCoroutine(DelayedRestart());
    }

    /// <summary>
    /// J'attends que le son se termine avant de relancer le jeu via GameManager
    /// </summary>
    private IEnumerator DelayedRestart()
    {
        // Je marque qu'une navigation est en cours
        _isNavigating = true;
        
        // Je joue le son "Let's Go!"
        AudioManager.Instance?.PlaySFX("LetsGo");

        // J'attends 0.7 seconde pour laisser le son se jouer
        yield return new WaitForSecondsRealtime(0.7f);

        // Je passe par GameManager qui gère l'état ET choisit la bonne scène
        // GameManager décide si on va en Tutorial ou en Gameplay selon les paramètres
        GameManager.Instance?.StartNewGame();
    }

    /// <summary>
    /// Je gère le clic sur le bouton Menu Principal
    /// </summary>
    private void OnMainMenuClicked()
    {
        // Si une navigation est déjà en cours, je ne fais rien
        if (_isNavigating) return;
        
        // Je démarre la coroutine de retour au menu avec délai
        StartCoroutine(DelayedReturnToMenu());
    }

    /// <summary>
    /// J'attends que le son se termine avant de retourner au menu
    /// </summary>
    private IEnumerator DelayedReturnToMenu()
    {
        // Je marque qu'une navigation est en cours
        _isNavigating = true;
        
        // Je joue un son de confirmation
        AudioManager.Instance?.PlaySFX("Blip");

        // J'attends 0.7 seconde pour laisser le son se jouer
        yield return new WaitForSecondsRealtime(0.7f);

        // Je demande au GameManager de retourner au menu principal
        GameManager.Instance?.ReturnToMainMenu();
    }

    /// <summary>
    /// Je gère le clic sur le bouton Quitter
    /// </summary>
    private void OnQuitClicked()
    {
        // Si une navigation est déjà en cours, je ne fais rien
        if (_isNavigating) return;
        
        // Je joue un son de confirmation
        AudioManager.Instance?.PlaySFX("Blip");
        
        // Je démarre la coroutine de sortie avec délai
        StartCoroutine(DelayedQuit());
    }

    /// <summary>
    /// J'attends que le son se termine avant de quitter le jeu
    /// </summary>
    private IEnumerator DelayedQuit()
    {
        // Je marque qu'une navigation est en cours
        _isNavigating = true;
        
        // J'attends 0.5 seconde pour laisser le son se jouer
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Je demande au GameManager de quitter le jeu
        GameManager.Instance?.QuitGame();
    }

    /// <summary>
    /// Je récupère et affiche le score final de la partie
    /// </summary>
    private void DisplayFinalScore()
    {
        // Je vérifie que j'ai le texte de score final
        if (_finalScoreText == null) return;

        // Je récupère le score final depuis le ScoreManager
        if (ScoreManager.Instance != null)
        {
            _finalScore = ScoreManager.Instance.CurrentScore;
        }

        // J'affiche le score avec animation ou directement
        if (_animateScoreAppearance)
        {
            // Je lance l'animation du score
            StartCoroutine(AnimateScoreDisplay());
        }
        else
        {
            // J'affiche le score directement sans animation
            _finalScoreText.text = $"Score : {_finalScore:N0}";
        }
    }

    /// <summary>
    /// J'anime l'affichage du score (compteur qui monte de 0 au score final)
    /// </summary>
    private IEnumerator AnimateScoreDisplay()
    {
        // Je commence avec un temps écoulé à 0
        float elapsedTime = 0f;
        
        // Je commence avec un score affiché à 0
        int displayedScore = 0;

        // Je joue un son de comptage (optionnel)
        // AudioManager.Instance?.PlaySFX("ScoreCount");

        // Tant que le temps écoulé est inférieur à la durée de l'animation
        while (elapsedTime < _scoreAnimationDuration)
        {
            // J'ajoute le temps de cette frame (en ignorant Time.timeScale car il est à 0 en Game Over)
            elapsedTime += Time.unscaledDeltaTime;
            
            // Je calcule le pourcentage d'avancement de l'animation (0 à 1)
            float t = elapsedTime / _scoreAnimationDuration;
            
            // Je calcule le score à afficher avec une interpolation linéaire
            displayedScore = Mathf.RoundToInt(Mathf.Lerp(0, _finalScore, t));
            
            // Je mets à jour le texte avec le score actuel
            _finalScoreText.text = $"Score : {displayedScore:N0}";
            
            // J'attends la prochaine frame
            yield return null;
        }

        // Je m'assure que le score final exact est affiché à la fin
        _finalScoreText.text = $"Score : {_finalScore:N0}";

        // Je joue un son de fin de comptage (optionnel)
        // AudioManager.Instance?.PlaySFX("ScoreComplete");
    }

    /// <summary>
    /// J'affiche le meilleur score et je détecte si c'est un nouveau record
    /// </summary>
    private void DisplayBestScore()
    {
        // Je vérifie que j'ai le texte de meilleur score
        if (_bestScoreText == null) return;

        // Je vérifie que le HighScoreManager existe et qu'il a des scores
        if (HighScoreManager.Instance != null && HighScoreManager.Instance.HighScores.Length > 0)
        {
            // Je récupère le premier score (le meilleur)
            int bestScore = HighScoreManager.Instance.HighScores[0];
            
            // Je vérifie si le score actuel est un nouveau record
            bool isNewRecord = _finalScore >= bestScore && _finalScore > 0;

            if (isNewRecord)
            {
                // Si c'est un nouveau record, je l'affiche avec un style spécial
                _bestScoreText.text = $"🏆 NOUVEAU RECORD ! 🏆\n{bestScore:N0}";
                
                // Je change la couleur en jaune pour le record
                _bestScoreText.color = Color.yellow;
                
                // Je joue un son de célébration
                AudioManager.Instance?.PlaySFX("NewRecord");
            }
            else
            {
                // Sinon, j'affiche le meilleur score normalement
                _bestScoreText.text = $"Meilleur Score : {bestScore:N0}";
            }
        }
        else
        {
            // Si le HighScoreManager n'existe pas ou n'a pas de scores
            _bestScoreText.text = "Meilleur Score : ---";
        }
    }

    /// <summary>
    /// Je me nettoie quand le menu est détruit
    /// </summary>
    private void OnDestroy()
    {
        // Je m'assure de retirer tous les listeners pour éviter les fuites mémoire
        _restartButton?.onClick.RemoveAllListeners();
        _mainMenuButton?.onClick.RemoveAllListeners();
        _quitButton?.onClick.RemoveAllListeners();
    }
}