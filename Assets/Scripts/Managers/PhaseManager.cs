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

    protected override void Awake()
    {
        // 1. Sortir du parent pour autoriser DontDestroyOnLoad (comme GameManager)
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        base.Awake(); 

        if (_mainCamera == null)
            _mainCamera = Camera.main;
    }

    private void Start()
    {
        SetPhase(TrackPhase.Green);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // MODIFICATION : On autorise la mise à jour visuelle (FOV) même en Tutorial
        GameState state = GameManager.Instance.CurrentState;
        if (state != GameState.Playing && state != GameState.Tutorial)
            return;

        // On n'avance le timer que si on est en train de jouer (pas en tutorial)
        if (state == GameState.Playing)
        {
            UpdatePhaseTimer();
        }

        UpdateCameraFOV();
    }

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
            TrackPhase.Black => TrackPhase.Black,
            _ => TrackPhase.Green
        };

        SetPhase(nextPhase);
    }

    public void SetPhase(TrackPhase newPhase)
    {
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
        
        // Petit check pour éviter le son au tout début du jeu si nécessaire
        if (Time.timeSinceLevelLoad > 0.1f)
            AudioManager.Instance?.PlaySFX("Tududum");

        Debug.Log($"[PhaseManager] Phase changée : {newPhase}, FOV cible : {_targetFOV}");
    }

    private void UpdateCameraFOV()
    {
        if (_mainCamera == null) return;

        _mainCamera.fieldOfView = Mathf.Lerp(
            _mainCamera.fieldOfView,
            _targetFOV,
            Time.deltaTime * _fovTransitionSpeed
        );
    }

    public void ResetPhases()
    {
        _phaseTimer = 0f;
        SetPhase(TrackPhase.Green);
    }
}