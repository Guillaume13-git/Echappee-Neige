using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton central qui gère l'état du jeu, les transitions et l'initialisation des managers.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("Game State")]
    [SerializeField] private GameState _currentState = GameState.MainMenu;
    [SerializeField] private bool _isPaused = false;

    public GameState CurrentState => _currentState;
    public bool IsPaused => _isPaused;

    public System.Action<GameState> OnGameStateChanged;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;

    protected override void Awake()
    {
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        base.Awake();

        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        SettingsManager.Instance?.LoadSettings();
    }

    public void SetGameState(GameState newState)
    {
        if (_currentState == newState) return;

        Debug.Log($"[GameManager] État changé : {_currentState} → {newState}");
        _currentState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                _isPaused = false;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                _isPaused = true;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                SaveFinalScore();
                break;
        }
    }

    public void StartNewGame()
    {
        // ⭐ CORRIGÉ - Vérifier le tutoriel
        if (SettingsManager.Instance != null && SettingsManager.Instance.ShowTutorial)
        {
            StartTutorial();
            return;
        }
        
        ResetAllManagers();
        SetGameState(GameState.Playing);
        SceneManager.LoadScene("Gameplay");
    }

    public void StartTutorial()
    {
        ResetAllManagers();
        SetGameState(GameState.Tutorial);
        SceneManager.LoadScene("Tutorial");
    }
    
    /// <summary>
    /// ⭐ MÉTHODE AJOUTÉE - Reset tous les managers
    /// </summary>
    private void ResetAllManagers()
    {
        ScoreManager.Instance?.ResetScore();
        ThreatManager.Instance?.ResetThreat();
        PhaseManager.Instance?.ResetPhases();
    }

    public void PauseGame()
    {
        if (_currentState != GameState.Playing) return;
        SetGameState(GameState.Paused);
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (_currentState != GameState.Paused) return;
        SetGameState(GameState.Playing);
        OnGameResumed?.Invoke();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // ⭐ AJOUTÉ - Reset timescale
        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void TriggerGameOver()
    {
        SetGameState(GameState.GameOver);
        SceneManager.LoadScene("GameOver");
    }

    private void SaveFinalScore()
    {
        if (ScoreManager.Instance != null && HighScoreManager.Instance != null)
        {
            int finalScore = ScoreManager.Instance.CurrentScore;
            HighScoreManager.Instance.AddScore(finalScore);
        }
    }

    public void QuitGame()
    {
        Debug.Log("[GameManager] Fermeture du jeu");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}