using UnityEngine;

/// <summary>
/// Gère les collisions du joueur avec les obstacles et collectibles.
/// Applique les effets correspondants et gère les frames d'invulnérabilité.
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    [Header("Invulnerability")]
    [SerializeField] private float _invulnerabilityDuration = 3f;
    private float _invulnerabilityTimer = 0f;
    private bool _isInvulnerable = false;
    
    [Header("Shield")]
    private bool _hasShield = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private Renderer[] _playerRenderers;
    [SerializeField] private float _blinkInterval = 0.1f;
    
    private PlayerController _playerController;
    
    public bool HasShield => _hasShield;
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }
    
    private void Update()
    {
        UpdateInvulnerability();
    }
    
    /// <summary>
    /// Détection des collisions via OnTriggerEnter (triggers sur obstacles/collectibles).
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        
        // Collectibles
        if (other.CompareTag("Collectible"))
        {
            CollectibleBase collectible = other.GetComponent<CollectibleBase>();
            if (collectible != null)
            {
                collectible.Collect();
            }
            return;
        }
        
        // Obstacles
        if (other.CompareTag("Obstacle"))
        {
            HandleObstacleCollision();
        }
    }
    
    /// <summary>
    /// Gère la collision avec un obstacle selon l'état du joueur.
    /// </summary>
    private void HandleObstacleCollision()
    {
        // Ignore pendant invulnérabilité
        if (_isInvulnerable) return;
        
        // Priorité 1 : Bouclier
        if (_hasShield)
        {
            ConsumeShield();
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            return;
        }
        
        // Priorité 2 : Vitesse accélérée
        if (_playerController.IsAccelerated)
        {
            CancelSpeedBoost();
            AudioManager.Instance?.PlaySFX("SpeedLost");
            return;
        }
        
        // Sinon : Augmentation de la menace
        ThreatManager.Instance?.AddThreatFromCollision();
        AudioManager.Instance?.PlaySFX("Ouch");
        
        // Déclenche l'invulnérabilité
        StartInvulnerability();
    }
    
    /// <summary>
    /// Active le bouclier (collectible Cadeau).
    /// </summary>
    public void ActivateShield()
    {
        _hasShield = true;
        Debug.Log("[PlayerCollision] Bouclier activé !");
        // TODO: Feedback visuel (aura, particules)
    }
    
    /// <summary>
    /// Consomme le bouclier lors d'une collision.
    /// </summary>
    private void ConsumeShield()
    {
        _hasShield = false;
        Debug.Log("[PlayerCollision] Bouclier consommé !");
    }
    
    /// <summary>
    /// Annule le boost de vitesse lors d'une collision.
    /// </summary>
    private void CancelSpeedBoost()
    {
        // La gestion est dans PlayerController via StopCoroutine
        // Ici on notifie juste
        Debug.Log("[PlayerCollision] Boost de vitesse perdu !");
    }
    
    /// <summary>
    /// Démarre les frames d'invulnérabilité.
    /// </summary>
    private void StartInvulnerability()
    {
        _isInvulnerable = true;
        _invulnerabilityTimer = _invulnerabilityDuration;
        
        // Désactive temporairement le remplissage auto de la menace
        ThreatManager.Instance?.SetInvulnerabilityActive(true);
        
        // Effet visuel de clignotement
        if (_playerRenderers.Length > 0)
        {
            StartCoroutine(BlinkEffect());
        }
    }
    
    /// <summary>
    /// Met à jour le timer d'invulnérabilité.
    /// </summary>
    private void UpdateInvulnerability()
    {
        if (!_isInvulnerable) return;
        
        _invulnerabilityTimer -= Time.deltaTime;
        
        if (_invulnerabilityTimer <= 0f)
        {
            _isInvulnerable = false;
            ThreatManager.Instance?.SetInvulnerabilityActive(false);
        }
    }
    
    /// <summary>
    /// Effet de clignotement pendant l'invulnérabilité.
    /// </summary>
    private System.Collections.IEnumerator BlinkEffect()
    {
        float elapsed = 0f;
        bool visible = true;
        
        while (elapsed < _invulnerabilityDuration)
        {
            visible = !visible;
            foreach (Renderer rend in _playerRenderers)
            {
                rend.enabled = visible;
            }
            
            yield return new WaitForSeconds(_blinkInterval);
            elapsed += _blinkInterval;
        }
        
        // Restaure la visibilité
        foreach (Renderer rend in _playerRenderers)
        {
            rend.enabled = true;
        }
    }
}