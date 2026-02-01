using UnityEngine;

/// <summary>
/// Spawne les collectibles sur un chunk de manière aléatoire sur les 3 voies.
/// VERSION SIMPLIFIÉE - Sans spawn points, positions directes
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    [Header("Collectible Prefabs")]
    [SerializeField] private GameObject _painEpicePrefab;   // Score
    [SerializeField] private GameObject _sucreOrgePrefab;   // Vitesse accélérée (Boost)
    [SerializeField] private GameObject _cadeauPrefab;      // Bouclier (Shield)
    [SerializeField] private GameObject _bouleNoelPrefab;   // -10% menace (Réduction jauge)

    [Header("Lane Positions (X)")]
    [SerializeField] private float _leftLaneX = -1.84f;
    [SerializeField] private float _centerLaneX = 0f;
    [SerializeField] private float _rightLaneX = 1.84f;

    [Header("Spawn Range Z")]
    [SerializeField] private float _minZ = 10f;
    [SerializeField] private float _maxZ = 40f;

    [Header("Height")]
    [SerializeField] private float _spawnHeight = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;

    /// <summary>
    /// Génère les collectibles pour ce chunk selon les probabilités du GDD.
    /// </summary>
    public void SpawnCollectibles(TrackPhase phase)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleSpawner] SpawnCollectibles appelé sur {gameObject.name} (Phase: {phase})");
        }

        // Résoudre le parent dynamiquement
        Transform chunkRoot = transform.parent;

        if (chunkRoot == null)
        {
            Debug.LogError($"[CollectibleSpawner] {gameObject.name} n'a pas de parent !");
            return;
        }

        // Tableau pour suivre l'occupation des 3 voies
        bool[] positionsUsed = new bool[3];
        int spawnedCount = 0;

        // 1. Sucre d'orge : 100% (OBLIGATOIRE)
        if (_sucreOrgePrefab != null)
        {
            if (SpawnCollectible(_sucreOrgePrefab, "SucreOrge", chunkRoot, ref positionsUsed))
            {
                spawnedCount++;
            }
        }
        else
        {
            Debug.LogWarning($"[CollectibleSpawner] SucreOrge prefab manquant sur {gameObject.name} !");
        }

        // 2. Cadeau : 25%
        if (_cadeauPrefab != null && Random.value <= 0.25f)
        {
            if (SpawnCollectible(_cadeauPrefab, "Cadeau", chunkRoot, ref positionsUsed))
            {
                spawnedCount++;
            }
        }

        // 3. Boule de Noël : 50%
        if (_bouleNoelPrefab != null && Random.value <= 0.5f)
        {
            if (SpawnCollectible(_bouleNoelPrefab, "BouleDeNoelRouge", chunkRoot, ref positionsUsed))
            {
                spawnedCount++;
            }
        }

        // 4. Pain d'épice : 30% s'il reste de la place
        if (_painEpicePrefab != null && Random.value <= 0.3f && HasAvailablePosition(positionsUsed))
        {
            if (SpawnCollectible(_painEpicePrefab, "PainEpice", chunkRoot, ref positionsUsed))
            {
                spawnedCount++;
            }
        }

        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleSpawner] {spawnedCount} collectible(s) spawné(s) sur {gameObject.name}");
        }
    }

    /// <summary>
    /// Instancie un collectible à une position aléatoire de voie disponible
    /// </summary>
    private bool SpawnCollectible(GameObject prefab, string collectibleName, Transform chunkRoot, ref bool[] positionsUsed)
    {
        if (prefab == null) return false;

        // Array des positions X possibles
        float[] lanePositions = { _leftLaneX, _centerLaneX, _rightLaneX };

        // Trouver une voie libre
        int attempts = 0;
        int randomLane;
        
        do
        {
            randomLane = Random.Range(0, 3);
            attempts++;
            if (attempts > 10)
            {
                if (_showDebugLogs)
                {
                    Debug.LogWarning($"[CollectibleSpawner] Impossible de trouver une voie libre pour {collectibleName}");
                }
                return false;
            }
        } while (positionsUsed[randomLane]);

        positionsUsed[randomLane] = true;

        // Position LOCALE aléatoire
        float laneX = lanePositions[randomLane];
        float randomZ = Random.Range(_minZ, _maxZ);
        
        Vector3 localPosition = new Vector3(laneX, _spawnHeight, randomZ);

        // Instancier avec le chunk comme parent
        GameObject collectible = Instantiate(prefab, chunkRoot);
        collectible.transform.localPosition = localPosition;
        collectible.transform.localRotation = Quaternion.identity;
        collectible.transform.localScale = Vector3.one;
        
        // Forcer le tag
        collectible.tag = "Collectible";

        // Vérifications
        if (collectible.GetComponent<CollectibleVisual>() == null)
        {
            Debug.LogWarning($"[CollectibleSpawner] {collectibleName} n'a pas CollectibleVisual !", collectible);
        }

        Collider col = collectible.GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"[CollectibleSpawner] {collectibleName} n'a pas de Collider !", collectible);
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"[CollectibleSpawner] {collectibleName} - Collider pas en mode Trigger !", collectible);
        }

        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleSpawner] {collectibleName} spawné sur voie {randomLane} (X={laneX}) à position locale {localPosition}", collectible);
        }

        return true;
    }

    private bool HasAvailablePosition(bool[] positionsUsed)
    {
        foreach (bool used in positionsUsed)
        {
            if (!used) return true;
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualiser les 3 voies dans l'éditeur
        Gizmos.color = Color.green;
        
        Vector3 worldPos = transform.position;
        
        // Voie gauche
        Gizmos.DrawLine(worldPos + new Vector3(_leftLaneX, _spawnHeight, 0), 
                       worldPos + new Vector3(_leftLaneX, _spawnHeight, 50));
        
        // Voie centre
        Gizmos.DrawLine(worldPos + new Vector3(_centerLaneX, _spawnHeight, 0), 
                       worldPos + new Vector3(_centerLaneX, _spawnHeight, 50));
        
        // Voie droite
        Gizmos.DrawLine(worldPos + new Vector3(_rightLaneX, _spawnHeight, 0), 
                       worldPos + new Vector3(_rightLaneX, _spawnHeight, 50));
        
        // Zone de spawn Z
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(worldPos + new Vector3(0, _spawnHeight, (_minZ + _maxZ) / 2), 
                           new Vector3(4, 1, _maxZ - _minZ));
    }

    [ContextMenu("Valider Configuration")]
    private void ValidateConfiguration()
    {
        bool hasErrors = false;

        if (_painEpicePrefab == null) { Debug.LogWarning("[CollectibleSpawner] PainEpice manquant"); hasErrors = true; }
        if (_sucreOrgePrefab == null) { Debug.LogError("[CollectibleSpawner] SucreOrge manquant (OBLIGATOIRE) !"); hasErrors = true; }
        if (_cadeauPrefab == null) { Debug.LogWarning("[CollectibleSpawner] Cadeau manquant"); hasErrors = true; }
        if (_bouleNoelPrefab == null) { Debug.LogWarning("[CollectibleSpawner] BouleDeNoel manquant"); hasErrors = true; }

        if (!hasErrors)
        {
            Debug.Log("[CollectibleSpawner] Configuration valide ✓");
        }
    }
#endif
}