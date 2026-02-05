using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Je gère l'affichage de la jauge de menace (avalanche) dans l'interface utilisateur.
/// Mon rôle : afficher visuellement le danger qui se rapproche du joueur avec des couleurs 
/// et des effets de clignotement en situation critique.
/// VERSION CORRIGÉE - J'initialise correctement à 0% pour éviter les bugs d'affichage.
/// </summary>
public class ThreatUI : MonoBehaviour
{
    // Je garde la référence vers le slider qui représente visuellement la jauge
    [SerializeField] private Slider _threatSlider;
    
    // Je garde la référence vers l'image de remplissage du slider (pour changer sa couleur)
    [SerializeField] private Image _fillImage;

    [Header("Couleurs de Progression")]
    // Je définis la couleur quand le danger est loin (0-40% de menace)
    [SerializeField] private Color _colorLoin = Color.green;
    
    // Je définis la couleur quand le danger se rapproche (41-80% de menace)
    [SerializeField] private Color _colorProche = new Color(1f, 0.5f, 0f); // Orange
    
    // Je définis la couleur critique quand le danger est imminent (81-100% de menace)
    [SerializeField] private Color _colorCritique = Color.red;

    [Header("Effet Critique")]
    // Je détermine la vitesse de clignotement en mode critique (en secondes)
    [SerializeField] private float _blinkSpeed = 0.2f;
    
    // Je garde en mémoire si je suis actuellement en train de clignoter
    private bool _isBlinking = false;
    
    // Je stocke la référence vers ma coroutine de clignotement pour pouvoir l'arrêter
    private Coroutine _blinkCoroutine;

    /// <summary>
    /// Au réveil, je m'initialise correctement à 0%.
    /// ✅ CORRECTION CRITIQUE : Avant cette correction, je pouvais afficher une valeur incorrecte au démarrage.
    /// </summary>
    private void Awake()
    {
        // Je configure mon slider avec les valeurs par défaut
        if (_threatSlider != null)
        {
            // Ma jauge va de 0 à 100 (pourcentage)
            _threatSlider.minValue = 0f;
            _threatSlider.maxValue = 100f;
            
            // ✅ Je démarre à 0% pour éviter tout affichage erroné
            _threatSlider.value = 0f;
        }

        // Je m'assure que ma couleur de départ est verte (danger loin)
        if (_fillImage != null)
        {
            _fillImage.color = _colorLoin;
        }
    }

    /// <summary>
    /// Au démarrage, je me connecte au ThreatManager pour recevoir les mises à jour.
    /// </summary>
    private void Start()
    {
        // Je vérifie que le ThreatManager existe
        if (ThreatManager.Instance != null)
        {
            // Je m'abonne à l'événement qui me notifie des changements de menace
            ThreatManager.Instance.OnThreatChanged += UpdateBar;
            
            // ✅ Je force l'affichage à 0% au démarrage pour garantir la cohérence
            UpdateBar(0f);
        }
        else
        {
            // Si le ThreatManager n'existe pas, je signale une erreur critique
            Debug.LogError("[ThreatUI] ThreatManager.Instance est null !");
        }
    }

    /// <summary>
    /// Avant d'être détruit, je me désabonne proprement des événements.
    /// </summary>
    private void OnDestroy()
    {
        // Je me désabonne si le ThreatManager existe encore
        if (ThreatManager.Instance != null)
        {
            ThreatManager.Instance.OnThreatChanged -= UpdateBar;
        }
    }

    /// <summary>
    /// Je mets à jour l'affichage de la jauge de menace.
    /// Cette méthode est appelée automatiquement chaque fois que la menace change.
    /// </summary>
    /// <param name="percent">Le pourcentage de menace actuel (0-100)</param>
    private void UpdateBar(float percent)
    {
        // Je mets à jour la valeur du slider
        if (_threatSlider != null)
        {
            _threatSlider.value = percent;
        }

        // Je gère le changement de couleur et les effets visuels
        if (_fillImage != null)
        {
            // Je détermine quelle couleur afficher selon le niveau de menace
            
            if (percent <= 40f)
            {
                // Zone verte : danger loin, tout va bien
                StopBlink(); // J'arrête le clignotement si je venais d'une zone critique
                _fillImage.color = _colorLoin;
            }
            else if (percent <= 80f)
            {
                // Zone orange : danger qui se rapproche, attention !
                StopBlink(); // J'arrête le clignotement si je venais d'une zone critique
                _fillImage.color = _colorProche;
            }
            else
            {
                // Zone rouge : danger critique ! Je lance le clignotement
                // Je vérifie que je ne suis pas déjà en train de clignoter pour éviter les doublons
                if (!_isBlinking)
                {
                    _blinkCoroutine = StartCoroutine(BlinkRoutine());
                }
            }
        }
    }

    /// <summary>
    /// Je crée un effet de clignotement rouge pour alerter le joueur du danger imminent.
    /// Cette coroutine alterne entre opacité pleine et réduite pour créer l'effet visuel.
    /// </summary>
    private IEnumerator BlinkRoutine()
    {
        // Je me marque comme étant en train de clignoter
        _isBlinking = true;
        
        // Je continue de clignoter tant que je suis en mode critique
        while (_isBlinking)
        {
            // Je vérifie que mon image existe toujours
            if (_fillImage != null)
            {
                // Phase 1 : Pleine opacité (rouge vif)
                _fillImage.color = _colorCritique;
                yield return new WaitForSeconds(_blinkSpeed);
                
                // Phase 2 : Opacité réduite (rouge atténué)
                Color dimColor = _colorCritique;
                dimColor.a = 0.3f; // Je réduis l'alpha à 30%
                _fillImage.color = dimColor;
                yield return new WaitForSeconds(_blinkSpeed);
            }
            else
            {
                // Si mon image a été détruite, je sors de la boucle
                break;
            }
        }
    }

    /// <summary>
    /// J'arrête le clignotement quand le joueur sort de la zone critique.
    /// </summary>
    private void StopBlink()
    {
        // Si je ne suis pas en train de clignoter, je n'ai rien à faire
        if (!_isBlinking) return;
        
        // Je me marque comme n'étant plus en train de clignoter
        _isBlinking = false;
        
        // J'arrête la coroutine si elle existe
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
    }
}