using UnityEngine;

/// <summary>
/// Je suis responsable de l'animation des bâtons de ski du joueur.
/// Mon rôle : créer un mouvement de balancier alterné et naturel pendant que le joueur skie.
/// Je dois être attaché au GameObject "PoleVisuals" dans la hiérarchie du joueur.
/// </summary>
public class PoleAnimator : MonoBehaviour
{
    [Header("Pole References")]
    // Je garde la référence vers le bâton de ski gauche
    [SerializeField] private Transform _poleLeft;
    
    // Je garde la référence vers le bâton de ski droit
    [SerializeField] private Transform _poleRight;
    
    [Header("Animation Settings")]
    // Je détermine l'amplitude du balancement des bâtons en degrés
    [SerializeField] private float _swingAmount = 5f;
    
    // Je détermine la vitesse du balancement (cycles par seconde)
    [SerializeField] private float _swingSpeed = 4f;
    
    // Je garde la référence vers le PlayerController pour savoir si le joueur est actif
    private PlayerController _player;
    
    // Je stocke mon offset de balancement qui augmente avec le temps pour créer l'oscillation
    private float _swingOffset = 0f;
    
    /// <summary>
    /// Au démarrage, je récupère la référence au PlayerController.
    /// Je cherche dans mes parents car je suis un enfant du GameObject du joueur.
    /// </summary>
    private void Start()
    {
        // Je remonte dans la hiérarchie pour trouver le PlayerController
        _player = GetComponentInParent<PlayerController>();
    }
    
    /// <summary>
    /// À chaque frame, j'anime les bâtons de ski avec un mouvement alterné.
    /// Mon animation simule le mouvement naturel des bras pendant le ski.
    /// </summary>
    private void Update()
    {
        // Si je n'ai pas trouvé le PlayerController, je ne fais rien
        if (_player == null) return;
        
        // J'augmente mon offset de temps pour créer une oscillation continue
        // Plus _swingSpeed est élevé, plus le balancement est rapide
        _swingOffset += Time.deltaTime * _swingSpeed;
        
        // Je calcule le balancement avec une fonction sinusoïdale pour un mouvement fluide
        // Mathf.Sin crée une oscillation entre -1 et +1, que je multiplie par l'amplitude
        float swing = Mathf.Sin(_swingOffset) * _swingAmount;
        
        // J'applique la rotation au bâton gauche
        if (_poleLeft != null)
        {
            // Je récupère la rotation actuelle en coordonnées locales
            Vector3 leftRot = _poleLeft.localEulerAngles;
            
            // Je modifie l'axe X (inclinaison avant/arrière)
            // Base de 15° + le balancement calculé
            leftRot.x = 15 + swing;
            
            // J'applique la nouvelle rotation
            _poleLeft.localEulerAngles = leftRot;
        }
        
        // J'applique la rotation au bâton droit (en opposition avec le gauche)
        if (_poleRight != null)
        {
            // Je récupère la rotation actuelle en coordonnées locales
            Vector3 rightRot = _poleRight.localEulerAngles;
            
            // Je modifie l'axe X avec le balancement OPPOSÉ (remarquez le signe -)
            // Cela crée le mouvement alterné : quand le gauche va en avant, le droit va en arrière
            rightRot.x = 15 - swing;
            
            // J'applique la nouvelle rotation
            _poleRight.localEulerAngles = rightRot;
        }
    }
}