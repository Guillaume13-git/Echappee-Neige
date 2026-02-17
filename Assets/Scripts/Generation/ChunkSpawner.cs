using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Je gère le spawn procédural des chunks (tronçons de piste).
/// J'utilise un système de pooling pour optimiser les performances.
/// </summary>
public class ChunkSpawner : MonoBehaviour
{
    /// <summary>
    /// Je stocke les informations d'un chunk actif dans la scène
    /// </summary>
    private struct ActiveChunk
    {
        public GameObject gameObject;  // Je stocke le GameObject du chunk
        public float length;           // Je stocke la longueur du chunk
        public string poolKey;         // Je stocke la clé pour le pooling
    }

    [Header("Phase Chunks Prefabs")]
    [SerializeField] private List<GameObject> _tutorialChunks = new List<GameObject>();  // Je stocke les chunks du tutoriel
    [SerializeField] private List<GameObject> _greenChunks = new List<GameObject>();     // Je stocke les chunks de phase verte
    [SerializeField] private List<GameObject> _blueChunks = new List<GameObject>();      // Je stocke les chunks de phase bleue
    [SerializeField] private List<GameObject> _redChunks = new List<GameObject>();       // Je stocke les chunks de phase rouge
    [SerializeField] private List<GameObject> _blackChunks = new List<GameObject>();     // Je stocke les chunks de phase noire

    [Header("Spawn Settings")]
    [SerializeField] private Transform _chunksParent;              // Je stocke le parent qui contient tous les chunks
    [SerializeField] private int _visibleChunksCount = 5;          // Je stocke combien de chunks doivent être visibles simultanément
    [SerializeField] private float _defaultChunkLength = 50f;      // Je stocke la longueur par défaut d'un chunk (50 mètres)
    [SerializeField] private float _recycleThresholdZ = -50f;      // Je stocke la position Z après laquelle je recycle un chunk

    [Header("Mode")]
    [SerializeField] private bool _isTutorialMode = false;  // Je stocke si je suis en mode tutoriel

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;  // Je stocke si j'affiche les logs de debug

    // Je stocke la file des chunks actifs dans la scène
    private readonly Queue<ActiveChunk> _activeChunks = new Queue<ActiveChunk>();
    
    // Je stocke les pools de chunks recyclés (par nom de prefab)
    private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    
    private float _nextSpawnZ = 0f;                // Je stocke la position Z du prochain chunk à spawner
    private int _lastChunkIndex = -1;              // Je stocke l'index du dernier chunk spawné (pour éviter les répétitions)
    private TrackPhase _currentPhase = TrackPhase.Green;  // Je stocke la phase actuelle du jeu

    /// <summary>
    /// Je m'initialise au démarrage
    /// </summary>
    private void Start()
    {
        // Si je ne suis pas en mode tutoriel, je m'abonne aux changements de phase
        if (!_isTutorialMode && PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }

        // Je spawne les chunks initiaux
        SpawnInitialChunks();
    }

    /// <summary>
    /// Je me désabonne des événements quand je suis détruit
    /// </summary>
    private void OnDestroy()
    {
        // Je me désabonne des changements de phase
        if (!_isTutorialMode && PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    /// <summary>
    /// Je vérifie à chaque frame si je dois recycler des chunks
    /// </summary>
    private void Update()
    {
        // Si je n'ai aucun chunk actif, je ne fais rien
        if (_activeChunks.Count == 0) return;
        
        // Je vérifie si le premier chunk doit être recyclé
        CheckChunkRecycling();
    }

    /// <summary>
    /// Je vérifie si le premier chunk est hors de vue et doit être recyclé
    /// </summary>
    private void CheckChunkRecycling()
    {
        // Je récupère le premier chunk de la file (sans le retirer)
        ActiveChunk firstChunk = _activeChunks.Peek();
        
        // Si le GameObject a été détruit, je le retire de la file
        if (firstChunk.gameObject == null)
        {
            _activeChunks.Dequeue();
            return;
        }

        // Je calcule la position Z de la fin du chunk
        float chunkWorldEndZ = firstChunk.gameObject.transform.position.z + firstChunk.length;

        // Si le chunk est passé derrière le seuil de recyclage
        if (chunkWorldEndZ < _recycleThresholdZ)
        {
            // Je recycle ce chunk
            RecycleChunk();
            
            // Je spawne un nouveau chunk à la fin
            SpawnNextChunk(false);
        }
    }

    /// <summary>
    /// Je spawne les chunks initiaux au début du jeu
    /// </summary>
    private void SpawnInitialChunks()
    {
        // Je spawne le nombre de chunks visible défini
        for (int i = 0; i < _visibleChunksCount; i++)
        {
            // Le premier chunk (i=0) est vide pour éviter les collisions au spawn
            SpawnNextChunk(i == 0); 
        }
    }

    /// <summary>
    /// Je spawne le prochain chunk
    /// </summary>
    /// <param name="isEmpty">True si le chunk doit être vide (sans obstacles ni collectibles)</param>
    public void SpawnNextChunk(bool isEmpty)
    {
        // Je récupère la liste des chunks disponibles pour la phase actuelle
        List<GameObject> availablePrefabs = GetChunksForCurrentPhase();
        
        // Si je n'ai aucun chunk disponible, je ne fais rien
        if (availablePrefabs == null || availablePrefabs.Count == 0) return;

        // Je choisis un index aléatoire (en évitant de répéter le dernier)
        int randomIndex = GetRandomIndex(availablePrefabs.Count);
        GameObject prefab = availablePrefabs[randomIndex];

        // ---------------------------------------------------------
        // 1. RÉCUPÉRATION VIA LE POOL
        // ---------------------------------------------------------
        
        // Je récupère ou crée un chunk depuis le pool
        GameObject chunkObj = GetFromPool(prefab);
        
        // ---------------------------------------------------------
        // 2. POSITIONNEMENT LOCAL
        // ---------------------------------------------------------
        
        // Je définis le chunk comme enfant du ChunksParent
        chunkObj.transform.SetParent(_chunksParent);
        
        // Je positionne le chunk en LOCAL (par rapport au parent)
        chunkObj.transform.localPosition = new Vector3(0f, 0f, _nextSpawnZ);
        chunkObj.transform.localRotation = Quaternion.identity;
        
        // J'active le chunk
        chunkObj.SetActive(true);

        // ---------------------------------------------------------
        // 3. RÉCUPÉRATION DE LA LONGUEUR
        // ---------------------------------------------------------
        
        // Je récupère la longueur du chunk (ou j'utilise la longueur par défaut)
        float length = _defaultChunkLength;
        if (chunkObj.TryGetComponent(out ChunkData data))
        {
            length = data.ChunkLength;
        }

        // ---------------------------------------------------------
        // 4. SPAWN DES OBSTACLES ET COLLECTIBLES (si pas vide)
        // ---------------------------------------------------------
        
        if (!isEmpty)
        {
            // ✅ SPAWN DES OBSTACLES
            ObstacleSpawner obstacleSpawner = chunkObj.GetComponentInChildren<ObstacleSpawner>();
            if (obstacleSpawner != null)
            {
                // J'appelle le spawner d'obstacles
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

            // ✅ SPAWN DES COLLECTIBLES (CORRECTION CRITIQUE)
            CollectibleSpawner collectibleSpawner = chunkObj.GetComponentInChildren<CollectibleSpawner>();
            if (collectibleSpawner != null)
            {
                // J'appelle le spawner de collectibles
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
            // Le chunk est vide (premier chunk de sécurité)
            if (_showDebugLogs)
            {
                Debug.Log($"[ChunkSpawner] Chunk vide spawné (sécurité spawn joueur)");
            }
        }

        // ---------------------------------------------------------
        // 6. ENREGISTREMENT DU CHUNK ACTIF
        // ---------------------------------------------------------
        
        // J'ajoute ce chunk à la file des chunks actifs
        _activeChunks.Enqueue(new ActiveChunk
        {
            gameObject = chunkObj,
            length = length,
            poolKey = prefab.name
        });

        // Je mémorise l'index de ce chunk pour éviter les répétitions
        _lastChunkIndex = randomIndex;
        
        // J'avance la position de spawn du prochain chunk
        _nextSpawnZ += length;
    }

    /// <summary>
    /// Je récupère un chunk depuis le pool (ou j'en crée un nouveau)
    /// </summary>
    /// <param name="prefab">Le prefab du chunk à récupérer</param>
    /// <returns>Un GameObject du chunk</returns>
    private GameObject GetFromPool(GameObject prefab)
    {
        // J'utilise le nom du prefab comme clé
        string key = prefab.name;
        
        // Je vérifie si un pool existe pour ce prefab
        if (!_pools.TryGetValue(key, out Queue<GameObject> pool))
        {
            // Si le pool n'existe pas, je le crée
            pool = new Queue<GameObject>();
            _pools[key] = pool;
        }

        // Si le pool contient des chunks recyclés, j'en récupère un
        if (pool.Count > 0) 
            return pool.Dequeue();

        // Sinon, je crée une nouvelle instance
        GameObject newInstance = Instantiate(prefab, _chunksParent);
        newInstance.name = key;  // Je garde le nom propre (sans "(Clone)")
        return newInstance;
    }

    /// <summary>
    /// Je recycle le premier chunk (le retire de la scène et le mets en pool)
    /// </summary>
    private void RecycleChunk()
    {
        // Si je n'ai aucun chunk actif, je ne fais rien
        if (_activeChunks.Count == 0) return;
        
        // Je retire le premier chunk de la file
        ActiveChunk oldChunk = _activeChunks.Dequeue();

        // Si le GameObject existe encore
        if (oldChunk.gameObject != null)
        {
            // ---------------------------------------------------------
            // NETTOYAGE DES OBSTACLES ET COLLECTIBLES
            // ---------------------------------------------------------
            
            // Je parcours tous les enfants du chunk
            foreach (Transform child in oldChunk.gameObject.transform)
            {
                // Si c'est un obstacle ou un collectible, je le détruis
                if (child.CompareTag("Obstacle") || child.CompareTag("Collectible"))
                {
                    Destroy(child.gameObject);
                }
            }

            // Je désactive le chunk
            oldChunk.gameObject.SetActive(false);
            
            // Je remets le chunk dans le pool pour réutilisation future
            _pools[oldChunk.poolKey].Enqueue(oldChunk.gameObject);
        }
    }

    /// <summary>
    /// Je retourne la liste des chunks disponibles pour la phase actuelle
    /// </summary>
    /// <returns>Une liste de prefabs de chunks</returns>
    private List<GameObject> GetChunksForCurrentPhase()
    {
        // Si je suis en mode tutoriel, je retourne les chunks du tutoriel
        if (_isTutorialMode) 
            return _tutorialChunks;

        // Sinon, je retourne les chunks selon la phase actuelle
        return _currentPhase switch
        {
            TrackPhase.Green => _greenChunks,
            TrackPhase.Blue => _blueChunks,
            TrackPhase.Red => _redChunks,
            TrackPhase.Black => _blackChunks,
            _ => _greenChunks  // Par défaut : chunks verts
        };
    }

    /// <summary>
    /// Je génère un index aléatoire en évitant de répéter le dernier
    /// </summary>
    /// <param name="maxCount">Le nombre maximum d'éléments</param>
    /// <returns>Un index aléatoire</returns>
    private int GetRandomIndex(int maxCount)
    {
        // S'il n'y a qu'un seul chunk, je retourne 0
        if (maxCount <= 1) return 0;
        
        int index;
        int attempts = 0;
        
        // Je tire un index aléatoire jusqu'à ce qu'il soit différent du dernier
        do 
        {
            index = Random.Range(0, maxCount);
            attempts++;
        } 
        while (index == _lastChunkIndex && attempts < 10);  // Maximum 10 tentatives
        
        return index;
    }

    /// <summary>
    /// Je gère le changement de phase du jeu
    /// </summary>
    /// <param name="newPhase">La nouvelle phase</param>
    private void OnPhaseChanged(TrackPhase newPhase)
    {
        // Je mets à jour ma phase actuelle
        _currentPhase = newPhase;
        
        // Je réinitialise l'index du dernier chunk (pour permettre tous les chunks de la nouvelle phase)
        _lastChunkIndex = -1;
    }

    /// <summary>
    /// Je nettoie tous les chunks actifs et je réinitialise
    /// </summary>
    public void ClearAllChunks()
    {
        // Je recycle tous les chunks actifs
        while (_activeChunks.Count > 0) 
            RecycleChunk();
        
        // Je réinitialise la position de spawn
        _nextSpawnZ = 0f;
        
        // Je réinitialise l'index du dernier chunk
        _lastChunkIndex = -1;
        
        // Je retourne à la phase verte
        _currentPhase = TrackPhase.Green;
        
        // Je respawne les chunks initiaux
        SpawnInitialChunks();
    }
}