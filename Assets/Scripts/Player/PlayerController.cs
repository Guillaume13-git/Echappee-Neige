using UnityEngine;
using System.Collections;

/// <summary>
/// Je contrôle le joueur dans Échappée-Neige.
/// Je gère les déplacements entre couloirs, la gravité, l'accroupissement et le freinage (chasse-neige).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane System")]
    [SerializeField] private float _laneDistance = 1.84f;      // Je stocke la distance entre les couloirs
    [SerializeField] private float _laneChangeSpeed = 15f;     // Je stocke la vitesse de changement de couloir
    [SerializeField] private float _leanAmount = 10f;          // Je stocke l'inclinaison lors du changement de couloir
    private int _currentLaneIndex = 1;                         // Je stocke l'index du couloir actuel (0=gauche, 1=centre, 2=droite)
    private float _targetXPosition = 0f;                       // Je stocke la position X cible vers laquelle je me déplace

    // Je calcule les positions X des trois couloirs
    private float _leftLaneX => -_laneDistance;    // Couloir gauche : -1.84
    private float _centerLaneX => 0f;              // Couloir central : 0
    private float _rightLaneX => _laneDistance;    // Couloir droit : +1.84

    [Header("Forward Movement (World Logic)")]
    [SerializeField] private float _baseSpeed = 12f;  // Je stocke la vitesse de base (sera modifiée par les phases)
    private float _currentBaseSpeed;                  // Je stocke la vitesse de base actuelle
    private float _speedMultiplier = 1f;              // Je stocke le multiplicateur de vitesse (boost x2.5)

    [Header("Physics")]
    [SerializeField] private float _gravity = -30f;  // Je stocke la force de gravité
    private float _verticalVelocity;                 // Je stocke la vélocité verticale actuelle
    private bool _isGrounded;                        // Je stocke si le joueur est au sol

    [Header("Slowdown (Chasse-Neige)")]
    [Tooltip("Réduction de vitesse en m/s quand le chasse-neige est actif (GDD : -4 m/s)")]
    [SerializeField] private float _snowplowReduction = 4f;  // Je réduis la vitesse de 4 m/s avec le chasse-neige
    private bool _isSlowingDown = false;                     // Je stocke si le chasse-neige est actif

    [Header("Crouch & Visuals")]
    [SerializeField] private float _normalHeight = 2f;       // Je stocke la hauteur normale du joueur
    [SerializeField] private float _crouchHeight = 1f;       // Je stocke la hauteur accroupie du joueur
    [SerializeField] private float _crouchSpeed = 10f;       // Je stocke la vitesse de transition accroupi
    [SerializeField] private Transform _cameraTransform;     // Je stocke la caméra pour ajuster sa hauteur
    [SerializeField] private float _cameraNormalY = 0.8f;    // Je stocke la hauteur normale de la caméra
    [SerializeField] private float _cameraCrouchY = 0.3f;    // Je stocke la hauteur accroupie de la caméra
    private bool _isCrouching = false;                       // Je stocke si le joueur est accroupi

    private CharacterController _controller;  // Je stocke le CharacterController
    
    // Je donne accès en lecture seule à l'état d'accélération
    public bool IsAccelerated { get; private set; } = false;

    /// <summary>
    /// Je récupère mes composants au démarrage
    /// </summary>
    private void Awake()
    {
        // Je récupère le CharacterController
        _controller = GetComponent<CharacterController>();
        
        // Si la caméra n'est pas assignée, je récupère la caméra principale
        if (_cameraTransform == null) 
            _cameraTransform = Camera.main?.transform;
    }

    /// <summary>
    /// Je m'initialise au début de la partie
    /// </summary>
    private void Start()
    {
        // J'initialise ma vitesse de base
        _currentBaseSpeed = _baseSpeed;
        
        // Je démarre au centre (couloir 1)
        _currentLaneIndex = 1;
        _targetXPosition = _centerLaneX;

        // Je désactive temporairement le CharacterController pour repositionner le joueur
        _controller.enabled = false;
        transform.position = new Vector3(_centerLaneX, transform.position.y, 0f);
        _controller.enabled = true;

        // Je m'abonne aux changements de phase pour ajuster ma vitesse
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdateSpeedForPhase;
    }

    /// <summary>
    /// Je me désabonne des événements quand je suis détruit
    /// </summary>
    private void OnDestroy()
    {
        // Je me désabonne des changements de phase
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdateSpeedForPhase;
    }

    /// <summary>
    /// Je mets à jour le joueur à chaque frame
    /// </summary>
    private void Update()
    {
        // Je ne fais rien si le jeu n'est pas actif
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;

        // Je gère les entrées du joueur
        HandleInput();
        
        // J'applique le mouvement
        ApplyMovement();
        
        // Je mets à jour la hauteur si le joueur est accroupi
        UpdateCrouchHeight();
    }

    /// <summary>
    /// Je gère les entrées du joueur (clavier)
    /// </summary>
    private void HandleInput()
    {
        // ---------------------------------------------------------
        // CHANGEMENT DE COULOIR (AZERTY / QWERTY / FLÈCHES)
        // ---------------------------------------------------------
        
        // Je détecte si le joueur veut aller à gauche
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Q))
            MoveLane(-1);  // Je déplace vers la gauche
        
        // Je détecte si le joueur veut aller à droite
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            MoveLane(1);   // Je déplace vers la droite

        // ---------------------------------------------------------
        // ACCROUPISSEMENT (SHIFT / BAS / S)
        // ---------------------------------------------------------
        
        // Je vérifie si le joueur maintient une des touches d'accroupissement
        _isCrouching = Input.GetKey(KeyCode.LeftShift) || 
                       Input.GetKey(KeyCode.DownArrow) || 
                       Input.GetKey(KeyCode.S);
        
        // ---------------------------------------------------------
        // CHASSE-NEIGE / FREINAGE (ESPACE)
        // ---------------------------------------------------------
        
        // Je mémorise l'état précédent pour détecter les changements
        bool wasSlowingDown = _isSlowingDown;
        
        // Je vérifie si le joueur maintient la touche espace
        _isSlowingDown = Input.GetKey(KeyCode.Space);
        
        // Si l'état a changé, je notifie le ThreatManager
        // (le chasse-neige accélère la progression de la menace)
        if (_isSlowingDown != wasSlowingDown && ThreatManager.Instance != null)
        {
            ThreatManager.Instance.SetSnowplowActive(_isSlowingDown);
        }
    }

    /// <summary>
    /// Je déplace le joueur vers un couloir adjacent
    /// </summary>
    /// <param name="direction">-1 pour gauche, +1 pour droite</param>
    private void MoveLane(int direction)
    {
        // Je mémorise le couloir précédent
        int previousLane = _currentLaneIndex;
        
        // Je calcule le nouveau couloir en restant dans les limites (0-2)
        _currentLaneIndex = Mathf.Clamp(_currentLaneIndex + direction, 0, 2);
        
        // Si je suis déjà au bord, je ne fais rien
        if (previousLane == _currentLaneIndex) return;

        // Je détermine la position X cible selon le nouveau couloir
        _targetXPosition = _currentLaneIndex switch
        {
            0 => _leftLaneX,    // Couloir gauche
            1 => _centerLaneX,  // Couloir central
            2 => _rightLaneX,   // Couloir droit
            _ => 0f             // Par défaut (centre)
        };

        // Je joue le son de déplacement
        AudioManager.Instance?.PlaySFX("Whoosh");
    }

    /// <summary>
    /// Je retourne le couloir actuel (pour le TutorialManager)
    /// </summary>
    /// <returns>L'index du couloir (0=gauche, 1=centre, 2=droite)</returns>
    public int GetCurrentLane() => _currentLaneIndex;

    /// <summary>
    /// J'applique le mouvement du joueur (latéral et gravité)
    /// </summary>
    private void ApplyMovement()
    {
        // Je vérifie si le joueur est au sol
        _isGrounded = _controller.isGrounded;

        // ---------------------------------------------------------
        // DÉPLACEMENT LATÉRAL (CHANGEMENT DE COULOIR)
        // ---------------------------------------------------------
        
        // Je récupère ma position X actuelle
        float currentX = transform.position.x;
        
        // Je calcule ma prochaine position X en me déplaçant vers la cible
        float nextX = Mathf.MoveTowards(currentX, _targetXPosition, _laneChangeSpeed * Time.deltaTime);
        
        // Je calcule le déplacement X de cette frame
        float deltaX = nextX - currentX;

        // ---------------------------------------------------------
        // GRAVITÉ SIMPLE
        // ---------------------------------------------------------
        
        // Si je suis au sol, je réinitialise la vélocité verticale
        if (_isGrounded) 
            _verticalVelocity = -2f;  // Petite valeur négative pour rester collé au sol
        
        // J'applique la gravité
        _verticalVelocity += _gravity * Time.deltaTime;

        // ---------------------------------------------------------
        // APPLICATION DU MOUVEMENT
        // ---------------------------------------------------------
        
        // Je crée le vecteur de mouvement (X + gravité)
        Vector3 move = new Vector3(deltaX, _verticalVelocity * Time.deltaTime, 0f);
        
        // Je déplace le joueur avec le CharacterController
        _controller.Move(move);

        // ---------------------------------------------------------
        // INCLINAISON VISUELLE (LEAN)
        // ---------------------------------------------------------
        
        // Je calcule l'inclinaison en fonction de la vitesse de déplacement latéral
        float tilt = (deltaX / Time.deltaTime) * (_leanAmount / _laneChangeSpeed);
        
        // Je crée la rotation cible (inclinaison sur l'axe Z)
        Quaternion targetRotation = Quaternion.Euler(0, 0, -tilt);
        
        // J'interpole vers la rotation cible pour un mouvement fluide
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        // ---------------------------------------------------------
        // CORRECTION DE SÉCURITÉ Z
        // ---------------------------------------------------------
        
        // Je m'assure que le joueur reste toujours à Z = 0 (plan 2D)
        if (transform.position.z != 0)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }
    }

    /// <summary>
    /// Je calcule ma vitesse actuelle en avant (utilisée par le ChunkMover)
    /// </summary>
    /// <returns>La vitesse en mètres par seconde</returns>
    public float GetCurrentForwardSpeed()
    {
        // 1. Je commence avec ma vitesse de base (phase) × multiplicateur (boost)
        float speed = _currentBaseSpeed * _speedMultiplier;
        
        // 2. Je soustrais la réduction du chasse-neige (GDD : -4 m/s)
        // IMPORTANT : Le chasse-neige ne fonctionne PAS pendant le boost
        if (_isSlowingDown && !IsAccelerated)
        {
            speed -= _snowplowReduction;
            
            // Sécurité : Je m'assure de ne jamais descendre en dessous de 2 m/s
            speed = Mathf.Max(speed, 2f);
        }
        
        // Je retourne la vitesse finale
        return speed;
    }

    #region PowerUps

    /// <summary>
    /// J'active le boost de vitesse pour une durée donnée
    /// </summary>
    /// <param name="duration">La durée en secondes</param>
    public void ActivateSpeedBoost(float duration)
    {
        // J'arrête toute coroutine de boost en cours
        StopCoroutine(nameof(SpeedBoostCoroutine));
        
        // Je démarre une nouvelle coroutine de boost
        StartCoroutine(SpeedBoostCoroutine(duration));
        
        // Je notifie l'UI pour afficher l'icône
        BonusUIManager.Instance?.TriggerSpeedBoost(duration);
    }

    /// <summary>
    /// J'arrête le boost de vitesse immédiatement
    /// </summary>
    public void StopSpeedBoost()
    {
        // J'arrête la coroutine
        StopCoroutine(nameof(SpeedBoostCoroutine));
        
        // Je réinitialise le multiplicateur à 1
        _speedMultiplier = 1f;
        
        // Je désactive l'état d'accélération
        IsAccelerated = false;
        
        // Je notifie le ThreatManager (la menace recommence à progresser)
        ThreatManager.Instance?.SetSpeedBoostActive(false);
        
        // Je notifie le ScoreManager (le score n'est plus doublé)
        ScoreManager.Instance?.SetSpeedMultiplier(false);
    }

    /// <summary>
    /// Je gère le boost de vitesse pendant sa durée
    /// </summary>
    /// <param name="duration">La durée en secondes</param>
    private IEnumerator SpeedBoostCoroutine(float duration)
    {
        // J'active l'état d'accélération
        IsAccelerated = true;
        
        // Je multiplie ma vitesse par 2.5
        _speedMultiplier = 2.5f;
        
        // Je notifie le ThreatManager (la menace arrête de progresser et diminue)
        ThreatManager.Instance?.SetSpeedBoostActive(true);
        
        // Je notifie le ScoreManager (le score est doublé)
        ScoreManager.Instance?.SetSpeedMultiplier(true);
        
        // J'attends la durée spécifiée
        yield return new WaitForSeconds(duration);
        
        // Je désactive le boost
        StopSpeedBoost();
    }

    /// <summary>
    /// J'active le bouclier pour une durée donnée
    /// </summary>
    /// <param name="duration">La durée en secondes</param>
    public void ActivateShield(float duration)
    {
        // Je notifie l'UI pour afficher l'icône (la gestion réelle est dans PlayerCollision)
        BonusUIManager.Instance?.TriggerShield(duration);
    }

    #endregion

    /// <summary>
    /// Je mets à jour la hauteur du joueur et de la caméra selon l'état d'accroupissement
    /// </summary>
    private void UpdateCrouchHeight()
    {
        // Je détermine la hauteur cible du joueur
        float targetH = _isCrouching ? _crouchHeight : _normalHeight;
        
        // Je détermine la hauteur cible de la caméra
        float targetCamY = _isCrouching ? _cameraCrouchY : _cameraNormalY;

        // J'interpole la hauteur du CharacterController vers la cible
        _controller.height = Mathf.Lerp(_controller.height, targetH, Time.deltaTime * _crouchSpeed);
        
        // Je centre le collider verticalement
        _controller.center = new Vector3(0, _controller.height / 2, 0);

        // J'interpole la position de la caméra
        if (_cameraTransform != null)
        {
            Vector3 camPos = _cameraTransform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * _crouchSpeed);
            _cameraTransform.localPosition = camPos;
        }
    }

    /// <summary>
    /// Je mets à jour ma vitesse quand la phase change
    /// </summary>
    /// <param name="phase">La nouvelle phase</param>
    private void UpdateSpeedForPhase(TrackPhase phase)
    {
        // Je définis ma vitesse de base selon la phase (GDD)
        _currentBaseSpeed = phase switch
        {
            TrackPhase.Green => 5f,   // Phase verte : 5 m/s
            TrackPhase.Blue  => 10f,  // Phase bleue : 10 m/s
            TrackPhase.Red   => 15f,  // Phase rouge : 15 m/s
            TrackPhase.Black => 20f,  // Phase noire : 20 m/s
            _                => 5f    // Par défaut : 5 m/s
        };
    }

    /// <summary>
    /// Je dessine les gizmos dans l'éditeur pour visualiser les couloirs
    /// </summary>
    private void OnDrawGizmos()
    {
        // Je dessine les trois couloirs en magenta
        Gizmos.color = Color.magenta;
        
        // Couloir gauche
        Gizmos.DrawLine(new Vector3(-_laneDistance, 0, -5), new Vector3(-_laneDistance, 0, 20));
        
        // Couloir central
        Gizmos.DrawLine(new Vector3(0, 0, -5), new Vector3(0, 0, 20));
        
        // Couloir droit
        Gizmos.DrawLine(new Vector3(_laneDistance, 0, -5), new Vector3(_laneDistance, 0, 20));
    }
}