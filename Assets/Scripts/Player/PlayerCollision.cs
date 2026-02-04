using UnityEngine;
using System.Collections;

/// <summary>
/// Je gère les collisions du joueur avec les obstacles et les collectibles.
/// J'inclus un système d'invulnérabilité au spawn pour éviter les dégâts à la frame 0.
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameManager _gameManager;           // Je stocke la référence au GameManager
    [SerializeField] private ThreatManager _threatManager;       // Je stocke la référence au ThreatManager
    [SerializeField] private ScoreManager _scoreManager;         // Je stocke la référence au ScoreManager
    [SerializeField] private PlayerController _playerController; // Je stocke la référence au PlayerController
    [SerializeField] private Renderer[] _playerRenderers;        // Je stocke les renderers du joueur pour le clignotement

    [Header("Invulnérabilité")]
    [SerializeField] private float _invulnerabilityDuration = 3f; // Je stocke la durée d'invulnérabilité après collision (3 secondes)
    [SerializeField] private float _blinkInterval = 0.1f;         // Je stocke l'intervalle de clignotement (0.1 seconde)
    
    private bool _isInvulnerable = false;              // Je stocke si le joueur est invulnérable (après collision)
    private bool _spawnInvulnerabilityActive = false;  // Je stocke si l'invulnérabilité de spawn est active
    
    [Header("Shield System")]
    private bool _hasShield = false;           // Je stocke si le joueur a un bouclier actif
    private Coroutine _shieldCoroutine;        // Je stocke la coroutine du bouclier
    
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false; // Je stocke si j'affiche les logs de debug

    /// <summary>
    /// Je récupère les références au démarrage
    /// </summary>
    private void Awake()
    {
        // Je récupère les références si elles ne sont pas assignées
        if (_gameManager == null) 
            _gameManager = FindFirstObjectByType<GameManager>();
        
        if (_threatManager == null) 
            _threatManager = FindFirstObjectByType<ThreatManager>();
        
        if (_scoreManager == null) 
            _scoreManager = FindFirstObjectByType<ScoreManager>();
        
        if (_playerController == null) 
            _playerController = GetComponent<PlayerController>();
    }

    /// <summary>
    /// Je m'abonne aux événements quand je suis activé
    /// </summary>
    private void OnEnable()
    {
        // Je m'abonne aux changements d'état du jeu pour détecter le début de partie
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged += OnGameStateChanged;
        }
    }

    /// <summary>
    /// Je me désabonne des événements quand je suis désactivé
    /// </summary>
    private void OnDisable()
    {
        // Je me désabonne des événements pour éviter les fuites mémoire
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    /// <summary>
    /// Je détecte quand le jeu démarre pour activer l'invulnérabilité de spawn
    /// Cela évite les collisions à la frame 0 (BUG 1 corrigé)
    /// </summary>
    /// <param name="newState">Le nouvel état du jeu</param>
    private void OnGameStateChanged(GameState newState)
    {
        // Si le jeu passe en mode Playing et que l'invulnérabilité de spawn n'est pas déjà active
        if (newState == GameState.Playing && !_spawnInvulnerabilityActive)
        {
            // Je démarre l'invulnérabilité de spawn
            StartCoroutine(SpawnInvulnerabilityCoroutine());
        }
    }

    /// <summary>
    /// Je gère l'invulnérabilité au spawn pour éviter les dégâts à la frame 0
    /// J'attends 2 frames pour que le CharacterController se stabilise après re-enable
    /// </summary>
    private IEnumerator SpawnInvulnerabilityCoroutine()
    {
        // J'active l'invulnérabilité de spawn
        _spawnInvulnerabilityActive = true;
        
        if (_showDebugLogs) 
            Debug.Log("[PlayerCollision] Invulnérabilité de spawn activée.");
        
        // J'attends 2 frames pour que Unity recalcule les collisions
        // après le re-enable du CharacterController
        yield return null;  // 1ère frame
        yield return null;  // 2ème frame
        
        // Je désactive l'invulnérabilité de spawn
        _spawnInvulnerabilityActive = false;
        
        if (_showDebugLogs) 
            Debug.Log("[PlayerCollision] Invulnérabilité de spawn terminée.");
    }

    /// <summary>
    /// Je gère les collisions avec les triggers (obstacles et collectibles)
    /// </summary>
    /// <param name="other">L'objet avec lequel je collisionne</param>
    private void OnTriggerEnter(Collider other)
    {
        // J'ignore les collisions pendant l'invulnérabilité de spawn
        if (_spawnInvulnerabilityActive)
        {
            if (_showDebugLogs) 
                Debug.Log($"[PlayerCollision] Collision ignorée (spawn invulnerable) : {other.gameObject.name}");
            return;
        }

        // J'ignore les collisions si le joueur est invulnérable
        if (_isInvulnerable) return;

        // Je vérifie le tag de l'objet avec lequel je collisionne
        if (other.CompareTag("Obstacle"))
        {
            // C'est un obstacle, je gère la collision
            HandleObstacleCollision(other.gameObject);
        }
        else if (other.CompareTag("Collectible"))
        {
            // C'est un collectible, je le ramasse
            HandleCollectible(other.gameObject);
        }
    }

    /// <summary>
    /// Je gère les collisions avec les obstacles
    /// </summary>
    /// <param name="obstacle">L'obstacle avec lequel je collisionne</param>
    private void HandleObstacleCollision(GameObject obstacle)
    {
        // ---------------------------------------------------------
        // CAS 1 : LE JOUEUR A UN BOUCLIER
        // ---------------------------------------------------------
        
        // Je vérifie si le joueur a un bouclier actif
        if (_hasShield)
        {
            if (_showDebugLogs) 
                Debug.Log("[PlayerCollision] Bouclier activé - obstacle bloqué !");
            
            // Je désactive le bouclier (il est consommé)
            DeactivateShield();
            
            // Je détruis l'obstacle sans faire de dégâts
            Destroy(obstacle);
            
            // Je joue le son du bouclier
            AudioManager.Instance?.PlaySFX("Shield");
            return; // Je ne fais rien d'autre
        }

        // ---------------------------------------------------------
        // CAS 2 : LE JOUEUR EST EN MODE VITESSE ACCÉLÉRÉE
        // ---------------------------------------------------------
        
        // Je vérifie si le joueur est en mode boost de vitesse
        if (_playerController != null && _playerController.IsAccelerated)
        {
            if (_showDebugLogs) 
                Debug.Log("[PlayerCollision] Boost actif - obstacle détruit !");
            
            // J'arrête le boost de vitesse (il est consommé)
            _playerController.StopSpeedBoost();
            
            // Je détruis l'obstacle sans faire de dégâts
            Destroy(obstacle);
            
            // Je joue le son de crash
            AudioManager.Instance?.PlaySFX("Crash");
            return; // Je ne fais rien d'autre
        }

        // ---------------------------------------------------------
        // CAS 3 : COLLISION NORMALE - LE JOUEUR PREND DES DÉGÂTS
        // ---------------------------------------------------------
        
        // J'ajoute de la menace au ThreatManager
        if (_threatManager != null)
        {
            _threatManager.AddThreatFromCollision();
        }

        // J'active l'invulnérabilité temporaire (3 secondes avec clignotement)
        StartCoroutine(InvulnerabilityCoroutine());

        // Je détruis l'obstacle
        Destroy(obstacle);
        
        // Je joue le son de douleur
        AudioManager.Instance?.PlaySFX("Ouch");
    }

    /// <summary>
    /// Je gère le ramassage des collectibles
    /// </summary>
    /// <param name="obj">Le collectible ramassé</param>
    private void HandleCollectible(GameObject obj)
    {
        // Je nettoie le nom de l'objet pour identifier son type
        string type = obj.name.Replace("(Clone)", "").Trim();
        
        if (_showDebugLogs) 
            Debug.Log($"[PlayerCollision] Collectible détecté : {type}");

        // Je détermine l'action selon le type de collectible
        switch (type)
        {
            // ---------------------------------------------------------
            // PAIN D'ÉPICE : BONUS DE SCORE
            // ---------------------------------------------------------
            case "PainEpice":
                // J'ajoute un bonus de score selon la phase actuelle
                if (_scoreManager != null)
                {
                    _scoreManager.AddBonusScore();
                }
                
                // Je joue le son "Miam"
                AudioManager.Instance?.PlaySFX("Miam");
                break;

            // ---------------------------------------------------------
            // SUCRE D'ORGE : BOOST DE VITESSE
            // ---------------------------------------------------------
            case "SucreOrge":
                // J'active le boost de vitesse pour 10 secondes
                if (_playerController != null)
                {
                    _playerController.ActivateSpeedBoost(10f);
                }
                
                // Je joue le son "Crunch"
                AudioManager.Instance?.PlaySFX("Crunch");
                break;

            // ---------------------------------------------------------
            // CADEAU : BOUCLIER
            // ---------------------------------------------------------
            case "Cadeau":
                // J'active le bouclier pour 10 secondes
                ActivateShield(10f);
                
                // Je joue le son "Oh Oh"
                AudioManager.Instance?.PlaySFX("OhOh");
                break;

            // ---------------------------------------------------------
            // BOULE DE NOËL : RÉDUCTION DE MENACE
            // ---------------------------------------------------------
            case "BouleDeNoel":
                // Je réduis la menace de 10%
                if (_threatManager != null)
                {
                    _threatManager.ReduceThreat(10f);
                }
                
                // Je joue le son "Wow Yeah"
                AudioManager.Instance?.PlaySFX("WowYeah");
                break;

            // ---------------------------------------------------------
            // COLLECTIBLE NON RECONNU
            // ---------------------------------------------------------
            default:
                Debug.LogWarning($"[PlayerCollision] Objet non reconnu : {type}");
                break;
        }

        // Je détruis le collectible après l'avoir ramassé
        Destroy(obj);
    }

    #region Shield System

    /// <summary>
    /// J'active le bouclier pour une durée donnée
    /// </summary>
    /// <param name="duration">La durée du bouclier en secondes</param>
    public void ActivateShield(float duration = 10f)
    {
        // Si un bouclier est déjà actif, j'arrête sa coroutine
        if (_shieldCoroutine != null)
        {
            StopCoroutine(_shieldCoroutine);
        }
        
        // J'active le bouclier
        _hasShield = true;
        
        // Je démarre la coroutine de gestion du bouclier
        _shieldCoroutine = StartCoroutine(ShieldCoroutine(duration));
        
        if (_showDebugLogs) 
            Debug.Log($"[PlayerCollision] Bouclier activé pour {duration}s");
    }

    /// <summary>
    /// Je désactive le bouclier
    /// </summary>
    public void DeactivateShield()
    {
        // J'arrête la coroutine du bouclier si elle existe
        if (_shieldCoroutine != null)
        {
            StopCoroutine(_shieldCoroutine);
            _shieldCoroutine = null;
        }
        
        // Je désactive le bouclier
        _hasShield = false;
        
        if (_showDebugLogs) 
            Debug.Log("[PlayerCollision] Bouclier désactivé");
    }

    /// <summary>
    /// Je gère la durée du bouclier
    /// </summary>
    /// <param name="duration">La durée en secondes</param>
    private IEnumerator ShieldCoroutine(float duration)
    {
        // J'attends la durée spécifiée
        yield return new WaitForSeconds(duration);
        
        // Je désactive le bouclier
        DeactivateShield();
    }

    /// <summary>
    /// Je donne accès en lecture seule à l'état du bouclier
    /// </summary>
    public bool HasShield => _hasShield;

    #endregion

    /// <summary>
    /// Je gère l'invulnérabilité temporaire après une collision
    /// Le joueur clignote pendant 3 secondes
    /// </summary>
    private IEnumerator InvulnerabilityCoroutine()
    {
        // J'active l'invulnérabilité
        _isInvulnerable = true;
        float elapsed = 0f;     // Je compte le temps écoulé
        bool visible = true;    // Je stocke l'état de visibilité pour le clignotement

        // J'active l'état d'invulnérabilité dans le ThreatManager
        // (pour arrêter la progression de la menace)
        if (_threatManager != null)
        {
            _threatManager.SetInvulnerabilityActive(true);
        }

        // Tant que la durée d'invulnérabilité n'est pas écoulée
        while (elapsed < _invulnerabilityDuration)
        {
            // J'inverse l'état de visibilité (clignotement)
            visible = !visible;
            
            // J'applique l'état de visibilité à tous les renderers
            foreach (var renderer in _playerRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }

            // J'attends l'intervalle de clignotement
            yield return new WaitForSeconds(_blinkInterval);
            
            // J'ajoute le temps écoulé
            elapsed += _blinkInterval;
        }

        // Je rends le joueur visible à nouveau (fin du clignotement)
        foreach (var renderer in _playerRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        // Je désactive l'état d'invulnérabilité dans le ThreatManager
        // (la menace recommence à progresser)
        if (_threatManager != null)
        {
            _threatManager.SetInvulnerabilityActive(false);
        }

        // Je désactive l'invulnérabilité
        _isInvulnerable = false;
    }
}