using UnityEngine;

/// <summary>
/// Je gère uniquement les effets visuels des collectibles (rotation, lévitation).
/// J'utilise la position locale pour suivre correctement mon parent (le chunk de route).
/// </summary>
public class CollectibleVisual : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 50f;        // Je stocke la vitesse de rotation en degrés par seconde
    [SerializeField] private Vector3 _rotationAxis = Vector3.up; // Je stocke l'axe autour duquel je tourne (Y par défaut)

    [Header("Lévitation")]
    [SerializeField] private bool _enableLevitation = true;      // Je stocke si la lévitation est activée
    [SerializeField] private float _levitationAmplitude = 0.3f;  // Je stocke l'amplitude du mouvement vertical (en mètres)
    [SerializeField] private float _levitationSpeed = 2f;        // Je stocke la vitesse de la lévitation

    private Vector3 _startLocalPosition;  // Je mémorise ma position locale de départ pour la lévitation
    private float _levitationTimer;       // Je stocke le timer pour calculer le mouvement sinusoïdal

    /// <summary>
    /// Je m'initialise au démarrage
    /// </summary>
    private void Start()
    {
        // CRITIQUE : Je sauvegarde ma position LOCALE au démarrage
        // Cela me permet de suivre correctement mon parent (le chunk) qui bouge
        _startLocalPosition = transform.localPosition;
        
        // Je démarre avec un décalage aléatoire pour que tous les collectibles
        // ne lévitent pas en même temps (effet plus naturel)
        _levitationTimer = Random.Range(0f, 2f * Mathf.PI);
    }

    /// <summary>
    /// Je mets à jour mes effets visuels à chaque frame
    /// </summary>
    private void Update()
    {
        // ---------------------------------------------------------
        // ROTATION
        // ---------------------------------------------------------
        // Je tourne en continu autour de mon axe configuré
        // Space.Self signifie que je tourne autour de mon propre axe local
        transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime, Space.Self);

        // ---------------------------------------------------------
        // LÉVITATION
        // ---------------------------------------------------------
        // Si la lévitation est activée, je calcule un mouvement de haut en bas
        if (_enableLevitation)
        {
            // J'avance mon timer en fonction du temps et de la vitesse
            _levitationTimer += Time.deltaTime * _levitationSpeed;
            
            // Je calcule le décalage vertical avec une fonction sinusoïdale
            // Cela crée un mouvement fluide de haut en bas
            float yOffset = Mathf.Sin(_levitationTimer) * _levitationAmplitude;
            
            // IMPORTANT : J'utilise localPosition pour bouger par rapport à mon parent
            // Si j'utilisais position (globale), je ne suivrais pas le chunk qui bouge
            transform.localPosition = _startLocalPosition + new Vector3(0, yOffset, 0);
        }
    }
}