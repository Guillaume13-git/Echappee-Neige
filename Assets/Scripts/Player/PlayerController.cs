using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane Movement")]
    [SerializeField] private float _laneDistance = 1.5f; 
    [SerializeField] private float _laneChangeSpeed = 10f;
    private int _currentLane = 2; 
    private float _targetXPosition = 0f;
    
    [Header("Forward Movement")]
    [SerializeField] private float _baseSpeed = 5f;
    private float _currentSpeed;
    private float _speedMultiplier = 1f;
    
    [Header("Crouch")]
    [SerializeField] private float _normalHeight = 2f;
    [SerializeField] private float _crouchHeight = 1f;
    [SerializeField] private float _crouchSpeed = 10f;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _cameraNormalY = 0.8f;
    [SerializeField] private float _cameraCrouchY = 0.3f;
    private bool _isCrouching = false;
    
    [Header("Snowplow")]
    [SerializeField] private float _snowplowSpeedReduction = 4f;
    private bool _isSnowplowing = false;
    
    [SerializeField] private CharacterController _controller;
    
    public bool IsAccelerated { get; private set; } = false;
    public float CurrentSpeed => _currentSpeed * _speedMultiplier;

    private void Awake()
    {
        if (_controller == null) _controller = GetComponent<CharacterController>();
    }
    
    private void Start()
    {
        _currentSpeed = _baseSpeed;
        UpdateLanePosition();
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
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        
        HandleInput();
        MoveForward();
        MoveLateralSmooth();
        UpdateCrouchPosition();
        HandleSnowplow();
    }
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q)) MoveLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) MoveLane(1);
        
        _isCrouching = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftShift);
        _isSnowplowing = Input.GetKey(KeyCode.Space) && !IsAccelerated;
    }
    
    private void MoveLane(int direction)
    {
        int newLane = Mathf.Clamp(_currentLane + direction, 1, 3);
        if (newLane == _currentLane) return;

        _currentLane = newLane;
        UpdateLanePosition();
        AudioManager.Instance?.PlaySFX("Whoosh");
    }
    
    private void UpdateLanePosition() => _targetXPosition = (_currentLane - 2) * _laneDistance;

    private void MoveLateralSmooth()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, _targetXPosition, Time.deltaTime * _laneChangeSpeed);
        transform.position = pos;
    }
    
    private void MoveForward()
    {
        Vector3 movement = Vector3.forward * (_currentSpeed * _speedMultiplier) * Time.deltaTime;
        _controller.Move(movement);
    }

    private void UpdateCrouchPosition()
    {
        float targetH = _isCrouching ? _crouchHeight : _normalHeight;
        float targetCamY = _isCrouching ? _cameraCrouchY : _cameraNormalY;
        
        _controller.height = Mathf.Lerp(_controller.height, targetH, Time.deltaTime * _crouchSpeed);
        
        if (_cameraTransform != null)
        {
            Vector3 camPos = _cameraTransform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * _crouchSpeed);
            _cameraTransform.localPosition = camPos;
        }
    }

    private void HandleSnowplow()
    {
        if (_isSnowplowing)
        {
            float reduced = Mathf.Max(_currentSpeed - _snowplowSpeedReduction, 1f);
            _speedMultiplier = reduced / _currentSpeed;
            ThreatManager.Instance?.SetSnowplowActive(true);
        }
        else if (!IsAccelerated)
        {
            _speedMultiplier = 1f;
            ThreatManager.Instance?.SetSnowplowActive(false);
        }
    }

    public void ActivateSpeedBoost(float duration)
    {
        StopCoroutine(nameof(SpeedBoostCoroutine)); // Empêche le cumul de vitesses
        StartCoroutine(SpeedBoostCoroutine(duration));
    }

    private IEnumerator SpeedBoostCoroutine(float duration)
    {
        IsAccelerated = true;
        _speedMultiplier = 4f;
        ThreatManager.Instance?.SetSpeedBoostActive(true);
        yield return new WaitForSeconds(duration);
        _speedMultiplier = 1f;
        IsAccelerated = false;
        ThreatManager.Instance?.SetSpeedBoostActive(false);
    }

    public void StopSpeedBoost() // Appelé par PlayerCollision lors d'un choc
    {
        StopCoroutine(nameof(SpeedBoostCoroutine));
        IsAccelerated = false;
        _speedMultiplier = 1f;
        ThreatManager.Instance?.SetSpeedBoostActive(false);
    }

    private void UpdateSpeedForPhase(TrackPhase phase)
    {
        _baseSpeed = phase switch {
            TrackPhase.Green => 5f,
            TrackPhase.Blue => 10f,
            TrackPhase.Red => 15f,
            TrackPhase.Black => 20f,
            _ => 5f
        };
        _currentSpeed = _baseSpeed;
    }
}