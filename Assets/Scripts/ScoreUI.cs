using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    
    [Header("Formatting")]
    [SerializeField] private string _scorePrefix = "Score: ";
    [SerializeField] private bool _useThousandsSeparator = true;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            // On s'abonne à l'événement qui envoie un float
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }
    }

    // ON CHANGE 'int score' PAR 'float score' ICI
    private void UpdateScoreDisplay(float score)
    {
        if (_scoreText == null) return;

        // On convertit le float en int pour l'affichage propre
        int scoreAsInt = Mathf.FloorToInt(score);

        string formattedScore = _useThousandsSeparator 
            ? scoreAsInt.ToString("N0") 
            : scoreAsInt.ToString();

        _scoreText.text = _scorePrefix + formattedScore;
    }
}