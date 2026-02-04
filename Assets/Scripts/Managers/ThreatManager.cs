using UnityEngine;
using System.Collections;

/// <summary>
/// Je gère la jauge de menace (avalanche) avec une progression fluide.
/// J'augmente la menace au fil du temps et je déclenche le Game Over si elle atteint 100%.
/// </summary>
public class ThreatManager : MonoBehaviour
{
    // Je fournis un accès statique à mon instance
    public static ThreatManager Instance { get; private set; }
    
    [Header("Threat Settings")]
    [SerializeField] private float _maxThreat = 100f;  // Je stocke la menace maximale (100%)
    private float _currentThreat = 0f;                 // Je stocke la menace actuelle
    
    [Header("Auto Fill Intervals (by phase)")]
    [SerializeField] private float _greenInterval = 8f;   // Je gagne 1 point toutes les 8 secondes en phase verte
    [SerializeField] private float _blueInterval = 6f;    // Je gagne 1 point toutes les 6 secondes en phase bleue
    [SerializeField] private float _redInterval = 4f;     // Je gagne 1 point toutes les 4 secondes en phase rouge
    [SerializeField] private float _blackInterval = 2f;   // Je gagne 1 point toutes les 2 secondes en phase noire
    
    private float _currentGrowthRate; // Je stocke le taux de croissance actuel (points par seconde)
    
    [Header("Collision Damage (by phase)")]
    [SerializeField] private float _greenDamage = 5f;   // Je donne 5% de dégâts en phase verte
    [SerializeField] private float _blueDamage = 10f;   // Je donne 10% de dégâts en phase bleue
    [SerializeField] private float _redDamage = 20f;    // Je donne 20% de dégâts en phase rouge
    [SerializeField] private float _blackDamage = 30f;  // Je donne 30% de dégâts en phase noire
    
    // ---------------------------------------------------------
    // ÉTATS
    // ---------------------------------------------------------
    
    private bool _isSpeedBoostActive = false;        // Je stocke si le boost de vitesse est actif (réduit la menace)
    private bool _isSnowplowActive = false;          // Je stocke si le chasse-neige est actif (accélère la menace)
    private bool _isInvulnerabilityActive = false;   // Je stocke si l'invulnérabilité est active (bloque la menace)
    private TrackPhase _currentPhase = TrackPhase.Green; // Je stocke la phase actuelle
    
    // ---------------------------------------------------------
    // ÉVÉNEMENTS
    // ---------------------------------------------------------
    
    // J'invoque cet événement quand la menace change
    public System.Action<float> OnThreatChanged;
    
    // J'invoque cet événement quand le Game Over est déclenché
    public System.Action OnGameOver;
    
    // ---------------------------------------------------------
    // PROPRIÉTÉS
    // ---------------------------------------------------------
    
    // Je donne accès au pourcentage de menace (0 à 100%)
    public float ThreatPercentage => (_currentThreat / _maxThreat) * 100f;
    
    // Je donne accès à l'état de Game Over
    public bool IsGameOver => _currentThreat >= _maxThreat;
    
    /// <summary>
    /// Je m'initialise en tant que singleton
    /// </summary>
    private void Awake()
    {
        // Si aucune instance n'existe
        if (Instance == null) 
            Instance = this; // Je deviens l'instance unique
        else 
        { 
            Destroy(gameObject); // Si une instance existe déjà, je me détruis
            return; 
        }
    }
    
    /// <summary>
    /// Je m'initialise au démarrage et je m'abonne aux événements
    /// </summary>
    private void Start()
    {
        // Je commence avec une menace à 0
        _currentThreat = 0f;
        
        // J'initialise ma vitesse de progression dès le départ
        UpdatePhase(TrackPhase.Green);
        
        // Je m'abonne aux changements de phase pour ajuster ma vitesse de croissance
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdatePhase;
    }

    /// <summary>
    /// Je mets à jour la menace à chaque frame avec une progression fluide
    /// </summary>
    private void Update()
    {
        // Sécurité : je n'augmente la menace que si le jeu est en cours (Playing)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // PROGRESSION FLUIDE
        // Je n'augmente pas la menace si le joueur est invulnérable ou en boost (il fuit l'avalanche)
        if (!_isInvulnerabilityActive && !_isSpeedBoostActive)
        {
            // Je commence avec mon taux de croissance actuel
            float actualGrowth = _currentGrowthRate;

            // Si le chasse-neige (Snowplow) est actif, je double la vitesse de croissance
            // (le joueur ralentit, l'avalanche se rapproche plus vite)
            if (_isSnowplowActive) 
                actualGrowth *= 2f;

            // J'ajoute la menace pour cette frame (taux × temps écoulé)
            AddThreat(actualGrowth * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Je me désabonne des événements quand je suis détruit
    /// </summary>
    private void OnDestroy()
    {
        // Je me désabonne des changements de phase
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdatePhase;
    }
    
    /// <summary>
    /// J'ajoute de la menace et je vérifie si le Game Over est atteint
    /// </summary>
    /// <param name="amount">La quantité de menace à ajouter</param>
    public void AddThreat(float amount)
    {
        // J'ajoute la menace en m'assurant de ne pas dépasser le maximum
        _currentThreat = Mathf.Min(_currentThreat + amount, _maxThreat);
        
        // J'invoque l'événement pour notifier le changement
        OnThreatChanged?.Invoke(ThreatPercentage);
        
        // Si j'ai atteint le maximum, je déclenche le Game Over
        if (_currentThreat >= _maxThreat)
        {
            TriggerGameOver();
        }
    }
    
    /// <summary>
    /// J'ajoute de la menace suite à une collision avec un obstacle
    /// </summary>
    public void AddThreatFromCollision()
    {
        // Je ne prends pas de dégâts si le joueur est invulnérable
        if (_isInvulnerabilityActive) return;

        // Je détermine les dégâts selon la phase actuelle
        float damage = _currentPhase switch
        {
            TrackPhase.Green => _greenDamage,   // 5% en vert
            TrackPhase.Blue => _blueDamage,     // 10% en bleu
            TrackPhase.Red => _redDamage,       // 20% en rouge
            TrackPhase.Black => _blackDamage,   // 30% en noir
            _ => _greenDamage                   // 5% par défaut
        };
        
        // J'ajoute les dégâts à la menace
        AddThreat(damage);
    }
    
    /// <summary>
    /// Je réduis la menace (le joueur repousse l'avalanche)
    /// </summary>
    /// <param name="amount">La quantité de menace à retirer</param>
    public void ReduceThreat(float amount)
    {
        // Je retire la menace en m'assurant de ne pas passer en dessous de 0
        _currentThreat = Mathf.Max(_currentThreat - amount, 0f);
        
        // J'invoque l'événement pour notifier le changement
        OnThreatChanged?.Invoke(ThreatPercentage);
    }
    
    /// <summary>
    /// J'active ou désactive le boost de vitesse
    /// Pendant le boost, le joueur fuit l'avalanche et la menace diminue
    /// </summary>
    /// <param name="active">True pour activer, False pour désactiver</param>
    public void SetSpeedBoostActive(bool active)
    {
        // Si l'état n'a pas changé, je ne fais rien
        if (_isSpeedBoostActive == active) return;
        
        // Je mets à jour l'état
        _isSpeedBoostActive = active;
        
        // Si j'active le boost, je démarre la coroutine de réduction
        if (active) 
            StartCoroutine(SpeedBoostReductionCoroutine());
    }
    
    /// <summary>
    /// Je réduis progressivement la menace pendant le boost de vitesse
    /// </summary>
    private IEnumerator SpeedBoostReductionCoroutine()
    {
        // Tant que le boost est actif
        while (_isSpeedBoostActive)
        {
            // J'attends 1 seconde
            yield return new WaitForSeconds(1f);
            
            // Si le boost est toujours actif, je réduis la menace de 1%
            if (_isSpeedBoostActive) 
                ReduceThreat(1f);
        }
    }
    
    /// <summary>
    /// J'active ou désactive le chasse-neige (ralentit le joueur)
    /// </summary>
    /// <param name="active">True pour activer, False pour désactiver</param>
    public void SetSnowplowActive(bool active) => _isSnowplowActive = active;

    /// <summary>
    /// J'active ou désactive l'invulnérabilité (le joueur ne prend pas de dégâts)
    /// </summary>
    /// <param name="active">True pour activer, False pour désactiver</param>
    public void SetInvulnerabilityActive(bool active) => _isInvulnerabilityActive = active;
    
    /// <summary>
    /// Je mets à jour mon taux de croissance quand la phase change
    /// </summary>
    /// <param name="phase">La nouvelle phase</param>
    private void UpdatePhase(TrackPhase phase)
    {
        // Je mémorise la nouvelle phase
        _currentPhase = phase;
        
        // Je calcule le nouveau taux de croissance (points par seconde)
        // Formule : 1 point divisé par l'intervalle
        // Par exemple : Phase verte = 1 point / 8 secondes = 0.125 points/seconde
        _currentGrowthRate = phase switch
        {
            TrackPhase.Green => 1f / _greenInterval,  // 0.125 points/seconde (1 point toutes les 8s)
            TrackPhase.Blue => 1f / _blueInterval,    // 0.167 points/seconde (1 point toutes les 6s)
            TrackPhase.Red => 1f / _redInterval,      // 0.25 points/seconde (1 point toutes les 4s)
            TrackPhase.Black => 1f / _blackInterval,  // 0.5 points/seconde (1 point toutes les 2s)
            _ => 1f / _greenInterval                  // 0.125 points/seconde par défaut
        };
    }
    
    /// <summary>
    /// Je déclenche le Game Over quand la menace atteint 100%
    /// </summary>
    private void TriggerGameOver()
    {
        // Je vérifie que le Game Over n'est pas déjà déclenché
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver) 
            return;

        // J'invoque l'événement Game Over
        OnGameOver?.Invoke();
        
        // Je demande au GameManager de changer l'état du jeu
        GameManager.Instance?.SetGameState(GameState.GameOver);
        
        // Je joue le son de crash
        AudioManager.Instance?.PlaySFX("Crash");
    }
    
    /// <summary>
    /// Je réinitialise la menace pour une nouvelle partie
    /// </summary>
    public void ResetThreat()
    {
        _currentThreat = 0f;                // Je réinitialise la menace à 0
        _isSpeedBoostActive = false;        // Je désactive le boost de vitesse
        _isSnowplowActive = false;          // Je désactive le chasse-neige
        _isInvulnerabilityActive = false;   // Je désactive l'invulnérabilité
        OnThreatChanged?.Invoke(0f);        // J'invoque l'événement avec 0%
    }
}