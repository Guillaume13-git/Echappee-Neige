using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Je génère les obstacles sur un chunk de manière intelligente.
/// Je gère différents types d'obstacles : 1 voie, 2 voies, et barrières.
/// Je garantis toujours au moins une voie libre pour le joueur.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacles 1 Voie")]
    [Tooltip("Obstacles qui bloquent UNE SEULE voie")]
    [SerializeField] private List<GameObject> _singleLaneObstacles = new List<GameObject>();

    [Header("Obstacles 2 Voies - Gauche + Centre")]
    [Tooltip("Obstacles qui bloquent les voies GAUCHE et CENTRE (droite libre)")]
    [SerializeField] private List<GameObject> _leftCenterObstacles = new List<GameObject>();

    [Header("Obstacles 2 Voies - Centre + Droite")]
    [Tooltip("Obstacles qui bloquent les voies CENTRE et DROITE (gauche libre)")]
    [SerializeField] private List<GameObject> _centerRightObstacles = new List<GameObject>();

    [Header("Obstacles 2 Voies - Gauche + Droite")]
    [Tooltip("Obstacles qui bloquent les voies GAUCHE et DROITE (centre libre)")]
    [SerializeField] private List<GameObject> _leftRightObstacles = new List<GameObject>();

    [Header("Barrières (se baisser)")]
    [Tooltip("Barrières hautes qui bloquent gauche+droite, le joueur doit se baisser au centre")]
    [SerializeField] private List<GameObject> _barrierObstacles = new List<GameObject>();

    [Header("Obstacles Inter-Voies")]
    [SerializeField] private List<GameObject> _interLaneObstacles = new List<GameObject>();

    [Header("Spawn Configuration")]
    [SerializeField] private int _minObstaclesPerChunk = 1;    // RÉDUIT : moins d'obstacles par chunk
    [SerializeField] private int _maxObstaclesPerChunk = 3;    // RÉDUIT : maximum 3 au lieu de 4
    [SerializeField] private bool _spawnInterLaneObstacles = true;
    [SerializeField] [Range(0f, 1f)] private float _interLaneSpawnChance = 0.2f;  // RÉDUIT : 20% au lieu de 30%
    [SerializeField] [Range(0f, 1f)] private float _barrierSpawnChance = 0.1f;    // RÉDUIT : 10% au lieu de 20%

    [Header("Lane Positions (X)")]
    [SerializeField] private float _leftLaneX = -1.84f;
    [SerializeField] private float _centerLaneX = 0f;
    [SerializeField] private float _rightLaneX = 1.84f;

    [Header("Spawn Range Z")]
    [SerializeField] private float _minZ = 10f;
    [SerializeField] private float _maxZ = 40f;
    [SerializeField] private float _minDistanceBetweenObstacles = 15f;        // AUGMENTÉ : 15m au lieu de 8m
    [SerializeField] private float _minDistanceForInterLane = 20f;            // NOUVEAU : distance spéciale pour inter-voies

    [Header("Height")]
    [SerializeField] private float _spawnHeight = 0f;
    [SerializeField] private float _barrierHeight = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;

    // Je stocke les zones occupées pour éviter les chevauchements
    private List<float> _occupiedZones = new List<float>();

    /// <summary>
    /// Je retourne les zones occupées pour que CollectibleSpawner puisse les éviter
    /// </summary>
    public List<float> GetOccupiedZones()
    {
        return new List<float>(_occupiedZones);
    }

    /// <summary>
    /// Je suis appelé par le ChunkSpawner pour générer les obstacles
    /// </summary>
    public void SpawnObstacles(TrackPhase phase = TrackPhase.Green)
    {
        // Je réinitialise mes zones occupées
        _occupiedZones.Clear();

        // Je récupère le chunk parent
        Transform chunkRoot = transform.parent;
        
        if (chunkRoot == null)
        {
            Debug.LogError($"[ObstacleSpawner] {gameObject.name} n'a pas de parent !");
            return;
        }

        // Je calcule le nombre d'obstacles selon la phase
        int obstacleCount = CalculateObstacleCount(phase);

        if (_showDebugLogs)
        {
            Debug.Log($"[ObstacleSpawner] Génération de {obstacleCount} obstacles sur {gameObject.name} (Phase: {phase})");
        }

        // Je génère les obstacles principaux
        SpawnMainObstacles(chunkRoot, obstacleCount);
        
        // Je génère éventuellement un obstacle inter-voies SEULEMENT s'il y a assez de place
        if (_spawnInterLaneObstacles && Random.value < _interLaneSpawnChance && _occupiedZones.Count < 2)
        {
            SpawnInterLaneObstacle(chunkRoot);
        }
    }

    /// <summary>
    /// Je calcule le nombre d'obstacles selon la phase
    /// MODIFIÉ : Réduit les quantités pour donner plus de temps de réaction
    /// </summary>
    private int CalculateObstacleCount(TrackPhase phase)
    {
        int baseCount = phase switch
        {
            TrackPhase.Green => 1,                                              // RÉDUIT : toujours 1 obstacle en phase verte
            TrackPhase.Blue => Random.Range(1, 2),                              // RÉDUIT : 1 ou 2 obstacles
            TrackPhase.Red => Random.Range(2, 3),                               // RÉDUIT : 2 ou 3 obstacles
            TrackPhase.Black => Random.Range(2, 4),                             // RÉDUIT : 2 à 3 obstacles max
            _ => 1
        };

        return Mathf.Clamp(baseCount, 1, 3);  // RÉDUIT : maximum 3 obstacles
    }

    /// <summary>
    /// Je génère les obstacles principaux avec une logique intelligente
    /// MODIFIÉ : Plus d'espace entre chaque obstacle
    /// </summary>
    private void SpawnMainObstacles(Transform chunkRoot, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Je trouve une position Z valide (sans chevauchement)
            float spawnZ = FindValidZPosition();
            
            if (spawnZ < 0)
            {
                if (_showDebugLogs)
                {
                    Debug.LogWarning($"[ObstacleSpawner] Impossible de trouver une position Z valide pour l'obstacle {i + 1}");
                }
                continue;
            }

            // Je décide du type d'obstacle à spawner
            ObstacleType type = ChooseObstacleType();
            
            // Je spawne l'obstacle selon son type
            SpawnObstacleByType(chunkRoot, type, spawnZ);
            
            // Je marque cette zone comme occupée
            _occupiedZones.Add(spawnZ);
        }
    }

    /// <summary>
    /// Je trouve une position Z valide qui ne chevauche pas les autres obstacles
    /// MODIFIÉ : Plus de tentatives et meilleure vérification
    /// </summary>
    private float FindValidZPosition()
    {
        int attempts = 0;
        float candidateZ;

        do
        {
            candidateZ = Random.Range(_minZ, _maxZ);
            attempts++;

            if (attempts > 30)  // AUGMENTÉ : 30 tentatives au lieu de 20
            {
                return -1f;
            }

            // Je vérifie si cette position est assez loin des obstacles existants
            bool isValid = true;
            foreach (float occupiedZ in _occupiedZones)
            {
                if (Mathf.Abs(candidateZ - occupiedZ) < _minDistanceBetweenObstacles)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                return candidateZ;
            }

        } while (true);
    }

    /// <summary>
    /// Je trouve une position Z valide pour un obstacle inter-voies
    /// NOUVEAU : Distance encore plus grande pour les inter-voies
    /// </summary>
    private float FindValidZPositionForInterLane()
    {
        int attempts = 0;
        float candidateZ;

        do
        {
            candidateZ = Random.Range(_minZ, _maxZ);
            attempts++;

            if (attempts > 30)
            {
                return -1f;
            }

            // Je vérifie avec une PLUS GRANDE distance pour les inter-voies
            bool isValid = true;
            foreach (float occupiedZ in _occupiedZones)
            {
                if (Mathf.Abs(candidateZ - occupiedZ) < _minDistanceForInterLane)  // Distance plus grande !
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                return candidateZ;
            }

        } while (true);
    }

    /// <summary>
    /// Je choisis le type d'obstacle à spawner
    /// </summary>
    private ObstacleType ChooseObstacleType()
    {
        // Je vérifie d'abord si je peux spawner une barrière
        if (_barrierObstacles.Count > 0 && Random.value < _barrierSpawnChance)
        {
            return ObstacleType.Barrier;
        }

        // Je crée une liste des types disponibles
        List<ObstacleType> availableTypes = new List<ObstacleType>();

        if (_singleLaneObstacles.Count > 0)
            availableTypes.Add(ObstacleType.SingleLane);

        if (_leftCenterObstacles.Count > 0)
            availableTypes.Add(ObstacleType.LeftCenter);

        if (_centerRightObstacles.Count > 0)
            availableTypes.Add(ObstacleType.CenterRight);

        if (_leftRightObstacles.Count > 0)
            availableTypes.Add(ObstacleType.LeftRight);

        if (availableTypes.Count == 0)
        {
            Debug.LogWarning("[ObstacleSpawner] Aucun obstacle configuré !");
            return ObstacleType.SingleLane;
        }

        return availableTypes[Random.Range(0, availableTypes.Count)];
    }

    /// <summary>
    /// Je spawne un obstacle selon son type
    /// </summary>
    private void SpawnObstacleByType(Transform chunkRoot, ObstacleType type, float spawnZ)
    {
        GameObject prefab = null;
        Vector3[] positions = null;
        float height = _spawnHeight;

        switch (type)
        {
            case ObstacleType.SingleLane:
                if (_singleLaneObstacles.Count > 0)
                {
                    prefab = _singleLaneObstacles[Random.Range(0, _singleLaneObstacles.Count)];
                    float[] lanes = { _leftLaneX, _centerLaneX, _rightLaneX };
                    positions = new Vector3[] { new Vector3(lanes[Random.Range(0, 3)], height, spawnZ) };
                }
                break;

            case ObstacleType.LeftCenter:
                if (_leftCenterObstacles.Count > 0)
                {
                    prefab = _leftCenterObstacles[Random.Range(0, _leftCenterObstacles.Count)];
                    positions = new Vector3[] 
                    { 
                        new Vector3(_leftLaneX, height, spawnZ),
                        new Vector3(_centerLaneX, height, spawnZ)
                    };
                }
                break;

            case ObstacleType.CenterRight:
                if (_centerRightObstacles.Count > 0)
                {
                    prefab = _centerRightObstacles[Random.Range(0, _centerRightObstacles.Count)];
                    positions = new Vector3[] 
                    { 
                        new Vector3(_centerLaneX, height, spawnZ),
                        new Vector3(_rightLaneX, height, spawnZ)
                    };
                }
                break;

            case ObstacleType.LeftRight:
                if (_leftRightObstacles.Count > 0)
                {
                    prefab = _leftRightObstacles[Random.Range(0, _leftRightObstacles.Count)];
                    positions = new Vector3[] 
                    { 
                        new Vector3(_leftLaneX, height, spawnZ),
                        new Vector3(_rightLaneX, height, spawnZ)
                    };
                }
                break;

            case ObstacleType.Barrier:
                if (_barrierObstacles.Count > 0)
                {
                    prefab = _barrierObstacles[Random.Range(0, _barrierObstacles.Count)];
                    height = _barrierHeight;
                    positions = new Vector3[] 
                    { 
                        new Vector3(_leftLaneX, height, spawnZ),
                        new Vector3(_rightLaneX, height, spawnZ)
                    };
                }
                break;
        }

        if (prefab != null && positions != null)
        {
            foreach (Vector3 pos in positions)
            {
                GameObject obstacle = Instantiate(prefab, chunkRoot);
                obstacle.transform.localPosition = pos;
                obstacle.transform.localRotation = Quaternion.identity;
                obstacle.tag = "Obstacle";

                if (_showDebugLogs)
                {
                    Debug.Log($"[ObstacleSpawner] {type} spawné à {pos}");
                }
            }
        }
    }

    /// <summary>
    /// Je génère un obstacle entre deux voies
    /// MODIFIÉ : Utilise une distance plus grande et vérifie mieux
    /// </summary>
    private void SpawnInterLaneObstacle(Transform chunkRoot)
    {
        if (_interLaneObstacles.Count == 0) return;

        // J'utilise la méthode spéciale pour inter-voies avec plus de distance
        float spawnZ = FindValidZPositionForInterLane();
        if (spawnZ < 0) 
        {
            if (_showDebugLogs)
            {
                Debug.Log("[ObstacleSpawner] Pas assez de place pour un obstacle inter-voies");
            }
            return;
        }

        float[] interLanePositions = { -0.92f, 0.92f };
        GameObject prefab = _interLaneObstacles[Random.Range(0, _interLaneObstacles.Count)];

        if (prefab != null)
        {
            float randomX = interLanePositions[Random.Range(0, 2)];
            Vector3 localPosition = new Vector3(randomX, _spawnHeight, spawnZ);

            GameObject obstacle = Instantiate(prefab, chunkRoot);
            obstacle.transform.localPosition = localPosition;
            obstacle.transform.localRotation = Quaternion.identity;
            obstacle.tag = "Obstacle";

            _occupiedZones.Add(spawnZ);

            if (_showDebugLogs)
            {
                Debug.Log($"[ObstacleSpawner] Obstacle inter-voies spawné à {localPosition}");
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 worldPos = transform.position;
        
        Gizmos.DrawLine(worldPos + new Vector3(_leftLaneX, 0, 0), 
                       worldPos + new Vector3(_leftLaneX, 0, 50));
        Gizmos.DrawLine(worldPos + new Vector3(_centerLaneX, 0, 0), 
                       worldPos + new Vector3(_centerLaneX, 0, 50));
        Gizmos.DrawLine(worldPos + new Vector3(_rightLaneX, 0, 0), 
                       worldPos + new Vector3(_rightLaneX, 0, 50));
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldPos + new Vector3(0, 0, (_minZ + _maxZ) / 2), 
                           new Vector3(4, 2, _maxZ - _minZ));
        
        // NOUVEAU : Je dessine la zone de distance minimale en vert
        Gizmos.color = Color.green;
        foreach (float z in _occupiedZones)
        {
            Gizmos.DrawWireSphere(worldPos + new Vector3(0, 1, z), _minDistanceBetweenObstacles / 2);
        }
    }
#endif
}

/// <summary>
/// Les différents types d'obstacles que je peux spawner
/// </summary>
public enum ObstacleType
{
    SingleLane,    // Bloque 1 voie
    LeftCenter,    // Bloque gauche + centre
    CenterRight,   // Bloque centre + droite
    LeftRight,     // Bloque gauche + droite
    Barrier        // Barrière haute (se baisser)
}