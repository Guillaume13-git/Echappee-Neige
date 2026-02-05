using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Je spawne les collectibles sur un chunk en évitant les zones d'obstacles.
/// Je garantis que les collectibles ne se chevauchent jamais avec les obstacles.
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    [Header("Collectible Prefabs")]
    [SerializeField] private GameObject _painEpicePrefab;   // Je stocke le prefab Pain d'épice (Score)
    [SerializeField] private GameObject _sucreOrgePrefab;   // Je stocke le prefab Sucre d'orge (Vitesse accélérée)
    [SerializeField] private GameObject _cadeauPrefab;      // Je stocke le prefab Cadeau (Bouclier)
    [SerializeField] private GameObject _bouleNoelPrefab;   // Je stocke le prefab Boule de Noël (-10% menace)

    [Header("Lane Positions (X)")]
    [SerializeField] private float _leftLaneX = -1.84f;     // Je stocke la position X de la voie gauche
    [SerializeField] private float _centerLaneX = 0f;       // Je stocke la position X de la voie centrale
    [SerializeField] private float _rightLaneX = 1.84f;     // Je stocke la position X de la voie droite

    [Header("Spawn Range Z")]
    [SerializeField] private float _minZ = 10f;             // Je stocke la position Z minimale pour spawner
    [SerializeField] private float _maxZ = 40f;             // Je stocke la position Z maximale pour spawner
    [SerializeField] private float _safeDistanceFromObstacles = 5f; // Distance minimale des obstacles (Sécurité)

    [Header("Height")]
    [SerializeField] private float _spawnHeight = 0.5f;     // Je stocke la hauteur à laquelle je spawne les collectibles

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;    // Je stocke si j'affiche les logs de debug

    /// <summary>
    /// Je génère les collectibles pour ce chunk en évitant les zones d'obstacles
    /// </summary>
    /// <param name="phase">La phase actuelle du jeu</param>
    public void SpawnCollectibles(TrackPhase phase)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleSpawner] SpawnCollectibles appelé sur {gameObject.name} (Phase: {phase})");
        }

        // Je récupère le chunk parent
        Transform chunkRoot = transform.parent;

        if (chunkRoot == null)
        {
            Debug.LogError($"[CollectibleSpawner] {gameObject.name} n'a pas de parent !");
            return;
        }

        // --- NOUVEAUTÉ : ÉVITEMENT OBSTACLES ---
        // Je récupère les zones occupées par les obstacles via le composant ObstacleSpawner voisin
        ObstacleSpawner obstacleSpawner = GetComponent<ObstacleSpawner>();
        List<float> occupiedZones = obstacleSpawner != null 
            ? obstacleSpawner.GetOccupiedZones() 
            : new List<float>();

        // Je crée un tableau pour suivre quelles voies sont déjà occupées
        bool[] positionsUsed = new bool[3]; // [gauche, centre, droite]
        int spawnedCount = 0;

        // ---------------------------------------------------------
        // 1. SUCRE D'ORGE : 100% (OBLIGATOIRE)
        // ---------------------------------------------------------
        if (_sucreOrgePrefab != null)
        {
            if (SpawnCollectible(_sucreOrgePrefab, "SucreOrge", chunkRoot, ref positionsUsed, occupiedZones))
            {
                spawnedCount++;
            }
        }

        // ---------------------------------------------------------
        // 2. CADEAU : 25%
        // ---------------------------------------------------------
        if (_cadeauPrefab != null && Random.value <= 0.25f)
        {
            if (SpawnCollectible(_cadeauPrefab, "Cadeau", chunkRoot, ref positionsUsed, occupiedZones))
            {
                spawnedCount++;
            }
        }

        // ---------------------------------------------------------
        // 3. BOULE DE NOËL : 50%
        // ---------------------------------------------------------
        if (_bouleNoelPrefab != null && Random.value <= 0.5f)
        {
            if (SpawnCollectible(_bouleNoelPrefab, "BouleDeNoelRouge", chunkRoot, ref positionsUsed, occupiedZones))
            {
                spawnedCount++;
            }
        }

        // ---------------------------------------------------------
        // 4. PAIN D'ÉPICE : 30% (si place disponible)
        // ---------------------------------------------------------
        if (_painEpicePrefab != null && Random.value <= 0.3f && HasAvailablePosition(positionsUsed))
        {
            if (SpawnCollectible(_painEpicePrefab, "PainEpice", chunkRoot, ref positionsUsed, occupiedZones))
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
    /// J'instancie un collectible en évitant les zones d'obstacles
    /// </summary>
    private bool SpawnCollectible(GameObject prefab, string collectibleName, Transform chunkRoot, 
                                 ref bool[] positionsUsed, List<float> occupiedZones)
    {
        if (prefab == null) return false;

        float[] lanePositions = { _leftLaneX, _centerLaneX, _rightLaneX };

        // Je cherche une voie libre
        int attempts = 0;
        int randomLane;
        
        do
        {
            randomLane = Random.Range(0, 3);
            attempts++;
            
            if (attempts > 10)
            {
                if (_showDebugLogs) Debug.LogWarning($"[CollectibleSpawner] Pas de voie libre pour {collectibleName}");
                return false;
            }
        } while (positionsUsed[randomLane]);

        // Je cherche une position Z qui est sûre (loin des obstacles)
        float safeZ = FindSafeZPosition(occupiedZones);
        
        if (safeZ < 0)
        {
            if (_showDebugLogs) Debug.LogWarning($"[CollectibleSpawner] Pas de position Z sûre pour {collectibleName}");
            return false;
        }

        // Je marque cette voie comme occupée pour ne pas avoir deux objets sur la même voie X
        positionsUsed[randomLane] = true;

        // Je calcule la position locale finale
        float laneX = lanePositions[randomLane];
        Vector3 localPosition = new Vector3(laneX, _spawnHeight, safeZ);

        // J'instancie
        GameObject collectible = Instantiate(prefab, chunkRoot);
        collectible.transform.localPosition = localPosition;
        collectible.transform.localRotation = Quaternion.identity;
        collectible.transform.localScale = Vector3.one;
        collectible.tag = "Collectible";

        // --- SÉCURITÉ ---
        if (collectible.GetComponent<CollectibleVisual>() == null)
            Debug.LogWarning($"[CollectibleSpawner] {collectibleName} n'a pas CollectibleVisual !");

        Collider col = collectible.GetComponent<Collider>();
        if (col == null || !col.isTrigger)
            Debug.LogWarning($"[CollectibleSpawner] {collectibleName} - Collider manquant ou pas Trigger !");

        if (_showDebugLogs)
            Debug.Log($"[CollectibleSpawner] {collectibleName} spawné Voie {randomLane} (Z={safeZ:F1})", collectible);

        return true;
    }

    /// <summary>
    /// Je cherche une position Z aléatoire et je vérifie qu'elle n'est pas sur un obstacle
    /// </summary>
    private float FindSafeZPosition(List<float> occupiedZones)
    {
        int attempts = 0;
        float candidateZ;

        while (attempts < 30)
        {
            candidateZ = Random.Range(_minZ, _maxZ);
            attempts++;

            bool isSafe = true;
            foreach (float obstacleZ in occupiedZones)
            {
                // Si la distance entre mon spawn et un obstacle est trop courte, c'est dangereux
                if (Mathf.Abs(candidateZ - obstacleZ) < _safeDistanceFromObstacles)
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe) return candidateZ;
        }

        return -1f; // Je n'ai pas trouvé de place après 30 essais
    }

    private bool HasAvailablePosition(bool[] positionsUsed)
    {
        foreach (bool used in positionsUsed) { if (!used) return true; }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 worldPos = transform.position;
        
        Gizmos.DrawLine(worldPos + new Vector3(_leftLaneX, _spawnHeight, 0), worldPos + new Vector3(_leftLaneX, _spawnHeight, 50));
        Gizmos.DrawLine(worldPos + new Vector3(_centerLaneX, _spawnHeight, 0), worldPos + new Vector3(_centerLaneX, _spawnHeight, 50));
        Gizmos.DrawLine(worldPos + new Vector3(_rightLaneX, _spawnHeight, 0), worldPos + new Vector3(_rightLaneX, _spawnHeight, 50));
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(worldPos + new Vector3(0, _spawnHeight, (_minZ + _maxZ) / 2), new Vector3(4, 1, _maxZ - _minZ));
    }

    [ContextMenu("Valider Configuration")]
    private void ValidateConfiguration()
    {
        if (_sucreOrgePrefab == null) Debug.LogError("[CollectibleSpawner] SucreOrge manquant !");
        else Debug.Log("[CollectibleSpawner] Configuration valide ✓");
    }
#endif
}