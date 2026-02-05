using UnityEngine;

/// <summary>
/// Je suis le gestionnaire central de l'affichage des bonus dans l'UI.
/// Mon rôle est de faire le lien entre le système de collectibles et l'affichage visuel des bonus actifs.
/// </summary>
public class BonusUIManager : MonoBehaviour
{
    // Je m'expose comme instance unique pour être accessible depuis n'importe où dans le code
    public static BonusUIManager Instance { get; private set; }

    // Je stocke la référence vers l'affichage du bonus de vitesse
    [SerializeField] private BonusDisplay _speedBoostDisplay;
    
    // Je stocke la référence vers l'affichage du bouclier
    [SerializeField] private BonusDisplay _shieldDisplay;

    /// <summary>
    /// Au réveil, je m'initialise comme instance unique (pattern Singleton).
    /// </summary>
    private void Awake()
    {
        // Si aucune instance n'existe encore, je deviens l'instance de référence
        // Cela me permet d'être appelé facilement via BonusUIManager.Instance depuis d'autres scripts
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// Je suis appelé quand le joueur ramasse un bonus de vitesse (sucre d'orge).
    /// Je déclenche alors l'affichage visuel du bonus avec son timer.
    /// </summary>
    /// <param name="duration">La durée pendant laquelle le bonus sera actif (en secondes)</param>
    public void TriggerSpeedBoost(float duration)
    {
        // Je demande à mon BonusDisplay dédié d'afficher le bonus de vitesse
        _speedBoostDisplay.ShowBonus(duration);
    }

    /// <summary>
    /// Je suis appelé quand le joueur ramasse un bouclier (cadeau de Noël).
    /// Je déclenche alors l'affichage visuel du bouclier avec son timer.
    /// </summary>
    /// <param name="duration">La durée pendant laquelle le bouclier sera actif (en secondes)</param>
    public void TriggerShield(float duration)
    {
        // Je demande à mon BonusDisplay dédié d'afficher le bouclier
        _shieldDisplay.ShowBonus(duration);
    }
}