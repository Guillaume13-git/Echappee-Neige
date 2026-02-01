using UnityEngine;
using System.Linq;

/// <summary>
/// Composant attaché à chaque préfab de chunk.
/// Contient les métadonnées pour la sélection et le placement.
/// VERSION CORRIGÉE - Utilise CollectibleVisual au lieu de CollectibleBase
/// </summary>
public class ChunkData : MonoBehaviour
{
    [Header("Chunk Properties")]
    [SerializeField] private string _chunkName = "Chunk_01";
    [SerializeField] private float _chunkLength = 50f;
    [SerializeField] private TrackPhase _recommendedPhase = TrackPhase.Green;
    [SerializeField] private ChunkType _chunkType = ChunkType.Normal;
    [SerializeField] private ChunkDifficulty _difficulty = ChunkDifficulty.Easy;
    
    [Header("Content Info (auto-calculated)")]
    [SerializeField] private int _obstacleCount = 0;
    [SerializeField] private int _collectibleCount = 0;
    
    // Propriétés publiques
    public string ChunkName => _chunkName;
    public float ChunkLength => _chunkLength;
    public TrackPhase RecommendedPhase => _recommendedPhase;
    public ChunkType Type => _chunkType;
    public ChunkDifficulty Difficulty => _difficulty;
    public int ObstacleCount => _obstacleCount;
    public int CollectibleCount => _collectibleCount;
    
    /// <summary>
    /// Appelé automatiquement quand tu modifies le prefab dans l'éditeur.
    /// Compte les obstacles et collectibles.
    /// VERSION CORRIGÉE - Cherche CollectibleVisual au lieu de CollectibleBase
    /// </summary>
    private void OnValidate()
    {
        // On ne calcule que dans l'éditeur, pas pendant le jeu
        if (Application.isPlaying) return;

        // Compte les obstacles (Colliders avec tag "Obstacle")
        _obstacleCount = GetComponentsInChildren<Collider>()
            .Count(c => c.CompareTag("Obstacle"));
            
        // ✅ CORRECTION : Cherche CollectibleVisual au lieu de CollectibleBase
        var collectibles = GetComponentsInChildren<CollectibleVisual>();
        _collectibleCount = collectibles != null ? collectibles.Length : 0;
        
        // Alternative : compter par tag "Collectible" (plus robuste)
        // _collectibleCount = GetComponentsInChildren<Transform>()
        //     .Count(t => t.CompareTag("Collectible") && t != transform);
    }
}