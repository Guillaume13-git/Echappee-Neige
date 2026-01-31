using UnityEngine;

/// <summary>
/// Initialise le chunk : génère obstacles et collectibles au spawn.
/// Attaché à la racine de chaque prefab de chunk.
/// </summary>
public class ChunkInitializer : MonoBehaviour
{
    [Header("Spawners")]
    [SerializeField] private ObstacleSpawner _obstacleSpawner;
    [SerializeField] private CollectibleSpawner _collectibleSpawner;
    
    [Header("Auto-Initialize")]
    [SerializeField] private bool _spawnOnEnable = true;
    
    private bool _hasSpawned = false;
    
    private void OnEnable()
    {
        if (_spawnOnEnable && !_hasSpawned)
        {
            InitializeChunk();
        }
    }
    
    /// <summary>
    /// Génère les obstacles et collectibles sur ce chunk.
    /// </summary>
    public void InitializeChunk()
    {
        if (_hasSpawned) return;
        
        TrackPhase currentPhase = TrackPhase.Green;
        
        // Récupérer la phase actuelle
        if (PhaseManager.Instance != null)
        {
            currentPhase = PhaseManager.Instance.CurrentPhase;
        }
        
        // Spawner les obstacles
        if (_obstacleSpawner != null)
        {
            _obstacleSpawner.SpawnObstacles(currentPhase);
        }
        
        // Spawner les collectibles
        if (_collectibleSpawner != null)
        {
            _collectibleSpawner.SpawnCollectibles(currentPhase);
        }
        
        _hasSpawned = true;
    }
    
    /// <summary>
    /// Reset le chunk pour permettre un nouveau spawn (recyclage).
    /// </summary>
    public void ResetChunk()
    {
        // Détruire tous les obstacles et collectibles spawnés
        if (_obstacleSpawner != null)
        {
            foreach (Transform child in _obstacleSpawner.transform)
            {
                // Ne pas détruire les spawn points
                if (child.name.Contains("SpawnPoint")) continue;
                
                Destroy(child.gameObject);
            }
        }
        
        if (_collectibleSpawner != null)
        {
            foreach (Transform child in _collectibleSpawner.transform)
            {
                // Ne pas détruire les spawn points
                if (child.name.Contains("SpawnPoint")) continue;
                
                Destroy(child.gameObject);
            }
        }
        
        _hasSpawned = false;
    }
    
    private void OnDisable()
    {
        // Optionnel : reset quand le chunk est désactivé (recyclage)
        ResetChunk();
    }
}