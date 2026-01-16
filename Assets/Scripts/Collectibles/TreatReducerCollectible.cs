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
        ThreatManager.Instance.ReduceThreatImmediate(_reductionAmount);
        
        AudioManager.Instance?.PlaySFX("WowYeah");
        
        Debug.Log($"[ThreatReducerCollectible] Menace réduite de {_reductionAmount}% !");
    }
}