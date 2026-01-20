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

        // On utilise maintenant la navigation avec délai
        _playButton?.onClick.AddListener(() => OnClickNavigation("Gameplay", "LetsGo"));
        _optionsButton?.onClick.AddListener(() => OnClickNavigation("Options", "Blip"));
        _scoresButton?.onClick.AddListener(() => OnClickNavigation("Scores", "Blip"));
        _quitButton?.onClick.AddListener(QuitGame);
    }

    /// <summary>
    /// Lance la coroutine de changement de scène.
    /// </summary>
    private void OnClickNavigation(string sceneName, string sfx)
    {
        if (_isNavigating) return;
        StartCoroutine(NavWithDelay(sceneName, sfx));
    }

    /// <summary>
    /// Coroutine qui joue le son, attend 0.2s, puis change de scène.
    /// Cela évite que le processeur ne coupe le son instantanément.
    /// </summary>
    private IEnumerator NavWithDelay(string sceneName, string sfx)
    {
        _isNavigating = true;

        if (AudioManager.Instance != null)
        {
            // On joue le son (Blip ou LetsGo)
            AudioManager.Instance.PlaySFX(sfx);
        }

        // WaitForSecondsRealtime permet d'ignorer une éventuelle pause du TimeScale
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