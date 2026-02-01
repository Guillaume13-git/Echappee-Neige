using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("Game State")]
    [SerializeField] private GameState _currentState = GameState.MainMenu;
    [SerializeField] private bool _isPaused = false;

    public GameState CurrentState => _currentState;
    public bool IsPaused => _isPaused;

    public bool IsGameActive => _currentState == GameState.Playing || _currentState == GameState.Tutorial;

    public System.Action<GameState> OnGameStateChanged;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;

    protected override void Awake()
    {
        if (transform.parent != null) transform.SetParent(null);
        base.Awake();
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SettingsManager.Instance?.LoadSettings();
        DetectInitialState();
    }

    private void DetectInitialState()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        _currentState = sceneName switch
        {
            "MainMenu" => GameState.MainMenu,
            "Tutorial" => GameState.Tutorial,
            "Gameplay" => GameState.Playing,
            "GameOver" => GameState.GameOver,
            _          => GameState.MainMenu
        };
        Debug.Log($"[GameManager] État initial : {_currentState}");
    }

    public void SetGameState(GameState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
            case GameState.Tutorial:
                Time.timeScale = 1f;
                _isPaused = false;
                break;
            case GameState.Paused:
            case GameState.GameOver:
                Time.timeScale = 0f;
                if (newState == GameState.GameOver) SaveFinalScore();
                break;
        }
    }

    /// <summary>
    /// Appelé par le bouton "Jouer" du menu principal.
    /// </summary>
    public void StartNewGame()
    {
        ResetAllManagers();

        // Si le tuto doit être affiché, on le lance
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

    /// <summary>
    /// ✅ NOUVELLE MÉTHODE : Appelé à la fin du tutoriel pour passer au jeu.
    /// </summary>
    public void CompleteTutorial()
    {
        Debug.Log("[GameManager] Tutoriel complété ! Passage au Gameplay.");
        
        // Optionnel : Désactiver le tuto pour les prochaines parties
        // SettingsManager.Instance?.SetTutorialCompleted(); 

        StartGameplay();
    }

    private void StartGameplay()
    {
        SetGameState(GameState.Playing);
        SceneManager.LoadScene("Gameplay");
    }

    // --- Reste des méthodes (Pause, Resume, Score, etc.) ---
    
    private void ResetAllManagers()
    {
        ScoreManager.Instance?.ResetScore();
        ThreatManager.Instance?.ResetThreat();
        PhaseManager.Instance?.ResetPhases();
    }

    public void PauseGame()
    {
        if (IsGameActive) SetGameState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (_currentState == GameState.Paused) 
            SetGameState(SceneManager.GetActiveScene().name == "Tutorial" ? GameState.Tutorial : GameState.Playing);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void TriggerGameOver() => SetGameState(GameState.GameOver);

    private void SaveFinalScore()
    {
        if (ScoreManager.Instance != null && HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.AddScore(ScoreManager.Instance.CurrentScore);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}