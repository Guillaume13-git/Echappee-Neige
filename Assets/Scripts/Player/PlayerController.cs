using UnityEngine;

/// <summary>
/// Contrôle les mouvements du joueur : déplacement latéral entre couloirs,
/// accroupissement, accélération, et synchronisation avec la caméra FPS.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane Movement")]
    [SerializeField] private float _laneDistance = 1f; // Distance entre couloirs
    [SerializeField] private float _laneChangeSpeed = 10f; // Vitesse de transition
    private int _currentLane = 2; // 1 = gauche, 2 = centre, 3 = droite
    private float _targetXPosition = 0f;
    
    [Header("Forward Movement")]
    [SerializeField] private float _baseSpeed = 5f; // Piste verte
    private float _currentSpeed;
    private float _speedMultiplier = 1f;
    
    [Header("Crouch")]
    [SerializeField] private float _normalHeight = 2f;
    [SerializeField] private float _crouchHeight = 1f;
    [SerializeField] private float _crouchSpeed = 5f;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _cameraNormalY = 0.8f;
    [SerializeField] private float _cameraCrouchY = 0.3f;
    private bool _isCrouching = false;
    
    [Header("References")]
    [SerializeField] private CharacterController _controller;
    
    // États
    public bool IsAccelerated { get; private set; } = false;
    public float CurrentSpeed => _currentSpeed;
    public int CurrentLane => _currentLane;
    
    // Événements
    public System.Action<int> OnLaneChanged;
    
    private void Awake()
    {
        if (_controller == null)
            _controller = GetComponent<CharacterController>();
            
        if (_cameraTransform == null)
            _cameraTransform = Camera.main.transform;
    }
    
    private void Start()
    {
        _currentSpeed = _baseSpeed;
        UpdateLanePosition();
        
        // S'abonner aux changements de phase pour ajuster la vitesse
        PhaseManager.Instance.OnPhaseChanged += UpdateSpeedForPhase;
    }
    
    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdateSpeedForPhase;
    }
    
    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        
        HandleInput();
        MoveForward();
        MoveLateralSmooth();
        UpdateCrouchPosition();
    }
    
    /// <summary>
    /// Gère les inputs clavier pour déplacement et accroupissement.
    /// </summary>
    private void HandleInput()
    {
        // Déplacement latéral
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
        {
            MoveLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveLane(1);
        }
        
        // Accroupissement
        bool crouchInput = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftShift);
        SetCrouching(crouchInput);
        
        // Chasse-neige (ralentissement)
        if (Input.GetKey(KeyCode.Space) && !IsAccelerated)
        {
            ApplySnowplowSlowdown();
        }
    }
    
    /// <summary>
    /// Change de couloir (direction = -1 pour gauche, +1 pour droite).
    /// </summary>
    private void MoveLane(int direction)
    {
        int newLane = _currentLane + direction;
        
        // Contraintes : couloirs 1, 2, 3
        if (newLane < 1 || newLane > 3) return;
        
        _currentLane = newLane;
        UpdateLanePosition();
        OnLaneChanged?.Invoke(_currentLane);
        
        // Son de déplacement
        AudioManager.Instance?.PlaySFX("Whoosh");
    }
    
    /// <summary>
    /// Met à jour la position cible X selon le couloir actuel.
    /// </summary>
    private void UpdateLanePosition()
    {
        // Couloir 1 = -1m, Couloir 2 = 0m, Couloir 3 = +1m
        _targetXPosition = (_currentLane - 2) * _laneDistance;
    }
    
    /// <summary>
    /// Déplacement latéral fluide vers la position cible.
    /// </summary>
    private void MoveLateralSmooth()
    {
        Vector3 currentPos = transform.position;
        currentPos.x = Mathf.Lerp(currentPos.x, _targetXPosition, Time.deltaTime * _laneChangeSpeed);
        transform.position = currentPos;
    }
    
    /// <summary>
    /// Déplacement constant vers l'avant.
    /// </summary>
    private void MoveForward()
    {
        float actualSpeed = _currentSpeed * _speedMultiplier;
        Vector3 movement = Vector3.forward * actualSpeed * Time.deltaTime;
        _controller.Move(movement);
    }
    
    /// <summary>
    /// Active/désactive l'accroupissement.
    /// </summary>
    private void SetCrouching(bool crouch)
    {
        _isCrouching = crouch;
    }
    
    /// <summary>
    /// Ajuste la hauteur du collider et la position de la caméra pour l'accroupissement.
    /// </summary>
    private void UpdateCrouchPosition()
    {
        float targetHeight = _isCrouching ? _crouchHeight : _normalHeight;
        float targetCameraY = _isCrouching ? _cameraCrouchY : _cameraNormalY;
        
        _controller.height = Mathf.Lerp(_controller.height, targetHeight, Time.deltaTime * _crouchSpeed);
        
        Vector3 camPos = _cameraTransform.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCameraY, Time.deltaTime * _crouchSpeed);
        _cameraTransform.localPosition = camPos;
    }
    
    /// <summary>
    /// Applique le ralentissement du chasse-neige.
    /// </summary>
    private void ApplySnowplowSlowdown()
    {
        // Réduction de vitesse : -4 m/s
        _speedMultiplier = Mathf.Max(0.2f, (_currentSpeed - 4f) / _currentSpeed);
        
        // Double le remplissage de la menace (géré dans ThreatManager)
        ThreatManager.Instance?.SetSnowplowActive(true);
    }
    
    /// <summary>
    /// Met à jour la vitesse de base selon la phase actuelle.
    /// </summary>
    private void UpdateSpeedForPhase(TrackPhase phase)
    {
        switch (phase)
        {
            case TrackPhase.Green:
                _baseSpeed = 5f;
                break;
            case TrackPhase.Blue:
                _baseSpeed = 10f;
                break;
            case TrackPhase.Red:
                _baseSpeed = 15f;
                break;
            case TrackPhase.Black:
                _baseSpeed = 20f;
                break;
        }
        _currentSpeed = _baseSpeed;
    }
    
    /// <summary>
    /// Active l'accélération temporaire (collectible Sucre d'orge).
    /// </summary>
    public void ActivateSpeedBoost(float duration)
    {
        if (!IsAccelerated)
        {
            StartCoroutine(SpeedBoostCoroutine(duration));
        }
    }
    
    private System.Collections.IEnumerator SpeedBoostCoroutine(float duration)
    {
        IsAccelerated = true;
        _speedMultiplier = 4f; // x4 vitesse selon GDD
        
        // Désactive le remplissage automatique de la menace
        ThreatManager.Instance?.SetSpeedBoostActive(true);
        
        yield return new WaitForSeconds(duration);
        
        _speedMultiplier = 1f;
        IsAccelerated = false;
        ThreatManager.Instance?.SetSpeedBoostActive(false);
    }
}