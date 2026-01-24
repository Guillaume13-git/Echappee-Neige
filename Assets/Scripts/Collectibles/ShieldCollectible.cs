using UnityEngine;

public class ShieldCollectible : CollectibleBase
{
    protected override void ApplyEffect()
    {
        // On cherche le composant de collision sur le joueur
        PlayerCollision playerCollision = FindFirstObjectByType<PlayerCollision>();

        if (playerCollision != null)
        {
            playerCollision.ActivateShield();
            // Le son est normalement déjà géré dans PlayerCollision ou ici, 
            // vérifie de ne pas le jouer deux fois.
            AudioManager.Instance?.PlaySFX("OhOh");
        }
        else 
        {
            Debug.LogWarning("[ShieldCollectible] PlayerCollision non trouvé !");
        }
    }
}