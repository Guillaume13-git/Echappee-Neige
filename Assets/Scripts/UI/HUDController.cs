using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contrôle l'affichage du HUD pendant le gameplay.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Threat UI")]
    [SerializeField] private Image _threatFillImage;
    [SerializeField] private Color _threatColorSafe = Color.green;
    [SerializeField] private Color _threatColorWarning = Color.yellow;
    [SerializeField] private Color _threatColorDanger = Color.red;
    
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    
    [Header("Bonus Icons")]
    [SerializeField] private GameObject _speedBoostIcon;
    [SerializeField] private GameObject _shieldIcon;
    [SerializeField] private Image _speedBoostTimerFill;
    
    [Header("Screen Effects")]
    [SerializeField] private Image _screenVignette;
    [SerializeField] private float _vignetteMaxAlpha = 0.8f;
    
    private void Start()
    {
        // S'abonner aux événements
        if (ThreatManager.Instance != null)
            ThreatManager.Instance.OnThreatChanged += UpdateThreatBar;
            
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += UpdateScore; // Plus d'erreur ici
        
        // Initialisation
        UpdateThreatBar(0f);
        UpdateScore(0f); // Initialiser avec un float
        _speedBoostIcon?.SetActive(false);
        _shieldIcon?.SetActive(false);
    }
    
    private void OnDestroy()
    {
        if (ThreatManager.Instance != null)
            ThreatManager.Instance.OnThreatChanged -= UpdateThreatBar;
            
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
    }
    
    private void UpdateThreatBar(float percentage)
    {
        if (_threatFillImage == null) return;
        
        _threatFillImage.fillAmount = percentage / 100f;
        
        // Couleur selon le niveau de menace
        if (percentage < 40f) _threatFillImage.color = _threatColorSafe;
        else if (percentage < 80f) _threatFillImage.color = _threatColorWarning;
        else _threatFillImage.color = _threatColorDanger;
        
        UpdateScreenVignette(percentage);
        
        if (percentage >= 80f && percentage < 80.5f) // Plus précis pour éviter de jouer le son en boucle
        {
            AudioManager.Instance?.PlaySFX("Alarm");
        }
    }
    
    private void UpdateScreenVignette(float percentage)
    {
        if (_screenVignette == null) return;
        float alpha = Mathf.Lerp(0f, _vignetteMaxAlpha, percentage / 100f);
        Color vignetteColor = _screenVignette.color;
        vignetteColor.a = alpha;
        _screenVignette.color = vignetteColor;
    }
    
    /// <summary>
    /// CORRECTION : Prend maintenant un FLOAT pour correspondre au ScoreManager
    /// </summary>
    private void UpdateScore(float score)
    {
        if (_scoreText == null) return;
        
        // On convertit en int pour l'affichage (pas de virgules pour le joueur)
        _scoreText.text = Mathf.FloorToInt(score).ToString("N0");
    }
    
    public void ShowSpeedBoostIcon(float duration)
    {
        if (_speedBoostIcon != null)
        {
            _speedBoostIcon.SetActive(true);
            // On arrête la coroutine précédente si elle existe pour éviter les conflits de timer
            StopAllCoroutines(); 
            StartCoroutine(AnimateTimerIcon(_speedBoostTimerFill, duration, _speedBoostIcon));
        }
    }

    public void ShowShieldIcon() => _shieldIcon?.SetActive(true);
    public void HideShieldIcon() => _shieldIcon?.SetActive(false);
    
    private System.Collections.IEnumerator AnimateTimerIcon(Image timerFill, float duration, GameObject icon)
    {
        if (timerFill == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            timerFill.fillAmount = 1f - (elapsed / duration);
            yield return null;
        }
        
        timerFill.fillAmount = 0f;
        icon.SetActive(false);
    }
}