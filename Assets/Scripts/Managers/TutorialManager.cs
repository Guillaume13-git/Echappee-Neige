using UnityEngine;
using TMPro;

/// <summary>
/// Gère le tutoriel du jeu.
/// Affiche des instructions séquentielles et attend les actions du joueur.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Steps")]
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _instructionText;
    
    [Header("Tutorial Obstacles")]
    [SerializeField] private GameObject _laneObstacleTutorial;
    [SerializeField] private GameObject _crouchObstacleTutorial;
    
    private int _currentStep = 0;
    private bool _tutorialActive = true;
    
    private readonly string[] _instructions = new string[]
    {
        "Bienvenue ! Utilisez ← et → pour changer de couloir.",
        "Bien ! Maintenant évitez cet obstacle en changeant de couloir.",
        "Parfait ! Utilisez ↓ ou Shift pour vous baisser sous cet obstacle.",
        "Excellent ! Vous êtes prêt. Bonne chance !"
    };
    
    private void Start()
    {
        ShowInstruction(0);
        Time.timeScale = 0.5f; // Ralentir au début
    }
    
    private void Update()
    {
        if (!_tutorialActive) return;
        
        CheckStepCompletion();
    }
    
    /// <summary>
    /// Vérifie si l'étape actuelle est complétée.
    /// </summary>
    private void CheckStepCompletion()
    {
        switch (_currentStep)
        {
            case 0:
                // Attendre un déplacement latéral
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                    Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.D))
                {
                    NextStep();
                }
                break;
                
            case 1:
                // Attendre qu'il évite l'obstacle (géré par collision)
                // → déclenché manuellement après évitement réussi
                break;
                
            case 2:
                // Attendre un accroupissement
                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftShift))
                {
                    NextStep();
                }
                break;
        }
    }
    
    /// <summary>
    /// Passe à l'étape suivante.
    /// </summary>
    public void NextStep()
    {
        _currentStep++;
        
        if (_currentStep >= _instructions.Length)
        {
            EndTutorial();
        }
        else
        {
            ShowInstruction(_currentStep);
        }
    }
    
    /// <summary>
    /// Affiche l'instruction de l'étape actuelle.
    /// </summary>
    private void ShowInstruction(int stepIndex)
    {
        if (_instructionText != null)
        {
            _instructionText.text = _instructions[stepIndex];
        }
        
        // Activer les obstacles spécifiques au bon moment
        if (stepIndex == 1)
        {
            _laneObstacleTutorial?.SetActive(true);
        }
        else if (stepIndex == 2)
        {
            _crouchObstacleTutorial?.SetActive(true);
        }
    }
    
    /// <summary>
    /// Termine le tutoriel et lance le jeu normal.
    /// </summary>
    private void EndTutorial()
    {
        _tutorialActive = false;
        _tutorialPanel?.SetActive(false);
        Time.timeScale = 1f;
        
        // Marquer le tutoriel comme vu pour les prochaines sessions
        SettingsManager.Instance.SetShowTutorial(false);
        SettingsManager.Instance.SaveSettings();
        
        // Transition vers le gameplay normal
        GameManager.Instance.StartNewGame();
    }
}