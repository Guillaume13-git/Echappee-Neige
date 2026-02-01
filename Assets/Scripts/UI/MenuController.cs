using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Contrôle le menu principal.
/// Gère la navigation avec un délai pour laisser les sons se jouer.
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _bestScoreText;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _scoresButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _quitButton;

    private bool _isNavigating = false;

    private void Start()
    {
        SetupButtons();
        DisplayBestScore();
        CheckAndPlayMenuMusic();
    }

    private void CheckAndPlayMenuMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    private void SetupButtons()
    {
        _playButton?.onClick.RemoveAllListeners();
        _optionsButton?.onClick.RemoveAllListeners();
        _scoresButton?.onClick.RemoveAllListeners();
        _quitButton?.onClick.RemoveAllListeners();

        // ✅ CORRECTION : Le bouton Play passe maintenant par GameManager
        // Avant, il chargeait la scène directement sans changer l'état du jeu.
        // Maintenant, GameManager.StartNewGame() gère l'état ET la scène.
        _playButton?.onClick.AddListener(OnPlayClicked);

        // Ces deux sont des navigations simples vers des menus, pas de changement d'état nécessaire
        _optionsButton?.onClick.AddListener(() => OnClickNavigation("Options", "Blip"));
        _scoresButton?.onClick.AddListener(() => OnClickNavigation("Scores", "Blip"));
        _quitButton?.onClick.AddListener(QuitGame);
    }

    /// <summary>
    /// Bouton Play : joue le son puis délègue à GameManager.
    /// GameManager décide si on va en Tutorial ou en Gameplay.
    /// </summary>
    private void OnPlayClicked()
    {
        if (_isNavigating) return;
        StartCoroutine(DelayedStartGame());
    }

    /// <summary>
    /// Attend que le son "LetsGo" se termine avant de lancer le jeu via GameManager.
    /// </summary>
    private IEnumerator DelayedStartGame()
    {
        _isNavigating = true;

        AudioManager.Instance?.PlaySFX("LetsGo");

        yield return new WaitForSecondsRealtime(0.7f);

        // ✅ On passe par GameManager qui gère l'état ET choisit la bonne scène
        GameManager.Instance?.StartNewGame();
    }

    /// <summary>
    /// Navigation générique pour les menus (Options, Scores).
    /// Ne change pas l'état du jeu, juste un changement de scène menu.
    /// </summary>
    private void OnClickNavigation(string sceneName, string sfx)
    {
        if (_isNavigating) return;
        StartCoroutine(NavWithDelay(sceneName, sfx));
    }

    /// <summary>
    /// Coroutine qui joue le son, attend puis change de scène.
    /// </summary>
    private IEnumerator NavWithDelay(string sceneName, string sfx)
    {
        _isNavigating = true;

        AudioManager.Instance?.PlaySFX(sfx);

        yield return new WaitForSecondsRealtime(0.7f);

        Debug.Log($"[MenuController] Chargement de la scène : {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    private void DisplayBestScore()
    {
        if (_bestScoreText == null) return;

        if (HighScoreManager.Instance != null && HighScoreManager.Instance.HighScores.Length > 0)
        {
            int score = HighScoreManager.Instance.HighScores[0];
            _bestScoreText.text = score > 0 ? $"Meilleur Score : {score:N0}" : "Aucun score enregistré";
        }
        else
        {
            _bestScoreText.text = "Meilleur Score : ---";
        }
    }

    private void QuitGame()
    {
        if (_isNavigating) return;

        Debug.Log("[MenuController] Quitter le jeu");
        AudioManager.Instance?.PlaySFX("Blip");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}