using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Je génère les obstacles sur un chunk de manière aléatoire sur les 3 voies.
/// J'utilise un spawn aléatoire simple sans spawn points prédéfinis.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Configuration Obstacles")]
    [SerializeField] private List<GameObject> _laneObstacles = new List<GameObject>();      // Je stocke la liste des obstacles de voie
    [SerializeField] private List<GameObject> _interLaneObstacles = new List<GameObject>(); // Je stocke la liste des obstacles entre les voies

    [Header("Spawn Configuration")]
    [SerializeField] private int _minObstaclesPerChunk = 2;           // Je stocke le nombre minimum d'obstacles par chunk
    [SerializeField] private int _maxObstaclesPerChunk = 4;           // Je stocke le nombre maximum d'obstacles par chunk
    [SerializeField] private bool _spawnInterLaneObstacles = true;    // Je stocke si je dois spawner des obstacles inter-voies
    [SerializeField] [Range(0f, 1f)] private float _interLaneSpawnChance = 0.3f; // Je stocke la probabilité de spawn inter-voies (30%)

    [Header("Lane Positions (X)")]
    [SerializeField] private float _leftLaneX = -1.84f;   // Je stocke la position X de la voie gauche
    [SerializeField] private float _centerLaneX = 0f;     // Je stocke la position X de la voie centrale
    [SerializeField] private float _rightLaneX = 1.84f;   // Je stocke la position X de la voie droite

    [Header("Spawn Range Z")]
    [SerializeField] private float _minZ = 10f;  // Je stocke la position Z minimale pour spawner
    [SerializeField] private float _maxZ = 40f;  // Je stocke la position Z maximale pour spawner

    [Header("Height")]
    [SerializeField] private float _spawnHeight = 0f; // Je stocke la hauteur à laquelle je spawne les obstacles

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false; // Je stocke si j'affiche les logs de debug

    /// <summary>
    /// Je suis appelé par le ChunkSpawner pour générer les obstacles
    /// </summary>
    /// <param name="phase">La phase actuelle du jeu (Green, Blue, Red, Black)</param>
    public void SpawnObstacles(TrackPhase phase = TrackPhase.Green)
    {
        // Je vérifie que j'ai des obstacles à spawner
        if (_laneObstacles.Count == 0)
        {
            Debug.LogWarning($"[ObstacleSpawner] {gameObject.name} : Aucun obstacle configuré !");
            return;
        }

        // Je récupère le chunk parent de manière dynamique
        Transform chunkRoot = transform.parent;
        
        // Je vérifie que j'ai bien un parent
        if (chunkRoot == null)
        {
            Debug.LogError($"[ObstacleSpawner] {gameObject.name} n'a pas de parent !");
            return;
        }

        // Je calcule le nombre d'obstacles à spawner selon la phase actuelle
        int obstacleCount = CalculateObstacleCount(phase);

        // J'affiche un log si le debug est activé
        if (_showDebugLogs)
        {
            Debug.Log($"[ObstacleSpawner] Génération de {obstacleCount} obstacles sur {gameObject.name} (Phase: {phase})");
        }

        // Je génère les obstacles sur les voies
        SpawnLaneObstacles(chunkRoot, obstacleCount);
        
        // Je génère éventuellement un obstacle inter-voies (selon la probabilité)
        if (_spawnInterLaneObstacles && Random.value < _interLaneSpawnChance)
        {
            SpawnInterLaneObstacle(chunkRoot);
        }
    }

    /// <summary>
    /// Je calcule le nombre d'obstacles à spawner selon la phase du jeu
    /// Plus la phase est avancée, plus il y a d'obstacles
    /// </summary>
    /// <param name="phase">La phase actuelle du jeu</param>
    /// <returns>Le nombre d'obstacles à spawner</returns>
    private int CalculateObstacleCount(TrackPhase phase)
    {
        // Je détermine le nombre de base selon la phase
        int baseCount = phase switch
        {
            TrackPhase.Green => Random.Range(_minObstaclesPerChunk, _minObstaclesPerChunk + 1),       // Phase verte : peu d'obstacles
            TrackPhase.Blue => Random.Range(_minObstaclesPerChunk, _maxObstaclesPerChunk - 1),        // Phase bleue : obstacles modérés
            TrackPhase.Red => Random.Range(_minObstaclesPerChunk + 1, _maxObstaclesPerChunk),         // Phase rouge : beaucoup d'obstacles
            TrackPhase.Black => Random.Range(_maxObstaclesPerChunk - 1, _maxObstaclesPerChunk + 1),   // Phase noire : maximum d'obstacles
            _ => _minObstaclesPerChunk  // Par défaut : minimum d'obstacles
        };

        // Je m'assure que le nombre reste entre 1 et 6
        return Mathf.Clamp(baseCount, 1, 6);
    }

    /// <summary>
    /// Je génère plusieurs obstacles sur différentes voies avec des positions LOCALES aléatoires
    /// </summary>
    /// <param name="chunkRoot">Le chunk parent où je dois spawner</param>
    /// <param name="count">Le nombre d'obstacles à spawner</param>
    private void SpawnLaneObstacles(Transform chunkRoot, int count)
    {
        // Je crée un tableau des positions X possibles pour les 3 voies
        float[] lanePositions = { _leftLaneX, _centerLaneX, _rightLaneX };
        
        // Je crée une liste des voies disponibles (0=gauche, 1=centre, 2=droite)
        List<int> availableLanes = new List<int> { 0, 1, 2 };
        
        // Je spawne le nombre d'obstacles demandé
        for (int i = 0; i < count; i++)
        {
            // Si j'ai utilisé toutes les voies, je réinitialise la liste
            // Cela permet de spawner plusieurs obstacles sur la même voie si nécessaire
            if (availableLanes.Count == 0)
            {
                availableLanes = new List<int> { 0, 1, 2 };
            }

            // Je choisis une voie aléatoire parmi celles disponibles
            int randomLaneIndex = Random.Range(0, availableLanes.Count);
            int selectedLane = availableLanes[randomLaneIndex];
            
            // Je retire cette voie de la liste pour éviter de la choisir à nouveau (dans ce cycle)
            availableLanes.RemoveAt(randomLaneIndex);

            // Je choisis un obstacle aléatoire dans ma liste
            int randomObstacle = Random.Range(0, _laneObstacles.Count);
            GameObject obstaclePrefab = _laneObstacles[randomObstacle];

            // Je vérifie que le prefab existe
            if (obstaclePrefab != null)
            {
                // Je calcule la position LOCALE de l'obstacle
                float laneX = lanePositions[selectedLane];     // Je prends la position X de la voie
                float randomZ = Random.Range(_minZ, _maxZ);    // Je choisis une position Z aléatoire
                
                Vector3 localPosition = new Vector3(laneX, _spawnHeight, randomZ);

                // J'instancie l'obstacle avec le chunk comme parent
                GameObject obstacle = Instantiate(obstaclePrefab, chunkRoot);
                
                // Je définis sa position locale (par rapport au chunk parent)
                obstacle.transform.localPosition = localPosition;
                obstacle.transform.localRotation = Quaternion.identity; // Pas de rotation

                // Je force le tag "Obstacle" pour la détection de collision
                obstacle.tag = "Obstacle";

                // J'affiche un log détaillé si le debug est activé
                if (_showDebugLogs)
                {
                    Debug.Log($"[ObstacleSpawner] Obstacle {i + 1}/{count} spawné sur voie {selectedLane} " +
                             $"(X={laneX}) à position locale {localPosition}");
                }
            }
        }
    }

    /// <summary>
    /// Je génère un obstacle entre deux voies
    /// Ces obstacles bloquent le changement de voie
    /// </summary>
    /// <param name="chunkRoot">Le chunk parent où je dois spawner</param>
    private void SpawnInterLaneObstacle(Transform chunkRoot)
    {
        // Je vérifie que j'ai des obstacles inter-voies configurés
        if (_interLaneObstacles.Count == 0) return;

        // Je définis les positions inter-voies possibles
        // -0.92 = entre voie gauche et centrale
        // 0.92 = entre voie centrale et droite
        float[] interLanePositions = { -0.92f, 0.92f };
        
        // Je choisis un obstacle aléatoire dans ma liste
        int randomObstacle = Random.Range(0, _interLaneObstacles.Count);
        GameObject obstaclePrefab = _interLaneObstacles[randomObstacle];

        // Je vérifie que le prefab existe
        if (obstaclePrefab != null)
        {
            // Je choisis une position inter-voies aléatoire
            float randomX = interLanePositions[Random.Range(0, 2)];
            
            // Je choisis une position Z aléatoire
            float randomZ = Random.Range(_minZ, _maxZ);
            
            Vector3 localPosition = new Vector3(randomX, _spawnHeight, randomZ);

            // J'instancie l'obstacle avec le chunk comme parent
            GameObject obstacle = Instantiate(obstaclePrefab, chunkRoot);
            
            // Je définis sa position locale
            obstacle.transform.localPosition = localPosition;
            obstacle.transform.localRotation = Quaternion.identity;
            
            // Je force le tag "Obstacle"
            obstacle.tag = "Obstacle";

            // J'affiche un log si le debug est activé
            if (_showDebugLogs)
            {
                Debug.Log($"[ObstacleSpawner] Obstacle inter-voies spawné à position locale {localPosition}");
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Je dessine des gizmos dans l'éditeur pour visualiser les voies et la zone de spawn
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Je dessine les 3 voies en rouge
        Gizmos.color = Color.red;
        
        Vector3 worldPos = transform.position;
        
        // Je dessine la voie gauche
        Gizmos.DrawLine(worldPos + new Vector3(_leftLaneX, 0, 0), 
                       worldPos + new Vector3(_leftLaneX, 0, 50));
        
        // Je dessine la voie centrale
        Gizmos.DrawLine(worldPos + new Vector3(_centerLaneX, 0, 0), 
                       worldPos + new Vector3(_centerLaneX, 0, 50));
        
        // Je dessine la voie droite
        Gizmos.DrawLine(worldPos + new Vector3(_rightLaneX, 0, 0), 
                       worldPos + new Vector3(_rightLaneX, 0, 50));
        
        // Je dessine la zone de spawn Z en jaune
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldPos + new Vector3(0, 0, (_minZ + _maxZ) / 2), 
                           new Vector3(4, 2, _maxZ - _minZ));
    }
#endif
}