using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Affiche la jauge de menace avec changement de couleur et clignotement.
/// VERSION CORRIGÉE - Initialise correctement à 0%
/// </summary>
public class ThreatUI : MonoBehaviour
{
    [SerializeField] private Slider _threatSlider;
    [SerializeField] private Image _fillImage;

    [Header("Couleurs de Progression")]
    [SerializeField] private Color _colorLoin = Color.green;           // 0 - 40%
    [SerializeField] private Color _colorProche = new Color(1f, 0.5f, 0f); // 41 - 80% (Orange)
    [SerializeField] private Color _colorCritique = Color.red;          // 81 - 100%

    [Header("Effet Critique")]
    [SerializeField] private float _blinkSpeed = 0.2f;
    
    private bool _isBlinking = false;
    private Coroutine _blinkCoroutine;

    private void Awake()
    {
        // ✅ CORRECTION CRITIQUE : Initialiser à 0% dès l'Awake
        if (_threatSlider != null)
        {
            _threatSlider.minValue = 0f;
            _threatSlider.maxValue = 100f;
            _threatSlider.value = 0f;
        }

        if (_fillImage != null)
        {
            _fillImage.color = _colorLoin;
        }
    }

    private void Start()
    {
        if (ThreatManager.Instance != null)
        {
            ThreatManager.Instance.OnThreatChanged += UpdateBar;
            
            // ✅ Forcer l'affichage à 0% au démarrage
            UpdateBar(0f);
        }
        else
        {
            Debug.LogError("[ThreatUI] ThreatManager.Instance est null !");
        }
    }

    private void OnDestroy()
    {
        if (ThreatManager.Instance != null)
        {
            ThreatManager.Instance.OnThreatChanged -= UpdateBar;
        }
    }

    private void UpdateBar(float percent)
    {
        // Mise à jour du slider
        if (_threatSlider != null)
        {
            _threatSlider.value = percent;
        }

        if (_fillImage != null)
        {
            // Changement de couleur selon les paliers
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
                // Mode critique : clignotement rouge
                if (!_isBlinking)
                {
                    _blinkCoroutine = StartCoroutine(BlinkRoutine());
                }
            }
        }
    }

    private IEnumerator BlinkRoutine()
    {
        _isBlinking = true;
        
        while (_isBlinking)
        {
            if (_fillImage != null)
            {
                // Pleine opacité
                _fillImage.color = _colorCritique;
                yield return new WaitForSeconds(_blinkSpeed);
                
                // Opacité réduite
                Color dimColor = _colorCritique;
                dimColor.a = 0.3f;
                _fillImage.color = dimColor;
                yield return new WaitForSeconds(_blinkSpeed);
            }
            else
            {
                break;
            }
        }
    }

    private void StopBlink()
    {
        if (!_isBlinking) return;
        
        _isBlinking = false;
        
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
    }
}