using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Je gère le tutoriel du jeu Échappée-Neige.
/// Je guide le joueur à travers ses premières actions sans variables de stockage inutiles.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _tutorialPanel;       // Je stocke le panneau du tutoriel
    [SerializeField] private TextMeshProUGUI _instructionText; // Je stocke le texte d'instruction
    [SerializeField] private TextMeshProUGUI _progressText;    // Je stocke le texte de progression
    [SerializeField] private Button _skipButton;               // Je stocke le bouton pour passer le tutoriel

    [Header("Player Reference")]
    [SerializeField] private PlayerController _player; // Je stocke la référence au contrôleur du joueur

    [Header("Tutorial Speed")]
    [SerializeField] private float _tutorialSpeed = 5f;      // Je stocke la vitesse du tutoriel (réduite)
    [SerializeField] private ChunkMover _chunkMover;         // Je stocke le contrôleur de vitesse des chunks

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true; // Je stocke si j'affiche les logs de debug

    // ---------------------------------------------------------
    // ÉTAT DU TUTORIEL
    // ---------------------------------------------------------
    
    private int _currentStep = 0;           // Je stocke l'étape actuelle du tutoriel
    private bool _tutorialCompleted = false; // Je stocke si le tutoriel est terminé

    // Pour détecter le changement de lane
    private int _initialLane = -1;         // Je stocke la lane initiale du joueur
    private bool _hasChangedLane = false;  // Je stocke si le joueur a changé de lane

    // ---------------------------------------------------------
    // INSTRUCTIONS
    // ---------------------------------------------------------
    
    // Je stocke toutes les instructions du tutoriel
    private readonly string[] _instructions = new string[]
    {
        "Bienvenue ! Utilisez ← et → (ou Q et D) pour changer de couloir.",
        "Bien joué ! Maintenant évitez les obstacles en changeant de couloir.",
        "Parfait ! Utilisez ↓ ou Shift pour vous baisser sous les obstacles hauts.",
        "Excellent ! Vous êtes prêt. Bonne chance dans la descente !"
    };

    /// <summary>
    /// Je m'initialise au démarrage du tutoriel
    /// </summary>
    private void Start()
    {
        // ---------------------------------------------------------
        // 1. RÉCUPÉRATION DES RÉFÉRENCES
        // ---------------------------------------------------------
        
        // Si le joueur n'est pas assigné, je le cherche dans la scène
        if (_player == null) 
            _player = FindFirstObjectByType<PlayerController>();
        
        // Si le ChunkMover n'est pas assigné, je le cherche dans la scène
        if (_chunkMover == null) 
            _chunkMover = FindFirstObjectByType<ChunkMover>();

        // ---------------------------------------------------------
        // 2. CONFIGURATION DU CHUNKMOVER
        // ---------------------------------------------------------
        
        // Je configure le ChunkMover avec une vitesse réduite pour le tutoriel
        if (_chunkMover != null)
        {
            _chunkMover.SetSpeed(_tutorialSpeed);
        }

        // ---------------------------------------------------------
        // 3. CONFIGURATION DE L'UI
        // ---------------------------------------------------------
        
        // Je configure le bouton "Passer"
        if (_skipButton != null)
            _skipButton.onClick.AddListener(OnSkipClicked);

        // J'active le panneau du tutoriel
        if (_tutorialPanel != null)
            _tutorialPanel.SetActive(true);

        // ---------------------------------------------------------
        // 4. ÉTAT INITIAL DU JEU
        // ---------------------------------------------------------
        
        // Je mets le jeu en mode Tutoriel
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameState.Tutorial);
        }

        // ---------------------------------------------------------
        // 5. ENREGISTREMENT DE LA LANE INITIALE
        // ---------------------------------------------------------
        
        // J'enregistre la lane de départ du joueur pour détecter les changements
        if (_player != null)
        {
            _initialLane = _player.GetCurrentLane();
            
            if (_showDebugLogs) 
                Debug.Log($"[TutorialManager] Lane initiale : {_initialLane}");
        }

        if (_showDebugLogs) 
            Debug.Log("[TutorialManager] 🎓 Session de tutoriel démarrée.");

        // J'affiche la première étape
        ShowStep(0);
    }

    /// <summary>
    /// Je vérifie à chaque frame si le joueur a complété l'étape actuelle
    /// </summary>
    private void Update()
    {
        // Si le tutoriel est terminé ou si le joueur n'existe pas, je ne fais rien
        if (_tutorialCompleted || _player == null) return;
        
        // Je vérifie si l'étape actuelle est complétée
        CheckStepCompletion();
    }

    /// <summary>
    /// J'affiche une étape du tutoriel
    /// </summary>
    /// <param name="stepIndex">L'index de l'étape à afficher</param>
    private void ShowStep(int stepIndex)
    {
        // Je mémorise l'étape actuelle
        _currentStep = stepIndex;

        // J'affiche l'instruction correspondante
        if (_instructionText != null)
            _instructionText.text = _instructions[stepIndex];

        // J'affiche la progression (Étape X / 3)
        if (_progressText != null)
        {
            // Je limite l'affichage à l'étape 3 maximum (les vraies étapes)
            int displayStep = Mathf.Min(stepIndex + 1, 3);
            _progressText.text = $"Étape {displayStep} / 3";
        }

        // Je joue un son pour signaler le changement d'étape
        AudioManager.Instance?.PlaySFX("Blip");
    }

    /// <summary>
    /// Je vérifie si le joueur a complété l'étape actuelle
    /// </summary>
    private void CheckStepCompletion()
    {
        switch (_currentStep)
        {
            // ---------------------------------------------------------
            // ÉTAPE 0 : CHANGER DE COULOIR
            // ---------------------------------------------------------
            case 0:
                // Je récupère la lane actuelle du joueur
                int currentLane = _player.GetCurrentLane();
                
                // Si le joueur n'a pas encore changé de lane ET qu'il n'est plus sur sa lane initiale
                if (!_hasChangedLane && currentLane != _initialLane)
                {
                    // Je marque qu'il a changé de lane
                    _hasChangedLane = true;
                    
                    if (_showDebugLogs) 
                        Debug.Log($"[TutorialManager] Changement de lane détecté ! {_initialLane} → {currentLane}");
                    
                    // Je passe à l'étape suivante
                    NextStep();
                }
                break;

            // ---------------------------------------------------------
            // ÉTAPE 1 : ÉVITER UN OBSTACLE
            // ---------------------------------------------------------
            case 1:
                // Je détecte si le joueur appuie sur les touches de déplacement latéral
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                    Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.D))
                {
                    // J'attends un peu avant de passer à l'étape suivante (pour que le joueur lise)
                    StartCoroutine(DelayedNextStep(1.2f));
                }
                break;

            // ---------------------------------------------------------
            // ÉTAPE 2 : S'ACCROUPIR
            // ---------------------------------------------------------
            case 2:
                // Je détecte si le joueur appuie sur les touches pour s'accroupir
                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.S))
                {
                    // J'attends un peu avant de passer à l'étape suivante
                    StartCoroutine(DelayedNextStep(1.2f));
                }
                break;

            // ---------------------------------------------------------
            // ÉTAPE 3 : FIN DU TUTORIEL
            // ---------------------------------------------------------
            case 3:
                // Si le tutoriel n'est pas encore marqué comme terminé
                if (!_tutorialCompleted)
                {
                    // Je termine automatiquement le tutoriel après 2 secondes
                    StartCoroutine(AutoCompleteTutorial(2f));
                }
                break;
        }
    }

    /// <summary>
    /// Je passe à l'étape suivante après un délai
    /// </summary>
    /// <param name="delay">Le délai en secondes</param>
    private IEnumerator DelayedNextStep(float delay)
    {
        // Je mémorise l'étape actuelle pour éviter les doublons
        int currentProcessingStep = _currentStep;
        
        // J'attends le délai spécifié
        yield return new WaitForSeconds(delay);
        
        // Je ne passe à l'étape suivante que si je suis toujours sur la même étape
        // (évite les bugs si le joueur a déjà avancé)
        if (_currentStep == currentProcessingStep)
        {
            NextStep();
        }
    }

    /// <summary>
    /// Je passe à l'étape suivante du tutoriel
    /// </summary>
    private void NextStep()
    {
        // J'incrémente le compteur d'étape
        _currentStep++;
        
        if (_showDebugLogs) 
            Debug.Log($"[TutorialManager] Étape complétée. Nouvelle étape : {_currentStep}");

        // Si j'ai encore des étapes à afficher
        if (_currentStep < _instructions.Length)
            ShowStep(_currentStep); // J'affiche l'étape suivante
        else
            CompleteTutorial(); // Sinon, je termine le tutoriel
    }

    /// <summary>
    /// Je termine automatiquement le tutoriel après un délai
    /// </summary>
    /// <param name="delay">Le délai en secondes</param>
    private IEnumerator AutoCompleteTutorial(float delay)
    {
        // J'attends le délai spécifié
        yield return new WaitForSeconds(delay);
        
        // Je termine le tutoriel
        CompleteTutorial();
    }

    /// <summary>
    /// Je termine le tutoriel et je prépare la transition vers le gameplay
    /// </summary>
    private void CompleteTutorial()
    {
        // Si le tutoriel est déjà terminé, je ne fais rien
        if (_tutorialCompleted) return;
        
        // Je marque le tutoriel comme terminé
        _tutorialCompleted = true;

        // Je rétablis la vitesse normale des chunks
        if (_chunkMover != null)
            _chunkMover.ReleaseForcedSpeed();

        // J'affiche le message de fin
        if (_instructionText != null)
            _instructionText.text = "Génial ! C'est parti pour la descente !";

        if (_showDebugLogs) 
            Debug.Log("[TutorialManager] 🎉 Tutoriel terminé avec succès.");

        // Je joue le son de victoire
        AudioManager.Instance?.PlaySFX("Victory");
        
        // Je lance la routine de fin
        StartCoroutine(FinishRoutine());
    }

    /// <summary>
    /// J'attends un peu puis je charge le gameplay
    /// </summary>
    private IEnumerator FinishRoutine()
    {
        // J'attends 2.5 secondes pour que le joueur lise le message final
        yield return new WaitForSecondsRealtime(2.5f);
        
        // Je charge la scène de gameplay
        LoadGameplay();
    }

    /// <summary>
    /// Je gère le clic sur le bouton "Passer"
    /// </summary>
    private void OnSkipClicked()
    {
        if (_showDebugLogs) 
            Debug.Log("[TutorialManager] ⏭️ Tutoriel passé par l'utilisateur.");
        
        // Je rétablis la vitesse normale
        if (_chunkMover != null) 
            _chunkMover.ReleaseForcedSpeed();
        
        // Je charge directement le gameplay
        LoadGameplay();
    }

    /// <summary>
    /// Je charge la scène de gameplay
    /// </summary>
    private void LoadGameplay()
    {
        // Je désactive le tutoriel pour les prochaines parties
        SettingsManager.Instance?.SetShowTutorial(false);
        
        // Je charge la scène de gameplay
        SceneManager.LoadScene("Gameplay");
    }

    /// <summary>
    /// Je me nettoie quand je suis détruit
    /// </summary>
    private void OnDestroy()
    {
        // Je me désabonne du bouton "Passer"
        if (_skipButton != null)
            _skipButton.onClick.RemoveListener(OnSkipClicked);
    }
}