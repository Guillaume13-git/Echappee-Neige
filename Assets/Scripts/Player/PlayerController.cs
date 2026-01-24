using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane Movement")]
    [SerializeField] private float _laneDistance = 2.0f; 
    [SerializeField] private float _laneChangeSpeed = 15f;
    private int _currentLane = 2; // 1 = Gauche, 2 = Centre, 3 = Droite
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
    
    [Header("Snowplow (Slow)")]
    [SerializeField] private float _snowplowSpeedReduction = 4f;
    private bool _isSnowplowing = false;
    
    private CharacterController _controller;
    
    public bool IsAccelerated { get; private set; } = false;
    public float CurrentSpeed => _currentSpeed * _speedMultiplier;

    private void Awake() => _controller = GetComponent<CharacterController>();
    
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
        ApplyMovement();
        UpdateCrouchHeight();
        HandleSnowplow();
    }
    
    private void HandleInput()
    {
        // GAUCHE (Q ou Flèche Gauche)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q)) 
        {
            Debug.Log("[Player] Touche Gauche détectée");
            ChangeLane(-1);
        }
        // DROITE (D ou Flèche Droite)
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) 
        {
            Debug.Log("[Player] Touche Droite détectée");
            ChangeLane(1);
        }
        
        // S'ACCROUPIR (Shift Gauche ou Flèche Bas)
        _isCrouching = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftShift);
        
        // RALENTIR / CHASSE-NEIGE (Espace)
        _isSnowplowing = Input.GetKey(KeyCode.Space) && !IsAccelerated;
    }
    
    private void ChangeLane(int direction)
    {
        int newLane = Mathf.Clamp(_currentLane + direction, 1, 3);
        if (newLane == _currentLane) return;

        _currentLane = newLane;
        UpdateLanePosition();
        AudioManager.Instance?.PlaySFX("Whoosh");
    }
    
    private void UpdateLanePosition() => _targetXPosition = (_currentLane - 2) * _laneDistance;

    private void ApplyMovement()
    {
        // 1. Calcul latéral (X)
        float newX = Mathf.Lerp(transform.position.x, _targetXPosition, Time.deltaTime * _laneChangeSpeed);
        float xMovement = newX - transform.position.x;

        // 2. Calcul frontal (Z)
        float zMovement = (_currentSpeed * _speedMultiplier) * Time.deltaTime;

        // 3. Application au CharacterController
        // On ajoute un peu de gravité (-9.81) pour que le joueur reste au sol
        Vector3 moveVector = new Vector3(xMovement, -9.81f * Time.deltaTime, zMovement);
        _controller.Move(moveVector);
    }

    private void UpdateCrouchHeight()
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
        StopCoroutine(nameof(SpeedBoostCoroutine)); 
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

    public void StopSpeedBoost()
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