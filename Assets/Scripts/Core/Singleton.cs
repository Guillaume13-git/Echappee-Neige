using UnityEngine;

/// <summary>
/// Je suis un pattern Singleton générique.
/// Je garantis qu'il n'existe qu'une seule instance d'un type de classe dans le jeu.
/// Je persiste automatiquement entre les changements de scènes.
/// </summary>
/// <typeparam name="T">Le type de la classe qui hérite de moi</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;                      // Je stocke l'instance unique de la classe
    private static readonly object _lock = new object(); // Je stocke un verrou pour la sécurité multi-thread
    private static bool _applicationIsQuitting = false;  // Je stocke si l'application est en train de se fermer

    /// <summary>
    /// Je fournis l'accès à l'instance unique de la classe
    /// </summary>
    public static T Instance
    {
        get
        {
            // Si l'application est en train de se fermer, je ne retourne rien
            // Cela évite de créer de nouvelles instances pendant la fermeture
            if (_applicationIsQuitting) return null;

            // J'utilise un verrou pour garantir la sécurité en cas d'accès multi-thread
            lock (_lock)
            {
                // Si aucune instance n'existe encore
                if (_instance == null)
                {
                    // Je cherche une instance existante dans la scène
                    _instance = FindFirstObjectByType<T>();

                    // Je ne mets PAS de LogError ici
                    // Je laisse le script appelant gérer le "null" avec un point d'interrogation (?)
                    // Exemple : AudioManager.Instance?.PlaySFX("Blip");
                }
                
                // Je retourne l'instance (qui peut être null si elle n'existe pas)
                return _instance;
            }
        }
    }

    /// <summary>
    /// Je m'initialise au démarrage et je gère les doublons
    /// </summary>
    protected virtual void Awake()
    {
        // Si l'application est en train de se fermer, je ne fais rien
        if (_applicationIsQuitting) return;

        // Si aucune instance n'existe encore
        if (_instance == null)
        {
            // Je me définis comme l'instance unique
            _instance = this as T;
            
            // Je ne me rends persistant que si je suis un objet racine (sans parent)
            // Cela évite de casser la hiérarchie des objets
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject); // Je persiste entre les changements de scènes
            }
        }
        else if (_instance != this)
        {
            // CRITIQUE : Si une instance existe déjà, c'est que je suis un doublon
            // Par exemple : l'instance du MainMenu existe déjà, et je suis celle de la scène Options
            
            // J'affiche un message pour indiquer que je détecte un doublon
            Debug.Log($"[Singleton] Doublon de {typeof(T)} détecté sur {gameObject.name}. Destruction du doublon.");
            
            // Je me détruis immédiatement pour éviter d'avoir deux instances
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Je détecte quand l'application se ferme
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        // Je marque que l'application est en train de se fermer
        // Cela évite de créer de nouvelles instances pendant la fermeture
        _applicationIsQuitting = true;
    }

    /// <summary>
    /// Je nettoie ma référence quand je suis détruit
    /// </summary>
    protected virtual void OnDestroy()
    {
        // Si je suis l'instance unique qui est détruite
        if (_instance == this)
        {
            // Je réinitialise la référence à null
            // Cela permet de créer une nouvelle instance si nécessaire
            _instance = null;
        }
    }
}