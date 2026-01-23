using UnityEngine;
using System.Collections;

/// <summary>
/// Collectible Sucre d'orge : active la vitesse accélérée pendant 10 secondes.
/// VERSION CORRIGÉE - Pas de StartCoroutine sur un objet détruit.
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
        
        // ⭐ CORRIGÉ - On utilise directement les managers qui persistent
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetSpeedMultiplier(true);
            
            // ⭐ SOLUTION - On démarre la coroutine sur un objet persistant
            ScoreManager.Instance.StartCoroutine(DeactivateSpeedMultiplierAfterDelay(_boostDuration));
        }
        
        // Active la réduction de menace
        if (ThreatManager.Instance != null)
        {
            ThreatManager.Instance.SetSpeedBoostActive(true);
            
            // ⭐ SOLUTION - Pareil ici
            ThreatManager.Instance.StartCoroutine(DeactivateThreatBoostAfterDelay(_boostDuration));
        }
        
        // Son spécifique
        AudioManager.Instance?.PlaySFX("Crunch");
        
        Debug.Log("[SpeedCollectible] Vitesse accélérée activée !");
    }
    
    /// <summary>
    /// Coroutine statique pour désactiver le multiplicateur de score.
    /// </summary>
    private static IEnumerator DeactivateSpeedMultiplierAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetSpeedMultiplier(false);
        }
    }
    
    /// <summary>
    /// Coroutine statique pour désactiver le boost de menace.
    /// </summary>
    private static IEnumerator DeactivateThreatBoostAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (ThreatManager.Instance != null)
        {
            ThreatManager.Instance.SetSpeedBoostActive(false);
        }
    }
}