using UnityEngine;

public class MainPersistence : MonoBehaviour
{
    private static MainPersistence _instance;

    void Awake()
    {
        // Si un Manager existe déjà (venant d'une scène précédente), on détruit celui-ci
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        // On rend l'objet PARENT immortel
        DontDestroyOnLoad(gameObject);
    }
}