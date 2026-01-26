using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane System")]
    [SerializeField] private float _laneDistance = 1.84f; 
    [SerializeField] private float _laneChangeSpeed = 15f;
    private int _currentLaneIndex = 1; // 0: Gauche, 1: Centre, 2: Droite
    private float _targetXPosition = 0f;
    
    // Propriétés de calcul pour la clarté
    private float _leftLaneX => -_laneDistance;
    private float _centerLaneX => 0f;
    private float _rightLaneX => _laneDistance;

    [Header("Forward Movement (World Logic)")]
    [SerializeField] private float _baseSpeed = 12f;
    private float _currentBaseSpeed; // Vitesse de base actuelle (change selon la phase)
    private float _speedMultiplier = 1f;

    [Header("Physics")]
    [SerializeField] private float _gravity = -30f;
    private float _verticalVelocity;
    private bool _isGrounded;
    
    [Header("Slowdown (Chasse-Neige)")]
    [SerializeField] private float _slowdownMultiplier = 0.6f;
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
    private Vector3 _moveDirection;

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
        
        // Positionnement initial
        Vector3 startPos = transform.position;
        startPos.x = _centerLaneX;
        startPos.z = 0f; // On s'assure de démarrer à 0
        transform.position = startPos;

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
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        HandleInput();
        ApplyMovement();
        UpdateCrouchHeight();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q)) 
            MoveLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) 
            MoveLane(1);

        _isCrouching = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        _isSlowingDown = Input.GetKey(KeyCode.Space);
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

    private void ApplyMovement()
    {
        _isGrounded = _controller.isGrounded;

        // 1. MOUVEMENT LATÉRAL (X)
        float currentX = transform.position.x;
        float nextX = Mathf.MoveTowards(currentX, _targetXPosition, _laneChangeSpeed * Time.deltaTime);
        float xVelocity = (nextX - currentX) / Time.deltaTime;

        // 2. PHYSIQUE (Y)
        if (_isGrounded && _verticalVelocity < 0) _verticalVelocity = -2f;
        _verticalVelocity += _gravity * Time.deltaTime;

        // 3. APPLICATION DU MOUVEMENT
        // On met 0 en Z : le joueur ne bouge plus physiquement vers l'avant.
        // C'est le ChunkMover qui lira la vitesse via GetCurrentForwardSpeed().
        _moveDirection = new Vector3(xVelocity, _verticalVelocity, 0f); 
        _controller.Move(_moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// Retourne la vitesse actuelle calculée (Base * Multiplicateurs).
    /// Utilisée par le ChunkMover pour déplacer le décor.
    /// </summary>
    public float GetCurrentForwardSpeed()
    {
        float finalMultiplier = _speedMultiplier;
        
        // Ralentissement uniquement si on n'est pas en boost
        if (_isSlowingDown && !IsAccelerated)
        {
            finalMultiplier *= _slowdownMultiplier;
        }
        
        return _currentBaseSpeed * finalMultiplier;
    }

    #region Speed Boost logic
    public void ActivateSpeedBoost(float duration)
    {
        StopCoroutine(nameof(SpeedBoostCoroutine));
        StartCoroutine(SpeedBoostCoroutine(duration));
    }

    public void StopSpeedBoost()
    {
        StopCoroutine(nameof(SpeedBoostCoroutine));
        IsAccelerated = false;
        _speedMultiplier = 1f;
        ThreatManager.Instance?.SetSpeedBoostActive(false);
    }

    private IEnumerator SpeedBoostCoroutine(float duration)
    {
        IsAccelerated = true;
        _speedMultiplier = 2.5f;
        ThreatManager.Instance?.SetSpeedBoostActive(true);
        yield return new WaitForSeconds(duration);
        _speedMultiplier = 1f;
        IsAccelerated = false;
        ThreatManager.Instance?.SetSpeedBoostActive(false);
    }
    #endregion

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

    private void UpdateSpeedForPhase(TrackPhase phase)
    {
        _currentBaseSpeed = phase switch {
            TrackPhase.Green => 12f,
            TrackPhase.Blue => 15f,
            TrackPhase.Red => 20f,
            TrackPhase.Black => 25f,
            _ => 12f
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