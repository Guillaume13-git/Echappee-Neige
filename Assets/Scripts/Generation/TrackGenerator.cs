using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Générateur procédural de piste.
/// Gère l'instanciation, le positionnement et la destruction des tronçons (chunks).
/// Respecte les contraintes : 50m par tronçon, 3 couloirs, toujours 2 chunks visibles.
/// </summary>
public class TrackGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    [SerializeField] private float _chunkLength = 50f;
    [SerializeField] private int _visibleChunksCount = 2; // Nombre de chunks visibles
    
    [Header("Chunk Prefabs by Phase")]
    [SerializeField] private GameObject[] _greenChunks;
    [SerializeField] private GameObject[] _blueChunks;
    [SerializeField] private GameObject[] _redChunks;
    [SerializeField] private GameObject[] _blackChunks;
    
    [Header("Player Reference")]
    [SerializeField] private Transform _playerTransform;
    
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 _spawnPosition = Vector3.zero;
    
    // État de génération
    private Queue<GameObject> _activeChunks = new Queue<GameObject>();
    private float _nextSpawnZ = 0f;
    private int _lastChunkIndex = -1; // Pour éviter les doublons
    private TrackPhase _currentPhase = TrackPhase.Green;
    
    // Pool d'objets (optionnel mais recommandé)
    private Dictionary<TrackPhase, Queue<GameObject>> _chunkPools = new Dictionary<TrackPhase, Queue<GameObject>>();
    
    private void Start()
    {
        // S'abonner aux changements de phase
        PhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
        
        // Initialiser les pools
        InitializePools();
        
        // Générer les chunks initiaux
        for (int i = 0; i < _visibleChunksCount; i++)
        {
            SpawnNextChunk();
        }
    }
    
    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }
    
    private void Update()
    {
        CheckChunkSpawning();
    }
    
    /// <summary>
    /// Initialise les pools d'objets pour chaque phase.
    /// </summary>
    private void InitializePools()
    {
        _chunkPools[TrackPhase.Green] = new Queue<GameObject>();
        _chunkPools[TrackPhase.Blue] = new Queue<GameObject>();
        _chunkPools[TrackPhase.Red] = new Queue<GameObject>();
        _chunkPools[TrackPhase.Black] = new Queue<GameObject>();
    }
    
    /// <summary>
    /// Vérifie si le joueur a avancé suffisamment pour générer un nouveau chunk.
    /// </summary>
    private void CheckChunkSpawning()
    {
        if (_playerTransform == null) return;
        
        // Quand le joueur atteint le prochain point de spawn, on génère
        if (_playerTransform.position.z >= _nextSpawnZ - (_chunkLength * _visibleChunksCount))
        {
            SpawnNextChunk();
            DestroyOldestChunk();
        }
    }
    
    /// <summary>
    /// Génère le prochain tronçon de piste.
    /// </summary>
    private void SpawnNextChunk()
    {
        GameObject[] currentPhasePrefabs = GetChunkPrefabsForCurrentPhase();
        
        if (currentPhasePrefabs == null || currentPhasePrefabs.Length == 0)
        {
            Debug.LogError($"[TrackGenerator] Aucun préfab disponible pour la phase {_currentPhase}");
            return;
        }
        
        // Sélection aléatoire SANS répétition
        int randomIndex = GetRandomChunkIndex(currentPhasePrefabs.Length);
        
        // Instanciation (ou récupération depuis le pool)
        GameObject newChunk = GetChunkFromPool(_currentPhase, currentPhasePrefabs[randomIndex]);
        
        // Positionnement
        newChunk.transform.position = new Vector3(0f, 0f, _nextSpawnZ);
        newChunk.SetActive(true);
        
        // Enregistrement
        _activeChunks.Enqueue(newChunk);
        _lastChunkIndex = randomIndex;
        _nextSpawnZ += _chunkLength;
        
        // Spawn des obstacles et collectibles sur ce chunk
        SpawnObstaclesAndCollectibles(newChunk);
        
        Debug.Log($"[TrackGenerator] Chunk spawné : Phase {_currentPhase}, Index {randomIndex}, Position Z {newChunk.transform.position.z}");
    }
    
    /// <summary>
    /// Récupère un index aléatoire DIFFÉRENT du précédent.
    /// </summary>
    private int GetRandomChunkIndex(int maxCount)
    {
        if (maxCount == 1) return 0;
        
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, maxCount);
        } while (randomIndex == _lastChunkIndex);
        
        return randomIndex;
    }
    
    /// <summary>
    /// Retourne les préfabs correspondant à la phase actuelle.
    /// </summary>
    private GameObject[] GetChunkPrefabsForCurrentPhase()
    {
        switch (_currentPhase)
        {
            case TrackPhase.Green: return _greenChunks;
            case TrackPhase.Blue: return _blueChunks;
            case TrackPhase.Red: return _redChunks;
            case TrackPhase.Black: return _blackChunks;
            default: return null;
        }
    }
    
    /// <summary>
    /// Récupère un chunk depuis le pool ou l'instancie si le pool est vide.
    /// </summary>
    private GameObject GetChunkFromPool(TrackPhase phase, GameObject prefab)
    {
        Queue<GameObject> pool = _chunkPools[phase];
        
        if (pool.Count > 0)
        {
            GameObject chunk = pool.Dequeue();
            chunk.SetActive(true);
            return chunk;
        }
        
        // Instanciation si le pool est vide
        return Instantiate(prefab);
    }
    
    /// <summary>
    /// Détruit (ou recycle) le chunk le plus ancien.
    /// </summary>
    private void DestroyOldestChunk()
    {
        if (_activeChunks.Count <= _visibleChunksCount) return;
        
        GameObject oldChunk = _activeChunks.Dequeue();
        
        // Désactiver et remettre dans le pool
        oldChunk.SetActive(false);
        _chunkPools[_currentPhase].Enqueue(oldChunk);
        
        Debug.Log($"[TrackGenerator] Chunk recyclé : {oldChunk.name}");
    }
    
    /// <summary>
    /// Spawn les obstacles et collectibles sur un chunk.
    /// Délégué aux spawners spécialisés.
    /// </summary>
    private void SpawnObstaclesAndCollectibles(GameObject chunk)
    {
        ObstacleSpawner obstacleSpawner = chunk.GetComponentInChildren<ObstacleSpawner>();
        if (obstacleSpawner != null)
        {
            obstacleSpawner.SpawnObstacles(_currentPhase);
        }
        
        CollectibleSpawner collectibleSpawner = chunk.GetComponentInChildren<CollectibleSpawner>();
        if (collectibleSpawner != null)
        {
            collectibleSpawner.SpawnCollectibles(_currentPhase);
        }
    }
    
    /// <summary>
    /// Callback appelé lors d'un changement de phase.
    /// Réinitialise le dernier index pour éviter les conflits entre phases.
    /// </summary>
    private void OnPhaseChanged(TrackPhase newPhase)
    {
        _currentPhase = newPhase;
        _lastChunkIndex = -1; // Reset pour permettre n'importe quel chunk de la nouvelle phase
        
        Debug.Log($"[TrackGenerator] Phase changée : {newPhase}");
    }
    
    /// <summary>
    /// Réinitialise complètement le générateur (nouvelle partie).
    /// </summary>
    public void ResetGenerator()
    {
        // Nettoyer tous les chunks actifs
        foreach (GameObject chunk in _activeChunks)
        {
            Destroy(chunk);
        }
        _activeChunks.Clear();
        
        // Nettoyer les pools
        foreach (var pool in _chunkPools.Values)
        {
            foreach (GameObject chunk in pool)
            {
                Destroy(chunk);
            }
            pool.Clear();
        }
        
        // Réinitialiser l'état
        _nextSpawnZ = 0f;
        _lastChunkIndex = -1;
        _currentPhase = TrackPhase.Green;
        
        // Regénérer les chunks initiaux
        for (int i = 0; i < _visibleChunksCount; i++)
        {
            SpawnNextChunk();
        }
    }
}