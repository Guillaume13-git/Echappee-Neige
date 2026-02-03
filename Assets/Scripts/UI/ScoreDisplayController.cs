using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Je gère l'affichage des 9 meilleurs scores avec un système de couleurs pour le podium.
/// J'affiche également le dernier score joué en le mettant en évidence.
/// </summary>
public class ScoresDisplayController : MonoBehaviour
{
    [Header("Score Entries (9 au total) - OBLIGATOIRE")]
    [Tooltip("Je stocke ici les 9 GameObjects ScoreEntry que je dois remplir")]
    [SerializeField] private GameObject[] _scoreEntries = new GameObject[9];

    [Header("Couleurs du Podium")]
    [SerializeField] private Color _goldColor = new Color(1f, 0.84f, 0f);      // J'utilise cette couleur pour le 1er
    [SerializeField] private Color _silverColor = new Color(0.75f, 0.75f, 0.75f); // J'utilise cette couleur pour le 2ème
    [SerializeField] private Color _bronzeColor = new Color(0.8f, 0.5f, 0.2f);    // J'utilise cette couleur pour le 3ème
    
    [Header("Autres Couleurs")]
    [SerializeField] private Color _highlightColor = Color.yellow;  // J'utilise cette couleur pour surligner le score actuel
    [SerializeField] private Color _normalColor = Color.white;      // J'utilise cette couleur pour les scores 4 à 9
    [SerializeField] private Color _grayColor = Color.gray;         // J'utilise cette couleur pour les positions vides

    [Header("Buttons")]
    [SerializeField] private Button _backButton; // Je stocke le bouton retour

    private int _lastPlayedScore = 0; // Je mémorise le dernier score joué pour le mettre en évidence

    /// <summary>
    /// Je m'initialise au démarrage de la scène
    /// </summary>
    private void Start()
    {
        // 1. Je récupère le dernier score joué
        GetLastPlayedScore();

        // 2. J'affiche tous les scores du top 9
        DisplayHighScores();

        // 3. Je configure le bouton retour
        if (_backButton != null)
            _backButton.onClick.AddListener(OnBackClicked);

        // 4. Je lance la musique du menu
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    /// <summary>
    /// Je récupère le score de la dernière partie jouée depuis le ScoreManager
    /// </summary>
    private void GetLastPlayedScore()
    {
        // Je vérifie que le ScoreManager existe
        if (ScoreManager.Instance != null)
        {
            // Je récupère le score actuel
            _lastPlayedScore = ScoreManager.Instance.CurrentScore;
            
            // J'affiche un log pour confirmer
            Debug.Log($"[ScoresDisplay] Dernier score récupéré : {_lastPlayedScore}");
        }
    }

    /// <summary>
    /// J'affiche les 9 meilleurs scores dans mes ScoreEntry
    /// </summary>
    private void DisplayHighScores()
    {
        // Je vérifie que le HighScoreManager existe
        if (HighScoreManager.Instance == null)
        {
            Debug.LogError("[ScoresDisplay] HighScoreManager introuvable !");
            return;
        }

        // Je récupère le tableau des meilleurs scores
        int[] highScores = HighScoreManager.Instance.HighScores;
        Debug.Log($"[ScoresDisplay] Affichage de {highScores.Length} scores");

        // Je parcours chacun de mes 9 ScoreEntry
        for (int i = 0; i < _scoreEntries.Length; i++)
        {
            // Je vérifie que ce ScoreEntry existe
            if (_scoreEntries[i] == null)
            {
                Debug.LogWarning($"[ScoresDisplay] ScoreEntry {i + 1} est NULL !");
                continue; // Je passe au suivant
            }

            // Je cherche le composant TextMeshProUGUI nommé "Score" dans ce ScoreEntry
            TextMeshProUGUI scoreText = _scoreEntries[i].transform.Find("Score")?.GetComponent<TextMeshProUGUI>();

            // Je vérifie que j'ai bien trouvé le composant
            if (scoreText == null)
            {
                Debug.LogError($"[ScoresDisplay] Aucun TextMeshProUGUI 'Score' trouvé dans {_scoreEntries[i].name} !");
                continue; // Je passe au suivant
            }

            // Je récupère le score pour cette position (0 si pas de score)
            int currentScore = (i < highScores.Length) ? highScores[i] : 0;
            bool hasScore = currentScore > 0; // Je vérifie s'il y a un score

            // Je vérifie si c'est le dernier score que le joueur a fait
            bool isLastPlayed = (currentScore == _lastPlayedScore && currentScore > 0);

            // J'affiche le score
            if (hasScore)
            {
                // Je formate le score avec des séparateurs de milliers
                string display = currentScore.ToString("N0");
                
                // Si c'est le dernier score joué, je le mets en évidence
                if (isLastPlayed)
                {
                    display = "➤ " + display; // J'ajoute une flèche
                    scoreText.color = _highlightColor; // J'applique la couleur de surbrillance (jaune)
                    scoreText.fontStyle = FontStyles.Bold; // Je mets en gras
                }
                else
                {
                    // Sinon, j'applique la couleur selon la position (podium)
                    scoreText.color = GetColorForPosition(i);
                    scoreText.fontStyle = FontStyles.Normal; // Je remets le style normal
                }
                
                // Je mets à jour le texte affiché
                scoreText.text = display;
                Debug.Log($"[ScoresDisplay] {_scoreEntries[i].name}: {display}");
            }
            else
            {
                // Il n'y a pas de score pour cette position, j'affiche un tiret
                scoreText.text = "-";
                scoreText.color = _grayColor; // J'applique la couleur grise
                scoreText.fontStyle = FontStyles.Normal;
            }
        }
    }

    /// <summary>
    /// Je retourne la couleur appropriée selon la position dans le classement
    /// </summary>
    private Color GetColorForPosition(int position)
    {
        // J'utilise un switch pour déterminer la couleur
        return position switch
        {
            0 => _goldColor,    // Pour le 1er, j'utilise l'or
            1 => _silverColor,  // Pour le 2ème, j'utilise l'argent
            2 => _bronzeColor,  // Pour le 3ème, j'utilise le bronze
            _ => _normalColor   // Pour les positions 4 à 9, j'utilise le blanc
        };
    }

    /// <summary>
    /// Je gère le clic sur le bouton retour
    /// </summary>
    private void OnBackClicked()
    {
        // Je joue le son de clic
        AudioManager.Instance?.PlaySFX("Blip");
        
        // Je retourne au menu principal
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            // Si le GameManager n'existe pas, je charge directement la scène
            SceneManager.LoadScene("MainMenu");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Je fournis un outil de debug pour vérifier la configuration des ScoreEntry
    /// Clic droit sur le composant → "Test - Afficher Info ScoreEntries"
    /// </summary>
    [ContextMenu("Test - Afficher Info ScoreEntries")]
    private void TestScoreEntries()
    {
        Debug.Log("=== TEST SCORE ENTRIES ===");
        
        // Je parcours tous mes ScoreEntry pour les vérifier
        for (int i = 0; i < _scoreEntries.Length; i++)
        {
            if (_scoreEntries[i] == null)
            {
                Debug.LogError($"ScoreEntry [{i}] : NULL - Pas assigné !");
                continue;
            }

            Debug.Log($"ScoreEntry [{i}] : {_scoreEntries[i].name}");
            
            // Je cherche le TextMeshProUGUI "Score"
            TextMeshProUGUI scoreText = _scoreEntries[i].transform.Find("Score")?.GetComponent<TextMeshProUGUI>();
            
            if (scoreText != null)
            {
                Debug.Log($"  ✓ TextMeshProUGUI 'Score' trouvé ! Texte actuel : '{scoreText.text}'");
            }
            else
            {
                Debug.LogError($"  ✗ Aucun TextMeshProUGUI 'Score' trouvé !");
                
                // Je liste tous les enfants pour aider au debug
                Debug.Log("  Enfants trouvés :");
                foreach (Transform child in _scoreEntries[i].transform)
                {
                    Debug.Log($"    - {child.name} (Type: {child.GetComponent<Component>()?.GetType().Name})");
                }
            }
        }
    }

    /// <summary>
    /// Je fournis un outil pour afficher les scores actuellement en mémoire
    /// Clic droit sur le composant → "Test - Afficher Scores du HighScoreManager"
    /// </summary>
    [ContextMenu("Test - Afficher Scores du HighScoreManager")]
    private void TestHighScores()
    {
        // Je vérifie que le HighScoreManager existe
        if (HighScoreManager.Instance == null)
        {
            Debug.LogError("HighScoreManager introuvable !");
            return;
        }

        Debug.Log("=== SCORES DANS LE JSON ===");
        int[] scores = HighScoreManager.Instance.HighScores;
        
        // J'affiche les 9 premiers scores
        for (int i = 0; i < scores.Length && i < 9; i++)
        {
            Debug.Log($"Position {i + 1}: {scores[i]}");
        }
    }

    /// <summary>
    /// Je fournis un outil pour prévisualiser les couleurs configurées
    /// Clic droit sur le composant → "Test - Prévisualiser Couleurs"
    /// </summary>
    [ContextMenu("Test - Prévisualiser Couleurs")]
    private void PreviewColors()
    {
        Debug.Log("=== PRÉVISUALISATION DES COULEURS ===");
        Debug.Log($"Position 1 (Or)     : RGB({_goldColor.r:F2}, {_goldColor.g:F2}, {_goldColor.b:F2})");
        Debug.Log($"Position 2 (Argent) : RGB({_silverColor.r:F2}, {_silverColor.g:F2}, {_silverColor.b:F2})");
        Debug.Log($"Position 3 (Bronze) : RGB({_bronzeColor.r:F2}, {_bronzeColor.g:F2}, {_bronzeColor.b:F2})");
        Debug.Log($"Position 4-9 (Normal) : RGB({_normalColor.r:F2}, {_normalColor.g:F2}, {_normalColor.b:F2})");
        Debug.Log($"Score Actuel (Highlight) : RGB({_highlightColor.r:F2}, {_highlightColor.g:F2}, {_highlightColor.b:F2})");
        Debug.Log($"Vide (Gris) : RGB({_grayColor.r:F2}, {_grayColor.g:F2}, {_grayColor.b:F2})");
    }
#endif
}