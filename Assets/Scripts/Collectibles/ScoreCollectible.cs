using UnityEngine;

/// <summary>
/// Collectible Pain d'épice : donne un bonus de score selon la phase.
/// </summary>
public class ScoreCollectible : CollectibleBase
{
    protected override void ApplyEffect()
    {
        int bonus = ScoreManager.Instance.GetCollectibleBonusForCurrentPhase();
        ScoreManager.Instance.AddBonusScore(bonus);
        
        Debug.Log($"[ScoreCollectible] +{bonus} points !");
    }
}