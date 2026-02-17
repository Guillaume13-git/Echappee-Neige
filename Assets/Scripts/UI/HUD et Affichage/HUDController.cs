using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Je contrôle l'affichage du HUD (interface) pendant le gameplay.
/// Je gère la jauge de menace, le score, les icônes de bonus et les effets visuels.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Threat UI")]
    [SerializeField] private Image _threatFillImage;                  // Je stocke l'image de remplissage de la jauge de menace
    [SerializeField] private Color _threatColorSafe = Color.green;    // Je stocke la couleur pour menace faible (vert)
    [SerializeField] private Color _threatColorWarning = Color.yellow; // Je stocke la couleur pour menace moyenne (jaune)
    [SerializeField] private Color _threatColorDanger = Color.red;    // Je stocke la couleur pour menace élevée (rouge)
    
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;  // Je stocke le texte d'affichage du score
    
    [Header("Bonus Icons")]
    [SerializeField] private GameObject _speedBoostIcon;       // Je stocke l'icône du boost de vitesse
    [SerializeField] private GameObject _shieldIcon;           // Je stocke l'icône du bouclier
    [SerializeField] private Image _speedBoostTimerFill;       // Je stocke l'image de timer du boost de vitesse
    
    [Header("Screen Effects")]
    [SerializeField] private Image _screenVignette;            // Je stocke l'image de vignette d'écran
    [SerializeField] private float _vignetteMaxAlpha = 0.8f;   // Je stocke l'opacité maximale de la vignette
    
    /// <summary>
    /// Je m'initialise au démarrage et je m'abonne aux événements
    /// </summary>
    private void Start()
    {
        // ---------------------------------------------------------
        // ABONNEMENT AUX ÉVÉNEMENTS
        // ---------------------------------------------------------
        
        // Je m'abonne aux changements de menace
        if (ThreatManager.Instance != null)
            ThreatManager.Instance.OnThreatChanged += UpdateThreatBar;
            
        // Je m'abonne aux changements de score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
        
        // ---------------------------------------------------------
        // INITIALISATION DE L'UI
        // ---------------------------------------------------------
        
        // J'initialise la jauge de menace à 0%
        UpdateThreatBar(0f);
        
        // J'initialise le score à 0
        UpdateScore(0f);
        
        // Je cache les icônes de bonus au départ
        _speedBoostIcon?.SetActive(false);
        _shieldIcon?.SetActive(false);
    }
    
    /// <summary>
    /// Je me désabonne des événements quand je suis détruit
    /// </summary>
    private void OnDestroy()
    {
        // Je me désabonne des changements de menace
        if (ThreatManager.Instance != null)
            ThreatManager.Instance.OnThreatChanged -= UpdateThreatBar;
            
        // Je me désabonne des changements de score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
    }
    
    /// <summary>
    /// Je mets à jour la jauge de menace (couleur, remplissage, effets)
    /// </summary>
    /// <param name="percentage">Le pourcentage de menace (0 à 100)</param>
    private void UpdateThreatBar(float percentage)
    {
        // Je vérifie que j'ai l'image de remplissage
        if (_threatFillImage == null) return;
        
        // Je mets à jour le remplissage de la jauge (0 à 1)
        _threatFillImage.fillAmount = percentage / 100f;
        
        // ---------------------------------------------------------
        // COULEUR SELON LE NIVEAU DE MENACE
        // ---------------------------------------------------------
        
        // Si la menace est inférieure à 40%, je mets du vert (sûr)
        if (percentage < 40f) 
            _threatFillImage.color = _threatColorSafe;
        
        // Si la menace est entre 40% et 80%, je mets du jaune (attention)
        else if (percentage < 80f) 
            _threatFillImage.color = _threatColorWarning;
        
        // Si la menace est supérieure à 80%, je mets du rouge (danger)
        else 
            _threatFillImage.color = _threatColorDanger;
        
        // Je mets à jour la vignette d'écran selon la menace
        UpdateScreenVignette(percentage);
        
        // ---------------------------------------------------------
        // SON D'ALARME À 80%
        // ---------------------------------------------------------
        
        // Si la menace atteint 80%, je joue le son d'alarme
        // J'utilise une plage de 0.5% pour éviter de jouer le son en boucle
        if (percentage >= 80f && percentage < 80.5f)
        {
            AudioManager.Instance?.PlaySFX("Alarm");
        }
    }
    
    /// <summary>
    /// Je mets à jour l'opacité de la vignette d'écran selon la menace
    /// </summary>
    /// <param name="percentage">Le pourcentage de menace (0 à 100)</param>
    private void UpdateScreenVignette(float percentage)
    {
        // Je vérifie que j'ai la vignette
        if (_screenVignette == null) return;
        
        // Je calcule l'opacité en fonction de la menace (0% = transparent, 100% = opaque)
        float alpha = Mathf.Lerp(0f, _vignetteMaxAlpha, percentage / 100f);
        
        // Je récupère la couleur actuelle
        Color vignetteColor = _screenVignette.color;
        
        // Je modifie l'opacité
        vignetteColor.a = alpha;
        
        // J'applique la nouvelle couleur
        _screenVignette.color = vignetteColor;
    }
    
    /// <summary>
    /// Je mets à jour l'affichage du score
    /// Je prends un float car le ScoreManager calcule le score en float pour la précision
    /// </summary>
    /// <param name="score">Le score actuel (en float)</param>
    private void UpdateScore(float score)
    {
        // Je vérifie que j'ai le texte de score
        if (_scoreText == null) return;
        
        // Je convertis le score en int pour l'affichage (pas de virgules pour le joueur)
        // J'utilise FloorToInt pour arrondir vers le bas
        // "N0" formate le nombre avec des espaces de milliers (ex: 1 000 000)
        _scoreText.text = Mathf.FloorToInt(score).ToString("N0");
    }
    
    /// <summary>
    /// J'affiche l'icône du boost de vitesse avec un timer circulaire
    /// </summary>
    /// <param name="duration">La durée du boost en secondes</param>
    public void ShowSpeedBoostIcon(float duration)
    {
        // Je vérifie que j'ai l'icône
        if (_speedBoostIcon != null)
        {
            // J'active l'icône
            _speedBoostIcon.SetActive(true);
            
            // J'arrête toutes les coroutines précédentes pour éviter les conflits de timer
            StopAllCoroutines(); 
            
            // Je démarre l'animation du timer circulaire
            StartCoroutine(AnimateTimerIcon(_speedBoostTimerFill, duration, _speedBoostIcon));
        }
    }

    /// <summary>
    /// J'affiche l'icône du bouclier
    /// </summary>
    public void ShowShieldIcon() => _shieldIcon?.SetActive(true);
    
    /// <summary>
    /// Je cache l'icône du bouclier
    /// </summary>
    public void HideShieldIcon() => _shieldIcon?.SetActive(false);
    
    /// <summary>
    /// J'anime le timer circulaire d'une icône de bonus
    /// </summary>
    /// <param name="timerFill">L'image de remplissage du timer</param>
    /// <param name="duration">La durée totale en secondes</param>
    /// <param name="icon">L'icône à cacher à la fin</param>
    private System.Collections.IEnumerator AnimateTimerIcon(Image timerFill, float duration, GameObject icon)
    {
        // Je vérifie que j'ai l'image de timer
        if (timerFill == null) yield break;
        
        // Je compte le temps écoulé
        float elapsed = 0f;
        
        // Tant que la durée n'est pas écoulée
        while (elapsed < duration)
        {
            // J'avance le temps
            elapsed += Time.deltaTime;
            
            // Je calcule le remplissage restant (1 = plein, 0 = vide)
            // Le timer se vide progressivement
            timerFill.fillAmount = 1f - (elapsed / duration);
            
            // J'attends la prochaine frame
            yield return null;
        }
        
        // À la fin, je m'assure que le timer est vide
        timerFill.fillAmount = 0f;
        
        // Je cache l'icône
        icon.SetActive(false);
    }
}