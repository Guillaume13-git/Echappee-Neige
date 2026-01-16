using UnityEngine;

/// <summary>
/// Collectible Cadeau : donne un bouclier qui absorbe la prochaine collision.
/// </summary>
public class ShieldCollectible : CollectibleBase
{
    protected override void ApplyEffect()
    {
        // Nouvelle API Unity (remplace FindObjectOfType)
        PlayerCollision playerCollision = FindFirstObjectByType<PlayerCollision>();

        if (playerCollision != null)
        {
            playerCollision.ActivateShield();
        }

        AudioManager.Instance?.PlaySFX("OhOh");

        Debug.Log("[ShieldCollectible] Bouclier activé !");
    }
}