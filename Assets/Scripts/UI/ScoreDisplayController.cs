using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Contrôle l'affichage de la liste des 10 meilleurs scores.
/// Remplit dynamiquement les ScoreEntry avec les données sauvegardées.
/// </summary>
public class ScoresDisplayController : MonoBehaviour
{
    [Header("Score Entries (10 au total)")]
    [Tooltip("Assigner les 10 ScoreEntry dans l'ordre (1 à 10)")]
    [SerializeField] private GameObject[] _scoreEntries = new GameObject[10];

    [Header("Medal Sprites")]
    [Tooltip("Médailles pour le top 3")]
    [SerializeField] private Sprite _goldMedal;
    [SerializeField] private Sprite _silverMedal;
    [SerializeField] private Sprite _bronzeMedal;

    [Header("Buttons")]
    [SerializeField] private Button _backButton;


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    private void Start()
    {
        DisplayHighScores();

        if (_backButton != null)
            _backButton.onClick.AddListener(OnBackClicked);

        AudioManager.Instance?.PlayMenuMusic();
    }


    // ---------------------------------------------------------
    // DISPLAY SCORES
    // ---------------------------------------------------------
    private void DisplayHighScores()
    {
        if (HighScoreManager.Instance == null)
        {
            Debug.LogError("[ScoresDisplay] HighScoreManager introuvable !");
            return;
        }

        int[] highScores = HighScoreManager.Instance.HighScores;

        for (int i = 0; i < _scoreEntries.Length; i++)
        {
            if (_scoreEntries[i] == null)
            {
                Debug.LogWarning($"[ScoresDisplay] ScoreEntry {i + 1} non assigné !");
                continue;
            }

            Transform entry = _scoreEntries[i].transform;

            TextMeshProUGUI rankText = entry.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entry.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            Image medalImage = entry.Find("MedalImage")?.GetComponent<Image>();

            // Rang
            if (rankText != null)
                rankText.text = $"#{i + 1}";

            // Score
            if (scoreText != null)
            {
                if (highScores[i] > 0)
                {
                    scoreText.text = highScores[i].ToString("N0");
                    scoreText.color = new Color(1f, 0.84f, 0f); // Or
                }
                else
                {
                    scoreText.text = "-";
                    scoreText.color = Color.gray;
                }
            }

            // Médailles
            if (medalImage != null)
            {
                if (i == 0 && _goldMedal != null && highScores[i] > 0)
                {
                    medalImage.sprite = _goldMedal;
                    medalImage.enabled = true;
                }
                else if (i == 1 && _silverMedal != null && highScores[i] > 0)
                {
                    medalImage.sprite = _silverMedal;
                    medalImage.enabled = true;
                }
                else if (i == 2 && _bronzeMedal != null && highScores[i] > 0)
                {
                    medalImage.sprite = _bronzeMedal;
                    medalImage.enabled = true;
                }
                else
                {
                    medalImage.enabled = false;
                }
            }
        }

        Debug.Log("[ScoresDisplay] Affichage de 10 scores terminé");
    }


    // ---------------------------------------------------------
    // BUTTONS
    // ---------------------------------------------------------
    private void OnBackClicked()
    {
        Debug.Log("[ScoresDisplay] Retour au menu principal");

        AudioManager.Instance?.PlaySFX("Blip");
        SceneManager.LoadScene("MainMenu");
    }
}