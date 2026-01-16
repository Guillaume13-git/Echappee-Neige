using UnityEngine;

/// <summary>
/// Classe générique Singleton pour Unity.
/// Garantit qu'une seule instance existe et la rend accessible globalement.
/// Utilisation : public class MonManager : Singleton<MonManager>
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// Accès global à l'instance unique.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance de '{typeof(T)}' déjà détruite. Retourne null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // Nouvelle API Unity 2023+
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        Debug.LogError($"[Singleton] Aucune instance de '{typeof(T)}' trouvée dans la scène !");
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// Initialisation du Singleton.
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[Singleton] Instance de '{typeof(T)}' déjà existante. Destruction de {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
    }

    /// <summary>
    /// Gestion de la destruction lors de la fermeture de l'application.
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    /// <summary>
    /// Réinitialisation pour éviter les problèmes en mode Editor.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}