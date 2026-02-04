using UnityEngine;

/// <summary>
/// Je gère les phases de piste (verte/bleue/rouge/noire).
/// Je déclenche les transitions toutes les 60 secondes.
/// J'ajuste le FOV de la caméra pour accentuer la sensation de vitesse.
/// </summary>
public class PhaseManager : Singleton<PhaseManager>
{
    [Header("Phase Timing")]
    [SerializeField] private float _phaseDuration = 60f; // Je stocke la durée d'une phase (60 secondes)

    [Header("Camera FOV")]
    [SerializeField] private Camera _mainCamera;           // Je stocke la caméra principale
    [SerializeField] private float _greenFOV = 90f;        // Je stocke le FOV pour la phase verte
    [SerializeField] private float _blueFOV = 85f;         // Je stocke le FOV pour la phase bleue
    [SerializeField] private float _redFOV = 80f;          // Je stocke le FOV pour la phase rouge
    [SerializeField] private float _blackFOV = 75f;        // Je stocke le FOV pour la phase noire
    [SerializeField] private float _fovTransitionSpeed = 2f; // Je stocke la vitesse de transition du FOV

    private TrackPhase _currentPhase = TrackPhase.Green; // Je stocke la phase actuelle (vert au départ)
    private float _phaseTimer = 0f;                      // Je stocke le timer de la phase actuelle
    private float _targetFOV = 90f;                      // Je stocke le FOV cible vers lequel je dois tendre

    // J'invoque cet événement quand la phase change
    public System.Action<TrackPhase> OnPhaseChanged;
    
    // Je donne accès en lecture seule à la phase actuelle
    public TrackPhase CurrentPhase => _currentPhase;

    /// <summary>
    /// Je m'initialise au démarrage
    /// </summary>
    protected override void Awake()
    {
        // 1. Je me détache de mon parent pour pouvoir persister entre les scènes
        // (comme le fait le GameManager)
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        base.Awake(); // J'initialise le Singleton

        // Si la caméra n'est pas assignée, je récupère la caméra principale
        if (_mainCamera == null)
            _mainCamera = Camera.main;
    }

    /// <summary>
    /// Je démarre en phase verte au lancement du jeu
    /// </summary>
    private void Start()
    {
        SetPhase(TrackPhase.Green); // Je commence toujours par la phase verte
    }

    /// <summary>
    /// Je mets à jour mon timer et le FOV de la caméra à chaque frame
    /// </summary>
    private void Update()
    {
        // Je vérifie que le GameManager existe
        if (GameManager.Instance == null) return;

        // Je récupère l'état actuel du jeu
        GameState state = GameManager.Instance.CurrentState;
        
        // Je ne fais rien si le jeu n'est ni en Playing ni en Tutorial
        if (state != GameState.Playing && state != GameState.Tutorial)
            return;

        // Je n'avance le timer de phase que si le jeu est en cours (pas en tutoriel)
        // En tutoriel, je mets à jour le FOV mais je ne change pas de phase
        if (state == GameState.Playing)
        {
            UpdatePhaseTimer(); // J'avance mon timer de phase
        }

        // Je mets toujours à jour le FOV de la caméra (même en tutoriel)
        UpdateCameraFOV();
    }

    /// <summary>
    /// Je mets à jour mon timer de phase et je passe à la phase suivante si nécessaire
    /// </summary>
    private void UpdatePhaseTimer()
    {
        // J'avance mon timer
        _phaseTimer += Time.deltaTime;

        // Si j'ai atteint la durée d'une phase (60 secondes)
        if (_phaseTimer >= _phaseDuration)
        {
            _phaseTimer = 0f;          // Je réinitialise mon timer
            AdvanceToNextPhase();      // Je passe à la phase suivante
        }
    }

    /// <summary>
    /// Je passe à la phase suivante dans la progression
    /// </summary>
    private void AdvanceToNextPhase()
    {
        // Je détermine la phase suivante selon la phase actuelle
        TrackPhase nextPhase = _currentPhase switch
        {
            TrackPhase.Green => TrackPhase.Blue,   // Après le vert, je passe au bleu
            TrackPhase.Blue => TrackPhase.Red,     // Après le bleu, je passe au rouge
            TrackPhase.Red => TrackPhase.Black,    // Après le rouge, je passe au noir
            TrackPhase.Black => TrackPhase.Black,  // Le noir est la phase finale, je reste noir
            _ => TrackPhase.Green                  // Par défaut, je retourne au vert
        };

        // J'applique la nouvelle phase
        SetPhase(nextPhase);
    }

    /// <summary>
    /// Je définis une nouvelle phase et j'applique ses paramètres
    /// </summary>
    /// <param name="newPhase">La nouvelle phase à appliquer</param>
    public void SetPhase(TrackPhase newPhase)
    {
        // Je mets à jour ma phase actuelle
        _currentPhase = newPhase;

        // Je détermine le FOV cible selon la nouvelle phase
        _targetFOV = newPhase switch
        {
            TrackPhase.Green => _greenFOV,  // Phase verte : FOV 90°
            TrackPhase.Blue => _blueFOV,    // Phase bleue : FOV 85°
            TrackPhase.Red => _redFOV,      // Phase rouge : FOV 80°
            TrackPhase.Black => _blackFOV,  // Phase noire : FOV 75°
            _ => 90f                        // Par défaut : FOV 90°
        };

        // J'invoque l'événement pour notifier les autres systèmes du changement
        OnPhaseChanged?.Invoke(newPhase);
        
        // Je joue le son de changement de phase (sauf au tout début du jeu)
        // Je vérifie que 0.1 seconde s'est écoulée pour éviter le son au démarrage
        if (Time.timeSinceLevelLoad > 0.1f)
            AudioManager.Instance?.PlaySFX("Tududum");

        // J'affiche un log pour confirmer le changement de phase
        Debug.Log($"[PhaseManager] Phase changée : {newPhase}, FOV cible : {_targetFOV}");
    }

    /// <summary>
    /// Je mets à jour progressivement le FOV de la caméra vers la valeur cible
    /// </summary>
    private void UpdateCameraFOV()
    {
        // Je vérifie que j'ai une caméra assignée
        if (_mainCamera == null) return;

        // J'interpole progressivement le FOV actuel vers le FOV cible
        // Cela crée une transition fluide au lieu d'un changement brutal
        _mainCamera.fieldOfView = Mathf.Lerp(
            _mainCamera.fieldOfView,           // FOV actuel
            _targetFOV,                        // FOV cible
            Time.deltaTime * _fovTransitionSpeed // Vitesse de transition
        );
    }

    /// <summary>
    /// Je réinitialise les phases pour une nouvelle partie
    /// </summary>
    public void ResetPhases()
    {
        _phaseTimer = 0f;              // Je réinitialise mon timer
        SetPhase(TrackPhase.Green);    // Je retourne à la phase verte
    }
}