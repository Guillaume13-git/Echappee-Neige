using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Je gère l'affichage visuel d'un bonus temporaire avec son timer de cooldown.
/// Mon rôle est de montrer au joueur combien de temps il lui reste avant que le bonus n'expire.
/// J'anime l'apparition et la disparition pour un effet visuel agréable.
/// Je peux fonctionner avec OU sans CooldownOverlay (optionnel).
/// </summary>
public class BonusDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("OPTIONNEL : Image du timer circulaire. Laisse vide si tu ne veux pas de timer visuel.")]
    [SerializeField] private Image _cooldownOverlay; // Je stocke l'image qui sert de barre de progression circulaire (OPTIONNEL)
    
    [Header("Animation Settings")]
    [SerializeField] private float _popupDuration = 0.3f;      // Durée de l'animation d'apparition
    [SerializeField] private float _disappearDuration = 0.2f;  // Durée de l'animation de disparition
    [SerializeField] private AnimationCurve _popupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Courbe d'animation pour le popup
    
    private float _duration;   // Je garde en mémoire la durée totale du bonus
    private float _timer;      // Je compte le temps restant avant expiration
    private bool _isActive;    // Je sais si le bonus est actuellement actif ou non
    private bool _isAnimating; // Je sais si je suis en train de m'animer

    private Vector3 _originalScale; // Je sauvegarde l'échelle d'origine

    /// <summary>
    /// Au réveil, je me masque car aucun bonus n'est actif au démarrage.
    /// </summary>
    private void Awake()
    {
        // Je sauvegarde mon échelle d'origine pour la restaurer après les animations
        _originalScale = transform.localScale;
        
        // Je cache mon GameObject au démarrage car je ne dois apparaître que lorsqu'un bonus est ramassé
        gameObject.SetActive(false);
        
        // J'affiche un avertissement si le CooldownOverlay n'est pas assigné (c'est OK, c'est optionnel)
        if (_cooldownOverlay == null)
        {
            Debug.Log($"[BonusDisplay] {gameObject.name} : Pas de CooldownOverlay assigné (mode sans timer visuel)");
        }
    }

    /// <summary>
    /// On m'appelle quand le joueur ramasse un bonus.
    /// Je m'affiche avec une animation de pop-up et je lance le décompte du timer.
    /// </summary>
    /// <param name="duration">La durée totale du bonus en secondes</param>
    public void ShowBonus(float duration)
    {
        Debug.Log($"[BonusDisplay] 🎯 ShowBonus appelé sur {gameObject.name} pour {duration}s");
        Debug.Log($"[BonusDisplay]   └─ GameObject actif AVANT : {gameObject.activeSelf}");
        Debug.Log($"[BonusDisplay]   └─ CooldownOverlay présent : {(_cooldownOverlay != null ? "OUI" : "NON")}");

        // Je sauvegarde la durée totale du bonus pour calculer le pourcentage restant
        _duration = duration;
        
        // J'initialise mon timer avec la durée complète
        _timer = duration;
        
        // Je passe en mode actif pour lancer le décompte dans Update()
        _isActive = true;
        
        // Je me rends visible à l'écran
        gameObject.SetActive(true);
        
        Debug.Log($"[BonusDisplay]   └─ GameObject actif APRÈS : {gameObject.activeSelf}");
        Debug.Log($"[BonusDisplay]   └─ Position : {transform.position}");
        Debug.Log($"[BonusDisplay]   └─ Scale : {transform.localScale}");
        
        // Si j'ai un CooldownOverlay, je le réinitialise à plein
        if (_cooldownOverlay != null)
        {
            _cooldownOverlay.fillAmount = 1f;
            Debug.Log($"[BonusDisplay]   └─ CooldownOverlay initialisé à 100%");
        }
        
        // Je lance l'animation d'apparition
        StartPopupAnimation();
        
        Debug.Log($"[BonusDisplay] ✓ Animation lancée sur {gameObject.name}");
    }

    /// <summary>
    /// Je lance l'animation d'apparition avec un effet de pop-up élastique.
    /// </summary>
    private void StartPopupAnimation()
    {
        // J'arrête toute animation en cours
        StopAllCoroutines();
        
        // Je démarre l'animation de pop-up
        StartCoroutine(PopupAnimation());
    }

    /// <summary>
    /// Coroutine qui gère l'animation d'apparition progressive.
    /// </summary>
    private System.Collections.IEnumerator PopupAnimation()
    {
        _isAnimating = true;
        
        // Je commence à échelle 0 (invisible)
        transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        
        // J'anime progressivement l'échelle de 0 à 1
        while (elapsed < _popupDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / _popupDuration;
            
            // J'utilise la courbe d'animation pour un effet élastique
            float curveValue = _popupCurve.Evaluate(progress);
            
            // J'applique l'échelle interpolée
            transform.localScale = _originalScale * curveValue;
            
            yield return null;
        }
        
        // Je m'assure d'arriver exactement à l'échelle finale
        transform.localScale = _originalScale;
        
        _isAnimating = false;
        
        Debug.Log($"[BonusDisplay] ✓ Animation d'apparition terminée sur {gameObject.name}");
    }

    /// <summary>
    /// Je lance l'animation de disparition avec un effet de rétrécissement.
    /// </summary>
    private void StartDisappearAnimation()
    {
        Debug.Log($"[BonusDisplay] 🔽 Début de l'animation de disparition sur {gameObject.name}");
        
        // J'arrête toute animation en cours
        StopAllCoroutines();
        
        // Je démarre l'animation de disparition
        StartCoroutine(DisappearAnimation());
    }

    /// <summary>
    /// Coroutine qui gère l'animation de disparition progressive.
    /// </summary>
    private System.Collections.IEnumerator DisappearAnimation()
    {
        _isAnimating = true;
        
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        // J'anime progressivement l'échelle de 1 à 0
        while (elapsed < _disappearDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / _disappearDuration;
            
            // J'applique l'échelle interpolée
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            
            yield return null;
        }
        
        // Je m'assure d'être complètement invisible
        transform.localScale = Vector3.zero;
        
        _isAnimating = false;
        
        // Je me masque complètement
        gameObject.SetActive(false);
        
        Debug.Log($"[BonusDisplay] ✓ Animation de disparition terminée sur {gameObject.name}");
    }

    /// <summary>
    /// À chaque frame, je mets à jour le timer et la barre de progression.
    /// Quand le timer atteint 0, je me masque automatiquement avec animation.
    /// </summary>
    private void Update()
    {
        // Si je ne suis pas actif, je ne fais rien (optimisation)
        if (!_isActive) return;

        // Je décrémente mon timer en fonction du temps écoulé depuis la dernière frame
        _timer -= Time.deltaTime;

        // Je mets à jour la barre de progression visuelle SI ELLE EXISTE
        // Si _cooldownOverlay est null, cette partie est simplement ignorée (pas d'erreur)
        if (_cooldownOverlay != null)
        {
            // Je calcule le ratio temps_restant/durée_totale pour avoir une valeur entre 0 et 1
            // La barre diminue progressivement de 1 (plein) vers 0 (vide)
            _cooldownOverlay.fillAmount = Mathf.Clamp01(_timer / _duration);
        }

        // Quand le timer arrive à 0 ou en dessous, le bonus a expiré
        if (_timer <= 0)
        {
            Debug.Log($"[BonusDisplay] ⏱️ Timer expiré sur {gameObject.name}");
            
            // Je passe en mode inactif
            _isActive = false;
            
            // Je lance l'animation de disparition au lieu de me masquer brutalement
            StartDisappearAnimation();
        }
    }

    /// <summary>
    /// Je force la désactivation immédiate du bonus (sans animation).
    /// Utilisé par exemple si le bouclier est consommé par un obstacle.
    /// </summary>
    public void ForceDeactivate()
    {
        Debug.Log($"[BonusDisplay] 🛑 Désactivation forcée de {gameObject.name}");
        
        _isActive = false;
        _timer = 0;
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Je retourne si le bonus est actuellement actif.
    /// </summary>
    public bool IsActive()
    {
        return _isActive;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Je teste l'animation d'apparition dans l'éditeur (menu contextuel).
    /// </summary>
    [ContextMenu("Test Popup Animation")]
    private void TestPopupAnimation()
    {
        Debug.Log($"[BonusDisplay] 🧪 Test d'apparition sur {gameObject.name}");
        ShowBonus(5f);
    }

    /// <summary>
    /// Je teste l'animation de disparition dans l'éditeur (menu contextuel).
    /// </summary>
    [ContextMenu("Test Disappear Animation")]
    private void TestDisappearAnimation()
    {
        Debug.Log($"[BonusDisplay] 🧪 Test de disparition sur {gameObject.name}");
        gameObject.SetActive(true);
        StartDisappearAnimation();
    }
#endif
}