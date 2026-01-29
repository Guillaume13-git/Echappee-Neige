using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Gère le tutoriel du jeu Échappée-Neige.
/// Guide le joueur à travers ses actions sans variables de stockage inutiles.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _instructionText;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private Button _skipButton;

    [Header("Player Reference")]
    [SerializeField] private PlayerController _player;

    [Header("Tutorial Speed")]
    [SerializeField] private float _tutorialSpeed = 5f;
    [SerializeField] private ChunkMover _chunkMover;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;

    // État du tutoriel
    private int _currentStep = 0;
    private bool _tutorialCompleted = false;

    // Pour détecter le changement de lane
    private int _initialLane = -1;
    private bool _hasChangedLane = false;

    // Instructions
    private readonly string[] _instructions = new string[]
    {
        "Bienvenue ! Utilisez ← et → (ou Q et D) pour changer de couloir.",
        "Bien joué ! Maintenant évitez les obstacles en changeant de couloir.",
        "Parfait ! Utilisez ↓ ou Shift pour vous baisser sous les obstacles hauts.",
        "Excellent ! Vous êtes prêt. Bonne chance dans la descente !"
    };

    private void Start()
    {
        // 1. Récupération des références
        if (_player == null) _player = FindFirstObjectByType<PlayerController>();
        if (_chunkMover == null) _chunkMover = FindFirstObjectByType<ChunkMover>();

        // 2. Configuration ChunkMover
        if (_chunkMover != null)
        {
            _chunkMover.SetSpeed(_tutorialSpeed);
        }

        // 3. Configuration UI
        if (_skipButton != null)
            _skipButton.onClick.AddListener(OnSkipClicked);

        if (_tutorialPanel != null)
            _tutorialPanel.SetActive(true);

        // 4. État initial
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameState.Tutorial);
        }

        // 5. Enregistrer la lane initiale
        if (_player != null)
        {
            _initialLane = _player.GetCurrentLane();
            if (_showDebugLogs) Debug.Log($"[TutorialManager] Lane initiale : {_initialLane}");
        }

        if (_showDebugLogs) Debug.Log("[TutorialManager] 🎓 Session de tutoriel démarrée.");

        ShowStep(0);
    }

    private void Update()
    {
        if (_tutorialCompleted || _player == null) return;
        CheckStepCompletion();
    }

    private void ShowStep(int stepIndex)
    {
        _currentStep = stepIndex;

        if (_instructionText != null)
            _instructionText.text = _instructions[stepIndex];

        if (_progressText != null)
        {
            int displayStep = Mathf.Min(stepIndex + 1, 3);
            _progressText.text = $"Étape {displayStep} / 3";
        }

        AudioManager.Instance?.PlaySFX("Blip");
    }

    private void CheckStepCompletion()
    {
        switch (_currentStep)
        {
            case 0: // Changer de couloir - VÉRIFICATION CORRIGÉE
                int currentLane = _player.GetCurrentLane();
                
                if (!_hasChangedLane && currentLane != _initialLane)
                {
                    _hasChangedLane = true;
                    if (_showDebugLogs) 
                        Debug.Log($"[TutorialManager] Changement de lane détecté ! {_initialLane} → {currentLane}");
                    
                    NextStep();
                }
                break;

            case 1: // Éviter obstacle (on peut garder la détection simple ou attendre une vraie collision évitée)
                // Pour l'instant, on garde votre logique existante
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                    Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.D))
                {
                    StartCoroutine(DelayedNextStep(1.2f));
                }
                break;

            case 2: // S'accroupir
                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.S))
                {
                    StartCoroutine(DelayedNextStep(1.2f));
                }
                break;

            case 3: // Fin / Prêt
                if (!_tutorialCompleted)
                {
                    StartCoroutine(AutoCompleteTutorial(2f));
                }
                break;
        }
    }

    private IEnumerator DelayedNextStep(float delay)
    {
        int currentProcessingStep = _currentStep;
        yield return new WaitForSeconds(delay);
        
        if (_currentStep == currentProcessingStep)
        {
            NextStep();
        }
    }

    private void NextStep()
    {
        _currentStep++;
        
        if (_showDebugLogs) Debug.Log($"[TutorialManager] Étape complétée. Nouvelle étape : {_currentStep}");

        if (_currentStep < _instructions.Length)
            ShowStep(_currentStep);
        else
            CompleteTutorial();
    }

    private IEnumerator AutoCompleteTutorial(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteTutorial();
    }

    private void CompleteTutorial()
    {
        if (_tutorialCompleted) return;
        _tutorialCompleted = true;

        if (_chunkMover != null)
            _chunkMover.ReleaseForcedSpeed();

        if (_instructionText != null)
            _instructionText.text = "Génial ! C'est parti pour la descente !";

        if (_showDebugLogs) Debug.Log("[TutorialManager] 🎉 Tutoriel terminé avec succès.");

        AudioManager.Instance?.PlaySFX("Victory");
        StartCoroutine(FinishRoutine());
    }

    private IEnumerator FinishRoutine()
    {
        yield return new WaitForSecondsRealtime(2.5f);
        LoadGameplay();
    }

    private void OnSkipClicked()
    {
        if (_showDebugLogs) Debug.Log("[TutorialManager] ⏭️ Tutoriel passé par l'utilisateur.");
        if (_chunkMover != null) _chunkMover.ReleaseForcedSpeed();
        LoadGameplay();
    }

    private void LoadGameplay()
    {
        SettingsManager.Instance?.SetShowTutorial(false);
        SceneManager.LoadScene("Gameplay");
    }

    private void OnDestroy()
    {
        if (_skipButton != null)
            _skipButton.onClick.RemoveListener(OnSkipClicked);
    }
}