using UnityEngine;
using System.Collections;

/// <summary>
/// Gère la jauge de menace (avalanche) : remplissage automatique,
/// augmentation après collision, réduction pendant accélération.
/// </summary>
public class ThreatManager : MonoBehaviour
{
    public static ThreatManager Instance { get; private set; }
    
    [Header("Threat Settings")]
    [SerializeField] private float _maxThreat = 100f;
    private float _currentThreat = 0f;
    
    [Header("Auto Fill Intervals (by phase)")]
    [SerializeField] private float _greenInterval = 8f;
    [SerializeField] private float _blueInterval = 6f;
    [SerializeField] private float _redInterval = 4f;
    [SerializeField] private float _blackInterval = 2f;
    
    private float _currentInterval;
    private float _autoFillAmount = 1f;
    
    [Header("Collision Damage (by phase)")]
    [SerializeField] private float _greenDamage = 5f;
    [SerializeField] private float _blueDamage = 10f;
    [SerializeField] private float _redDamage = 20f;
    [SerializeField] private float _blackDamage = 30f;
    
    // États
    private bool _isSpeedBoostActive = false;
    private bool _isSnowplowActive = false;
    private bool _isInvulnerabilityActive = false; // ⭐ AJOUTÉ
    private TrackPhase _currentPhase = TrackPhase.Green;
    
    // Événements
    public System.Action<float> OnThreatChanged;
    public System.Action OnGameOver;
    
    // Propriétés
    public float ThreatPercentage => (_currentThreat / _maxThreat) * 100f;
    public bool IsGameOver => _currentThreat >= _maxThreat;
    
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
        _currentInterval = _greenInterval;
        _currentThreat = 0f;
        
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdatePhase;
        
        StartCoroutine(AutoFillCoroutine());
    }
    
    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdatePhase;
    }
    
    private IEnumerator AutoFillCoroutine()
    {
        while (true)
        {
            float interval = _currentInterval;
            
            if (_isSnowplowActive)
                interval /= 2f;
            
            yield return new WaitForSeconds(interval);
            
            // ⭐ CORRIGÉ - Vérifie aussi l'invulnérabilité
            if (!_isInvulnerabilityActive && !_isSpeedBoostActive)
            {
                AddThreat(_autoFillAmount);
            }
        }
    }
    
    public void AddThreat(float amount)
    {
        _currentThreat = Mathf.Min(_currentThreat + amount, _maxThreat);
        OnThreatChanged?.Invoke(ThreatPercentage);
        
        if (_currentThreat >= _maxThreat)
        {
            TriggerGameOver();
        }
    }
    
    /// <summary>
    /// ⭐ ALIAS pour compatibilité avec PlayerCollision
    /// </summary>
    public void AddThreatFromCollision()
    {
        AddThreatFromObstacle();
    }
    
    public void AddThreatFromObstacle()
    {
        float damage = _currentPhase switch
        {
            TrackPhase.Green => _greenDamage,
            TrackPhase.Blue => _blueDamage,
            TrackPhase.Red => _redDamage,
            TrackPhase.Black => _blackDamage,
            _ => _greenDamage
        };
        
        AddThreat(damage);
    }
    
    public void ReduceThreat(float amount)
    {
        _currentThreat = Mathf.Max(_currentThreat - amount, 0f);
        OnThreatChanged?.Invoke(ThreatPercentage);
    }
    
    /// <summary>
    /// ⭐ ALIAS pour compatibilité
    /// </summary>
    public void ReduceThreatImmediate(float amount)
    {
        ReduceThreat(amount);
    }
    
    public void SetSpeedBoostActive(bool active)
    {
        if (_isSpeedBoostActive == active) return;
        
        _isSpeedBoostActive = active;
        
        if (active)
        {
            StartCoroutine(SpeedBoostReductionCoroutine());
        }
    }
    
    private IEnumerator SpeedBoostReductionCoroutine()
    {
        while (_isSpeedBoostActive)
        {
            yield return new WaitForSeconds(1f);
            
            if (_isSpeedBoostActive)
            {
                ReduceThreat(1f);
            }
        }
    }
    
    public void SetSnowplowActive(bool active)
    {
        _isSnowplowActive = active;
    }
    
    /// <summary>
    /// ⭐ MÉTHODE AJOUTÉE - Gère l'invulnérabilité
    /// </summary>
    public void SetInvulnerabilityActive(bool active)
    {
        _isInvulnerabilityActive = active;
    }
    
    private void UpdatePhase(TrackPhase phase)
    {
        _currentPhase = phase;
        
        _currentInterval = phase switch
        {
            TrackPhase.Green => _greenInterval,
            TrackPhase.Blue => _blueInterval,
            TrackPhase.Red => _redInterval,
            TrackPhase.Black => _blackInterval,
            _ => _greenInterval
        };
    }
    
    private void TriggerGameOver()
    {
        OnGameOver?.Invoke();
        
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameState.GameOver);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Crash");
    }
    
    public void ResetThreat()
    {
        _currentThreat = 0f;
        _isSpeedBoostActive = false;
        _isSnowplowActive = false;
        _isInvulnerabilityActive = false;
        OnThreatChanged?.Invoke(0f);
    }
}