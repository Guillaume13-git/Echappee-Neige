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
    
    private PlayerController _playerController;

    public bool HasShield => _hasShield;

    private void Awake() => _playerController = GetComponent<PlayerController>();

    private void OnTriggerEnter(Collider other)
    {
        // Sécurité GameManager
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

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
        // On peut ajouter ici un AudioManager.Instance?.PlaySFX("ShieldUp"); si besoin
        Debug.Log("[PlayerCollision] Bouclier activé !");
    }

    private void HandleObstacleCollision()
    {
        if (_isInvulnerable) return;

        // 1. Priorité au Bouclier
        if (_hasShield)
        {
            _hasShield = false;
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            StartCoroutine(InvulnerabilityEffect());
            return;
        }

        // 2. Priorité au Boost de vitesse
        if (_playerController != null && _playerController.IsAccelerated)
        {
            _playerController.StopSpeedBoost();
            AudioManager.Instance?.PlaySFX("SpeedLost");
            StartCoroutine(InvulnerabilityEffect());
            return;
        }

        // 3. Sinon : Dégâts normaux (Menace)
        ThreatManager.Instance?.AddThreatFromCollision();
        AudioManager.Instance?.PlaySFX("Ouch");
        StartCoroutine(InvulnerabilityEffect());
    }

    private void HandleCollectible(GameObject obj)
    {
        // Déduit le type par le nom
        string type = obj.name.Replace("(Clone)", "").Trim();

        switch (type)
        {
            case "PainEpice":
                ScoreManager.Instance?.AddBonusScore();
                AudioManager.Instance?.PlaySFX("Miam");
                break;
            case "SucreOrge":
                _playerController?.ActivateSpeedBoost(10f);
                AudioManager.Instance?.PlaySFX("Crunch");
                break;
            case "Cadeau":
                ActivateShield(); // Utilise maintenant la méthode publique
                AudioManager.Instance?.PlaySFX("OhOh");
                break;
            case "BouleNoel":
                ThreatManager.Instance?.ReduceThreat(10f);
                AudioManager.Instance?.PlaySFX("WowYeah");
                break;
        }
        Destroy(obj);
    }

    private IEnumerator InvulnerabilityEffect()
    {
        _isInvulnerable = true;
        ThreatManager.Instance?.SetInvulnerabilityActive(true);

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
        ThreatManager.Instance?.SetInvulnerabilityActive(false);
    }
}