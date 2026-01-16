using UnityEngine;

/// <summary>
/// Collectible Sucre d'orge : active la vitesse accélérée pendant 10 secondes.
/// </summary>
public class SpeedCollectible : CollectibleBase
{
    [Header("Speed Boost Settings")]
    [SerializeField] private float _boostDuration = 10f;
    
    protected override void ApplyEffect()
    {
        // Active le boost de vitesse sur le joueur
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.ActivateSpeedBoost(_boostDuration);
        }
        
        // Active le multiplicateur de score x2
        ScoreManager.Instance.SetSpeedMultiplier(true);
        
        // Désactive le remplissage de la menace + réduction active
        ThreatManager.Instance.SetSpeedBoostActive(true);
        
        // Son spécifique
        AudioManager.Instance?.PlaySFX("Crunch");
        
        Debug.Log("[SpeedCollectible] Vitesse accélérée activée !");
        
        // Désactive après la durée
        StartCoroutine(DeactivateAfterDuration());
    }
    
    private System.Collections.IEnumerator DeactivateAfterDuration()
    {
        yield return new WaitForSeconds(_boostDuration);
        
        ScoreManager.Instance.SetSpeedMultiplier(false);
        ThreatManager.Instance.SetSpeedBoostActive(false);
    }
}