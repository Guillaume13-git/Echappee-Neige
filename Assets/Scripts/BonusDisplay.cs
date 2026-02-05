using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Je gère l'affichage visuel d'un bonus temporaire avec son timer de cooldown.
/// Mon rôle est de montrer au joueur combien de temps il lui reste avant que le bonus n'expire.
/// </summary>
public class BonusDisplay : MonoBehaviour
{
    [SerializeField] private Image _cooldownOverlay; // Je stocke l'image qui sert de barre de progression circulaire
    private float _duration; // Je garde en mémoire la durée totale du bonus
    private float _timer; // Je compte le temps restant avant expiration
    private bool _isActive; // Je sais si le bonus est actuellement actif ou non

    /// <summary>
    /// Au réveil, je me masque car aucun bonus n'est actif au démarrage.
    /// </summary>
    private void Awake()
    {
        // Je cache mon GameObject au démarrage car je ne dois apparaître que lorsqu'un bonus est ramassé
        gameObject.SetActive(false);
    }

    /// <summary>
    /// On m'appelle quand le joueur ramasse un bonus.
    /// Je m'affiche et je lance le décompte du timer.
    /// </summary>
    /// <param name="duration">La durée totale du bonus en secondes</param>
    public void ShowBonus(float duration)
    {
        // Je sauvegarde la durée totale du bonus pour calculer le pourcentage restant
        _duration = duration;
        
        // J'initialise mon timer avec la durée complète
        _timer = duration;
        
        // Je passe en mode actif pour lancer le décompte dans Update()
        _isActive = true;
        
        // Je me rends visible à l'écran
        gameObject.SetActive(true);
    }

    /// <summary>
    /// À chaque frame, je mets à jour le timer et la barre de progression.
    /// Quand le timer atteint 0, je me masque automatiquement.
    /// </summary>
    private void Update()
    {
        // Si je ne suis pas actif, je ne fais rien (optimisation)
        if (!_isActive) return;

        // Je décrémente mon timer en fonction du temps écoulé depuis la dernière frame
        _timer -= Time.deltaTime;

        // Je mets à jour la barre de progression visuelle si elle existe
        if (_cooldownOverlay != null)
        {
            // Je calcule le ratio temps_restant/durée_totale pour avoir une valeur entre 0 et 1
            // La barre diminue progressivement de 1 (plein) vers 0 (vide)
            _cooldownOverlay.fillAmount = _timer / _duration;
        }

        // Quand le timer arrive à 0 ou en dessous, le bonus a expiré
        if (_timer <= 0)
        {
            // Je passe en mode inactif
            _isActive = false;
            
            // Je me masque car le bonus n'est plus actif
            gameObject.SetActive(false);
        }
    }
}