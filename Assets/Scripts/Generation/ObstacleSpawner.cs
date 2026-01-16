using UnityEngine;

/// <summary>
/// Spawne les obstacles sur un tronçon de piste.
/// Gère la fréquence, le timing et le positionnement selon la phase.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject[] _lanObstacles; // Sapins, arches, barrières
    [SerializeField] private GameObject[] _interLaneObstacles; // Obstacles entre couloirs
    
    [Header("Spawn Positions")]
    [SerializeField] private Transform[] _laneSpawnPoints; // 3 positions (couloir 1, 2, 3)
    [SerializeField] private Transform[] _interLaneSpawnPoints; // 2 positions (entre 1-2, 2-3)
    
    [Header("Spawn Settings")]
    [SerializeField] private float _baseSpawnDistance = 170f;
    [SerializeField] private float _minSpawnDistance = 30f;
    [SerializeField] private float _distanceReductionPerPhase = 15f;
    
    private float _currentSpawnDistance;
    private int _completedObjectives = 0; // Synchronisé avec ScoreManager
    
    private void Awake()
    {
        _currentSpawnDistance = _baseSpawnDistance;
    }
    
    /// <summary>
    /// Génère les obstacles pour ce chunk selon la phase actuelle.
    /// </summary>
    public void SpawnObstacles(TrackPhase phase)
    {
        // Nombre d'obstacles augmente avec la difficulté
        int obstacleCount = GetObstacleCountForPhase(phase);
        
        for (int i = 0; i < obstacleCount; i++)
        {
            SpawnRandomLaneObstacle();
        }
        
        // Obstacles inter-couloirs (moins fréquents)
        if (Random.value > 0.5f) // 50% de chance
        {
            SpawnRandomInterLaneObstacle();
        }
    }
    
    /// <summary>
    /// Retourne le nombre d'obstacles selon la phase.
    /// </summary>
    private int GetObstacleCountForPhase(TrackPhase phase)
    {
        switch (phase)
        {
            case TrackPhase.Green: return Random.Range(1, 3);
            case TrackPhase.Blue: return Random.Range(2, 4);
            case TrackPhase.Red: return Random.Range(3, 6);
            case TrackPhase.Black: return Random.Range(5, 8);
            default: return 1;
        }
    }
    
    /// <summary>
    /// Génère un obstacle aléatoire sur un couloir.
    /// </summary>
    private void SpawnRandomLaneObstacle()
    {
        if (_lanObstacles.Length == 0 || _laneSpawnPoints.Length == 0) return;
        
        // Sélection aléatoire
        GameObject obstaclePrefab = _lanObstacles[Random.Range(0, _lanObstacles.Length)];
        Transform spawnPoint = _laneSpawnPoints[Random.Range(0, _laneSpawnPoints.Length)];
        
        // Position aléatoire en Z sur le chunk (entre 0 et 50m)
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.z += Random.Range(5f, 45f);
        
        // Instanciation
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, transform);
        obstacle.tag = "Obstacle";
    }
    
    /// <summary>
    /// Génère un obstacle entre deux couloirs.
    /// </summary>
    private void SpawnRandomInterLaneObstacle()
    {
        if (_interLaneObstacles.Length == 0 || _interLaneSpawnPoints.Length == 0) return;
        
        GameObject obstaclePrefab = _interLaneObstacles[Random.Range(0, _interLaneObstacles.Length)];
        Transform spawnPoint = _interLaneSpawnPoints[Random.Range(0, _interLaneSpawnPoints.Length)];
        
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.z += Random.Range(10f, 40f);
        
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, transform);
        obstacle.tag = "Obstacle";
    }
    
    /// <summary>
    /// Met à jour la distance de spawn après validation d'un objectif.
    /// (Appelé par un événement du ScoreManager ou PhaseManager)
    /// </summary>
    public void OnObjectiveCompleted()
    {
        _completedObjectives++;
        
        // Réduction de la distance (augmentation de la difficulté)
        _currentSpawnDistance = Mathf.Max(
            _minSpawnDistance, 
            _baseSpawnDistance - (_completedObjectives * _distanceReductionPerPhase)
        );
        
        Debug.Log($"[ObstacleSpawner] Distance de spawn réduite à {_currentSpawnDistance}m");
    }
}