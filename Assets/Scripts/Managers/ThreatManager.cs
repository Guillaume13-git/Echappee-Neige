using UnityEngine;

/// <summary>
/// Gère la jauge de menace (avalanche).
/// Remplissage automatique, collisions, invulnérabilité, chasse-neige, vitesse accélérée.
/// </summary>
public class ThreatManager : Singleton<ThreatManager>
{
    [Header("Threat Settings")]
    [SerializeField] private float _maxThreat = 100f;
    private float _currentThreat = 0f;

    [Header("Auto Fill Intervals (seconds)")]
    [SerializeField] private float _greenInterval = 8f;
    [SerializeField] private float _blueInterval = 6f;
    [SerializeField] private float _redInterval = 4f;
    [SerializeField] private float _blackInterval = 2f;

    [Header("Collision Damage")]
    [SerializeField] private float _greenDamage = 5f;
    [SerializeField] private float _blueDamage = 10f;
    [SerializeField] private float _redDamage = 20f;
    [SerializeField] private float _blackDamage = 30f;

    // Timers
    private float _autoFillTimer = 0f;
    private float _currentInterval;

    // États
    private bool _isInvulnerable = false;
    private bool _isSpeedBoostActive = false;
    private bool _isSnowplowActive = false;

    // Événements
    public System.Action<float> OnThreatChanged;
    public System.Action OnGameOver;

    public float ThreatPercentage => (_currentThreat / _maxThreat) * 100f;


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    protected override void Awake()
    {
        base.Awake(); // ← Initialise le Singleton
    }

    private void Start()
    {
        _currentInterval = _greenInterval;

        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdateIntervalForPhase;
    }

    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdateIntervalForPhase;
    }


    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------
    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        UpdateAutoFill();
        CheckGameOver();
    }


    // ---------------------------------------------------------
    // AUTO-FILL
    // ---------------------------------------------------------
    private void UpdateAutoFill()
    {
        if (_isInvulnerable || _isSpeedBoostActive)
            return;

        _autoFillTimer += Time.deltaTime;

        float effectiveInterval = _isSnowplowActive ? _currentInterval / 2f : _currentInterval;

        if (_autoFillTimer >= effectiveInterval)
        {
            _autoFillTimer = 0f;
            AddThreat(1f);
        }
    }


    // ---------------------------------------------------------
    // SPEED BOOST REDUCTION
    // ---------------------------------------------------------
    public void SetSpeedBoostActive(bool active)
    {
        _isSpeedBoostActive = active;

        if (active)
            StartCoroutine(SpeedBoostReductionCoroutine());
    }

    private System.Collections.IEnumerator SpeedBoostReductionCoroutine()
    {
        while (_isSpeedBoostActive)
        {
            AddThreat(-Time.deltaTime); // -1% par seconde
            yield return null;
        }
    }


    // ---------------------------------------------------------
    // THREAT MODIFICATION
    // ---------------------------------------------------------
    public void AddThreat(float amount)
    {
        _currentThreat = Mathf.Clamp(_currentThreat + amount, 0f, _maxThreat);
        OnThreatChanged?.Invoke(ThreatPercentage);
    }

    public void AddThreatFromCollision()
    {
        float damage = PhaseManager.Instance.CurrentPhase switch
        {
            TrackPhase.Green => _greenDamage,
            TrackPhase.Blue => _blueDamage,
            TrackPhase.Red => _redDamage,
            TrackPhase.Black => _blackDamage,
            _ => 5f
        };

        AddThreat(damage);
        Debug.Log($"[ThreatManager] Collision ! +{damage}% menace");
    }

    public void ReduceThreatImmediate(float amount = 10f)
    {
        AddThreat(-amount);
        Debug.Log($"[ThreatManager] Menace réduite de {amount}%");
    }


    // ---------------------------------------------------------
    // STATES
    // ---------------------------------------------------------
    public void SetInvulnerabilityActive(bool active)
    {
        _isInvulnerable = active;
    }

    public void SetSnowplowActive(bool active)
    {
        _isSnowplowActive = active;
    }


    // ---------------------------------------------------------
    // PHASE CHANGES
    // ---------------------------------------------------------
    private void UpdateIntervalForPhase(TrackPhase phase)
    {
        _currentInterval = phase switch
        {
            TrackPhase.Green => _greenInterval,
            TrackPhase.Blue => _blueInterval,
            TrackPhase.Red => _redInterval,
            TrackPhase.Black => _blackInterval,
            _ => _greenInterval
        };

        Debug.Log($"[ThreatManager] Intervalle mis à jour : {_currentInterval}s pour phase {phase}");
    }


    // ---------------------------------------------------------
    // GAME OVER
    // ---------------------------------------------------------
    private void CheckGameOver()
    {
        if (_currentThreat >= _maxThreat)
            TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        Debug.Log("[ThreatManager] GAME OVER - Avalanche a rattrapé le joueur !");
        OnGameOver?.Invoke();
        GameManager.Instance.TriggerGameOver();
    }


    // ---------------------------------------------------------
    // RESET
    // ---------------------------------------------------------
    public void ResetThreat()
    {
        _currentThreat = 0f;
        _autoFillTimer = 0f;
        _isInvulnerable = false;
        _isSpeedBoostActive = false;
        _isSnowplowActive = false;

        OnThreatChanged?.Invoke(0f);
    }
}