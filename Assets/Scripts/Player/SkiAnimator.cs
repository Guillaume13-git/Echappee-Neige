using UnityEngine;

/// <summary>
/// Je suis responsable de l'animation subtile des skis pendant le mouvement du joueur.
/// Mon rôle : créer un balancement léger et naturel des skis pour donner vie à l'animation.
/// Je dois être attaché au GameObject "SkiVisuals" dans la hiérarchie du joueur.
/// </summary>
public class SkiAnimator : MonoBehaviour
{
    [Header("Ski References")]
    // Je garde la référence vers le ski gauche
    [SerializeField] private Transform _skiLeft;
    
    // Je garde la référence vers le ski droit
    [SerializeField] private Transform _skiRight;
    
    [Header("Animation Settings")]
    // Je détermine l'angle maximum de balancement des skis en degrés
    [SerializeField] private float _tiltAmount = 2f;
    
    // Je détermine la vitesse du balancement (cycles par seconde)
    [SerializeField] private float _tiltSpeed = 3f;
    
    // Je garde la référence vers le PlayerController pour savoir si le joueur est actif
    private PlayerController _player;
    
    // Je stocke mon offset de balancement qui augmente avec le temps pour créer l'oscillation
    private float _tiltOffset = 0f;
    
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
    /// À chaque frame, j'anime les skis avec un mouvement de balancement subtil.
    /// Mon animation simule le mouvement naturel des carres de ski pendant la glisse.
    /// </summary>
    private void Update()
    {
        // Si je n'ai pas trouvé le PlayerController, je ne fais rien
        if (_player == null) return;
        
        // J'augmente mon offset de temps pour créer une oscillation continue
        // Plus _tiltSpeed est élevé, plus le balancement est rapide
        _tiltOffset += Time.deltaTime * _tiltSpeed;
        
        // Je calcule le balancement avec une fonction sinusoïdale pour un mouvement fluide
        // Mathf.Sin crée une oscillation entre -1 et +1, que je multiplie par l'amplitude
        // Ce balancement est volontairement subtil (_tiltAmount = 2°) pour rester réaliste
        float tilt = Mathf.Sin(_tiltOffset) * _tiltAmount;
        
        // J'applique la rotation au ski gauche
        if (_skiLeft != null)
        {
            // Je récupère la rotation actuelle en coordonnées locales
            Vector3 leftRot = _skiLeft.localEulerAngles;
            
            // Je modifie l'axe Z (rotation latérale) avec un balancement NÉGATIF
            // Cela fait que le ski gauche penche dans un sens
            leftRot.z = -tilt;
            
            // J'applique la nouvelle rotation
            _skiLeft.localEulerAngles = leftRot;
        }
        
        // J'applique la rotation au ski droit (en opposition avec le gauche)
        if (_skiRight != null)
        {
            // Je récupère la rotation actuelle en coordonnées locales
            Vector3 rightRot = _skiRight.localEulerAngles;
            
            // Je modifie l'axe Z avec le balancement POSITIF (remarquez l'absence de signe -)
            // Cela crée le mouvement opposé : quand le gauche penche à gauche, le droit penche à droite
            // Ce mouvement simule l'alternance naturelle des carres de ski pendant la glisse
            rightRot.z = tilt;
            
            // J'applique la nouvelle rotation
            _skiRight.localEulerAngles = rightRot;
        }
    }
}