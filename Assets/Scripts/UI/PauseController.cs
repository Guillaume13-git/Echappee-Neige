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
    [SerializeField] private Button _resumeButton; // Je garde la référence du bouton "Reprendre"
    [SerializeField] private Button _menuButton; // Je garde la référence du bouton "Menu"
    
    /// <summary>
    /// Au démarrage, j'initialise mon menu de pause.
    /// </summary>
    private void Start()
    {
        // Je masque le panneau au démarrage car le jeu commence en mode actif
        _pausePanel?.SetActive(false);
        
        // Je lie mes boutons à leurs fonctions respectives
        // L'opérateur ?. me protège contre les références nulles
        _resumeButton?.onClick.AddListener(OnResumeClicked);
        _menuButton?.onClick.AddListener(OnMenuClicked);
        
        // Je m'abonne aux événements du GameManager pour être notifié des changements d'état
        // Ainsi, je réagis automatiquement quand le jeu se met en pause ou reprend
        GameManager.Instance.OnGamePaused += ShowPauseMenu;
        GameManager.Instance.OnGameResumed += HidePauseMenu;
    }
    
    /// <summary>
    /// Avant d'être détruit, je me désabonne proprement des événements.
    /// Cela évite les erreurs de références nulles et les fuites mémoire.
    /// </summary>
    private void OnDestroy()
    {
        // Je vérifie que le GameManager existe encore avant de me désabonner
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
        // J'écoute la touche Echap pour gérer la pause/reprise
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Si le jeu est en cours, je le mets en pause
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                GameManager.Instance.PauseGame();
            }
            // Si le jeu est déjà en pause, je le reprends
            else if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
            }
        }
    }
    
    /// <summary>
    /// J'affiche le menu de pause à l'écran.
    /// Cette méthode est appelée automatiquement via l'événement OnGamePaused.
    /// </summary>
    private void ShowPauseMenu()
    {
        // J'active le panneau pour qu'il devienne visible
        _pausePanel?.SetActive(true);
    }
    
    /// <summary>
    /// Je masque le menu de pause.
    /// Cette méthode est appelée automatiquement via l'événement OnGameResumed.
    /// </summary>
    private void HidePauseMenu()
    {
        // Je désactive le panneau pour qu'il disparaisse
        _pausePanel?.SetActive(false);
    }
    
    /// <summary>
    /// Je gère le clic sur le bouton "Reprendre".
    /// Je joue un son de feedback puis je reprends le jeu.
    /// </summary>
    private void OnResumeClicked()
    {
        // Je joue un son pour confirmer l'action du joueur
        AudioManager.Instance?.PlaySFX("Blip");
        
        // Je demande au GameManager de reprendre le jeu
        GameManager.Instance.ResumeGame();
    }
    
    /// <summary>
    /// Je gère le clic sur le bouton "Retour au menu".
    /// Je joue un son de feedback puis je retourne au menu principal.
    /// </summary>
    private void OnMenuClicked()
    {
        // Je joue un son pour confirmer l'action du joueur
        AudioManager.Instance?.PlaySFX("Blip");
        
        // Je demande au GameManager de retourner au menu principal
        GameManager.Instance.ReturnToMainMenu();
    }
}