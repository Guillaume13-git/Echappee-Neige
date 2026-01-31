using UnityEngine;
using UnityEngine.UI;

public class BonusDisplay : MonoBehaviour
{
    [SerializeField] private Image _cooldownOverlay;
    private float _duration;
    private float _timer;
    private bool _isActive;

    private void Awake()
    {
        // On cache le bonus au démarrage
        gameObject.SetActive(false);
    }

    public void ShowBonus(float duration)
    {
        _duration = duration;
        _timer = duration;
        _isActive = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!_isActive) return;

        _timer -= Time.deltaTime;

        if (_cooldownOverlay != null)
        {
            // La barre de progression diminue de 1 vers 0
            _cooldownOverlay.fillAmount = _timer / _duration;
        }

        if (_timer <= 0)
        {
            _isActive = false;
            gameObject.SetActive(false);
        }
    }
}