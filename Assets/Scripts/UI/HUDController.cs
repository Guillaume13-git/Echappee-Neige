using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contrôle l'affichage du HUD pendant le gameplay.
/// Jauge de menace, score, icônes de bonus actifs.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Threat UI")]
    [SerializeField] private Image _threatFillImage;
    [SerializeField] private Image _threatBarBackground;
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
        ThreatManager.Instance.OnThreatChanged += UpdateThreatBar;
        ScoreManager.Instance.OnScoreChanged += UpdateScore;
        
        // Initialisation
        UpdateThreatBar(0f);
        UpdateScore(0);
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
    
    /// <summary>
    /// Met à jour la jauge de menace visuellement.
    /// </summary>
    private void UpdateThreatBar(float percentage)
    {
        if (_threatFillImage == null) return;
        
        // Remplissage
        _threatFillImage.fillAmount = percentage / 100f;
        
        // Couleur selon le niveau de menace
        Color targetColor;
        if (percentage < 40f)
        {
            targetColor = _threatColorSafe;
        }
        else if (percentage < 80f)
        {
            targetColor = _threatColorWarning;
        }
        else
        {
            targetColor = _threatColorDanger;
        }
        
        _threatFillImage.color = targetColor;
        
        // Effet vignette (brouillard visuel)
        UpdateScreenVignette(percentage);
        
        // Son d'alarme si > 80%
        if (percentage >= 80f && percentage < 81f)
        {
            AudioManager.Instance?.PlaySFX("Alarm");
        }
    }
    
    /// <summary>
    /// Met à jour l'effet de vignette selon la menace.
    /// </summary>
    private void UpdateScreenVignette(float percentage)
    {
        if (_screenVignette == null) return;
        
        // Alpha augmente de 0 (0%) à max (100%)
        float alpha = Mathf.Lerp(0f, _vignetteMaxAlpha, percentage / 100f);
        
        Color vignetteColor = _screenVignette.color;
        vignetteColor.a = alpha;
        _screenVignette.color = vignetteColor;
    }
    
    /// <summary>
    /// Met à jour l'affichage du score.
    /// </summary>
    private void UpdateScore(int score)
    {
        if (_scoreText == null) return;
        
        _scoreText.text = score.ToString("N0"); // Format avec séparateurs de milliers
    }
    
    /// <summary>
    /// Affiche l'icône de boost de vitesse avec timer circulaire.
    /// </summary>
    public void ShowSpeedBoostIcon(float duration)
    {
        if (_speedBoostIcon != null)
        {
            _speedBoostIcon.SetActive(true);
            StartCoroutine(AnimateTimerIcon(_speedBoostTimerFill, duration, _speedBoostIcon));
        }
    }
    
    /// <summary>
    /// Affiche l'icône de bouclier.
    /// </summary>
    public void ShowShieldIcon()
    {
        if (_shieldIcon != null)
        {
            _shieldIcon.SetActive(true);
        }
    }
    
    /// <summary>
    /// Masque l'icône de bouclier.
    /// </summary>
    public void HideShieldIcon()
    {
        if (_shieldIcon != null)
        {
            _shieldIcon.SetActive(false);
        }
    }
    
    /// <summary>
    /// Anime un timer circulaire autour d'une icône de bonus.
    /// </summary>
    private System.Collections.IEnumerator AnimateTimerIcon(Image timerFill, float duration, GameObject icon)
    {
        if (timerFill == null) yield break;
        
        float elapsed = 0f;
        timerFill.fillAmount = 1f;
        
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