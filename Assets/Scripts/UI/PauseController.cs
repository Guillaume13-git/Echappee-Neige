using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contrôle le menu de pause.
/// </summary>
public class PauseController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _menuButton;
    
    private void Start()
    {
        // Masquer le panneau au démarrage
        _pausePanel?.SetActive(false);
        
        // Lier les boutons
        _resumeButton?.onClick.AddListener(OnResumeClicked);
        _menuButton?.onClick.AddListener(OnMenuClicked);
        
        // S'abonner aux événements de pause
        GameManager.Instance.OnGamePaused += ShowPauseMenu;
        GameManager.Instance.OnGameResumed += HidePauseMenu;
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePaused -= ShowPauseMenu;
            GameManager.Instance.OnGameResumed -= HidePauseMenu;
        }
    }
    
    private void Update()
    {
        // Touche Echap pour pause/reprise
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                GameManager.Instance.PauseGame();
            }
            else if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
            }
        }
    }
    
    /// <summary>
    /// Affiche le menu de pause.
    /// </summary>
    private void ShowPauseMenu()
    {
        _pausePanel?.SetActive(true);
    }
    
    /// <summary>
    /// Masque le menu de pause.
    /// </summary>
    private void HidePauseMenu()
    {
        _pausePanel?.SetActive(false);
    }
    
    /// <summary>
    /// Bouton Reprendre.
    /// </summary>
    private void OnResumeClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        GameManager.Instance.ResumeGame();
    }
    
    /// <summary>
    /// Bouton Retour au menu.
    /// </summary>
    private void OnMenuClicked()
    {
        AudioManager.Instance?.PlaySFX("Blip");
        GameManager.Instance.ReturnToMainMenu();
    }
}