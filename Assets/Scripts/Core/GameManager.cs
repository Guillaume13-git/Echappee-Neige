using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton central qui gère l'état du jeu, les transitions et l'initialisation des managers.
/// Persiste entre les scènes via DontDestroyOnLoad.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("Game State")]
    [SerializeField] private GameState _currentState = GameState.MainMenu;
    [SerializeField] private bool _isPaused = false;

    public GameState CurrentState => _currentState;
    public bool IsPaused => _isPaused;

    // Événements
    public System.Action<GameState> OnGameStateChanged;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;

    protected override void Awake()
    {
        // 1. On se détache de tout parent pour autoriser DontDestroyOnLoad
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        // 2. Initialisation du Singleton (Gère les doublons)
        base.Awake(); 

        // 3. Persistance (Note: base.Awake() le fait déjà si ton Singleton est bien codé, 
        // mais on le sécurise ici si besoin)
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // Utilisation du Null Check (?.) pour éviter l'erreur si SettingsManager n'est pas encore prêt
        SettingsManager.Instance?.LoadSettings();
    }

    // -----------------------------
    // Gestion des états du jeu
    // -----------------------------
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

    // -----------------------------
    // Transitions
    // -----------------------------
    public void StartNewGame()
    {
        // Utilisation systématique de ?. pour éviter les NullReferenceException au démarrage
        ScoreManager.Instance?.ResetScore();
        // ThreatManager.Instance?.ResetThreat(); // Commenté si non présent
        // PhaseManager.Instance?.ResetPhases(); // Commenté si non présent

        SetGameState(GameState.Playing);
        SceneManager.LoadScene("Gameplay");
    }

    public void StartTutorial()
    {
        SetGameState(GameState.Tutorial);
        SceneManager.LoadScene("Tutorial");
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
        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void TriggerGameOver()
    {
        SetGameState(GameState.GameOver);
        SceneManager.LoadScene("GameOver");
    }

    // -----------------------------
    // Score & Quit
    // -----------------------------
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

public enum GameState
{
    MainMenu,
    Tutorial,
    Playing,
    Paused,
    GameOver
}