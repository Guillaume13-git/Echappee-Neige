using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestionnaire de chunks procédural.
/// VERSION CORRIGÉE - Appelle AUSSI CollectibleSpawner
/// </summary>
public class ChunkSpawner : MonoBehaviour
{
    private struct ActiveChunk
    {
        public GameObject gameObject;
        public float length;
        public string poolKey;
    }

    [Header("Phase Chunks Prefabs")]
    [SerializeField] private List<GameObject> _tutorialChunks = new List<GameObject>();
    [SerializeField] private List<GameObject> _greenChunks = new List<GameObject>();
    [SerializeField] private List<GameObject> _blueChunks = new List<GameObject>();
    [SerializeField] private List<GameObject> _redChunks = new List<GameObject>();
    [SerializeField] private List<GameObject> _blackChunks = new List<GameObject>();

    [Header("Spawn Settings")]
    [SerializeField] private Transform _chunksParent;
    [SerializeField] private int _visibleChunksCount = 5; 
    [SerializeField] private float _defaultChunkLength = 50f;
    [SerializeField] private float _recycleThresholdZ = -50f;

    [Header("Mode")]
    [SerializeField] private bool _isTutorialMode = false;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;

    private readonly Queue<ActiveChunk> _activeChunks = new Queue<ActiveChunk>();
    private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    
    private float _nextSpawnZ = 0f;
    private int _lastChunkIndex = -1;
    private TrackPhase _currentPhase = TrackPhase.Green;

    private void Start()
    {
        if (!_isTutorialMode && PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }

        SpawnInitialChunks();
    }

    private void OnDestroy()
    {
        if (!_isTutorialMode && PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    private void Update()
    {
        if (_activeChunks.Count == 0) return;
        CheckChunkRecycling();
    }

    private void CheckChunkRecycling()
    {
        ActiveChunk firstChunk = _activeChunks.Peek();
        
        if (firstChunk.gameObject == null)
        {
            _activeChunks.Dequeue();
            return;
        }

        float chunkWorldEndZ = firstChunk.gameObject.transform.position.z + firstChunk.length;

        if (chunkWorldEndZ < _recycleThresholdZ)
        {
            RecycleChunk();
            SpawnNextChunk(false);
        }
    }

    private void SpawnInitialChunks()
    {
        for (int i = 0; i < _visibleChunksCount; i++)
        {
            // Le premier chunk (i=0) est vide pour la sécurité au spawn
            SpawnNextChunk(i == 0); 
        }
    }

    public void SpawnNextChunk(bool isEmpty)
    {
        List<GameObject> availablePrefabs = GetChunksForCurrentPhase();
        if (availablePrefabs == null || availablePrefabs.Count == 0) return;

        int randomIndex = GetRandomIndex(availablePrefabs.Count);
        GameObject prefab = availablePrefabs[randomIndex];

        // 1. Récupération via le Pool
        GameObject chunkObj = GetFromPool(prefab);
        
        // 2. Positionnement LOCAL
        chunkObj.transform.SetParent(_chunksParent);
        chunkObj.transform.localPosition = new Vector3(0f, 0f, _nextSpawnZ);
        chunkObj.transform.localRotation = Quaternion.identity;
        chunkObj.SetActive(true);

        // 3. Récupération de la longueur
        float length = _defaultChunkLength;
        if (chunkObj.TryGetComponent(out ChunkData data))
        {
            length = data.ChunkLength;
        }

        if (!isEmpty)
        {
            // 4. ✅ SPAWN DES OBSTACLES
            ObstacleSpawner obstacleSpawner = chunkObj.GetComponentInChildren<ObstacleSpawner>();
            if (obstacleSpawner != null)
            {
                obstacleSpawner.SpawnObstacles(_currentPhase);
                if (_showDebugLogs)
                {
                    Debug.Log($"[ChunkSpawner] ObstacleSpawner.SpawnObstacles() appelé sur {chunkObj.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[ChunkSpawner] Pas d'ObstacleSpawner sur {chunkObj.name} !");
            }

            // 5. ✅ SPAWN DES COLLECTIBLES (CORRECTION CRITIQUE)
            CollectibleSpawner collectibleSpawner = chunkObj.GetComponentInChildren<CollectibleSpawner>();
            if (collectibleSpawner != null)
            {
                collectibleSpawner.SpawnCollectibles(_currentPhase);
                if (_showDebugLogs)
                {
                    Debug.Log($"[ChunkSpawner] CollectibleSpawner.SpawnCollectibles() appelé sur {chunkObj.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[ChunkSpawner] Pas de CollectibleSpawner sur {chunkObj.name} !");
            }
        }
        else
        {
            if (_showDebugLogs)
            {
                Debug.Log($"[ChunkSpawner] Chunk vide spawné (sécurité spawn joueur)");
            }
        }

        // 6. Enregistrement
        _activeChunks.Enqueue(new ActiveChunk
        {
            gameObject = chunkObj,
            length = length,
            poolKey = prefab.name
        });

        _lastChunkIndex = randomIndex;
        _nextSpawnZ += length;
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        string key = prefab.name;
        if (!_pools.TryGetValue(key, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            _pools[key] = pool;
        }

        if (pool.Count > 0) return pool.Dequeue();

        GameObject newInstance = Instantiate(prefab, _chunksParent);
        newInstance.name = key;
        return newInstance;
    }

    private void RecycleChunk()
    {
        if (_activeChunks.Count == 0) return;
        ActiveChunk oldChunk = _activeChunks.Dequeue();

        if (oldChunk.gameObject != null)
        {
            // ✅ Nettoyage des obstacles ET collectibles avant recyclage
            foreach (Transform child in oldChunk.gameObject.transform)
            {
                if (child.CompareTag("Obstacle") || child.CompareTag("Collectible"))
                {
                    Destroy(child.gameObject);
                }
            }

            oldChunk.gameObject.SetActive(false);
            _pools[oldChunk.poolKey].Enqueue(oldChunk.gameObject);
        }
    }

    private List<GameObject> GetChunksForCurrentPhase()
    {
        if (_isTutorialMode) return _tutorialChunks;

        return _currentPhase switch
        {
            TrackPhase.Green => _greenChunks,
            TrackPhase.Blue => _blueChunks,
            TrackPhase.Red => _redChunks,
            TrackPhase.Black => _blackChunks,
            _ => _greenChunks
        };
    }

    private int GetRandomIndex(int maxCount)
    {
        if (maxCount <= 1) return 0;
        int index;
        int attempts = 0;
        do {
            index = Random.Range(0, maxCount);
            attempts++;
        } while (index == _lastChunkIndex && attempts < 10);
        return index;
    }

    private void OnPhaseChanged(TrackPhase newPhase)
    {
        _currentPhase = newPhase;
        _lastChunkIndex = -1;
    }

    public void ClearAllChunks()
    {
        while (_activeChunks.Count > 0) RecycleChunk();
        _nextSpawnZ = 0f;
        _lastChunkIndex = -1;
        _currentPhase = TrackPhase.Green;
        SpawnInitialChunks();
    }
}