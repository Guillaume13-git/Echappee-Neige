using UnityEngine;
using System.Collections;

/// <summary>
/// Gère les collisions et les bonus du joueur.
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    [Header("Invulnerability")]
    [SerializeField] private float _invulnerabilityDuration = 3f;
    private bool _isInvulnerable = false;
    
    [Header("Shield")]
    private bool _hasShield = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private Renderer[] _playerRenderers;
    [SerializeField] private float _blinkInterval = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;
    
    private PlayerController _playerController;

    public bool HasShield => _hasShield;

    private void Awake() => _playerController = GetComponent<PlayerController>();

    private void OnTriggerEnter(Collider other)
    {
        // Sécurité GameManager - Autoriser Tutorial ET Playing
        if (GameManager.Instance != null && 
            GameManager.Instance.CurrentState != GameState.Playing && 
            GameManager.Instance.CurrentState != GameState.Tutorial) 
        {
            return;
        }

        if (other.CompareTag("Collectible"))
        {
            HandleCollectible(other.gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            HandleObstacleCollision();
        }
    }

    /// <summary>
    /// Active le bouclier (appelé par les collectibles).
    /// </summary>
    public void ActivateShield()
    {
        _hasShield = true;
        AudioManager.Instance?.PlaySFX("ShieldUp");
        if (_showDebugLogs) Debug.Log("[PlayerCollision] 🛡️ Bouclier activé !");
    }

    private void HandleObstacleCollision()
    {
        if (_isInvulnerable) 
        {
            if (_showDebugLogs) Debug.Log("[PlayerCollision] Collision ignorée (invulnérable)");
            return;
        }

        if (_showDebugLogs) Debug.Log("[PlayerCollision] 💥 Collision avec obstacle détectée !");

        // 1. Priorité au Bouclier
        if (_hasShield)
        {
            _hasShield = false;
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            if (_showDebugLogs) Debug.Log("[PlayerCollision] Bouclier brisé !");
            StartCoroutine(InvulnerabilityEffect());
            return;
        }

        // 2. Priorité au Boost de vitesse
        if (_playerController != null && _playerController.IsAccelerated)
        {
            _playerController.StopSpeedBoost();
            AudioManager.Instance?.PlaySFX("SpeedLost");
            if (_showDebugLogs) Debug.Log("[PlayerCollision] Boost de vitesse perdu !");
            StartCoroutine(InvulnerabilityEffect());
            return;
        }

        // 3. Sinon : Dégâts normaux (Menace)
        // Ne pas ajouter de menace pendant le tutoriel
        if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
        {
            ThreatManager.Instance?.AddThreatFromCollision();
            if (_showDebugLogs) Debug.Log("[PlayerCollision] Menace ajoutée !");
        }
        else if (_showDebugLogs)
        {
            Debug.Log("[PlayerCollision] Mode Tutorial : pas de menace ajoutée");
        }
        
        AudioManager.Instance?.PlaySFX("Ouch");
        StartCoroutine(InvulnerabilityEffect());
    }

    private void HandleCollectible(GameObject obj)
    {
        // Déduit le type par le nom
        string type = obj.name.Replace("(Clone)", "").Trim();

        if (_showDebugLogs) Debug.Log($"[PlayerCollision] ⭐ Collectible ramassé : {type}");

        switch (type)
        {
            case "PainEpice":
                // Ne pas ajouter de score pendant le tutoriel
                if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
                {
                    ScoreManager.Instance?.AddBonusScore();
                }
                AudioManager.Instance?.PlaySFX("Miam");
                break;
                
            case "SucreOrge":
                _playerController?.ActivateSpeedBoost(10f);
                AudioManager.Instance?.PlaySFX("Crunch");
                break;
                
            case "Cadeau":
                ActivateShield();
                AudioManager.Instance?.PlaySFX("OhOh");
                break;
                
            case "BouleNoel":
                // Ne pas réduire la menace pendant le tutoriel
                if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
                {
                    ThreatManager.Instance?.ReduceThreat(10f);
                }
                AudioManager.Instance?.PlaySFX("WowYeah");
                break;
        }
        Destroy(obj);
    }

    private IEnumerator InvulnerabilityEffect()
    {
        _isInvulnerable = true;
        
        // Ne pas notifier le ThreatManager pendant le tutoriel
        if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
        {
            ThreatManager.Instance?.SetInvulnerabilityActive(true);
        }

        float timer = 0;
        while (timer < _invulnerabilityDuration)
        {
            foreach (var r in _playerRenderers) 
            {
                if(r) r.enabled = !r.enabled;
            }
            yield return new WaitForSeconds(_blinkInterval);
            timer += _blinkInterval;
        }

        // Reset visuel
        foreach (var r in _playerRenderers) 
        {
            if(r) r.enabled = true;
        }
        
        _isInvulnerable = false;
        
        // Ne pas notifier le ThreatManager pendant le tutoriel
        if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
        {
            ThreatManager.Instance?.SetInvulnerabilityActive(false);
        }
    }
}