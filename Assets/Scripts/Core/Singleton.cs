using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting) return null;

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    // On ne met PAS de LogError ici. 
                    // On laisse le script appelant gérer le "null" avec un point d'interrogation.
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_applicationIsQuitting) return;

        if (_instance == null)
        {
            _instance = this as T;
            
            // On ne rend persistant que si c'est l'objet racine
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            // ✅ CRITIQUE : Si une instance existe déjà (celle du MainMenu), 
            // on détruit immédiatement la nouvelle (celle de la scène Options).
            Debug.Log($"[Singleton] Doublon de {typeof(T)} détecté sur {gameObject.name}. Destruction du doublon.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit() => _applicationIsQuitting = true;

    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}