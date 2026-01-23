using UnityEngine;
using System.Collections;

/// <summary>
/// Contrôle les mouvements du joueur : déplacement latéral entre couloirs,
/// accroupissement, accélération, et synchronisation avec la caméra FPS.
/// Gère également les collisions avec obstacles et collectibles.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane Movement")]
    [SerializeField] private float _laneDistance = 1f; // Distance entre couloirs (1m selon GDD)
    [SerializeField] private float _laneChangeSpeed = 10f; // Vitesse de transition entre couloirs
    private int _currentLane = 2; // 1 = gauche, 2 = centre, 3 = droite
    private float _targetXPosition = 0f;
    
    [Header("Forward Movement")]
    [SerializeField] private float _baseSpeed = 5f; // Piste verte (5 m/s selon GDD)
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
    
    [Header("Snowplow (Chasse-neige)")]
    [SerializeField] private float _snowplowSpeedReduction = 4f; // -4 m/s selon GDD
    private bool _isSnowplowing = false;
    
    [Header("References")]
    [SerializeField] private CharacterController _controller;
    
    [Header("Invulnerability")]
    [SerializeField] private float _invulnerabilityDuration = 3f; // 3s selon GDD
    private bool _isInvulnerable = false;
    
    // États publics
    public bool IsAccelerated { get; private set; } = false;
    public bool IsInvulnerable => _isInvulnerable;
    public float CurrentSpeed => _currentSpeed * _speedMultiplier;
    public int CurrentLane => _currentLane;
    public bool IsCrouching => _isCrouching;
    
    // Événements
    public System.Action<int> OnLaneChanged;
    public System.Action OnObstacleHit;
    public System.Action<string> OnCollectiblePickup; // Type de collectible
    
    private void Awake()
    {
        // Initialisation des références
        if (_controller == null)
            _controller = GetComponent<CharacterController>();
            
        if (_cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                _cameraTransform = mainCam.transform;
        }
    }
    
    private void Start()
    {
        _currentSpeed = _baseSpeed;
        UpdateLanePosition();
        
        // S'abonner aux événements du PhaseManager
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdateSpeedForPhase;
    }
    
    private void OnDestroy()
    {
        // Se désabonner pour éviter les fuites mémoire
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdateSpeedForPhase;
    }
    
    private void Update()
    {
        // Ne pas bouger si le jeu n'est pas en cours
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) 
            return;
        
        HandleInput();
        MoveForward();
        MoveLateralSmooth();
        UpdateCrouchPosition();
        HandleSnowplow();
    }
    
    /// <summary>
    /// Gère les inputs clavier pour déplacement et accroupissement.
    /// </summary>
    private void HandleInput()
    {
        // Déplacement latéral (KeyDown pour éviter les répétitions)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
        {
            MoveLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveLane(1);
        }
        
        // Accroupissement (maintien de touche)
        bool crouchInput = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftShift);
        SetCrouching(crouchInput);
        
        // Chasse-neige (maintien de touche, désactivé si accéléré)
        _isSnowplowing = Input.GetKey(KeyCode.Space) && !IsAccelerated;
    }
    
    /// <summary>
    /// Change de couloir (direction = -1 pour gauche, +1 pour droite).
    /// </summary>
    private void MoveLane(int direction)
    {
        int newLane = _currentLane + direction;
        
        // Contraintes : couloirs 1, 2, 3 uniquement
        if (newLane < 1 || newLane > 3) return;
        
        _currentLane = newLane;
        UpdateLanePosition();
        OnLaneChanged?.Invoke(_currentLane);
        
        // Son de déplacement "Whoosh"
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Whoosh");
    }
    
    /// <summary>
    /// Met à jour la position cible X selon le couloir actuel.
    /// Couloir 1 = -1m, Couloir 2 = 0m, Couloir 3 = +1m
    /// </summary>
    private void UpdateLanePosition()
    {
        _targetXPosition = (_currentLane - 2) * _laneDistance;
    }
    
    /// <summary>
    /// Déplacement latéral fluide vers la position cible avec lerp.
    /// </summary>
    private void MoveLateralSmooth()
    {
        Vector3 currentPos = transform.position;
        currentPos.x = Mathf.Lerp(currentPos.x, _targetXPosition, Time.deltaTime * _laneChangeSpeed);
        transform.position = currentPos;
    }
    
    /// <summary>
    /// Déplacement constant vers l'avant selon la vitesse actuelle.
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
    /// Transition fluide avec lerp.
    /// </summary>
    private void UpdateCrouchPosition()
    {
        float targetHeight = _isCrouching ? _crouchHeight : _normalHeight;
        float targetCameraY = _isCrouching ? _cameraCrouchY : _cameraNormalY;
        
        // Ajustement du collider
        _controller.height = Mathf.Lerp(_controller.height, targetHeight, Time.deltaTime * _crouchSpeed);
        
        // Ajustement de la caméra (descente de 0.5m selon GDD)
        if (_cameraTransform != null)
        {
            Vector3 camPos = _cameraTransform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCameraY, Time.deltaTime * _crouchSpeed);
            _cameraTransform.localPosition = camPos;
        }
    }
    
    /// <summary>
    /// Gère l'effet du chasse-neige (ralentissement et menace accélérée).
    /// </summary>
    private void HandleSnowplow()
    {
        if (_isSnowplowing)
        {
            // Réduction de vitesse : vitesse actuelle - 4 m/s
            float reducedSpeed = Mathf.Max(_currentSpeed - _snowplowSpeedReduction, 1f);
            _speedMultiplier = reducedSpeed / _currentSpeed;
            
            // Notifier le ThreatManager pour doubler le remplissage
            if (ThreatManager.Instance != null)
                ThreatManager.Instance.SetSnowplowActive(true);
        }
        else
        {
            // Retour à la vitesse normale si pas accéléré
            if (!IsAccelerated)
                _speedMultiplier = 1f;
                
            if (ThreatManager.Instance != null)
                ThreatManager.Instance.SetSnowplowActive(false);
        }
    }
    
    /// <summary>
    /// Met à jour la vitesse de base selon la phase actuelle (couleur de piste).
    /// Vert: 5m/s, Bleu: 10m/s, Rouge: 15m/s, Noir: 20m/s
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
    /// Durée: 10s selon GDD, multiplicateur x4.
    /// </summary>
    public void ActivateSpeedBoost(float duration = 10f)
    {
        if (!IsAccelerated)
        {
            StartCoroutine(SpeedBoostCoroutine(duration));
        }
    }
    
    /// <summary>
    /// Coroutine de gestion du boost de vitesse.
    /// </summary>
    private IEnumerator SpeedBoostCoroutine(float duration)
    {
        IsAccelerated = true;
        _speedMultiplier = 4f; // x4 vitesse selon GDD
        
        // Désactive le remplissage automatique de la menace et active la réduction
        if (ThreatManager.Instance != null)
            ThreatManager.Instance.SetSpeedBoostActive(true);
        
        // Son d'activation
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("SpeedBoost");
        
        yield return new WaitForSeconds(duration);
        
        // Retour à la normale
        _speedMultiplier = 1f;
        IsAccelerated = false;
        
        if (ThreatManager.Instance != null)
            ThreatManager.Instance.SetSpeedBoostActive(false);
    }
    
    /// <summary>
    /// Active les frames d'invulnérabilité après collision (3s selon GDD).
    /// </summary>
    private void ActivateInvulnerability()
    {
        if (!_isInvulnerable)
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }
    
    /// <summary>
    /// Coroutine de gestion de l'invulnérabilité.
    /// </summary>
    private IEnumerator InvulnerabilityCoroutine()
    {
        _isInvulnerable = true;
        
        // Effet visuel optionnel (clignotement)
        // TODO: Ajouter effet visuel
        
        yield return new WaitForSeconds(_invulnerabilityDuration);
        
        _isInvulnerable = false;
    }
    
    // États pour les bonus
    private bool _hasShield = false;
    public bool HasShield => _hasShield;
    
    /// <summary>
    /// Active le bouclier (collectible Cadeau).
    /// </summary>
    public void ActivateShield()
    {
        _hasShield = true;
        
        // Son d'activation
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("ShieldActivate");
    }
    
    /// <summary>
    /// Gère la collision avec un obstacle.
    /// Priorité: Bouclier > Accélération > Dégâts normaux.
    /// </summary>
    public void HandleObstacleCollision()
    {
        // Ignorer si invulnérable
        if (_isInvulnerable) return;
        
        // Priorité 1: Bouclier actif
        if (_hasShield)
        {
            _hasShield = false; // Consommer le bouclier
            
            // Son de bouclier
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("ShieldBlock");
            // Pas de dégâts
        }
        // Priorité 2: Vitesse accélérée
        else if (IsAccelerated)
        {
            // Annuler l'accélération mais pas de dégâts
            StopAllCoroutines();
            IsAccelerated = false;
            _speedMultiplier = 1f;
            
            if (ThreatManager.Instance != null)
                ThreatManager.Instance.SetSpeedBoostActive(false);
        }
        // Priorité 3: Dégâts normaux
        else
        {
            // Ajouter de la menace selon la phase
            if (ThreatManager.Instance != null)
                ThreatManager.Instance.AddThreatFromObstacle();
        }
        
        // Son de collision
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Ouch");
        
        // Activer l'invulnérabilité
        ActivateInvulnerability();
        
        // Notifier les observers
        OnObstacleHit?.Invoke();
    }
    
    /// <summary>
    /// Gère la collision avec un collectible selon son type.
    /// </summary>
    public void HandleCollectibleCollision(string collectibleType)
    {
        OnCollectiblePickup?.Invoke(collectibleType);
        
        switch (collectibleType)
        {
            case "PainEpice": // Bonus de score
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddBonusScore();
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("Miam");
                break;
                
            case "SucreOrge": // Accélération
                ActivateSpeedBoost(10f);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("Crunch");
                break;
                
            case "Cadeau": // Bouclier
                ActivateShield();
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("OhOh");
                break;
                
            case "BouleNoel": // Réduction menace
                if (ThreatManager.Instance != null)
                    ThreatManager.Instance.ReduceThreat(10f);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("WowYeah");
                break;
        }
    }
    
    /// <summary>
    /// Détection des collisions avec le CharacterController.
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Vérifier si c'est un obstacle
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            HandleObstacleCollision();
        }
        // Vérifier si c'est un collectible
        else if (hit.gameObject.CompareTag("Collectible"))
        {
            // Récupérer le type via le nom du GameObject ou un composant
            string collectibleType = hit.gameObject.name.Replace("(Clone)", "").Trim();
            HandleCollectibleCollision(collectibleType);
            Destroy(hit.gameObject); // Détruire le collectible après ramassage
        }
    }
    
    /// <summary>
    /// Détection alternative avec triggers (si les collectibles utilisent des triggers).
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            // Récupérer le type via le nom du GameObject
            string collectibleType = other.gameObject.name.Replace("(Clone)", "").Trim();
            HandleCollectibleCollision(collectibleType);
            Destroy(other.gameObject);
        }
    }
}