using UnityEngine;
using TMPro;

/// <summary>
/// Je gère l'affichage du score dans l'interface utilisateur.
/// Mon rôle : afficher le score actuel du joueur de manière claire et formatée,
/// et le mettre à jour automatiquement quand le score change.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    // Je garde la référence vers le texte qui affiche le score à l'écran
    [SerializeField] private TextMeshProUGUI _scoreText;
    
    [Header("Formatting")]
    // Je stocke le préfixe à afficher avant le score (par exemple "Score: ")
    [SerializeField] private string _scorePrefix = "Score: ";
    
    // Je détermine si je dois utiliser des séparateurs de milliers (ex: 1,000 au lieu de 1000)
    [SerializeField] private bool _useThousandsSeparator = true;

    /// <summary>
    /// Au démarrage, je m'initialise en me connectant au ScoreManager.
    /// </summary>
    private void Start()
    {
        // Je vérifie que le ScoreManager existe
        if (ScoreManager.Instance != null)
        {
            // Je m'abonne à l'événement qui m'envoie un float quand le score change
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            
            // J'affiche immédiatement le score actuel au démarrage
            UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
        }
    }

    /// <summary>
    /// Avant d'être détruit, je me désabonne proprement des événements.
    /// Cela évite les erreurs de référence nulle et les fuites mémoire.
    /// </summary>
    private void OnDestroy()
    {
        // Je me désabonne si le ScoreManager existe encore
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }
    }

    /// <summary>
    /// Je mets à jour l'affichage du score à l'écran.
    /// Cette méthode est appelée automatiquement chaque fois que le score change.
    /// 
    /// NOTE IMPORTANTE : Je reçois un float du ScoreManager car le score s'incrémente
    /// progressivement frame par frame, mais je l'affiche comme un entier pour plus de clarté.
    /// </summary>
    /// <param name="score">Le score actuel en tant que float</param>
    private void UpdateScoreDisplay(float score)
    {
        // Si mon texte n'est pas assigné, je ne fais rien (protection contre les erreurs)
        if (_scoreText == null) return;

        // Je convertis le float en int pour un affichage propre
        // Mathf.FloorToInt arrondit vers le bas (ex: 1234.7 devient 1234)
        int scoreAsInt = Mathf.FloorToInt(score);

        // Je formate le score selon mes paramètres
        // Si _useThousandsSeparator est vrai : 1234 → "1,234"
        // Sinon : 1234 → "1234"
        string formattedScore = _useThousandsSeparator 
            ? scoreAsInt.ToString("N0")  // "N0" = format numérique avec 0 décimales et séparateurs
            : scoreAsInt.ToString();      // Format simple sans séparateurs

        // J'assemble le texte final : préfixe + score formaté
        // Résultat : "Score: 1,234" ou "Score: 1234"
        _scoreText.text = _scorePrefix + formattedScore;
    }
}