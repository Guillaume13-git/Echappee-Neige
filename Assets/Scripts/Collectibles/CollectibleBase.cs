using UnityEngine;

/// <summary>
/// Classe de base pour tous les collectibles.
/// Gère la collecte, l'effet, le son, et la destruction.
/// </summary>
public abstract class CollectibleBase : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] protected string _collectSoundName = "Collect";
    [SerializeField] protected GameObject _collectParticlePrefab;
    
    /// <summary>
    /// Appelé quand le joueur collecte cet objet.
    /// </summary>
    public void Collect()
    {
        // Applique l'effet spécifique
        ApplyEffect();
        
        // Feedback audio
        AudioManager.Instance?.PlaySFX(_collectSoundName);
        
        // Feedback visuel
        SpawnCollectParticles();
        
        // Détruit le collectible
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Effet spécifique du collectible (à implémenter dans les classes dérivées).
    /// </summary>
    protected abstract void ApplyEffect();
    
    /// <summary>
    /// Spawn des particules de collecte.
    /// </summary>
    private void SpawnCollectParticles()
    {
        if (_collectParticlePrefab != null)
        {
            GameObject particles = Instantiate(_collectParticlePrefab, transform.position, Quaternion.identity);
            Destroy(particles, 2f);
        }
    }
    
    /// <summary>
    /// Rotation visuelle du collectible (optionnel).
    /// </summary>
    protected virtual void Update()
    {
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }
}