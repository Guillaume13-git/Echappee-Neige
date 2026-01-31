using UnityEngine;
using System.Collections;

/// <summary>
/// Gère la jauge de menace (avalanche) avec une progression fluide.
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
    
    private float _currentGrowthRate; // Points par seconde calculés
    
    [Header("Collision Damage (by phase)")]
    [SerializeField] private float _greenDamage = 5f;
    [SerializeField] private float _blueDamage = 10f;
    [SerializeField] private float _redDamage = 20f;
    [SerializeField] private float _blackDamage = 30f;
    
    // États
    private bool _isSpeedBoostActive = false;
    private bool _isSnowplowActive = false;
    private bool _isInvulnerabilityActive = false; 
    private TrackPhase _currentPhase = TrackPhase.Green;
    
    // Événements
    public System.Action<float> OnThreatChanged;
    public System.Action OnGameOver;
    
    // Propriétés
    public float ThreatPercentage => (_currentThreat / _maxThreat) * 100f;
    public bool IsGameOver => _currentThreat >= _maxThreat;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }
    
    private void Start()
    {
        _currentThreat = 0f;
        
        // Initialise la vitesse de progression dès le départ
        UpdatePhase(TrackPhase.Green);
        
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdatePhase;
    }

    private void Update()
    {
        // Sécurité : on n'augmente que si l'état du jeu est "Playing"
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // PROGRESSION FLUIDE
        // On n'augmente pas la menace si on est invulnérable ou en boost (fuite)
        if (!_isInvulnerabilityActive && !_isSpeedBoostActive)
        {
            float actualGrowth = _currentGrowthRate;

            // Si le chasse-neige (Snowplow) est actif, on accélère la menace (selon ton ancienne logique)
            if (_isSnowplowActive) actualGrowth *= 2f;

            AddThreat(actualGrowth * Time.deltaTime);
        }
    }
    
    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdatePhase;
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
    
    public void AddThreatFromCollision()
    {
        // On ne prend pas de dégâts de collision si invulnérable
        if (_isInvulnerabilityActive) return;

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
    
    public void SetSpeedBoostActive(bool active)
    {
        if (_isSpeedBoostActive == active) return;
        _isSpeedBoostActive = active;
        
        if (active) StartCoroutine(SpeedBoostReductionCoroutine());
    }
    
    private IEnumerator SpeedBoostReductionCoroutine()
    {
        while (_isSpeedBoostActive)
        {
            yield return new WaitForSeconds(1f);
            if (_isSpeedBoostActive) ReduceThreat(1f);
        }
    }
    
    public void SetSnowplowActive(bool active) => _isSnowplowActive = active;

    public void SetInvulnerabilityActive(bool active) => _isInvulnerabilityActive = active;
    
    private void UpdatePhase(TrackPhase phase)
    {
        _currentPhase = phase;
        
        // Calcul du taux de croissance : 1 point divisé par l'intervalle
        _currentGrowthRate = phase switch
        {
            TrackPhase.Green => 1f / _greenInterval, 
            TrackPhase.Blue => 1f / _blueInterval,   
            TrackPhase.Red => 1f / _redInterval,     
            TrackPhase.Black => 1f / _blackInterval, 
            _ => 1f / _greenInterval
        };
    }
    
    private void TriggerGameOver()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver) return;

        OnGameOver?.Invoke();
        GameManager.Instance?.SetGameState(GameState.GameOver);
        AudioManager.Instance?.PlaySFX("Crash");
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