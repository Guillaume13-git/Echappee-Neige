using UnityEngine;
using System.Linq;

/// <summary>
/// Composant attaché à chaque préfab de chunk.
/// Contient les métadonnées pour la sélection et le placement.
/// </summary>
public class ChunkData : MonoBehaviour
{
    [Header("Chunk Properties")]
    [SerializeField] private string _chunkName = "Chunk_01";
    [SerializeField] private float _chunkLength = 50f;
    [SerializeField] private TrackPhase _recommendedPhase = TrackPhase.Green;
    [SerializeField] private ChunkType _chunkType = ChunkType.Normal;
    [SerializeField] private ChunkDifficulty _difficulty = ChunkDifficulty.Easy;
    
    [Header("Content Info (for debugging)")]
    [SerializeField] private int _obstacleCount = 0;
    [SerializeField] private int _collectibleCount = 0;
    
    // Propriétés publiques
    public string ChunkName => _chunkName;
    public float ChunkLength => _chunkLength;
    public TrackPhase RecommendedPhase => _recommendedPhase;
    public ChunkType Type => _chunkType;
    public ChunkDifficulty Difficulty => _difficulty;
    
    /// <summary>
    /// Appelé automatiquement quand tu modifies le prefab dans l'éditeur.
    /// </summary>
    private void OnValidate()
    {
        // On vérifie si on n'est pas en train de jouer pour éviter des calculs inutiles
        if (Application.isPlaying) return;

        // .Count() nécessite 'using System.Linq;'
        _obstacleCount = GetComponentsInChildren<Collider>()
            .Count(c => c.CompareTag("Obstacle"));
            
        // Pour les collectibles, GetLength(0) ou .Length fonctionne sur un tableau
        var collectibles = GetComponentsInChildren<CollectibleBase>();
        _collectibleCount = collectibles != null ? collectibles.Length : 0;
    }
}