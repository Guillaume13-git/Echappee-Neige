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

    private void Start()
    {
        // 1. Afficher les scores
        DisplayHighScores();

        // 2. Configurer le bouton retour
        if (_backButton != null)
            _backButton.onClick.AddListener(OnBackClicked);

        // 3. Gestion de la musique (Sécurité Singleton)
        if (AudioManager.Instance != null)
        {
            // Utilise ta méthode avec vérification par nom pour éviter la coupure
            AudioManager.Instance.PlayMenuMusic();
        }
    }

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
            if (_scoreEntries[i] == null) continue;

            Transform entry = _scoreEntries[i].transform;

            // Récupération des composants
            TextMeshProUGUI rankText = entry.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entry.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            Image medalImage = entry.Find("MedalImage")?.GetComponent<Image>();

            // Affichage du Rang
            if (rankText != null)
                rankText.text = $"#{i + 1}";

            // Affichage du Score
            if (scoreText != null)
            {
                if (i < highScores.Length && highScores[i] > 0)
                {
                    scoreText.text = highScores[i].ToString("N0");
                    scoreText.color = new Color(1f, 0.84f, 0f); // Couleur dorée pour les scores actifs
                }
                else
                {
                    scoreText.text = "-";
                    scoreText.color = Color.gray;
                }
            }

            // Gestion des Médailles
            if (medalImage != null)
            {
                bool hasScore = i < highScores.Length && highScores[i] > 0;
                Sprite medalToAssign = null;

                if (hasScore)
                {
                    if (i == 0) medalToAssign = _goldMedal;
                    else if (i == 1) medalToAssign = _silverMedal;
                    else if (i == 2) medalToAssign = _bronzeMedal;
                }

                if (medalToAssign != null)
                {
                    medalImage.sprite = medalToAssign;
                    medalImage.enabled = true;
                }
                else
                {
                    medalImage.enabled = false;
                }
            }
        }
    }

    private void OnBackClicked()
    {
        // Joue le son avant de changer de scène
        AudioManager.Instance?.PlaySFX("Blip");
        SceneManager.LoadScene("MainMenu");
    }
}