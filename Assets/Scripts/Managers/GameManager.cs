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

    // Je donne accès en lecture seule aux informations d'état
    public GameState CurrentState => _currentState;
    public bool IsPaused => _isPaused;
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
        Debug.Log($"[GameManager] État initial détecté : {_currentState}");
    }

    /// <summary>
    /// Je change l'état du jeu et applique les modifications de logique (TimeScale, Événements)
    /// </summary>
    /// <param name="newState">Le nouvel état du jeu</param>
    public void SetGameState(GameState newState)
    {
        // Si l'état demandé est le même que l'actuel, je ne fais rien
        if (_currentState == newState) return;

        Debug.Log($"[GameManager] SetGameState : {_currentState} → {newState}");

        // Je mets à jour l'état
        _currentState = newState;
        
        // J'invoque l'événement global de changement d'état
        OnGameStateChanged?.Invoke(newState);

        // J'applique les modifications système selon le nouvel état
        switch (newState)
        {
            case GameState.Playing:
            case GameState.Tutorial:
                Time.timeScale = 1f;         // Je remets le temps normal
                _isPaused = false;           // Je désactive l'état de pause
                OnGameResumed?.Invoke();     // Je notifie les scripts UI de masquer le panel Pause
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;         // Je fige le temps
                _isPaused = true;
                // Debug pour vérifier que le PauseController est bien abonné
                Debug.Log($"[GameManager] OnGamePaused invoqué. Abonnés : {OnGamePaused?.GetInvocationList().Length ?? 0}");
                OnGamePaused?.Invoke();      // Je notifie les scripts UI d'afficher le panel Pause
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;         // Je fige le temps
                SaveFinalScore();            // Je sauvegarde le score final via HighScoreManager
                break;
        }
    }

    // ---------------------------------------------------------
    // FLUX DE JEU (SCÈNES)
    // ---------------------------------------------------------

    /// <summary>
    /// Je démarre une nouvelle partie
    /// </summary>
    public void StartNewGame()
    {
        ResetAllManagers();

        // Je vérifie via le SettingsManager s'il faut forcer le tutoriel
        if (SettingsManager.Instance != null && SettingsManager.Instance.ShowTutorial)
        {
            StartTutorial();
        }
        else
        {
            StartGameplay();
        }
    }

    public void StartTutorial()
    {
        SetGameState(GameState.Tutorial);
        SceneManager.LoadScene("Tutorial");
    }

    public void CompleteTutorial()
    {
        Debug.Log("[GameManager] Tutoriel complété ! Passage au Gameplay.");
        StartGameplay();
    }

    private void StartGameplay()
    {
        SetGameState(GameState.Playing);
        SceneManager.LoadScene("Gameplay");
    }

    private void ResetAllManagers()
    {
        ScoreManager.Instance?.ResetScore();
        ThreatManager.Instance?.ResetThreat();
        PhaseManager.Instance?.ResetPhases();
    }

    // ---------------------------------------------------------
    // GESTION DE LA PAUSE & NAVIGATION
    // ---------------------------------------------------------
    
    public void PauseGame()
    {
        Debug.Log($"[GameManager] PauseGame appelé. IsGameActive={IsGameActive}, état={_currentState}");
        if (IsGameActive) 
            SetGameState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (_currentState == GameState.Paused)
        {
            // Je détermine si je dois revenir en mode Tutorial ou Playing selon la scène
            GameState resumeState = SceneManager.GetActiveScene().name == "Tutorial" 
                ? GameState.Tutorial 
                : GameState.Playing;
                
            SetGameState(resumeState);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void TriggerGameOver()
    {
        SetGameState(GameState.GameOver);
        SceneManager.LoadScene("GameOver");
        Debug.Log("[GameManager] Game Over ! Chargement de la scène GameOver.");
    }

    private void SaveFinalScore()
    {
        if (ScoreManager.Instance != null && HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.AddScore(ScoreManager.Instance.CurrentScore);
        }
    }

    // ---------------------------------------------------------
    // QUITTER
    // ---------------------------------------------------------
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}