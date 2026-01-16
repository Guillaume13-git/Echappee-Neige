using UnityEngine;

/// <summary>
/// Gère les phases de piste (verte/bleue/rouge/noire).
/// Déclenche les transitions toutes les 60 secondes.
/// Ajuste le FOV de la caméra pour accentuer la sensation de vitesse.
/// </summary>
public class PhaseManager : Singleton<PhaseManager>
{
    [Header("Phase Timing")]
    [SerializeField] private float _phaseDuration = 60f;

    [Header("Camera FOV")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _greenFOV = 90f;
    [SerializeField] private float _blueFOV = 85f;
    [SerializeField] private float _redFOV = 80f;
    [SerializeField] private float _blackFOV = 75f;
    [SerializeField] private float _fovTransitionSpeed = 2f;

    private TrackPhase _currentPhase = TrackPhase.Green;
    private float _phaseTimer = 0f;
    private float _targetFOV = 90f;

    public System.Action<TrackPhase> OnPhaseChanged;
    public TrackPhase CurrentPhase => _currentPhase;


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    protected override void Awake()
    {
        base.Awake(); // ← Initialise le Singleton

        if (_mainCamera == null)
            _mainCamera = Camera.main;
    }

    private void Start()
    {
        SetPhase(TrackPhase.Green);
    }


    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------
    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        UpdatePhaseTimer();
        UpdateCameraFOV();
    }


    // ---------------------------------------------------------
    // PHASE LOGIC
    // ---------------------------------------------------------
    private void UpdatePhaseTimer()
    {
        _phaseTimer += Time.deltaTime;

        if (_phaseTimer >= _phaseDuration)
        {
            _phaseTimer = 0f;
            AdvanceToNextPhase();
        }
    }

    private void AdvanceToNextPhase()
    {
        TrackPhase nextPhase = _currentPhase switch
        {
            TrackPhase.Green => TrackPhase.Blue,
            TrackPhase.Blue => TrackPhase.Red,
            TrackPhase.Red => TrackPhase.Black,
            TrackPhase.Black => TrackPhase.Black, // Reste en noir
            _ => TrackPhase.Green
        };

        SetPhase(nextPhase);
    }

    private void SetPhase(TrackPhase newPhase)
    {
        if (_currentPhase == newPhase && newPhase != TrackPhase.Green)
            return;

        _currentPhase = newPhase;

        _targetFOV = newPhase switch
        {
            TrackPhase.Green => _greenFOV,
            TrackPhase.Blue => _blueFOV,
            TrackPhase.Red => _redFOV,
            TrackPhase.Black => _blackFOV,
            _ => 90f
        };

        OnPhaseChanged?.Invoke(newPhase);
        AudioManager.Instance?.PlaySFX("Tududum");

        Debug.Log($"[PhaseManager] Phase changée : {newPhase}, FOV cible : {_targetFOV}");
    }


    // ---------------------------------------------------------
    // CAMERA FOV TRANSITION
    // ---------------------------------------------------------
    private void UpdateCameraFOV()
    {
        if (_mainCamera == null) return;

        _mainCamera.fieldOfView = Mathf.Lerp(
            _mainCamera.fieldOfView,
            _targetFOV,
            Time.deltaTime * _fovTransitionSpeed
        );
    }


    // ---------------------------------------------------------
    // RESET
    // ---------------------------------------------------------
    public void ResetPhases()
    {
        _phaseTimer = 0f;
        SetPhase(TrackPhase.Green);
    }
}


// ---------------------------------------------------------
// ENUM
// ---------------------------------------------------------
public enum TrackPhase
{
    Green,
    Blue,
    Red,
    Black
}