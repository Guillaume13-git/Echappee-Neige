using UnityEngine;

/// <summary>
/// Initialise le chunk : génère obstacles et collectibles.
/// Attaché à la racine de chaque prefab de chunk.
/// 
/// ✅ CORRECTION CRITIQUE :
/// Résout le problème de timing où les objets spawnaient avec un parent incorrect.
/// </summary>
public class ChunkInitializer : MonoBehaviour
{
    [Header("Spawners")]
    [SerializeField] private ObstacleSpawner _obstacleSpawner;
    [SerializeField] private CollectibleSpawner _collectibleSpawner;

    [Header("Settings")]
    [SerializeField] private bool _spawnOnEnable = true;

    private bool _hasSpawned = false;

    private void OnEnable()
    {
        // ✅ À ce point, le ChunkSpawner a déjà fait le SetParent() et le SetActive(true)
        // Le transform.parent est donc maintenant stabilisé et correct.
        if (_spawnOnEnable && !_hasSpawned)
        {
            InitializeChunk();
        }
    }

    /// <summary>
    /// Déclenche la génération procédurale du contenu du chunk.
    /// </summary>
    public void InitializeChunk()
    {
        if (_hasSpawned) return;

        // Détermination de la phase via le Singleton PhaseManager
        TrackPhase currentPhase = TrackPhase.Green;
        if (PhaseManager.Instance != null)
        {
            currentPhase = PhaseManager.Instance.CurrentPhase;
        }

        // Debug log pour valider la hiérarchie en temps réel dans la console
        Debug.Log($"[ChunkInitializer] Initialisation de {gameObject.name} | Parent: {transform.parent?.name ?? "None"}");

        // Spawn des obstacles
        if (_obstacleSpawner != null)
        {
            _obstacleSpawner.SpawnObstacles(currentPhase);
        }

        // Spawn des collectibles
        if (_collectibleSpawner != null)
        {
            _collectibleSpawner.SpawnCollectibles(currentPhase);
        }

        _hasSpawned = true;
    }

    /// <summary>
    /// Nettoie le chunk pour le recyclage (Object Pooling).
    /// </summary>
    public void ResetChunk()
    {
        // 1. Nettoyage des enfants des spawners (au cas où)
        if (_obstacleSpawner != null) DestroySpawnedChildren(_obstacleSpawner.transform);
        if (_collectibleSpawner != null) DestroySpawnedChildren(_collectibleSpawner.transform);

        // 2. Nettoyage des objets directement attachés au chunk (via les Tags)
        // C'est ici que la majorité des objets seront détruits car ils sont parentés au chunk
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Obstacle") || child.CompareTag("Collectible"))
            {
                Destroy(child.gameObject);
            }
        }

        _hasSpawned = false;
    }

    private void DestroySpawnedChildren(Transform spawnerTransform)
    {
        // Utilisation d'une boucle inverse ou d'un check de nom pour ne pas supprimer les points de spawn
        foreach (Transform child in spawnerTransform)
        {
            if (child.name.Contains("SpawnPoint")) continue;
            Destroy(child.gameObject);
        }
    }

    private void OnDisable()
    {
        // On prépare le chunk pour sa prochaine réutilisation
        ResetChunk();
    }
}