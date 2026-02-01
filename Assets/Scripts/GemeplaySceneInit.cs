using UnityEngine;

/// <summary>
/// Initialise la scène Gameplay au chargement.
/// Force le GameManager en état Playing et lance la musique de gameplay.
/// </summary>
public class GameplaySceneInit : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private AudioManager _audioManager;

    [Header("Configuration")]
    [SerializeField] private bool _forcePlayingState = true;
    [SerializeField] private bool _playGameplayMusic = true;

    private void Awake()
    {
        // Trouver les managers si non assignés
        if (_gameManager == null)
        {
            _gameManager = FindFirstObjectByType<GameManager>();
        }

        if (_audioManager == null)
        {
            _audioManager = FindFirstObjectByType<AudioManager>();
        }
    }

    private void Start()
    {
        InitializeGameplay();
    }

    private void InitializeGameplay()
    {
        // Forcer l'état Playing
        if (_forcePlayingState && _gameManager != null)
        {
            _gameManager.SetGameState(GameState.Playing);
            Debug.Log("[GameplaySceneInit] État du jeu forcé à Playing.");
        }

        // Lancer la musique de gameplay
        if (_playGameplayMusic && _audioManager != null)
        {
            _audioManager.PlayGameplayMusic();
            Debug.Log("[GameplaySceneInit] Musique de gameplay lancée.");
        }
    }

    /// <summary>
    /// Permet de détecter si on lance directement la scène depuis l'éditeur Unity.
    /// Utile pour les tests : si on double-clic sur la scène Gameplay, elle s'initialise correctement.
    /// </summary>
    private void OnValidate()
    {
        #if UNITY_EDITOR
        // Dans l'éditeur, toujours activer l'initialisation automatique
        _forcePlayingState = true;
        _playGameplayMusic = true;
        #endif
    }
}