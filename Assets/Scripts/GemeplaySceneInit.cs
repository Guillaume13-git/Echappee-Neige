using UnityEngine;

/// <summary>
/// Je suis responsable de l'initialisation de la scène Gameplay au chargement.
/// Mon rôle principal : m'assurer que le jeu démarre correctement en état Playing avec la musique adéquate.
/// Je suis particulièrement utile quand on lance directement la scène Gameplay depuis l'éditeur Unity pour tester.
/// </summary>
public class GameplaySceneInit : MonoBehaviour
{
    [Header("Références")]
    // Je garde la référence vers le GameManager pour contrôler l'état du jeu
    [SerializeField] private GameManager _gameManager;
    
    // Je garde la référence vers l'AudioManager pour gérer la musique
    [SerializeField] private AudioManager _audioManager;

    [Header("Configuration")]
    // Je détermine si je dois forcer l'état Playing au démarrage
    [SerializeField] private bool _forcePlayingState = true;
    
    // Je détermine si je dois lancer la musique de gameplay automatiquement
    [SerializeField] private bool _playGameplayMusic = true;

    /// <summary>
    /// Au réveil, je m'assure d'avoir mes références aux managers.
    /// Si elles ne sont pas assignées manuellement, je les cherche dans la scène.
    /// </summary>
    private void Awake()
    {
        // Si mon GameManager n'est pas assigné dans l'inspecteur, je le cherche moi-même
        if (_gameManager == null)
        {
            _gameManager = FindFirstObjectByType<GameManager>();
        }

        // Si mon AudioManager n'est pas assigné dans l'inspecteur, je le cherche moi-même
        if (_audioManager == null)
        {
            _audioManager = FindFirstObjectByType<AudioManager>();
        }
    }

    /// <summary>
    /// Au démarrage, je lance l'initialisation complète de la scène de gameplay.
    /// </summary>
    private void Start()
    {
        InitializeGameplay();
    }

    /// <summary>
    /// Je configure correctement la scène de gameplay.
    /// Mon rôle : mettre le jeu en état Playing et lancer la musique appropriée.
    /// </summary>
    private void InitializeGameplay()
    {
        // Je force l'état du jeu à Playing si c'est activé et que j'ai accès au GameManager
        if (_forcePlayingState && _gameManager != null)
        {
            _gameManager.SetGameState(GameState.Playing);
            Debug.Log("[GameplaySceneInit] État du jeu forcé à Playing.");
        }

        // Je lance la musique de gameplay si c'est activé et que j'ai accès à l'AudioManager
        if (_playGameplayMusic && _audioManager != null)
        {
            _audioManager.PlayGameplayMusic();
            Debug.Log("[GameplaySceneInit] Musique de gameplay lancée.");
        }
    }

    /// <summary>
    /// Je me configure automatiquement quand on modifie mes paramètres dans l'inspecteur Unity.
    /// Mon but : faciliter les tests en éditeur en activant automatiquement mes fonctionnalités.
    /// 
    /// Cas d'usage : Si un développeur double-clique sur la scène Gameplay pour la tester directement,
    /// je m'assure que tout est correctement initialisé même si on n'est pas passé par le MainMenu.
    /// </summary>
    private void OnValidate()
    {
        #if UNITY_EDITOR
        // Dans l'éditeur Unity, je m'assure que mes options d'initialisation sont toujours activées
        // Cela évite les oublis et garantit une expérience de test fluide
        _forcePlayingState = true;
        _playGameplayMusic = true;
        #endif
    }
}