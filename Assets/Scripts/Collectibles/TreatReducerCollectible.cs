using UnityEngine;

/// <summary>
/// Collectible Boule de Noël : réduit immédiatement la menace de 10%.
/// </summary>
public class ThreatReducerCollectible : CollectibleBase
{
    [Header("Threat Reduction")]
    [SerializeField] private float _reductionAmount = 10f;
    
    protected override void ApplyEffect()
    {
        // On vérifie si l'instance existe pour éviter les erreurs
        if (ThreatManager.Instance != null)
        {
            // CORRECTION : On utilise ReduceThreat au lieu de ReduceThreatImmediate
            ThreatManager.Instance.ReduceThreat(_reductionAmount);
            
            Debug.Log($"[ThreatReducerCollectible] Menace réduite de {_reductionAmount}% !");
        }
        
        // Le son est généralement déjà géré par ton PlayerCollision, 
        // mais si tu veux le doubler ici :
        AudioManager.Instance?.PlaySFX("WowYeah");
    }
}