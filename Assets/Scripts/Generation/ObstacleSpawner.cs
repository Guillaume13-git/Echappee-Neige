using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Je génère des obstacles sur chaque chunk avec une GARANTIE ABSOLUE qu'au moins une voie reste libre.
/// VERSION CORRIGÉE : Espacement augmenté + vérification stricte qu'une voie est toujours libre.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacles 1 Voie")]
    [SerializeField] private List<GameObject> _singleLaneObstacles = new List<GameObject>();

    [Header("Obstacles 2 Voies - Gauche + Centre")]
    [SerializeField] private List<GameObject> _leftCenterObstacles = new List<GameObject>();

    [Header("Obstacles 2 Voies - Centre + Droite")]
    [SerializeField] private List<GameObject> _centerRightObstacles = new List<GameObject>();

    [Header("Obstacles 2 Voies - Gauche + Droite")]
    [SerializeField] private List<GameObject> _leftRightObstacles = new List<GameObject>();

    [Header("Barrières (se baisser)")]
    [SerializeField] private List<GameObject> _barrierObstacles = new List<GameObject>();

    [Header("Obstacles Inter-Voies")]
    [SerializeField] private List<GameObject> _interLaneObstacles = new List<GameObject>();

    [Header("Spawn Configuration")]
    [SerializeField] private int _minObstaclesPerChunk = 1;
    [SerializeField] private int _maxObstaclesPerChunk = 2; // ✅ RÉDUIT de 3 à 2
    [SerializeField] private bool _spawnInterLaneObstacles = false; // ✅ DÉSACTIVÉ par défaut
    [SerializeField] [Range(0f, 1f)] private float _interLaneSpawnChance = 0.1f; // ✅ RÉDUIT de 0.2 à 0.1
    [SerializeField] [Range(0f, 1f)] private float _barrierSpawnChance = 0.05f; // ✅ RÉDUIT de 0.1 à 0.05

    [Header("Lane Positions (X)")]
    [SerializeField] private float _leftLaneX = -1.84f;
    [SerializeField] private float _centerLaneX = 0f;
    [SerializeField] private float _rightLaneX = 1.84f;

    [Header("Spawn Range Z")]
    [SerializeField] private float _minZ = 10f;
    [SerializeField] private float _maxZ = 40f;
    [SerializeField] private float _minDistanceBetweenObstacles = 25f; // ✅ AUGMENTÉ de 15 à 25
    [SerializeField] private float _minDistanceForInterLane = 30f;     // ✅ AUGMENTÉ de 20 à 30

    [Header("Height")]
    [SerializeField] private float _spawnHeight = 0f;
    [SerializeField] private float _barrierHeight = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true; // ✅ ACTIVÉ par défaut pour diagnostic

    private List<float> _occupiedZones = new List<float>();
    
    // ✅ NOUVEAU : Je garde en mémoire quelle voie était libre au dernier obstacle
    // pour m'assurer que je ne bloque pas toujours la même
    private int _lastFreeLane = -1;

    public List<float> GetOccupiedZones()
    {
        return new List<float>(_occupiedZones);
    }

    public void SpawnObstacles(TrackPhase phase = TrackPhase.Green)
    {
        _occupiedZones.Clear();
        _lastFreeLane = -1; // ✅ Je réinitialise

        Transform chunkRoot = transform.parent;
        
        if (chunkRoot == null)
        {
            Debug.LogError($"[ObstacleSpawner] {gameObject.name} n'a pas de parent !");
            return;
        }

        int obstacleCount = CalculateObstacleCount(phase);

        if (_showDebugLogs)
        {
            Debug.Log("========================================");
            Debug.Log($"[ObstacleSpawner] 🎯 Génération de {obstacleCount} obstacles (Phase: {phase})");
            Debug.Log($"[ObstacleSpawner] Distance minimale : {_minDistanceBetweenObstacles}m");
        }

        SpawnMainObstacles(chunkRoot, obstacleCount);
        
        // ✅ Inter-voies DÉSACTIVÉ pour éviter de bloquer les passages
        if (_spawnInterLaneObstacles && Random.value < _interLaneSpawnChance && _occupiedZones.Count < 2)
        {
            SpawnInterLaneObstacle(chunkRoot);
        }
        
        if (_showDebugLogs)
        {
            Debug.Log($"[ObstacleSpawner] ✓ {_occupiedZones.Count} obstacles générés");
            Debug.Log("========================================");
        }
    }

    private int CalculateObstacleCount(TrackPhase phase)
    {
        int baseCount = phase switch
        {
            TrackPhase.Green => 1,                                           // ✅ Phase verte : 1 seul
            TrackPhase.Blue => Random.Range(1, 2),                          // ✅ Phase bleue : 1-2
            TrackPhase.Red => Random.Range(1, 3),                           // ✅ Phase rouge : 1-3
            TrackPhase.Black => Random.Range(2, 3),                         // ✅ Phase noire : 2-3
            _ => 1
        };

        return Mathf.Clamp(baseCount, _minObstaclesPerChunk, _maxObstaclesPerChunk);
    }

    private void SpawnMainObstacles(Transform chunkRoot, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float spawnZ = FindValidZPosition();
            
            if (spawnZ < 0)
            {
                if (_showDebugLogs)
                {
                    Debug.LogWarning($"[ObstacleSpawner] ⚠️ Pas de place pour l'obstacle {i + 1}");
                }
                continue;
            }

            // ✅ NOUVEAU : Je choisis un type qui GARANTIT qu'une voie reste libre
            ObstacleType type = ChooseSafeObstacleType();
            
            if (_showDebugLogs)
            {
                Debug.Log($"[ObstacleSpawner] Obstacle {i + 1}/{count} : Type={type}, Z={spawnZ:F1}m");
            }
            
            SpawnObstacleByType(chunkRoot, type, spawnZ);
            _occupiedZones.Add(spawnZ);
        }
    }

    private float FindValidZPosition()
    {
        int attempts = 0;
        float candidateZ;

        do
        {
            candidateZ = Random.Range(_minZ, _maxZ);
            attempts++;

            if (attempts > 50) // ✅ AUGMENTÉ de 30 à 50 tentatives
            {
                if (_showDebugLogs)
                {
                    Debug.LogWarning($"[ObstacleSpawner] ❌ Échec après 50 tentatives - Chunk saturé");
                }
                return -1f;
            }

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

    private float FindValidZPositionForInterLane()
    {
        int attempts = 0;
        float candidateZ;

        do
        {
            candidateZ = Random.Range(_minZ, _maxZ);
            attempts++;

            if (attempts > 50)
            {
                return -1f;
            }

            bool isValid = true;
            foreach (float occupiedZ in _occupiedZones)
            {
                if (Mathf.Abs(candidateZ - occupiedZ) < _minDistanceForInterLane)
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
    /// ✅ NOUVELLE MÉTHODE : Je choisis un type d'obstacle en GARANTISSANT qu'une voie reste libre
    /// et en évitant de bloquer toujours la même voie
    /// </summary>
    private ObstacleType ChooseSafeObstacleType()
    {
        // ✅ Probabilité RÉDUITE pour les barrières (trop difficiles)
        if (_barrierObstacles.Count > 0 && Random.value < _barrierSpawnChance)
        {
            if (_showDebugLogs)
            {
                Debug.Log("[ObstacleSpawner]   └─ Type choisi : Barrière (se baisser au centre)");
            }
            return ObstacleType.Barrier;
        }

        // ✅ Je favorise les obstacles 1 voie (les plus fair-play)
        float randomValue = Random.value;
        
        if (randomValue < 0.5f && _singleLaneObstacles.Count > 0) // 50% de chance
        {
            if (_showDebugLogs)
            {
                Debug.Log("[ObstacleSpawner]   └─ Type choisi : 1 voie (2 voies libres)");
            }
            return ObstacleType.SingleLane;
        }

        // ✅ Pour les obstacles 2 voies, je m'assure de varier les voies bloquées
        List<ObstacleType> twoLaneTypes = new List<ObstacleType>();
        
        // Je détermine quelles voies je peux bloquer selon la dernière voie libre
        if (_lastFreeLane != 2 && _leftCenterObstacles.Count > 0) // Si je n'ai pas laissé la droite libre la dernière fois
        {
            twoLaneTypes.Add(ObstacleType.LeftCenter); // Je peux bloquer gauche+centre (droite libre)
        }
        
        if (_lastFreeLane != 0 && _centerRightObstacles.Count > 0) // Si je n'ai pas laissé la gauche libre la dernière fois
        {
            twoLaneTypes.Add(ObstacleType.CenterRight); // Je peux bloquer centre+droite (gauche libre)
        }
        
        if (_lastFreeLane != 1 && _leftRightObstacles.Count > 0) // Si je n'ai pas laissé le centre libre la dernière fois
        {
            twoLaneTypes.Add(ObstacleType.LeftRight); // Je peux bloquer gauche+droite (centre libre)
        }

        // Si j'ai des options, je choisis aléatoirement
        if (twoLaneTypes.Count > 0)
        {
            ObstacleType chosenType = twoLaneTypes[Random.Range(0, twoLaneTypes.Count)];
            
            // ✅ Je mémorise quelle voie sera libre
            _lastFreeLane = chosenType switch
            {
                ObstacleType.LeftCenter => 2,   // Droite libre
                ObstacleType.CenterRight => 0,  // Gauche libre
                ObstacleType.LeftRight => 1,    // Centre libre
                _ => -1
            };
            
            if (_showDebugLogs)
            {
                string freeLaneName = _lastFreeLane == 0 ? "Gauche" : _lastFreeLane == 1 ? "Centre" : "Droite";
                Debug.Log($"[ObstacleSpawner]   └─ Type choisi : 2 voies ({chosenType}) - Voie {freeLaneName} LIBRE");
            }
            
            return chosenType;
        }

        // ✅ Par défaut, je reviens à SingleLane (le plus safe)
        if (_showDebugLogs)
        {
            Debug.Log("[ObstacleSpawner]   └─ Type par défaut : 1 voie");
        }
        return ObstacleType.SingleLane;
    }

    private void SpawnObstacleByType(Transform chunkRoot, ObstacleType type, float spawnZ)
    {
        GameObject prefab = null;
        Vector3 position = Vector3.zero;
        float height = _spawnHeight;

        switch (type)
        {
            case ObstacleType.SingleLane:
                if (_singleLaneObstacles.Count > 0)
                {
                    prefab = _singleLaneObstacles[Random.Range(0, _singleLaneObstacles.Count)];
                    float[] lanes = { _leftLaneX, _centerLaneX, _rightLaneX };
                    float selectedLane = lanes[Random.Range(0, 3)];
                    position = new Vector3(selectedLane, height, spawnZ);
                    
                    if (_showDebugLogs)
                    {
                        string laneName = selectedLane == _leftLaneX ? "Gauche" : selectedLane == _centerLaneX ? "Centre" : "Droite";
                        Debug.Log($"[ObstacleSpawner]   └─ 1 voie bloquée : {laneName}");
                    }
                }
                break;

            case ObstacleType.LeftCenter:
                if (_leftCenterObstacles.Count > 0)
                {
                    prefab = _leftCenterObstacles[Random.Range(0, _leftCenterObstacles.Count)];
                    float centerX = (_leftLaneX + _centerLaneX) / 2f;
                    position = new Vector3(centerX, height, spawnZ);
                    
                    if (_showDebugLogs)
                    {
                        Debug.Log($"[ObstacleSpawner]   └─ Gauche+Centre bloqués → DROITE LIBRE ✅");
                    }
                }
                break;

            case ObstacleType.CenterRight:
                if (_centerRightObstacles.Count > 0)
                {
                    prefab = _centerRightObstacles[Random.Range(0, _centerRightObstacles.Count)];
                    float centerX = (_centerLaneX + _rightLaneX) / 2f;
                    position = new Vector3(centerX, height, spawnZ);
                    
                    if (_showDebugLogs)
                    {
                        Debug.Log($"[ObstacleSpawner]   └─ Centre+Droite bloqués → GAUCHE LIBRE ✅");
                    }
                }
                break;

            case ObstacleType.LeftRight:
                if (_leftRightObstacles.Count > 0)
                {
                    prefab = _leftRightObstacles[Random.Range(0, _leftRightObstacles.Count)];
                    
                    GameObject leftObstacle = Instantiate(prefab, chunkRoot);
                    leftObstacle.transform.localPosition = new Vector3(_leftLaneX, height, spawnZ);
                    leftObstacle.transform.localRotation = Quaternion.identity;
                    leftObstacle.tag = "Obstacle";
                    
                    GameObject rightObstacle = Instantiate(prefab, chunkRoot);
                    rightObstacle.transform.localPosition = new Vector3(_rightLaneX, height, spawnZ);
                    rightObstacle.transform.localRotation = Quaternion.identity;
                    rightObstacle.tag = "Obstacle";
                    
                    if (_showDebugLogs)
                    {
                        Debug.Log($"[ObstacleSpawner]   └─ Gauche+Droite bloqués → CENTRE LIBRE ✅");
                    }
                    
                    return;
                }
                break;

            case ObstacleType.Barrier:
                if (_barrierObstacles.Count > 0)
                {
                    prefab = _barrierObstacles[Random.Range(0, _barrierObstacles.Count)];
                    height = _barrierHeight;
                    
                    GameObject leftBarrier = Instantiate(prefab, chunkRoot);
                    leftBarrier.transform.localPosition = new Vector3(_leftLaneX, height, spawnZ);
                    leftBarrier.transform.localRotation = Quaternion.identity;
                    leftBarrier.tag = "Obstacle";
                    
                    GameObject rightBarrier = Instantiate(prefab, chunkRoot);
                    rightBarrier.transform.localPosition = new Vector3(_rightLaneX, height, spawnZ);
                    rightBarrier.transform.localRotation = Quaternion.identity;
                    rightBarrier.tag = "Obstacle";
                    
                    if (_showDebugLogs)
                    {
                        Debug.Log($"[ObstacleSpawner]   └─ Barrières hautes → SE BAISSER AU CENTRE ✅");
                    }
                    
                    return;
                }
                break;
        }

        if (prefab != null)
        {
            GameObject obstacle = Instantiate(prefab, chunkRoot);
            obstacle.transform.localPosition = position;
            obstacle.transform.localRotation = Quaternion.identity;
            obstacle.tag = "Obstacle";
        }
    }

    private void SpawnInterLaneObstacle(Transform chunkRoot)
    {
        if (_interLaneObstacles.Count == 0) return;

        float spawnZ = FindValidZPositionForInterLane();
        
        if (spawnZ < 0) 
        {
            if (_showDebugLogs)
            {
                Debug.Log("[ObstacleSpawner] Pas de place pour inter-voies");
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
                Debug.Log($"[ObstacleSpawner] Obstacle inter-voies à {localPosition}");
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
        
        Gizmos.color = Color.green;
        foreach (float z in _occupiedZones)
        {
            Gizmos.DrawWireSphere(worldPos + new Vector3(0, 1, z), _minDistanceBetweenObstacles / 2);
        }
    }
#endif
}