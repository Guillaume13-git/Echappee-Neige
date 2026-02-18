using UnityEngine;
using System.Collections;

/// <summary>
/// Je gère les collisions du joueur avec les obstacles et les collectibles.
/// Mon rôle : Détecter les collisions, appliquer les dégâts, ramasser les collectibles, et gérer l'invulnérabilité.
/// Je m'assure aussi qu'aucun dégât n'est pris à la frame 0 du jeu (invulnérabilité de spawn).
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameManager _gameManager;           // Je stocke la référence au GameManager
    [SerializeField] private ThreatManager _threatManager;       // Je stocke la référence au ThreatManager
    [SerializeField] private ScoreManager _scoreManager;         // Je stocke la référence au ScoreManager
    [SerializeField] private PlayerController _playerController; // Je stocke la référence au PlayerController
    [SerializeField] private Renderer[] _playerRenderers;        // Je stocke les renderers pour le clignotement

    [Header("Invulnérabilité")]
    [SerializeField] private float _invulnerabilityDuration = 3f; // Je stocke la durée d'invulnérabilité (3s)
    [SerializeField] private float _blinkInterval = 0.1f;         // Je stocke l'intervalle de clignotement (0.1s)
    
    private bool _isInvulnerable = false;              // Je sais si le joueur est invulnérable après collision
    private bool _spawnInvulnerabilityActive = false;  // Je sais si l'invulnérabilité de spawn est active
    
    [Header("Shield System")]
    private bool _hasShield = false; // Je stocke si le joueur a un bouclier actif (permanent jusqu'à collision)
    
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false; // Je décide si j'affiche mes logs de debug

    /// <summary>
    /// Au réveil, je récupère toutes mes références nécessaires
    /// </summary>
    private void Awake()
    {
        if (_showDebugLogs) Debug.Log("[PlayerCollision] 🎬 Awake - Récupération des références...");
        
        // Je récupère les managers si ils ne sont pas assignés dans l'Inspector
        if (_gameManager == null) 
        {
            _gameManager = FindFirstObjectByType<GameManager>();
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] GameManager trouvé : {(_gameManager != null ? "✓" : "❌")}");
        }
        
        if (_threatManager == null) 
        {
            _threatManager = FindFirstObjectByType<ThreatManager>();
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] ThreatManager trouvé : {(_threatManager != null ? "✓" : "❌")}");
        }
        
        if (_scoreManager == null) 
        {
            _scoreManager = FindFirstObjectByType<ScoreManager>();
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] ScoreManager trouvé : {(_scoreManager != null ? "✓" : "❌")}");
        }
        
        if (_playerController == null) 
        {
            _playerController = GetComponent<PlayerController>();
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] PlayerController trouvé : {(_playerController != null ? "✓" : "❌")}");
        }
        
        if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Awake terminé");
    }

    /// <summary>
    /// Quand je suis activé, je m'abonne aux événements du GameManager
    /// </summary>
    private void OnEnable()
    {
        if (_showDebugLogs) Debug.Log("[PlayerCollision] 📡 OnEnable - Abonnement aux événements...");
        
        // Je m'abonne aux changements d'état du jeu pour détecter le début de partie
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged += OnGameStateChanged;
            if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Abonné à OnGameStateChanged");
        }
        else
        {
            if (_showDebugLogs) Debug.LogWarning("[PlayerCollision] ⚠️ GameManager NULL, impossible de s'abonner");
        }
    }

    /// <summary>
    /// Quand je suis désactivé, je me désabonne proprement des événements
    /// </summary>
    private void OnDisable()
    {
        if (_showDebugLogs) Debug.Log("[PlayerCollision] 📡 OnDisable - Désabonnement des événements...");
        
        // Je me désabonne pour éviter les fuites mémoire
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged -= OnGameStateChanged;
            if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Désabonné de OnGameStateChanged");
        }
    }

    /// <summary>
    /// Je détecte quand le jeu passe en mode Playing pour activer l'invulnérabilité de spawn
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        if (_showDebugLogs) Debug.Log($"[PlayerCollision] 🎮 État du jeu changé : {newState}");
        
        // Si le jeu démarre et que l'invulnérabilité de spawn n'est pas déjà active
        if (newState == GameState.Playing && !_spawnInvulnerabilityActive)
        {
            if (_showDebugLogs) Debug.Log("[PlayerCollision] 🛡️ Démarrage de l'invulnérabilité de spawn...");
            StartCoroutine(SpawnInvulnerabilityCoroutine());
        }
    }

    /// <summary>
    /// Je gère l'invulnérabilité au spawn (2 frames) pour éviter les collisions à la frame 0
    /// </summary>
    private IEnumerator SpawnInvulnerabilityCoroutine()
    {
        // J'active l'invulnérabilité de spawn
        _spawnInvulnerabilityActive = true;
        if (_showDebugLogs) Debug.Log("[PlayerCollision] 🛡️ Invulnérabilité de spawn ACTIVÉE");
        
        // J'attends 2 frames pour que Unity stabilise les collisions
        yield return null; // Frame 1
        yield return null; // Frame 2
        
        // Je désactive l'invulnérabilité de spawn
        _spawnInvulnerabilityActive = false;
        if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Invulnérabilité de spawn TERMINÉE");
    }

    /// <summary>
    /// Je détecte toutes les collisions avec les triggers (obstacles et collectibles)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // J'ignore les collisions si je suis invulnérable (spawn ou après dégâts)
        if (_spawnInvulnerabilityActive)
        {
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] 🚫 Collision ignorée (spawn invulnérable) : {other.gameObject.name}");
            return;
        }
        
        if (_isInvulnerable)
        {
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] 🚫 Collision ignorée (invulnérable) : {other.gameObject.name}");
            return;
        }

        // Je détecte le type d'objet avec lequel je collisionne
        if (other.CompareTag("Obstacle"))
        {
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] 💥 OBSTACLE détecté : {other.gameObject.name}");
            HandleObstacleCollision(other.gameObject);
        }
        else if (other.CompareTag("Collectible"))
        {
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] ⭐ COLLECTIBLE détecté : {other.gameObject.name}");
            HandleCollectible(other.gameObject);
        }
        else
        {
            if (_showDebugLogs) Debug.Log($"[PlayerCollision] ❓ Objet non tagué : {other.gameObject.name}");
        }
    }

    /// <summary>
    /// Je gère les collisions avec les obstacles (3 cas : bouclier, boost, dégâts normaux)
    /// </summary>
    private void HandleObstacleCollision(GameObject obstacle)
    {
        if (_showDebugLogs) Debug.Log("[PlayerCollision] ⚔️ Traitement de la collision obstacle...");
        
        // ---------------------------------------------------------
        // CAS 1 : LE JOUEUR A UN BOUCLIER (absorbe le coup)
        // ---------------------------------------------------------
        if (_hasShield)
        {
            if (_showDebugLogs) Debug.Log("[PlayerCollision] 🛡️ BOUCLIER ACTIF - Obstacle bloqué !");
            
            // Je fais disparaître l'icône du bouclier dans l'UI
            BonusUIManager.Instance?.DeactivateShield();
            
            // Je désactive le bouclier (il est consommé)
            DeactivateShield();
            
            // Je détruis l'obstacle sans dégâts
            Destroy(obstacle);
            
            // Je joue le son du bouclier
            AudioManager.Instance?.PlaySFX("Shield");
            
            if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Obstacle bloqué par le bouclier");
            return;
        }

        // ---------------------------------------------------------
        // CAS 2 : LE JOUEUR EST EN BOOST DE VITESSE (détruit l'obstacle)
        // ---------------------------------------------------------
        if (_playerController != null && _playerController.IsAccelerated)
        {
            if (_showDebugLogs) Debug.Log("[PlayerCollision] 🚀 BOOST ACTIF - Obstacle détruit !");
            
            // J'arrête le boost de vitesse (il est consommé)
            _playerController.StopSpeedBoost();
            
            // Je détruis l'obstacle sans dégâts
            Destroy(obstacle);
            
            // Je joue le son de crash
            AudioManager.Instance?.PlaySFX("Crash");
            
            if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Obstacle détruit par le boost");
            return;
        }

        // ---------------------------------------------------------
        // CAS 3 : COLLISION NORMALE - LE JOUEUR PREND DES DÉGÂTS
        // ---------------------------------------------------------
        if (_showDebugLogs) Debug.Log("[PlayerCollision] 💔 Collision normale - Application des dégâts...");
        
        // J'ajoute de la menace au ThreatManager
        if (_threatManager != null)
        {
            _threatManager.AddThreatFromCollision();
            if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Menace ajoutée");
        }
        else
        {
            if (_showDebugLogs) Debug.LogWarning("[PlayerCollision] ⚠️ ThreatManager NULL, pas de dégâts appliqués");
        }

        // J'active l'invulnérabilité temporaire (3s avec clignotement)
        StartCoroutine(InvulnerabilityCoroutine());
        
        // Je détruis l'obstacle
        Destroy(obstacle);
        
        // Je joue le son de douleur
        AudioManager.Instance?.PlaySFX("Ouch");
        
        if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Dégâts appliqués, invulnérabilité activée");
    }

    /// <summary>
    /// Je gère le ramassage des collectibles selon leur type
    /// </summary>
    private void HandleCollectible(GameObject obj)
    {
        // Je nettoie le nom pour identifier le type (enlève "(Clone)")
        string type = obj.name.Replace("(Clone)", "").Trim();
        
        if (_showDebugLogs) Debug.Log($"[PlayerCollision] 🎁 Traitement du collectible : {type}");

        switch (type)
        {
            // ---------------------------------------------------------
            // PAIN D'ÉPICE : BONUS DE SCORE
            // ---------------------------------------------------------
            case "PainEpice":
                if (_showDebugLogs) Debug.Log("[PlayerCollision] 🍪 Pain d'Épice ramassé !");
                
                // J'ajoute le bonus de score selon la phase actuelle
                if (_scoreManager != null)
                {
                    _scoreManager.AddBonusScore();
                    if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Bonus de score ajouté");
                }
                else
                {
                    if (_showDebugLogs) Debug.LogWarning("[PlayerCollision] ⚠️ ScoreManager NULL");
                }
                
                // Je joue le son "Miam"
                AudioManager.Instance?.PlaySFX("Miam");
                break;

            // ---------------------------------------------------------
            // SUCRE D'ORGE : BOOST DE VITESSE (10 secondes)
            // ---------------------------------------------------------
            case "SucreOrge":
                if (_showDebugLogs) Debug.Log("[PlayerCollision] 🍬 Sucre d'Orge ramassé !");
                
                // J'active le boost de vitesse pour 10 secondes
                if (_playerController != null)
                {
                    _playerController.ActivateSpeedBoost(10f);
                    if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Boost de vitesse activé (10s)");
                }
                else
                {
                    if (_showDebugLogs) Debug.LogWarning("[PlayerCollision] ⚠️ PlayerController NULL");
                }
                
                // J'affiche l'icône UI avec timer circulaire
                BonusUIManager.Instance?.TriggerSpeedBoost(10f);
                if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Icône Speed Boost affichée");
                
                // Je joue le son "Crunch"
                AudioManager.Instance?.PlaySFX("Crunch");
                break;

            // ---------------------------------------------------------
            // CADEAU : BOUCLIER (PERMANENT jusqu'à collision)
            // ---------------------------------------------------------
            case "Cadeau":
                if (_showDebugLogs) Debug.Log("[PlayerCollision] 🎁 Cadeau ramassé !");
                
                // J'active le bouclier (permanent, pas de durée)
                ActivateShield();
                if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Bouclier permanent activé");
                
                // J'affiche l'icône UI (999s = durée factice, disparaît au contact obstacle)
                BonusUIManager.Instance?.TriggerShield(999f);
                if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Icône Bouclier affichée");
                
                // Je joue le son "Oh Oh"
                AudioManager.Instance?.PlaySFX("OhOh");
                break;

            // ---------------------------------------------------------
            // BOULE DE NOËL : RÉDUCTION DE MENACE (-10%)
            // ---------------------------------------------------------
            case "BouleDeNoel":
                if (_showDebugLogs) Debug.Log("[PlayerCollision] 🔴 Boule de Noël ramassée !");
                
                // Je réduis la menace de 10%
                if (_threatManager != null)
                {
                    _threatManager.ReduceThreat(10f);
                    if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Menace réduite de 10%");
                }
                else
                {
                    if (_showDebugLogs) Debug.LogWarning("[PlayerCollision] ⚠️ ThreatManager NULL");
                }
                
                // Je joue le son "Wow Yeah"
                AudioManager.Instance?.PlaySFX("WowYeah");
                break;

            // ---------------------------------------------------------
            // COLLECTIBLE NON RECONNU
            // ---------------------------------------------------------
            default:
                Debug.LogWarning($"[PlayerCollision] ❌ Collectible non reconnu : '{type}'");
                break;
        }
        
        // Je détruis le collectible après l'avoir ramassé
        Destroy(obj);
        if (_showDebugLogs) Debug.Log($"[PlayerCollision] ✓ Collectible '{type}' détruit");
    }

    #region Shield System

    /// <summary>
    /// J'active le bouclier de manière permanente (il reste jusqu'à absorption d'un coup)
    /// </summary>
    public void ActivateShield()
    {
        // J'active le booléen du bouclier
        _hasShield = true;
        
        if (_showDebugLogs) Debug.Log("[PlayerCollision] 🛡️ Bouclier ACTIVÉ (permanent jusqu'à collision)");
    }

    /// <summary>
    /// Je désactive le bouclier (appelé quand il absorbe un obstacle)
    /// </summary>
    public void DeactivateShield()
    {
        // Je désactive le booléen du bouclier
        _hasShield = false;
        
        if (_showDebugLogs) Debug.Log("[PlayerCollision] 🛡️ Bouclier DÉSACTIVÉ (consommé)");
    }

    /// <summary>
    /// Je donne accès en lecture seule à l'état du bouclier
    /// </summary>
    public bool HasShield => _hasShield;

    #endregion

    /// <summary>
    /// Je gère l'invulnérabilité temporaire de 3 secondes avec clignotement
    /// </summary>
    private IEnumerator InvulnerabilityCoroutine()
    {
        if (_showDebugLogs) Debug.Log("[PlayerCollision] 🛡️ Début de l'invulnérabilité temporaire (3s)");
        
        // J'active l'invulnérabilité
        _isInvulnerable = true;
        float elapsed = 0f;     // Je compte le temps écoulé
        bool visible = true;    // J'alterne la visibilité pour le clignotement

        // J'informe le ThreatManager que je suis invulnérable (arrête la progression de la menace)
        if (_threatManager != null)
        {
            _threatManager.SetInvulnerabilityActive(true);
            if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ ThreatManager informé (arrêt menace)");
        }

        // Tant que les 3 secondes ne sont pas écoulées
        while (elapsed < _invulnerabilityDuration)
        {
            // J'inverse l'état de visibilité (clignotement)
            visible = !visible;
            
            // J'applique la visibilité à tous les renderers du joueur
            foreach (var renderer in _playerRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }

            // J'attends l'intervalle de clignotement (0.1s)
            yield return new WaitForSeconds(_blinkInterval);
            
            // J'ajoute le temps écoulé
            elapsed += _blinkInterval;
        }

        // Fin de l'invulnérabilité : je rends le joueur visible
        foreach (var renderer in _playerRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        // J'informe le ThreatManager que l'invulnérabilité est terminée
        if (_threatManager != null)
        {
            _threatManager.SetInvulnerabilityActive(false);
            if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ ThreatManager informé (reprise menace)");
        }

        // Je désactive l'invulnérabilité
        _isInvulnerable = false;
        
        if (_showDebugLogs) Debug.Log("[PlayerCollision] ✓ Invulnérabilité temporaire TERMINÉE");
    }
}