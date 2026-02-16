using UnityEngine;

/// <summary>
/// Je gère le système de score du jeu.
/// J'incrémente automatiquement le score selon la phase actuelle.
/// Je gère les bonus de collectibles et la multiplication pendant l'accélération.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // Je fournis un accès statique à mon instance
    public static ScoreManager Instance { get; private set; }
    
    [Header("Score Settings")]
    private float _currentScore = 0f; // Je stocke le score actuel (en float pour la précision)
    
    [Header("Score per Second (by phase)")]
    [SerializeField] private float _greenScoreRate = 10f;   // Je gagne 10 points/seconde en phase verte
    [SerializeField] private float _blueScoreRate = 25f;    // Je gagne 25 points/seconde en phase bleue
    [SerializeField] private float _redScoreRate = 50f;     // Je gagne 50 points/seconde en phase rouge
    [SerializeField] private float _blackScoreRate = 100f;  // Je gagne 100 points/seconde en phase noire
    
    private float _currentScoreRate; // Je stocke le taux de score actuel selon la phase
    
    [Header("Bonus Scores (by phase)")]
    [SerializeField] private float _greenBonus = 50f;   // Je donne 50 points de bonus en phase verte
    [SerializeField] private float _blueBonus = 100f;   // Je donne 100 points de bonus en phase bleue
    [SerializeField] private float _redBonus = 200f;    // Je donne 200 points de bonus en phase rouge
    [SerializeField] private float _blackBonus = 400f;  // Je donne 400 points de bonus en phase noire
    
    // ---------------------------------------------------------
    // ÉTATS
    // ---------------------------------------------------------
    
    private bool _isSpeedBoosted = false;            // Je stocke si le multiplicateur x2 est actif
    private TrackPhase _currentPhase = TrackPhase.Green; // Je stocke la phase actuelle
    
    // ---------------------------------------------------------
    // ÉVÉNEMENTS
    // ---------------------------------------------------------
    
    // J'invoque cet événement quand le score change
    public System.Action<float> OnScoreChanged;
    
    // ---------------------------------------------------------
    // PROPRIÉTÉS
    // ---------------------------------------------------------
    
    // Je donne accès au score en float (avec décimales)
    public float CurrentScoreFloat => _currentScore;
    
    // Je donne accès au score en int (arrondi vers le bas)
    public int CurrentScore => Mathf.FloorToInt(_currentScore);
    
    /// <summary>
    /// Je m'initialise en tant que singleton
    /// </summary>
    private void Awake()
    {
        // Si aucune instance n'existe
        if (Instance == null)
        {
            Instance = this; // Je deviens l'instance unique
            
            // ✅ CORRECTION 1 : Je me détache de mon parent pour être à la racine
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            
            // ✅ CORRECTION 2 : Maintenant je peux me rendre persistant
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[ScoreManager] Instance créée et rendue persistante");
        }
        else
        {
            // Si une instance existe déjà, je me détruis
            Destroy(gameObject);
            return;
        }
    }
    
    /// <summary>
    /// Je m'initialise au démarrage et je m'abonne aux événements
    /// </summary>
    private void Start()
    {
        // Je démarre avec le taux de score de la phase verte
        _currentScoreRate = _greenScoreRate;
        
        // Je réinitialise mon score à 0
        _currentScore = 0f;
        
        // Je m'abonne aux changements de phase pour ajuster mon taux de score
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += UpdatePhase;
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
    /// Je mets à jour le score à chaque frame si le jeu est en cours
    /// </summary>
    private void Update()
    {
        // Je vérifie que le jeu est en mode Playing
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            AddScorePerFrame(); // J'ajoute le score de cette frame
        }
    }
    
    /// <summary>
    /// J'ajoute le score pour cette frame
    /// </summary>
    private void AddScorePerFrame()
    {
        // Je calcule le score à ajouter pour cette frame
        // (taux par seconde × temps écoulé depuis la dernière frame)
        float scoreThisFrame = _currentScoreRate * Time.deltaTime;
        
        // Si le multiplicateur de vitesse est actif, je double le score
        if (_isSpeedBoosted)
            scoreThisFrame *= 2f;
        
        // J'ajoute le score calculé à mon total
        _currentScore += scoreThisFrame;
        
        // J'invoque l'événement pour notifier le changement
        OnScoreChanged?.Invoke(_currentScore);
    }
    
    /// <summary>
    /// Je retourne le bonus de collectible selon la phase actuelle
    /// </summary>
    /// <returns>Le montant du bonus en points</returns>
    public int GetCollectibleBonusForCurrentPhase()
    {
        // Je retourne le bonus approprié selon la phase actuelle
        return _currentPhase switch
        {
            TrackPhase.Green => Mathf.RoundToInt(_greenBonus),  // 50 points en vert
            TrackPhase.Blue => Mathf.RoundToInt(_blueBonus),    // 100 points en bleu
            TrackPhase.Red => Mathf.RoundToInt(_redBonus),      // 200 points en rouge
            TrackPhase.Black => Mathf.RoundToInt(_blackBonus),  // 400 points en noir
            _ => Mathf.RoundToInt(_greenBonus)                  // 50 points par défaut
        };
    }
    
    /// <summary>
    /// J'ajoute un bonus de score fixe
    /// </summary>
    /// <param name="bonus">Le montant du bonus à ajouter</param>
    public void AddBonusScore(int bonus)
    {
        // J'ajoute le bonus au score
        _currentScore += bonus;
        
        // J'invoque l'événement pour notifier le changement
        OnScoreChanged?.Invoke(_currentScore);
        
        // Je log l'ajout pour le debug
        Debug.Log($"[ScoreManager] Bonus ajouté : +{bonus}. Score total : {CurrentScore}");
    }
    
    /// <summary>
    /// J'ajoute un bonus selon la phase actuelle
    /// </summary>
    public void AddBonusScore()
    {
        // Je détermine le bonus selon la phase actuelle
        float bonus = _currentPhase switch
        {
            TrackPhase.Green => _greenBonus,  // 50 points en vert
            TrackPhase.Blue => _blueBonus,    // 100 points en bleu
            TrackPhase.Red => _redBonus,      // 200 points en rouge
            TrackPhase.Black => _blackBonus,  // 400 points en noir
            _ => _greenBonus                  // 50 points par défaut
        };
        
        // J'ajoute le bonus au score
        _currentScore += bonus;
        
        // J'invoque l'événement pour notifier le changement
        OnScoreChanged?.Invoke(_currentScore);
        
        // Je log l'ajout pour le debug
        Debug.Log($"[ScoreManager] Bonus de phase ajouté : +{bonus}. Score total : {CurrentScore}");
    }
    
    /// <summary>
    /// J'active ou désactive le multiplicateur de score x2
    /// </summary>
    /// <param name="active">True pour activer le multiplicateur, False pour le désactiver</param>
    public void SetSpeedMultiplier(bool active)
    {
        // Je mets à jour l'état du multiplicateur
        _isSpeedBoosted = active;
        
        // Je log le changement
        Debug.Log($"[ScoreManager] Multiplicateur x2 : {(active ? "ACTIVÉ" : "DÉSACTIVÉ")}");
    }
    
    /// <summary>
    /// J'active ou désactive le boost de vitesse (alias pour compatibilité)
    /// </summary>
    /// <param name="active">True pour activer, False pour désactiver</param>
    public void SetSpeedBoostActive(bool active)
    {
        // J'appelle la méthode principale
        SetSpeedMultiplier(active);
    }
    
    /// <summary>
    /// Je mets à jour mon taux de score quand la phase change
    /// </summary>
    /// <param name="phase">La nouvelle phase</param>
    private void UpdatePhase(TrackPhase phase)
    {
        // Je mémorise la nouvelle phase
        _currentPhase = phase;
        
        // Je mets à jour mon taux de score selon la nouvelle phase
        _currentScoreRate = phase switch
        {
            TrackPhase.Green => _greenScoreRate,  // 10 points/seconde en vert
            TrackPhase.Blue => _blueScoreRate,    // 25 points/seconde en bleu
            TrackPhase.Red => _redScoreRate,      // 50 points/seconde en rouge
            TrackPhase.Black => _blackScoreRate,  // 100 points/seconde en noir
            _ => _greenScoreRate                  // 10 points/seconde par défaut
        };
        
        // Je log le changement pour le debug
        Debug.Log($"[ScoreManager] Phase changée vers {phase}. Nouveau taux : {_currentScoreRate} pts/sec");
    }
    
    /// <summary>
    /// Je réinitialise le score pour une nouvelle partie
    /// </summary>
    public void ResetScore()
    {
        // Je sauvegarde l'ancien score pour le log
        float oldScore = _currentScore;
        
        _currentScore = 0f;              // Je réinitialise le score à 0
        _isSpeedBoosted = false;         // Je désactive le multiplicateur
        _currentPhase = TrackPhase.Green; // Je reviens à la phase verte
        _currentScoreRate = _greenScoreRate; // Je réinitialise le taux de score
        
        OnScoreChanged?.Invoke(0f);      // J'invoque l'événement avec le score à 0
        
        // Je log la réinitialisation
        Debug.Log($"[ScoreManager] Score réinitialisé. Ancien score : {Mathf.FloorToInt(oldScore)}");
    }
}