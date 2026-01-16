using UnityEngine;

/// <summary>
/// Gère le score du joueur.
/// Incrémentation par seconde selon la phase, multiplicateur x2 en vitesse accélérée.
/// </summary>
public class ScoreManager : Singleton<ScoreManager>
{
    [Header("Score Settings")]
    private int _currentScore = 0;

    [Header("Score Per Second by Phase")]
    [SerializeField] private int _greenScorePerSecond = 10;
    [SerializeField] private int _blueScorePerSecond = 25;
    [SerializeField] private int _redScorePerSecond = 50;
    [SerializeField] private int _blackScorePerSecond = 100;

    private int _currentScorePerSecond;
    private float _scoreMultiplier = 1f;

    public System.Action<int> OnScoreChanged;
    public int CurrentScore => _currentScore;


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    protected override void Awake()
    {
        base.Awake(); // ← Initialise le Singleton
    }

    private void Start()
    {
        _currentScorePerSecond = _greenScorePerSecond;

        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdateScoreForPhase;
    }

    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdateScoreForPhase;
    }


    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------
    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        IncrementScore();
    }


    // ---------------------------------------------------------
    // SCORE LOGIC
    // ---------------------------------------------------------
    private void IncrementScore()
    {
        float scoreThisFrame = _currentScorePerSecond * _scoreMultiplier * Time.deltaTime;

        _currentScore += Mathf.RoundToInt(scoreThisFrame);
        OnScoreChanged?.Invoke(_currentScore);
    }

    public void AddBonusScore(int bonus)
    {
        _currentScore += bonus;
        OnScoreChanged?.Invoke(_currentScore);

        Debug.Log($"[ScoreManager] Bonus de score : +{bonus} points");
    }

    public int GetCollectibleBonusForCurrentPhase()
    {
        return PhaseManager.Instance.CurrentPhase switch
        {
            TrackPhase.Green => 50,
            TrackPhase.Blue => 100,
            TrackPhase.Red => 200,
            TrackPhase.Black => 400,
            _ => 50
        };
    }

    public void SetSpeedMultiplier(bool active)
    {
        _scoreMultiplier = active ? 2f : 1f;
        Debug.Log($"[ScoreManager] Multiplicateur de score : x{_scoreMultiplier}");
    }


    // ---------------------------------------------------------
    // PHASE CHANGES
    // ---------------------------------------------------------
    private void UpdateScoreForPhase(TrackPhase phase)
    {
        _currentScorePerSecond = phase switch
        {
            TrackPhase.Green => _greenScorePerSecond,
            TrackPhase.Blue => _blueScorePerSecond,
            TrackPhase.Red => _redScorePerSecond,
            TrackPhase.Black => _blackScorePerSecond,
            _ => _greenScorePerSecond
        };

        Debug.Log($"[ScoreManager] Score par seconde : {_currentScorePerSecond} pour phase {phase}");
    }


    // ---------------------------------------------------------
    // RESET
    // ---------------------------------------------------------
    public void ResetScore()
    {
        _currentScore = 0;
        _scoreMultiplier = 1f;
        _currentScorePerSecond = _greenScorePerSecond;

        OnScoreChanged?.Invoke(_currentScore);
    }
}