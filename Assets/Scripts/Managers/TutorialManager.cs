using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Gère le tutoriel du jeu.
/// Guide le joueur à travers 3 étapes : déplacement latéral, évitement et accroupissement.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _instructionText;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private Button _skipButton;

    [Header("Tutorial Objects (Obstacles)")]
    [SerializeField] private GameObject _obstacleLane;
    [SerializeField] private GameObject _obstacleCrouch;

    [Header("Thresholds (Z Position)")]
    [SerializeField] private float _laneObstacleZ = 50f;
    [SerializeField] private float _crouchObstacleZ = 90f;
    [SerializeField] private float _finishZ = 130f;

    [Header("Player Reference")]
    [SerializeField] private Transform _playerTransform;

    private int _currentStep = 0;
    private bool _tutorialCompleted = false;

    private readonly string[] _instructions = new string[]
    {
        "Bienvenue ! Utilisez ← et → (ou Q et D) pour changer de couloir.",
        "Bien ! Maintenant évitez cet obstacle en changeant de couloir.",
        "Parfait ! Utilisez ↓ ou Shift pour vous baisser sous cet obstacle.",
        "Excellent ! Vous êtes prêt. Bonne chance !"
    };

    private void Start()
    {
        Debug.Log("[TutorialManager] Initialisation");

        // Configuration initiale de l'UI
        if (_skipButton != null) _skipButton.onClick.AddListener(OnSkipClicked);
        if (_tutorialPanel != null) _tutorialPanel.SetActive(true);

        // Masquer les obstacles au départ
        if (_obstacleLane != null) _obstacleLane.SetActive(false);
        if (_obstacleCrouch != null) _obstacleCrouch.SetActive(false);

        // Ralentir légèrement pour l'apprentissage
        Time.timeScale = 0.7f;

        ShowStep(0);
    }

    private void Update()
    {
        if (_tutorialCompleted || _playerTransform == null) return;

        CheckStepCompletion();
    }

    /// <summary>
    /// Affiche les instructions et active les objets selon l'étape.
    /// </summary>
    private void ShowStep(int stepIndex)
    {
        _currentStep = stepIndex;

        // Mise à jour Textes
        if (_instructionText != null) _instructionText.text = _instructions[stepIndex];
        if (_progressText != null) _progressText.text = $"Étape {Mathf.Min(stepIndex + 1, 3)} / 3";

        // Activation séquentielle des obstacles
        if (stepIndex == 1 && _obstacleLane != null) _obstacleLane.SetActive(true);
        if (stepIndex == 2 && _obstacleCrouch != null) _obstacleCrouch.SetActive(true);

        // Petit feedback sonore
        AudioManager.Instance?.PlaySFX("Blip"); 
    }

    /// <summary>
    /// Logique de validation des étapes.
    /// </summary>
    private void CheckStepCompletion()
    {
        float playerZ = _playerTransform.position.z;

        switch (_currentStep)
        {
            case 0:
                // Étape 1 : Le joueur doit juste presser une touche de direction
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                    Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.D))
                {
                    NextStep();
                }
                break;

            case 1:
                // Étape 2 : Le joueur doit avoir dépassé l'obstacle au sol
                if (playerZ > _laneObstacleZ)
                {
                    NextStep();
                }
                break;

            case 2:
                // Étape 3 : Le joueur doit avoir dépassé la barrière haute
                if (playerZ > _crouchObstacleZ)
                {
                    NextStep();
                }
                break;

            case 3:
                // Fin : Le joueur atteint la zone de fin du tuto
                if (playerZ > _finishZ)
                {
                    CompleteTutorial();
                }
                break;
        }
    }

    public void NextStep()
    {
        _currentStep++;
        if (_currentStep < _instructions.Length)
        {
            ShowStep(_currentStep);
        }
    }

    private void CompleteTutorial()
    {
        if (_tutorialCompleted) return;
        _tutorialCompleted = true;

        Debug.Log("[TutorialManager] Tutoriel Réussi");

        // Sauvegarde de l'état
        SettingsManager.Instance?.SetShowTutorial(false);

        if (_instructionText != null) _instructionText.text = "Génial ! C'est parti !";
        
        StartCoroutine(FinishRoutine());
    }

    private IEnumerator FinishRoutine()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        Time.timeScale = 1f;
        LoadGameplay();
    }

    private void OnSkipClicked()
    {
        SettingsManager.Instance?.SetShowTutorial(false);
        Time.timeScale = 1f;
        LoadGameplay();
    }

    private void LoadGameplay()
    {
        SceneManager.LoadScene("Gameplay");
    }

    private void OnDestroy()
    {
        // Sécurité pour ne pas laisser le jeu au ralenti
        Time.timeScale = 1f;
    }
}