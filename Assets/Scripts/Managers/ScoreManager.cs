using UnityEngine;

/// <summary>
/// Gère le système de score : incrément automatique selon la phase,
/// bonus de collectibles, multiplication pendant accélération.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("Score Settings")]
    private float _currentScore = 0f;
    
    [Header("Score per Second (by phase)")]
    [SerializeField] private float _greenScoreRate = 10f;
    [SerializeField] private float _blueScoreRate = 25f;
    [SerializeField] private float _redScoreRate = 50f;
    [SerializeField] private float _blackScoreRate = 100f;
    
    private float _currentScoreRate;
    
    [Header("Bonus Scores (by phase)")]
    [SerializeField] private float _greenBonus = 50f;
    [SerializeField] private float _blueBonus = 100f;
    [SerializeField] private float _redBonus = 200f;
    [SerializeField] private float _blackBonus = 400f;
    
    // États
    private bool _isSpeedBoosted = false;
    private TrackPhase _currentPhase = TrackPhase.Green;
    
    // Événements
    public System.Action<float> OnScoreChanged;
    
    // Propriétés
    public float CurrentScoreFloat => _currentScore;
    public int CurrentScore => Mathf.FloorToInt(_currentScore);
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        _currentScoreRate = _greenScoreRate;
        _currentScore = 0f;
        
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdatePhase;
    }
    
    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdatePhase;
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            AddScorePerFrame();
        }
    }
    
    private void AddScorePerFrame()
    {
        float scoreThisFrame = _currentScoreRate * Time.deltaTime;
        
        if (_isSpeedBoosted)
            scoreThisFrame *= 2f;
        
        _currentScore += scoreThisFrame;
        OnScoreChanged?.Invoke(_currentScore);
    }
    
    /// <summary>
    /// ⭐ MÉTHODE AJOUTÉE - Retourne le bonus selon la phase actuelle
    /// </summary>
    public int GetCollectibleBonusForCurrentPhase()
    {
        return _currentPhase switch
        {
            TrackPhase.Green => Mathf.RoundToInt(_greenBonus),
            TrackPhase.Blue => Mathf.RoundToInt(_blueBonus),
            TrackPhase.Red => Mathf.RoundToInt(_redBonus),
            TrackPhase.Black => Mathf.RoundToInt(_blackBonus),
            _ => Mathf.RoundToInt(_greenBonus)
        };
    }
    
    /// <summary>
    /// Ajoute un bonus de score fixe.
    /// </summary>
    public void AddBonusScore(int bonus)
    {
        _currentScore += bonus;
        OnScoreChanged?.Invoke(_currentScore);
    }
    
    /// <summary>
    /// Ajoute un bonus selon la phase actuelle.
    /// </summary>
    public void AddBonusScore()
    {
        float bonus = _currentPhase switch
        {
            TrackPhase.Green => _greenBonus,
            TrackPhase.Blue => _blueBonus,
            TrackPhase.Red => _redBonus,
            TrackPhase.Black => _blackBonus,
            _ => _greenBonus
        };
        
        _currentScore += bonus;
        OnScoreChanged?.Invoke(_currentScore);
    }
    
    /// <summary>
    /// ⭐ MÉTHODE CORRIGÉE - Active/désactive le multiplicateur x2
    /// </summary>
    public void SetSpeedMultiplier(bool active)
    {
        _isSpeedBoosted = active;
    }
    
    /// <summary>
    /// ⭐ ALIAS pour compatibilité
    /// </summary>
    public void SetSpeedBoostActive(bool active)
    {
        SetSpeedMultiplier(active);
    }
    
    private void UpdatePhase(TrackPhase phase)
    {
        _currentPhase = phase;
        
        _currentScoreRate = phase switch
        {
            TrackPhase.Green => _greenScoreRate,
            TrackPhase.Blue => _blueScoreRate,
            TrackPhase.Red => _redScoreRate,
            TrackPhase.Black => _blackScoreRate,
            _ => _greenScoreRate
        };
    }
    
    public void ResetScore()
    {
        _currentScore = 0f;
        _isSpeedBoosted = false;
        OnScoreChanged?.Invoke(0f);
    }
}