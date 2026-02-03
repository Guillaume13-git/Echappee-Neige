using UnityEngine;
using System.Collections;

/// <summary>
/// Contrôleur du joueur.
/// Gère les couloirs, la gravité, l'accroupissement et le freinage additif (Chasse-neige).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane System")]
    [SerializeField] private float _laneDistance = 1.84f;
    [SerializeField] private float _laneChangeSpeed = 15f;
    [SerializeField] private float _leanAmount = 10f;
    private int _currentLaneIndex = 1;
    private float _targetXPosition = 0f;

    private float _leftLaneX => -_laneDistance;
    private float _centerLaneX => 0f;
    private float _rightLaneX => _laneDistance;

    [Header("Forward Movement (World Logic)")]
    [SerializeField] private float _baseSpeed = 12f;
    private float _currentBaseSpeed;
    private float _speedMultiplier = 1f;

    [Header("Physics")]
    [SerializeField] private float _gravity = -30f;
    private float _verticalVelocity;
    private bool _isGrounded;

    [Header("Slowdown (Chasse-Neige)")]
    [Tooltip("Réduction de vitesse en m/s quand le chasse-neige est actif (GDD : -4 m/s)")]
    [SerializeField] private float _snowplowReduction = 4f; 
    private bool _isSlowingDown = false;

    [Header("Crouch & Visuals")]
    [SerializeField] private float _normalHeight = 2f;
    [SerializeField] private float _crouchHeight = 1f;
    [SerializeField] private float _crouchSpeed = 10f;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _cameraNormalY = 0.8f;
    [SerializeField] private float _cameraCrouchY = 0.3f;
    private bool _isCrouching = false;

    private CharacterController _controller;
    public bool IsAccelerated { get; private set; } = false;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (_cameraTransform == null) _cameraTransform = Camera.main?.transform;
    }

    private void Start()
    {
        _currentBaseSpeed = _baseSpeed;
        _currentLaneIndex = 1;
        _targetXPosition = _centerLaneX;

        _controller.enabled = false;
        transform.position = new Vector3(_centerLaneX, transform.position.y, 0f);
        _controller.enabled = true;

        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdateSpeedForPhase;
    }

    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdateSpeedForPhase;
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;

        HandleInput();
        ApplyMovement();
        UpdateCrouchHeight();
    }

    private void HandleInput()
    {
        // Changement de couloir (AZERTY / Flèches)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Q))
            MoveLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            MoveLane(1);

        // Accroupissement
        _isCrouching = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        
        // Chasse-neige (Freinage)
        bool wasSlowingDown = _isSlowingDown;
        _isSlowingDown = Input.GetKey(KeyCode.Space);
        
        // Notification au ThreatManager si l'état change
        if (_isSlowingDown != wasSlowingDown && ThreatManager.Instance != null)
        {
            ThreatManager.Instance.SetSnowplowActive(_isSlowingDown);
        }
    }

    private void MoveLane(int direction)
    {
        int previousLane = _currentLaneIndex;
        _currentLaneIndex = Mathf.Clamp(_currentLaneIndex + direction, 0, 2);
        if (previousLane == _currentLaneIndex) return;

        _targetXPosition = _currentLaneIndex switch
        {
            0 => _leftLaneX,
            1 => _centerLaneX,
            2 => _rightLaneX,
            _ => 0f
        };

        AudioManager.Instance?.PlaySFX("Whoosh");
    }

    public int GetCurrentLane() => _currentLaneIndex;

    private void ApplyMovement()
    {
        _isGrounded = _controller.isGrounded;

        // Déplacement latéral
        float currentX = transform.position.x;
        float nextX = Mathf.MoveTowards(currentX, _targetXPosition, _laneChangeSpeed * Time.deltaTime);
        float deltaX = nextX - currentX;

        // Gravité simple
        if (_isGrounded) _verticalVelocity = -2f; 
        _verticalVelocity += _gravity * Time.deltaTime;

        Vector3 move = new Vector3(deltaX, _verticalVelocity * Time.deltaTime, 0f);
        _controller.Move(move);

        // Inclinaison visuelle lors du changement de couloir
        float tilt = (deltaX / Time.deltaTime) * (_leanAmount / _laneChangeSpeed);
        Quaternion targetRotation = Quaternion.Euler(0, 0, -tilt);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        // Correction de sécurité Z
        if (transform.position.z != 0)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }
    }

    /// <summary>
    /// Calcule la vitesse actuelle du joueur (utilisé par ChunkMover).
    /// </summary>
    public float GetCurrentForwardSpeed()
    {
        // 1. Vitesse de base (Phases) * Multiplicateur (Boost)
        float speed = _currentBaseSpeed * _speedMultiplier;
        
        // 2. Soustraction additive du chasse-neige (GDD : -4m/s)
        if (_isSlowingDown && !IsAccelerated)
        {
            speed -= _snowplowReduction;
            speed = Mathf.Max(speed, 2f); // Sécurité : Vitesse minimum de 2m/s
        }
        
        return speed;
    }

    #region PowerUps

    public void ActivateSpeedBoost(float duration)
    {
        StopCoroutine(nameof(SpeedBoostCoroutine));
        StartCoroutine(SpeedBoostCoroutine(duration));
        BonusUIManager.Instance?.TriggerSpeedBoost(duration);
    }

    public void StopSpeedBoost()
    {
        StopCoroutine(nameof(SpeedBoostCoroutine));
        _speedMultiplier = 1f;
        IsAccelerated = false;
        ThreatManager.Instance?.SetSpeedBoostActive(false);
        ScoreManager.Instance?.SetSpeedMultiplier(false);
    }

    private IEnumerator SpeedBoostCoroutine(float duration)
    {
        IsAccelerated = true;
        _speedMultiplier = 2.5f;
        ThreatManager.Instance?.SetSpeedBoostActive(true);
        ScoreManager.Instance?.SetSpeedMultiplier(true);
        yield return new WaitForSeconds(duration);
        StopSpeedBoost();
    }

    public void ActivateShield(float duration)
    {
        BonusUIManager.Instance?.TriggerShield(duration);
    }

    #endregion

    private void UpdateCrouchHeight()
    {
        float targetH = _isCrouching ? _crouchHeight : _normalHeight;
        float targetCamY = _isCrouching ? _cameraCrouchY : _cameraNormalY;

        _controller.height = Mathf.Lerp(_controller.height, targetH, Time.deltaTime * _crouchSpeed);
        _controller.center = new Vector3(0, _controller.height / 2, 0);

        if (_cameraTransform != null)
        {
            Vector3 camPos = _cameraTransform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * _crouchSpeed);
            _cameraTransform.localPosition = camPos;
        }
    }

    private void UpdateSpeedForPhase(TrackPhase phase)
    {
        _currentBaseSpeed = phase switch
        {
            TrackPhase.Green => 5f,
            TrackPhase.Blue  => 10f,
            TrackPhase.Red   => 15f,
            TrackPhase.Black => 20f,
            _                => 5f
        };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector3(-_laneDistance, 0, -5), new Vector3(-_laneDistance, 0, 20));
        Gizmos.DrawLine(new Vector3(0, 0, -5), new Vector3(0, 0, 20));
        Gizmos.DrawLine(new Vector3(_laneDistance, 0, -5), new Vector3(_laneDistance, 0, 20));
    }
}