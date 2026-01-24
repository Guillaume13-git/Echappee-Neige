using UnityEngine;
using System.Collections.Generic;

public class ChunkSpawner : MonoBehaviour
{
    // Structure interne pour éviter les GetComponent répétitifs dans l'Update
    private struct ActiveChunk
    {
        public GameObject gameObject;
        public float length;
        public string poolKey;
    }

    [Header("Chunk Pools")]
    [SerializeField] private GameObject[] _tutorialChunks;
    [SerializeField] private GameObject[] _greenChunks;
    [SerializeField] private GameObject[] _blueChunks;
    [SerializeField] private GameObject[] _redChunks;
    [SerializeField] private GameObject[] _blackChunks;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform _spawnParent;
    [SerializeField] private int _visibleChunksCount = 5;
    [SerializeField] private float _recycleDistance = -25f;

    [Header("Player Reference")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private bool _isTutorialMode = false;
    
    private readonly Queue<ActiveChunk> _activeChunks = new Queue<ActiveChunk>();
    private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    
    private float _nextSpawnZ = 0f;
    private int _lastChunkIndex = -1;
    private TrackPhase _currentPhase = TrackPhase.Green;

    private void Start()
    {
        if (_playerTransform == null)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) _playerTransform = player.transform;
        }
        
        if (!_isTutorialMode && PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
        
        // Warmup : On pourrait instancier quelques chunks ici pour éviter les lags
        for (int i = 0; i < _visibleChunksCount; i++)
            SpawnNextChunk();
    }

    private void OnDestroy()
    {
        if (!_isTutorialMode && PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }
    
    private void Update()
    {
        if (_activeChunks.Count == 0 || _playerTransform == null) return;

        // Optimisation : On regarde le premier élément sans GetComponent
        ActiveChunk firstChunk = _activeChunks.Peek();

        // On utilise la position Z + la longueur du chunk stockée
        if (firstChunk.gameObject.transform.position.z + firstChunk.length < _playerTransform.position.z + _recycleDistance)
        {
            RecycleChunk();
            SpawnNextChunk();
        }
    }

    private void SpawnNextChunk()
    {
        GameObject[] availableChunks = GetChunksForCurrentPhase();
        if (availableChunks == null || availableChunks.Length == 0) return;
        
        int randomIndex = GetRandomChunkIndex(availableChunks.Length);
        GameObject prefab = availableChunks[randomIndex];
        
        GameObject chunkObj = GetFromPool(prefab);
        chunkObj.transform.position = new Vector3(0, 0, _nextSpawnZ);
        chunkObj.SetActive(true);
        
        // Sécurité : Si ChunkData est oublié sur le prefab
        ChunkData data = chunkObj.GetComponent<ChunkData>();
        float length = (data != null) ? data.ChunkLength : 50f;

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

        GameObject newInstance = Instantiate(prefab, _spawnParent);
        newInstance.name = key; 
        return newInstance;
    }

    private void RecycleChunk()
    {
        ActiveChunk oldChunk = _activeChunks.Dequeue();
        oldChunk.gameObject.SetActive(false);
        
        if (_pools.TryGetValue(oldChunk.poolKey, out Queue<GameObject> pool))
        {
            pool.Enqueue(oldChunk.gameObject);
        }
    }

    private GameObject[] GetChunksForCurrentPhase()
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

    private int GetRandomChunkIndex(int maxCount)
    {
        if (maxCount <= 1) return 0;
        int randomIndex;
        do {
            randomIndex = Random.Range(0, maxCount);
        } while (randomIndex == _lastChunkIndex);
        return randomIndex;
    }

    private void OnPhaseChanged(TrackPhase newPhase)
    {
        _currentPhase = newPhase;
        _lastChunkIndex = -1;
    }
}