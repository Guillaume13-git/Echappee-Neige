using UnityEngine;
using System.Collections;

/// <summary>
/// Gère les collisions du joueur avec les obstacles et collectibles.
/// VERSION CORRIGÉE - Ajout de l'invulnérabilité au spawn (BUG 1)
/// VERSION FINALE - Compatible avec vos managers existants
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private ThreatManager _threatManager;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Renderer[] _playerRenderers;

    [Header("Invulnérabilité")]
    [SerializeField] private float _invulnerabilityDuration = 3f;
    [SerializeField] private float _blinkInterval = 0.1f;
    
    private bool _isInvulnerable = false;
    private bool _spawnInvulnerabilityActive = false; // NOUVEAU : pour le spawn
    
    [Header("Shield System")]
    private bool _hasShield = false;
    private Coroutine _shieldCoroutine;
    
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;

    private void Awake()
    {
        if (_gameManager == null) _gameManager = FindFirstObjectByType<GameManager>();
        if (_threatManager == null) _threatManager = FindFirstObjectByType<ThreatManager>();
        if (_scoreManager == null) _scoreManager = FindFirstObjectByType<ScoreManager>();
        if (_playerController == null) _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        // NOUVEAU : S'abonner aux changements d'état du jeu pour détecter le début de partie
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged += OnGameStateChanged;
        }
    }

    private void OnDisable()
    {
        // NOUVEAU : Se désabonner des événements
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    /// <summary>
    /// NOUVEAU : Détecte quand le jeu démarre pour activer l'invulnérabilité de spawn
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Playing && !_spawnInvulnerabilityActive)
        {
            StartCoroutine(SpawnInvulnerabilityCoroutine());
        }
    }

    /// <summary>
    /// NOUVEAU : Coroutine d'invulnérabilité au spawn pour éviter les dégâts frame 0
    /// Attend 2 frames pour que le CharacterController se stabilise après re-enable
    /// </summary>
    private IEnumerator SpawnInvulnerabilityCoroutine()
    {
        _spawnInvulnerabilityActive = true;
        
        if (_showDebugLogs) Debug.Log("[PlayerCollision] Invulnérabilité de spawn activée.");
        
        // Attendre 2 frames pour que Unity recalcule les collisions après le re-enable du CharacterController
        yield return null;
        yield return null;
        
        _spawnInvulnerabilityActive = false;
        
        if (_showDebugLogs) Debug.Log("[PlayerCollision] Invulnérabilité de spawn terminée.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // MODIFIÉ : Ignorer les collisions pendant l'invulnérabilité de spawn
        if (_spawnInvulnerabilityActive)
        {
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] Collision ignorée (spawn invulnerable) : {other.gameObject.name}");
            return;
        }

        if (_isInvulnerable) return;

        if (other.CompareTag("Obstacle"))
        {
            HandleObstacleCollision(other.gameObject);
        }
        else if (other.CompareTag("Collectible"))
        {
            HandleCollectible(other.gameObject);
        }
    }

    private void HandleObstacleCollision(GameObject obstacle)
    {
        // Vérifier si le joueur a un bouclier actif
        if (_hasShield)
        {
            if (_showDebugLogs) Debug.Log("[PlayerCollision] Bouclier activé - obstacle bloqué !");
            DeactivateShield();
            Destroy(obstacle);
            AudioManager.Instance?.PlaySFX("Shield");
            return;
        }

        // Vérifier si le joueur est en mode vitesse accélérée
        if (_playerController != null && _playerController.IsAccelerated)
        {
            if (_showDebugLogs) Debug.Log("[PlayerCollision] Boost actif - obstacle détruit !");
            _playerController.StopSpeedBoost();
            Destroy(obstacle);
            AudioManager.Instance?.PlaySFX("Crash");
            return;
        }

        // Appliquer les dégâts de menace
        if (_threatManager != null)
        {
            _threatManager.AddThreatFromCollision();
        }

        // Activer l'invulnérabilité temporaire
        StartCoroutine(InvulnerabilityCoroutine());

        // Détruire l'obstacle
        Destroy(obstacle);
        
        // Son de collision
        AudioManager.Instance?.PlaySFX("Ouch");
    }

    private void HandleCollectible(GameObject obj)
    {
        string type = obj.name.Replace("(Clone)", "").Trim();
        
        if (_showDebugLogs) Debug.Log($"[PlayerCollision] Collectible détecté : {type}");

        switch (type)
        {
            case "PainEpice":
                // Bonus de score selon la phase
                if (_scoreManager != null)
                {
                    _scoreManager.AddBonusScore();
                }
                AudioManager.Instance?.PlaySFX("Miam");
                break;

            case "SucreOrge":
                // Boost de vitesse
                if (_playerController != null)
                {
                    _playerController.ActivateSpeedBoost(10f);
                }
                AudioManager.Instance?.PlaySFX("Crunch");
                break;

            case "Cadeau":
                // Bouclier
                ActivateShield(10f); // 10 secondes de bouclier
                AudioManager.Instance?.PlaySFX("OhOh");
                break;

            case "BouleDeNoel":
                // Réduction de menace
                if (_threatManager != null)
                {
                    _threatManager.ReduceThreat(10f);
                }
                AudioManager.Instance?.PlaySFX("WowYeah");
                break;

            default:
                Debug.LogWarning($"[PlayerCollision] Objet non reconnu : {type}");
                break;
        }

        Destroy(obj);
    }

    #region Shield System

    /// <summary>
    /// Active le bouclier pour une durée donnée
    /// </summary>
    public void ActivateShield(float duration = 10f)
    {
        if (_shieldCoroutine != null)
        {
            StopCoroutine(_shieldCoroutine);
        }
        
        _hasShield = true;
        _shieldCoroutine = StartCoroutine(ShieldCoroutine(duration));
        
        if (_showDebugLogs) Debug.Log($"[PlayerCollision] Bouclier activé pour {duration}s");
    }

    /// <summary>
    /// Désactive le bouclier
    /// </summary>
    public void DeactivateShield()
    {
        if (_shieldCoroutine != null)
        {
            StopCoroutine(_shieldCoroutine);
            _shieldCoroutine = null;
        }
        
        _hasShield = false;
        
        if (_showDebugLogs) Debug.Log("[PlayerCollision] Bouclier désactivé");
    }

    /// <summary>
    /// Coroutine de gestion du bouclier
    /// </summary>
    private IEnumerator ShieldCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        DeactivateShield();
    }

    /// <summary>
    /// Propriété publique pour vérifier si le bouclier est actif
    /// </summary>
    public bool HasShield => _hasShield;

    #endregion

    private IEnumerator InvulnerabilityCoroutine()
    {
        _isInvulnerable = true;
        float elapsed = 0f;
        bool visible = true;

        // Activer l'état d'invulnérabilité dans le ThreatManager
        if (_threatManager != null)
        {
            _threatManager.SetInvulnerabilityActive(true);
        }

        while (elapsed < _invulnerabilityDuration)
        {
            // Clignotement visuel
            visible = !visible;
            foreach (var renderer in _playerRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }

            yield return new WaitForSeconds(_blinkInterval);
            elapsed += _blinkInterval;
        }

        // Rendre le joueur visible à nouveau
        foreach (var renderer in _playerRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        // Désactiver l'état d'invulnérabilité dans le ThreatManager
        if (_threatManager != null)
        {
            _threatManager.SetInvulnerabilityActive(false);
        }

        _isInvulnerable = false;
    }
}