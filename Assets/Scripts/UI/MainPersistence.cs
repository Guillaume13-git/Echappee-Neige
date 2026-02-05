using UnityEngine;

/// <summary>
/// Je gère la persistance des managers principaux entre les scènes.
/// Je m'assure qu'il n'existe qu'une seule instance et qu'elle ne soit pas détruite au changement de scène.
/// </summary>
public class MainPersistence : MonoBehaviour
{
    // Je stocke l'instance unique de MainPersistence
    private static MainPersistence _instance;

    /// <summary>
    /// Je m'initialise et je garantis qu'il n'existe qu'une seule instance
    /// </summary>
    void Awake()
    {
        // ---------------------------------------------------------
        // VÉRIFICATION DE L'INSTANCE UNIQUE (SINGLETON)
        // ---------------------------------------------------------
        
        // Si une instance existe déjà (venant d'une scène précédente) et que ce n'est pas moi
        if (_instance != null && _instance != this)
        {
            // Je me détruis car je suis un doublon
            Destroy(gameObject);
            return; // J'arrête l'exécution ici
        }

        // ---------------------------------------------------------
        // ENREGISTREMENT COMME INSTANCE UNIQUE
        // ---------------------------------------------------------
        
        // Je deviens l'instance unique
        _instance = this;
        
        // ---------------------------------------------------------
        // PERSISTANCE ENTRE LES SCÈNES
        // ---------------------------------------------------------
        
        // Je rends mon GameObject parent immortel (il ne sera pas détruit lors des changements de scène)
        // Cela permet aux managers enfants (GameManager, AudioManager, etc.) de persister
        DontDestroyOnLoad(gameObject);
    }
}