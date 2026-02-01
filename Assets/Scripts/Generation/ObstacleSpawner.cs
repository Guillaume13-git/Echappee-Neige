using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Génère les obstacles sur un chunk de manière aléatoire sur les 3 voies.
/// VERSION FINALE - Spawn aléatoire simple sans spawn points
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Configuration Obstacles")]
    [SerializeField] private List<GameObject> _laneObstacles = new List<GameObject>();
    [SerializeField] private List<GameObject> _interLaneObstacles = new List<GameObject>();

    [Header("Spawn Configuration")]
    [SerializeField] private int _minObstaclesPerChunk = 2;
    [SerializeField] private int _maxObstaclesPerChunk = 4;
    [SerializeField] private bool _spawnInterLaneObstacles = true;
    [SerializeField] [Range(0f, 1f)] private float _interLaneSpawnChance = 0.3f;

    [Header("Lane Positions (X)")]
    [SerializeField] private float _leftLaneX = -1.84f;
    [SerializeField] private float _centerLaneX = 0f;
    [SerializeField] private float _rightLaneX = 1.84f;

    [Header("Spawn Range Z")]
    [SerializeField] private float _minZ = 10f;
    [SerializeField] private float _maxZ = 40f;

    [Header("Height")]
    [SerializeField] private float _spawnHeight = 0f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;

    /// <summary>
    /// Appelé par ChunkSpawner pour générer les obstacles
    /// </summary>
    public void SpawnObstacles(TrackPhase phase = TrackPhase.Green)
    {
        if (_laneObstacles.Count == 0)
        {
            Debug.LogWarning($"[ObstacleSpawner] {gameObject.name} : Aucun obstacle configuré !");
            return;
        }

        // Résoudre le parent dynamiquement
        Transform chunkRoot = transform.parent;
        
        if (chunkRoot == null)
        {
            Debug.LogError($"[ObstacleSpawner] {gameObject.name} n'a pas de parent !");
            return;
        }

        // Calculer le nombre d'obstacles selon la phase
        int obstacleCount = CalculateObstacleCount(phase);

        if (_showDebugLogs)
        {
            Debug.Log($"[ObstacleSpawner] Génération de {obstacleCount} obstacles sur {gameObject.name} (Phase: {phase})");
        }

        // Générer les obstacles
        SpawnLaneObstacles(chunkRoot, obstacleCount);
        
        // Générer obstacles inter-voies (optionnel)
        if (_spawnInterLaneObstacles && Random.value < _interLaneSpawnChance)
        {
            SpawnInterLaneObstacle(chunkRoot);
        }
    }

    /// <summary>
    /// Calcule le nombre d'obstacles selon la phase
    /// </summary>
    private int CalculateObstacleCount(TrackPhase phase)
    {
        int baseCount = phase switch
        {
            TrackPhase.Green => Random.Range(_minObstaclesPerChunk, _minObstaclesPerChunk + 1),
            TrackPhase.Blue => Random.Range(_minObstaclesPerChunk, _maxObstaclesPerChunk - 1),
            TrackPhase.Red => Random.Range(_minObstaclesPerChunk + 1, _maxObstaclesPerChunk),
            TrackPhase.Black => Random.Range(_maxObstaclesPerChunk - 1, _maxObstaclesPerChunk + 1),
            _ => _minObstaclesPerChunk
        };

        return Mathf.Clamp(baseCount, 1, 6);
    }

    /// <summary>
    /// Génère plusieurs obstacles sur différentes voies avec positions LOCALES aléatoires
    /// </summary>
    private void SpawnLaneObstacles(Transform chunkRoot, int count)
    {
        // Array des positions X possibles
        float[] lanePositions = { _leftLaneX, _centerLaneX, _rightLaneX };
        List<int> availableLanes = new List<int> { 0, 1, 2 };
        
        for (int i = 0; i < count; i++)
        {
            // Réinitialiser les voies si toutes utilisées
            if (availableLanes.Count == 0)
            {
                availableLanes = new List<int> { 0, 1, 2 };
            }

            // Choisir une voie aléatoire
            int randomLaneIndex = Random.Range(0, availableLanes.Count);
            int selectedLane = availableLanes[randomLaneIndex];
            availableLanes.RemoveAt(randomLaneIndex);

            // Choisir un obstacle aléatoire
            int randomObstacle = Random.Range(0, _laneObstacles.Count);
            GameObject obstaclePrefab = _laneObstacles[randomObstacle];

            if (obstaclePrefab != null)
            {
                // Position LOCALE aléatoire
                float laneX = lanePositions[selectedLane];
                float randomZ = Random.Range(_minZ, _maxZ);
                
                Vector3 localPosition = new Vector3(laneX, _spawnHeight, randomZ);

                // Instancier avec le chunk comme parent
                GameObject obstacle = Instantiate(obstaclePrefab, chunkRoot);
                obstacle.transform.localPosition = localPosition;
                obstacle.transform.localRotation = Quaternion.identity;

                // Forcer le tag
                obstacle.tag = "Obstacle";

                if (_showDebugLogs)
                {
                    Debug.Log($"[ObstacleSpawner] Obstacle {i + 1}/{count} spawné sur voie {selectedLane} " +
                             $"(X={laneX}) à position locale {localPosition}");
                }
            }
        }
    }

    /// <summary>
    /// Génère un obstacle entre deux voies
    /// </summary>
    private void SpawnInterLaneObstacle(Transform chunkRoot)
    {
        if (_interLaneObstacles.Count == 0) return;

        // Positions inter-voies : entre gauche-centre (-0.92) ou centre-droite (0.92)
        float[] interLanePositions = { -0.92f, 0.92f };
        
        int randomObstacle = Random.Range(0, _interLaneObstacles.Count);
        GameObject obstaclePrefab = _interLaneObstacles[randomObstacle];

        if (obstaclePrefab != null)
        {
            float randomX = interLanePositions[Random.Range(0, 2)];
            float randomZ = Random.Range(_minZ, _maxZ);
            
            Vector3 localPosition = new Vector3(randomX, _spawnHeight, randomZ);

            GameObject obstacle = Instantiate(obstaclePrefab, chunkRoot);
            obstacle.transform.localPosition = localPosition;
            obstacle.transform.localRotation = Quaternion.identity;
            obstacle.tag = "Obstacle";

            if (_showDebugLogs)
            {
                Debug.Log($"[ObstacleSpawner] Obstacle inter-voies spawné à position locale {localPosition}");
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualiser les 3 voies dans l'éditeur
        Gizmos.color = Color.red;
        
        Vector3 worldPos = transform.position;
        
        // Voie gauche
        Gizmos.DrawLine(worldPos + new Vector3(_leftLaneX, 0, 0), 
                       worldPos + new Vector3(_leftLaneX, 0, 50));
        
        // Voie centre
        Gizmos.DrawLine(worldPos + new Vector3(_centerLaneX, 0, 0), 
                       worldPos + new Vector3(_centerLaneX, 0, 50));
        
        // Voie droite
        Gizmos.DrawLine(worldPos + new Vector3(_rightLaneX, 0, 0), 
                       worldPos + new Vector3(_rightLaneX, 0, 50));
        
        // Zone de spawn Z
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldPos + new Vector3(0, 0, (_minZ + _maxZ) / 2), 
                           new Vector3(4, 2, _maxZ - _minZ));
    }
#endif
}