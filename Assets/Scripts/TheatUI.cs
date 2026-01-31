using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ThreatUI : MonoBehaviour
{
    [SerializeField] private Slider _threatSlider;
    [SerializeField] private Image _fillImage;

    [Header("Couleurs de Progression")]
    [SerializeField] private Color _colorLoin = Color.green;     // 0 - 40%
    [SerializeField] private Color _colorProche = new Color(1f, 0.5f, 0f); // 41 - 80% (Orange)
    [SerializeField] private Color _colorCritique = Color.red;    // 81 - 100%

    [Header("Effet Critique")]
    [SerializeField] private float _blinkSpeed = 0.2f; 
    private bool _isBlinking = false;

    private void Start()
    {
        if (ThreatManager.Instance != null)
        {
            ThreatManager.Instance.OnThreatChanged += UpdateBar;
            // Initialisation immédiate au lancement
            UpdateBar(ThreatManager.Instance.ThreatPercentage);
        }
    }

    private void UpdateBar(float percent)
    {
        // On met à jour le slider (Assure-toi que Max Value du Slider est 100 dans Unity)
        if (_threatSlider != null) _threatSlider.value = percent;

        if (_fillImage != null)
        {
            // --- CORRECTION DES PALIERS (0-100) ---
            if (percent <= 40f) 
            {
                StopBlink();
                _fillImage.color = _colorLoin;
            }
            else if (percent <= 80f) 
            {
                StopBlink();
                _fillImage.color = _colorProche;
            }
            else 
            {
                if (!_isBlinking) StartCoroutine(BlinkRoutine());
            }
        }
    }

    private IEnumerator BlinkRoutine()
    {
        _isBlinking = true;
        while (_isBlinking)
        {
            _fillImage.color = _colorCritique;
            yield return new WaitForSeconds(_blinkSpeed);
            // On baisse l'opacité pour le clignotement
            _fillImage.color = new Color(_colorCritique.r, _colorCritique.g, _colorCritique.b, 0.3f);
            yield return new WaitForSeconds(_blinkSpeed);
        }
    }

    private void StopBlink()
    {
        if (!_isBlinking) return; // Évite de stopper si c'est déjà fait
        _isBlinking = false;
        StopAllCoroutines();
    }
}