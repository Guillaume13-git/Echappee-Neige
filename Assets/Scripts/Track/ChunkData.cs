using UnityEngine;
using System.Linq;

/// <summary>
/// Je suis attaché à chaque préfab de chunk.
/// Je contiens les métadonnées pour la sélection et le placement des chunks.
/// </summary>
public class ChunkData : MonoBehaviour
{
    [Header("Chunk Properties")]
    [SerializeField] private string _chunkName = "Chunk_01";                      // Je stocke le nom du chunk
    [SerializeField] private float _chunkLength = 50f;                            // Je stocke la longueur du chunk (50 mètres)
    [SerializeField] private TrackPhase _recommendedPhase = TrackPhase.Green;     // Je stocke la phase recommandée pour ce chunk
    [SerializeField] private ChunkType _chunkType = ChunkType.Normal;             // Je stocke le type de chunk
    [SerializeField] private ChunkDifficulty _difficulty = ChunkDifficulty.Easy;  // Je stocke la difficulté du chunk
    
    [Header("Content Info (auto-calculated)")]
    [SerializeField] private int _obstacleCount = 0;      // Je stocke le nombre d'obstacles (calculé automatiquement)
    [SerializeField] private int _collectibleCount = 0;   // Je stocke le nombre de collectibles (calculé automatiquement)
    
    // ---------------------------------------------------------
    // PROPRIÉTÉS PUBLIQUES
    // ---------------------------------------------------------
    
    // Je donne accès en lecture seule au nom du chunk
    public string ChunkName => _chunkName;
    
    // Je donne accès en lecture seule à la longueur du chunk
    public float ChunkLength => _chunkLength;
    
    // Je donne accès en lecture seule à la phase recommandée
    public TrackPhase RecommendedPhase => _recommendedPhase;
    
    // Je donne accès en lecture seule au type de chunk
    public ChunkType Type => _chunkType;
    
    // Je donne accès en lecture seule à la difficulté
    public ChunkDifficulty Difficulty => _difficulty;
    
    // Je donne accès en lecture seule au nombre d'obstacles
    public int ObstacleCount => _obstacleCount;
    
    // Je donne accès en lecture seule au nombre de collectibles
    public int CollectibleCount => _collectibleCount;
    
    /// <summary>
    /// Je suis appelé automatiquement quand le prefab est modifié dans l'éditeur.
    /// Je compte les obstacles et les collectibles présents dans le chunk.
    /// </summary>
    private void OnValidate()
    {
        // Je ne calcule que dans l'éditeur, pas pendant le jeu
        if (Application.isPlaying) return;

        // ---------------------------------------------------------
        // COMPTAGE DES OBSTACLES
        // ---------------------------------------------------------
        
        // Je compte tous les Colliders enfants qui ont le tag "Obstacle"
        _obstacleCount = GetComponentsInChildren<Collider>()
            .Count(c => c.CompareTag("Obstacle"));
            
        // ---------------------------------------------------------
        // COMPTAGE DES COLLECTIBLES
        // ---------------------------------------------------------
        
        // Je cherche tous les composants CollectibleVisual dans les enfants
        var collectibles = GetComponentsInChildren<CollectibleVisual>();
        
        // Je compte le nombre de collectibles trouvés
        _collectibleCount = collectibles != null ? collectibles.Length : 0;
        
        // Alternative : Je peux aussi compter par tag "Collectible" (plus robuste)
        // Cette méthode fonctionne même si CollectibleVisual n'est pas présent
        // _collectibleCount = GetComponentsInChildren<Transform>()
        //     .Count(t => t.CompareTag("Collectible") && t != transform);
    }
}