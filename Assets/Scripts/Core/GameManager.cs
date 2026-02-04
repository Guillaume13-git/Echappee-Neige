using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Je suis le gestionnaire principal du jeu.
/// Je contrôle l'état du jeu, les transitions entre scènes, et la sauvegarde des scores.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("Game State")]
    [SerializeField] private GameState _currentState = GameState.MainMenu; // Je stocke l'état actuel du jeu
    [SerializeField] private bool _isPaused = false; // Je stocke si le jeu est en pause

    // Je donne accès en lecture seule à l'état actuel
    public GameState CurrentState => _currentState;
    
    // Je donne accès en lecture seule à l'état de pause
    public bool IsPaused => _isPaused;

    // Je détermine si le jeu est actif (en cours ou en tutoriel)
    public bool IsGameActive => _currentState == GameState.Playing || _currentState == GameState.Tutorial;

    // Je fournis des événements pour notifier les changements d'état
    public System.Action<GameState> OnGameStateChanged; // J'invoque cet événement quand l'état change
    public System.Action OnGamePaused;                  // J'invoque cet événement quand le jeu est mis en pause
    public System.Action OnGameResumed;                 // J'invoque cet événement quand le jeu reprend

    /// <summary>
    /// Je m'initialise au démarrage
    /// </summary>
    protected override void Awake()
    {
        // Je m'assure de ne pas avoir de parent pour persister correctement
        if (transform.parent != null) transform.SetParent(null);
        
        base.Awake(); // J'initialise le Singleton
        
        // Je me rends persistant entre les changements de scènes
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Je charge les paramètres et détecte l'état initial au démarrage
    /// </summary>
    private void Start()
    {
        // Je demande au SettingsManager de charger les paramètres sauvegardés
        SettingsManager.Instance?.LoadSettings();
        
        // Je détecte dans quelle scène je me trouve pour définir mon état initial
        DetectInitialState();
    }

    /// <summary>
    /// Je détecte l'état initial du jeu en fonction de la scène active
    /// </summary>
    private void DetectInitialState()
    {
        // Je récupère le nom de la scène actuelle
        string sceneName = SceneManager.GetActiveScene().name;
        
        // Je détermine mon état en fonction du nom de la scène
        _currentState = sceneName switch
        {
            "MainMenu" => GameState.MainMenu,   // Si je suis dans le menu principal
            "Tutorial" => GameState.Tutorial,   // Si je suis dans le tutoriel
            "Gameplay" => GameState.Playing,    // Si je suis dans le gameplay
            "GameOver" => GameState.GameOver,   // Si je suis dans l'écran de game over
            _          => GameState.MainMenu    // Par défaut, je considère que je suis dans le menu
        };
        
        // J'affiche l'état détecté dans la console
        Debug.Log($"[GameManager] État initial : {_currentState}");
    }

    /// <summary>
    /// Je change l'état du jeu et applique les modifications nécessaires
    /// </summary>
    /// <param name="newState">Le nouvel état du jeu</param>
    public void SetGameState(GameState newState)
    {
        // Si l'état demandé est le même que l'actuel, je ne fais rien
        if (_currentState == newState) return;

        // Je mets à jour mon état
        _currentState = newState;
        
        // J'invoque l'événement pour notifier les autres scripts
        OnGameStateChanged?.Invoke(newState);

        // J'applique les modifications selon le nouvel état
        switch (newState)
        {
            case GameState.Playing:
            case GameState.Tutorial:
                Time.timeScale = 1f;      // Je remets le temps normal
                _isPaused = false;        // Je désactive l'état de pause
                break;
                
            case GameState.Paused:
            case GameState.GameOver:
                Time.timeScale = 0f;      // Je fige le temps
                
                // Si c'est un game over, je sauvegarde le score final
                if (newState == GameState.GameOver) 
                    SaveFinalScore();
                break;
        }
    }

    /// <summary>
    /// Je démarre une nouvelle partie (appelé par le bouton "Jouer" du menu principal)
    /// </summary>
    public void StartNewGame()
    {
        // Je réinitialise tous les managers pour repartir à zéro
        ResetAllManagers();

        // Je vérifie si le tutoriel doit être affiché
        if (SettingsManager.Instance != null && SettingsManager.Instance.ShowTutorial)
        {
            // Si oui, je lance le tutoriel
            StartTutorial();
        }
        else
        {
            // Sinon, je lance directement le gameplay
            StartGameplay();
        }
    }

    /// <summary>
    /// Je démarre le tutoriel
    /// </summary>
    public void StartTutorial()
    {
        // Je change mon état vers Tutorial
        SetGameState(GameState.Tutorial);
        
        // Je charge la scène du tutoriel
        SceneManager.LoadScene("Tutorial");
    }

    /// <summary>
    /// Je termine le tutoriel et je lance le gameplay
    /// (Appelé à la fin du tutoriel)
    /// </summary>
    public void CompleteTutorial()
    {
        Debug.Log("[GameManager] Tutoriel complété ! Passage au Gameplay.");
        
        // Optionnel : Je peux désactiver le tuto pour les prochaines parties
        // SettingsManager.Instance?.SetTutorialCompleted(); 

        // Je lance le gameplay
        StartGameplay();
    }

    /// <summary>
    /// Je démarre le gameplay principal
    /// </summary>
    private void StartGameplay()
    {
        // Je change mon état vers Playing
        SetGameState(GameState.Playing);
        
        // Je charge la scène de gameplay
        SceneManager.LoadScene("Gameplay");
    }

    // ---------------------------------------------------------
    // GESTION DES MANAGERS
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je réinitialise tous les managers pour une nouvelle partie
    /// </summary>
    private void ResetAllManagers()
    {
        // Je demande au ScoreManager de réinitialiser le score
        ScoreManager.Instance?.ResetScore();
        
        // Je demande au ThreatManager de réinitialiser la menace
        ThreatManager.Instance?.ResetThreat();
        
        // Je demande au PhaseManager de réinitialiser les phases
        PhaseManager.Instance?.ResetPhases();
    }

    // ---------------------------------------------------------
    // GESTION DE LA PAUSE
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je mets le jeu en pause
    /// </summary>
    public void PauseGame()
    {
        // Je ne mets en pause que si le jeu est actif
        if (IsGameActive) 
            SetGameState(GameState.Paused);
    }

    /// <summary>
    /// Je reprends le jeu après une pause
    /// </summary>
    public void ResumeGame()
    {
        // Je ne reprends que si je suis en pause
        if (_currentState == GameState.Paused)
        {
            // Je détecte si je dois retourner en Tutorial ou en Playing
            GameState resumeState = SceneManager.GetActiveScene().name == "Tutorial" 
                ? GameState.Tutorial 
                : GameState.Playing;
                
            SetGameState(resumeState);
        }
    }

    // ---------------------------------------------------------
    // NAVIGATION
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je retourne au menu principal
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;              // Je remets le temps normal
        SetGameState(GameState.MainMenu); // Je change mon état
        SceneManager.LoadScene("MainMenu"); // Je charge la scène du menu
    }

    // ---------------------------------------------------------
    // GAME OVER
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je déclenche un game over
    /// </summary>
    public void TriggerGameOver() => SetGameState(GameState.GameOver);

    /// <summary>
    /// Je sauvegarde le score final du joueur
    /// </summary>
    private void SaveFinalScore()
    {
        // Je vérifie que les managers nécessaires existent
        if (ScoreManager.Instance != null && HighScoreManager.Instance != null)
        {
            // Je demande au HighScoreManager d'ajouter le score actuel
            HighScoreManager.Instance.AddScore(ScoreManager.Instance.CurrentScore);
        }
    }

    // ---------------------------------------------------------
    // QUITTER LE JEU
    // ---------------------------------------------------------
    
    /// <summary>
    /// Je quitte l'application
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Si je suis dans l'éditeur Unity, j'arrête le mode Play
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Sinon, je quitte l'application
        Application.Quit();
#endif
    }
}