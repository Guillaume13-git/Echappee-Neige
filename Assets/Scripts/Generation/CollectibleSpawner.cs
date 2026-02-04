using UnityEngine;

/// <summary>
/// Je spawne les collectibles sur un chunk de manière aléatoire sur les 3 voies.
/// J'utilise des positions directes sans spawn points pour plus de simplicité.
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
    [SerializeField] private float _minZ = 10f;  // Je stocke la position Z minimale pour spawner
    [SerializeField] private float _maxZ = 40f;  // Je stocke la position Z maximale pour spawner

    [Header("Height")]
    [SerializeField] private float _spawnHeight = 0.5f; // Je stocke la hauteur à laquelle je spawne les collectibles

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true; // Je stocke si j'affiche les logs de debug

    /// <summary>
    /// Je génère les collectibles pour ce chunk selon les probabilités définies dans le GDD
    /// </summary>
    /// <param name="phase">La phase actuelle du jeu (pour de futures extensions)</param>
    public void SpawnCollectibles(TrackPhase phase)
    {
        // J'affiche un log si le debug est activé
        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleSpawner] SpawnCollectibles appelé sur {gameObject.name} (Phase: {phase})");
        }

        // Je récupère le chunk parent de manière dynamique
        Transform chunkRoot = transform.parent;

        // Je vérifie que j'ai bien un parent
        if (chunkRoot == null)
        {
            Debug.LogError($"[CollectibleSpawner] {gameObject.name} n'a pas de parent !");
            return;
        }

        // Je crée un tableau pour suivre quelles voies sont déjà occupées
        bool[] positionsUsed = new bool[3]; // [gauche, centre, droite]
        int spawnedCount = 0; // Je compte combien de collectibles j'ai spawné

        // ---------------------------------------------------------
        // 1. SUCRE D'ORGE : 100% (OBLIGATOIRE)
        // ---------------------------------------------------------
        // Je spawne toujours un sucre d'orge car il est obligatoire selon le GDD
        if (_sucreOrgePrefab != null)
        {
            if (SpawnCollectible(_sucreOrgePrefab, "SucreOrge", chunkRoot, ref positionsUsed))
            {
                spawnedCount++; // J'incrémente mon compteur si le spawn a réussi
            }
        }
        else
        {
            Debug.LogWarning($"[CollectibleSpawner] SucreOrge prefab manquant sur {gameObject.name} !");
        }

        // ---------------------------------------------------------
        // 2. CADEAU : 25%
        // ---------------------------------------------------------
        // Je spawne un cadeau avec 25% de chances
        if (_cadeauPrefab != null && Random.value <= 0.25f)
        {
            if (SpawnCollectible(_cadeauPrefab, "Cadeau", chunkRoot, ref positionsUsed))
            {
                spawnedCount++;
            }
        }

        // ---------------------------------------------------------
        // 3. BOULE DE NOËL : 50%
        // ---------------------------------------------------------
        // Je spawne une boule de Noël avec 50% de chances
        if (_bouleNoelPrefab != null && Random.value <= 0.5f)
        {
            if (SpawnCollectible(_bouleNoelPrefab, "BouleDeNoelRouge", chunkRoot, ref positionsUsed))
            {
                spawnedCount++;
            }
        }

        // ---------------------------------------------------------
        // 4. PAIN D'ÉPICE : 30% (si place disponible)
        // ---------------------------------------------------------
        // Je spawne un pain d'épice avec 30% de chances, mais seulement s'il reste de la place
        if (_painEpicePrefab != null && Random.value <= 0.3f && HasAvailablePosition(positionsUsed))
        {
            if (SpawnCollectible(_painEpicePrefab, "PainEpice", chunkRoot, ref positionsUsed))
            {
                spawnedCount++;
            }
        }

        // J'affiche le nombre total de collectibles spawnés
        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleSpawner] {spawnedCount} collectible(s) spawné(s) sur {gameObject.name}");
        }
    }

    /// <summary>
    /// J'instancie un collectible à une position aléatoire sur une voie disponible
    /// </summary>
    /// <param name="prefab">Le prefab à instancier</param>
    /// <param name="collectibleName">Le nom du collectible (pour les logs)</param>
    /// <param name="chunkRoot">Le chunk parent où je dois spawner</param>
    /// <param name="positionsUsed">Le tableau des positions déjà occupées (passé par référence)</param>
    /// <returns>True si le spawn a réussi, False sinon</returns>
    private bool SpawnCollectible(GameObject prefab, string collectibleName, Transform chunkRoot, ref bool[] positionsUsed)
    {
        // Je vérifie que le prefab existe
        if (prefab == null) return false;

        // Je crée un tableau des positions X possibles pour les 3 voies
        float[] lanePositions = { _leftLaneX, _centerLaneX, _rightLaneX };

        // Je cherche une voie libre de manière aléatoire
        int attempts = 0;      // Je compte mes tentatives pour éviter une boucle infinie
        int randomLane;        // Je stocke l'index de la voie choisie
        
        do
        {
            randomLane = Random.Range(0, 3); // Je choisis une voie au hasard (0, 1, ou 2)
            attempts++;
            
            // Si j'ai fait trop de tentatives, j'abandonne
            if (attempts > 10)
            {
                if (_showDebugLogs)
                {
                    Debug.LogWarning($"[CollectibleSpawner] Impossible de trouver une voie libre pour {collectibleName}");
                }
                return false;
            }
        } while (positionsUsed[randomLane]); // Je continue tant que la voie est occupée

        // Je marque cette voie comme occupée
        positionsUsed[randomLane] = true;

        // Je calcule la position LOCALE du collectible
        float laneX = lanePositions[randomLane];           // Je prends la position X de la voie choisie
        float randomZ = Random.Range(_minZ, _maxZ);        // Je choisis une position Z aléatoire dans la plage
        
        Vector3 localPosition = new Vector3(laneX, _spawnHeight, randomZ);

        // J'instancie le collectible avec le chunk comme parent
        GameObject collectible = Instantiate(prefab, chunkRoot);
        
        // Je définis sa position locale (par rapport au chunk parent)
        collectible.transform.localPosition = localPosition;
        collectible.transform.localRotation = Quaternion.identity; // Pas de rotation
        collectible.transform.localScale = Vector3.one;            // Échelle normale
        
        // Je force le tag "Collectible" pour la détection de collision
        collectible.tag = "Collectible";

        // ---------------------------------------------------------
        // VÉRIFICATIONS DE SÉCURITÉ
        // ---------------------------------------------------------
        
        // Je vérifie que le collectible a bien le script CollectibleVisual
        if (collectible.GetComponent<CollectibleVisual>() == null)
        {
            Debug.LogWarning($"[CollectibleSpawner] {collectibleName} n'a pas CollectibleVisual !", collectible);
        }

        // Je vérifie que le collectible a bien un Collider
        Collider col = collectible.GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"[CollectibleSpawner] {collectibleName} n'a pas de Collider !", collectible);
        }
        else if (!col.isTrigger)
        {
            // Je vérifie que le Collider est bien en mode Trigger
            Debug.LogWarning($"[CollectibleSpawner] {collectibleName} - Collider pas en mode Trigger !", collectible);
        }

        // J'affiche un log détaillé si le debug est activé
        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleSpawner] {collectibleName} spawné sur voie {randomLane} (X={laneX}) à position locale {localPosition}", collectible);
        }

        return true; // Je confirme que le spawn a réussi
    }

    /// <summary>
    /// Je vérifie s'il reste au moins une position disponible dans les 3 voies
    /// </summary>
    /// <param name="positionsUsed">Le tableau des positions occupées</param>
    /// <returns>True s'il reste de la place, False sinon</returns>
    private bool HasAvailablePosition(bool[] positionsUsed)
    {
        // Je parcours toutes les positions
        foreach (bool used in positionsUsed)
        {
            // Si je trouve une position libre, je retourne true
            if (!used) return true;
        }
        
        // Si toutes les positions sont occupées, je retourne false
        return false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Je dessine des gizmos dans l'éditeur pour visualiser les voies et la zone de spawn
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Je dessine les 3 voies en vert
        Gizmos.color = Color.green;
        
        Vector3 worldPos = transform.position;
        
        // Je dessine la voie gauche
        Gizmos.DrawLine(worldPos + new Vector3(_leftLaneX, _spawnHeight, 0), 
                       worldPos + new Vector3(_leftLaneX, _spawnHeight, 50));
        
        // Je dessine la voie centrale
        Gizmos.DrawLine(worldPos + new Vector3(_centerLaneX, _spawnHeight, 0), 
                       worldPos + new Vector3(_centerLaneX, _spawnHeight, 50));
        
        // Je dessine la voie droite
        Gizmos.DrawLine(worldPos + new Vector3(_rightLaneX, _spawnHeight, 0), 
                       worldPos + new Vector3(_rightLaneX, _spawnHeight, 50));
        
        // Je dessine la zone de spawn Z en cyan
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(worldPos + new Vector3(0, _spawnHeight, (_minZ + _maxZ) / 2), 
                           new Vector3(4, 1, _maxZ - _minZ));
    }

    /// <summary>
    /// Je valide ma configuration (menu contextuel dans l'éditeur)
    /// Clic droit sur le composant → "Valider Configuration"
    /// </summary>
    [ContextMenu("Valider Configuration")]
    private void ValidateConfiguration()
    {
        bool hasErrors = false;

        // Je vérifie que tous les prefabs sont assignés
        if (_painEpicePrefab == null) 
        { 
            Debug.LogWarning("[CollectibleSpawner] PainEpice manquant"); 
            hasErrors = true; 
        }
        
        if (_sucreOrgePrefab == null) 
        { 
            Debug.LogError("[CollectibleSpawner] SucreOrge manquant (OBLIGATOIRE) !"); 
            hasErrors = true; 
        }
        
        if (_cadeauPrefab == null) 
        { 
            Debug.LogWarning("[CollectibleSpawner] Cadeau manquant"); 
            hasErrors = true; 
        }
        
        if (_bouleNoelPrefab == null) 
        { 
            Debug.LogWarning("[CollectibleSpawner] BouleDeNoel manquant"); 
            hasErrors = true; 
        }

        // Si tout est OK, j'affiche un message de confirmation
        if (!hasErrors)
        {
            Debug.Log("[CollectibleSpawner] Configuration valide ✓");
        }
    }
#endif
}