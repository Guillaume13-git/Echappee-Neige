using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestionnaire de chunks procédural mis à jour pour le système de mouvement relatif (Z=0).
/// Recycle les chunks lorsqu'ils dépassent une limite négative derrière le joueur.
/// </summary>
public class ChunkSpawner : MonoBehaviour
{
    private struct ActiveChunk
    {
        public GameObject gameObject;
        public float length;
        public string poolKey;
        public TrackPhase phase;
    }

    [Header("Tutorial Chunks")]
    [SerializeField] private List<GameObject> _tutorialChunks = new List<GameObject>();
    
    [Header("Phase Chunks")]
    [SerializeField] private List<GameObject> _greenChunks = new List<GameObject>();
    [SerializeField] private List<GameObject> _blueChunks = new List<GameObject>();
    [SerializeField] private List<GameObject> _redChunks = new List<GameObject>();
    [SerializeField] private List<GameObject> _blackChunks = new List<GameObject>();

    [Header("Spawn Settings")]
    [SerializeField] private Transform _spawnParent; 
    [SerializeField] private int _visibleChunksCount = 5; 
    [SerializeField] private float _defaultChunkLength = 50f;
    [Tooltip("Position Z en dessous de laquelle le chunk est recyclé (ex: -50)")]
    [SerializeField] private float _recycleThresholdZ = -50f; 

    [Header("Player Reference")]
    [SerializeField] private Transform _playerTransform;

    [Header("Mode")]
    [SerializeField] private bool _isTutorialMode = false;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;

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

        // On vérifie le recyclage par rapport à la position World (Z fixe)
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

        // ⭐ LOGIQUE MISE À JOUR :
        // On calcule la position de FIN du chunk dans le monde.
        // Puisque le décor recule, cette valeur diminue.
        float chunkWorldEndZ = firstChunk.gameObject.transform.position.z + firstChunk.length;

        // Si la fin du chunk est passée derrière le seuil (ex: -50)
        if (chunkWorldEndZ < _recycleThresholdZ)
        {
            if (_showDebugLogs)
                Debug.Log($"[ChunkSpawner] ♻️ Recyclage de {firstChunk.gameObject.name} (Fin à {chunkWorldEndZ:F1})");
            
            RecycleChunk();
            SpawnNextChunk();
        }
    }

    private void SpawnInitialChunks()
    {
        for (int i = 0; i < _visibleChunksCount; i++)
        {
            SpawnNextChunk();
        }
    }

    private void SpawnNextChunk()
    {
        List<GameObject> availablePrefabs = GetChunksForCurrentPhase();
        if (availablePrefabs == null || availablePrefabs.Count == 0) return;

        int randomIndex = GetRandomIndex(availablePrefabs.Count);
        GameObject prefab = availablePrefabs[randomIndex];

        // Récupération et positionnement
        GameObject chunkObj = GetFromPool(prefab);
        
        // ⭐ POSITIONNEMENT : 
        // On le place à _nextSpawnZ qui est relatif au parent qui bouge (ChunkMover)
        chunkObj.transform.SetParent(_spawnParent);
        chunkObj.transform.localPosition = new Vector3(0f, 0f, _nextSpawnZ);
        chunkObj.transform.localRotation = Quaternion.identity;
        chunkObj.SetActive(true);

        float length = _defaultChunkLength;
        if (chunkObj.TryGetComponent(out ChunkData data))
        {
            length = data.ChunkLength;
        }

        _activeChunks.Enqueue(new ActiveChunk
        {
            gameObject = chunkObj,
            length = length,
            poolKey = prefab.name,
            phase = _currentPhase
        });

        _lastChunkIndex = randomIndex;
        _nextSpawnZ += length; // On prépare la position du prochain chunk
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
        if (_activeChunks.Count == 0) return;
        ActiveChunk oldChunk = _activeChunks.Dequeue();

        if (oldChunk.gameObject != null)
        {
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