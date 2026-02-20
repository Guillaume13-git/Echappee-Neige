using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Je gère le menu de pause du jeu.
/// Mon rôle est d'afficher/masquer le panneau de pause et de gérer les interactions du joueur.
/// </summary>
public class PauseController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _pausePanel; // Je stocke ici la référence vers le panneau de pause
    [SerializeField] private Button _resumeButton;   // Je garde la référence du bouton "Reprendre"
    [SerializeField] private Button _menuButton;     // Je garde la référence du bouton "Menu"
    [SerializeField] private Button _quitButton;     // Je garde la référence du bouton "Quitter le jeu"

    /// <summary>
    /// Au démarrage, j'initialise mon menu de pause.
    /// </summary>
    private void Start()
    {
        Debug.Log($"[PauseController] Start appelé. _pausePanel null ? {_pausePanel == null}");

        // Je masque le panneau au démarrage car le jeu commence en mode actif
        _pausePanel?.SetActive(false);

        // Je lie mes boutons à leurs fonctions respectives
        _resumeButton?.onClick.AddListener(OnResumeClicked);
        _menuButton?.onClick.AddListener(OnMenuClicked);
        _quitButton?.onClick.AddListener(OnQuitClicked);

        // Je m'abonne aux événements du GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePaused += ShowPauseMenu;
            GameManager.Instance.OnGameResumed += HidePauseMenu;
            Debug.Log("[PauseController] Abonnement au GameManager OK.");
        }
        else
        {
            Debug.LogError("[PauseController] GameManager.Instance est NULL dans Start !");
        }
    }

    /// <summary>
    /// Avant d'être détruit, je me désabonne proprement des événements.
    /// </summary>
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePaused -= ShowPauseMenu;
            GameManager.Instance.OnGameResumed -= HidePauseMenu;
        }
    }

    /// <summary>
    /// À chaque frame, je vérifie si le joueur veut mettre le jeu en pause ou le reprendre.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"[PauseController] Escape détecté ! État actuel : {GameManager.Instance.CurrentState}");

            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                Debug.Log("[PauseController] → Appel PauseGame()");
                GameManager.Instance.PauseGame();
            }
            else if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                Debug.Log("[PauseController] → Appel ResumeGame()");
                GameManager.Instance.ResumeGame();
            }
        }
    }

    /// <summary>
    /// J'affiche le menu de pause à l'écran.
    /// </summary>
    private void ShowPauseMenu()
    {
        Debug.Log($"[PauseController] ShowPauseMenu appelé ! _pausePanel null ? {_pausePanel == null}");
        _pausePanel?.SetActive(true);
    }

    /// <summary>
    /// Je masque le menu de pause.
    /// </summary>
    private void HidePauseMenu()
    {
        Debug.Log("[PauseController] HidePauseMenu appelé !");
        _pausePanel?.SetActive(false);
    }

    /// <summary>
    /// Je gère le clic sur le bouton "Reprendre".
    /// </summary>
    private void OnResumeClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        GameManager.Instance.ResumeGame();
    }

    /// <summary>
    /// Je gère le clic sur le bouton "Retour au menu".
    /// </summary>
    private void OnMenuClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        GameManager.Instance.ReturnToMainMenu();
    }

    /// <summary>
    /// Je gère le clic sur le bouton "Quitter le jeu".
    /// </summary>
    private void OnQuitClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        GameManager.Instance.QuitGame();
    }
}