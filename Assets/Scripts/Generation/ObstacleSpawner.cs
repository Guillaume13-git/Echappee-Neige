using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Je suis responsable de la génération intelligente des obstacles sur chaque chunk de piste.
/// Mon rôle : Créer des patterns d'obstacles variés et équilibrés qui défient le joueur sans être injustes.
/// Je gère différents types d'obstacles : 1 voie, 2 voies, et barrières hautes.
/// Ma garantie : Je laisse TOUJOURS au moins une voie libre pour que le joueur puisse passer.
/// Mon emplacement : Je suis attaché à chaque prefab de chunk dans la hiérarchie Unity.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacles 1 Voie")]
    [Tooltip("Je stocke les prefabs d'obstacles qui bloquent UNE SEULE voie (les 2 autres restent libres)")]
    [SerializeField] private List<GameObject> _singleLaneObstacles = new List<GameObject>();

    [Header("Obstacles 2 Voies - Gauche + Centre")]
    [Tooltip("Je stocke les prefabs d'obstacles qui bloquent les voies GAUCHE et CENTRE (la voie de DROITE reste libre)")]
    [SerializeField] private List<GameObject> _leftCenterObstacles = new List<GameObject>();

    [Header("Obstacles 2 Voies - Centre + Droite")]
    [Tooltip("Je stocke les prefabs d'obstacles qui bloquent les voies CENTRE et DROITE (la voie de GAUCHE reste libre)")]
    [SerializeField] private List<GameObject> _centerRightObstacles = new List<GameObject>();

    [Header("Obstacles 2 Voies - Gauche + Droite")]
    [Tooltip("Je stocke les prefabs d'obstacles qui bloquent les voies GAUCHE et DROITE (la voie du CENTRE reste libre)")]
    [SerializeField] private List<GameObject> _leftRightObstacles = new List<GameObject>();

    [Header("Barrières (se baisser)")]
    [Tooltip("Je stocke les prefabs de barrières hautes qui bloquent gauche+droite en hauteur (le joueur doit se BAISSER au centre)")]
    [SerializeField] private List<GameObject> _barrierObstacles = new List<GameObject>();

    [Header("Obstacles Inter-Voies")]
    [Tooltip("Je stocke les prefabs d'obstacles placés entre deux voies pour bloquer les changements de direction")]
    [SerializeField] private List<GameObject> _interLaneObstacles = new List<GameObject>();

    [Header("Spawn Configuration")]
    [Tooltip("Je définis le nombre MINIMUM d'obstacles que je vais générer par chunk")]
    [SerializeField] private int _minObstaclesPerChunk = 1;
    
    [Tooltip("Je définis le nombre MAXIMUM d'obstacles que je vais générer par chunk")]
    [SerializeField] private int _maxObstaclesPerChunk = 3;
    
    [Tooltip("J'active ou désactive la génération d'obstacles inter-voies")]
    [SerializeField] private bool _spawnInterLaneObstacles = true;
    
    [Tooltip("Je définis la probabilité (0-100%) de générer un obstacle inter-voies sur ce chunk")]
    [SerializeField] [Range(0f, 1f)] private float _interLaneSpawnChance = 0.2f;
    
    [Tooltip("Je définis la probabilité (0-100%) de choisir une barrière haute au lieu d'un obstacle normal")]
    [SerializeField] [Range(0f, 1f)] private float _barrierSpawnChance = 0.1f;

    [Header("Lane Positions (X)")]
    [Tooltip("Je stocke la position X de la voie de GAUCHE sur la piste")]
    [SerializeField] private float _leftLaneX = -1.84f;
    
    [Tooltip("Je stocke la position X de la voie du CENTRE sur la piste")]
    [SerializeField] private float _centerLaneX = 0f;
    
    [Tooltip("Je stocke la position X de la voie de DROITE sur la piste")]
    [SerializeField] private float _rightLaneX = 1.84f;

    [Header("Spawn Range Z")]
    [Tooltip("Je définis la position Z minimale où je peux placer un obstacle (début du chunk)")]
    [SerializeField] private float _minZ = 10f;
    
    [Tooltip("Je définis la position Z maximale où je peux placer un obstacle (fin du chunk)")]
    [SerializeField] private float _maxZ = 40f;
    
    [Tooltip("Je définis la distance MINIMALE entre deux obstacles pour laisser le temps au joueur de réagir")]
    [SerializeField] private float _minDistanceBetweenObstacles = 15f;
    
    [Tooltip("Je définis une distance ENCORE PLUS GRANDE pour les obstacles inter-voies (plus difficiles à éviter)")]
    [SerializeField] private float _minDistanceForInterLane = 20f;

    [Header("Height")]
    [Tooltip("Je définis la hauteur (Y) à laquelle je place les obstacles normaux au sol")]
    [SerializeField] private float _spawnHeight = 0f;
    
    [Tooltip("Je définis la hauteur (Y) à laquelle je place les barrières hautes (pour que le joueur doive se baisser)")]
    [SerializeField] private float _barrierHeight = 1.5f;

    [Header("Debug")]
    [Tooltip("J'active ou désactive l'affichage de logs détaillés dans la Console Unity pour déboguer")]
    [SerializeField] private bool _showDebugLogs = false;

    // Je stocke les positions Z où j'ai déjà placé des obstacles pour éviter les chevauchements
    private List<float> _occupiedZones = new List<float>();

    /// <summary>
    /// Je retourne une copie de mes zones occupées pour que le CollectibleSpawner puisse les éviter.
    /// Mon rôle : Permettre au CollectibleSpawner de placer les collectibles loin des obstacles.
    /// </summary>
    public List<float> GetOccupiedZones()
    {
        // Je retourne une COPIE pour éviter que d'autres scripts modifient ma liste interne
        return new List<float>(_occupiedZones);
    }

    /// <summary>
    /// Je suis appelé par le ChunkSpawner pour générer tous les obstacles de ce chunk.
    /// Mon rôle : Coordonner la génération complète des obstacles selon la phase de jeu actuelle.
    /// </summary>
    /// <param name="phase">La phase de piste actuelle (Green, Blue, Red, Black) qui détermine la difficulté</param>
    public void SpawnObstacles(TrackPhase phase = TrackPhase.Green)
    {
        // Je réinitialise ma liste des zones occupées pour ce nouveau chunk
        _occupiedZones.Clear();

        // Je récupère le chunk parent dans lequel je dois placer mes obstacles
        Transform chunkRoot = transform.parent;
        
        // Je vérifie que j'ai bien un parent (sécurité)
        if (chunkRoot == null)
        {
            Debug.LogError($"[ObstacleSpawner] {gameObject.name} n'a pas de parent ! Je ne peux pas spawner d'obstacles.");
            return;
        }

        // Je calcule combien d'obstacles je vais générer selon la phase de difficulté
        int obstacleCount = CalculateObstacleCount(phase);

        // J'affiche un log de debug si activé
        if (_showDebugLogs)
        {
            Debug.Log($"[ObstacleSpawner] Génération de {obstacleCount} obstacles sur {gameObject.name} (Phase: {phase})");
        }

        // Je génère tous mes obstacles principaux
        SpawnMainObstacles(chunkRoot, obstacleCount);
        
        // J'ai une chance de générer UN obstacle inter-voies supplémentaire (si activé et s'il y a de la place)
        if (_spawnInterLaneObstacles && Random.value < _interLaneSpawnChance && _occupiedZones.Count < 2)
        {
            SpawnInterLaneObstacle(chunkRoot);
        }
    }

    /// <summary>
    /// Je calcule le nombre d'obstacles à générer selon la phase de piste actuelle.
    /// Mon rôle : Adapter la quantité d'obstacles à la difficulté croissante du jeu.
    /// Plus la phase est avancée (Green → Black), plus je génère d'obstacles.
    /// </summary>
    /// <param name="phase">La phase de piste actuelle</param>
    /// <returns>Le nombre d'obstacles que je vais spawner (entre 1 et 3)</returns>
    private int CalculateObstacleCount(TrackPhase phase)
    {
        // Je détermine un nombre de base selon la phase de difficulté
        int baseCount = phase switch
        {
            TrackPhase.Green => 1,                      // Phase verte : toujours 1 obstacle (facile pour commencer)
            TrackPhase.Blue => Random.Range(1, 2),      // Phase bleue : 1 ou 2 obstacles aléatoirement
            TrackPhase.Red => Random.Range(2, 3),       // Phase rouge : 2 ou 3 obstacles
            TrackPhase.Black => Random.Range(2, 4),     // Phase noire : 2 à 3 obstacles (maximum de difficulté)
            _ => 1                                       // Cas par défaut : 1 obstacle
        };

        // Je m'assure que le nombre reste entre 1 et 3 pour éviter de saturer le chunk
        return Mathf.Clamp(baseCount, 1, 3);
    }

    /// <summary>
    /// Je génère tous les obstacles principaux du chunk.
    /// Mon rôle : Placer stratégiquement chaque obstacle avec assez d'espace entre eux.
    /// </summary>
    /// <param name="chunkRoot">Le transform parent où je vais placer mes obstacles</param>
    /// <param name="count">Le nombre d'obstacles que je dois générer</param>
    private void SpawnMainObstacles(Transform chunkRoot, int count)
    {
        // Je boucle pour générer chaque obstacle un par un
        for (int i = 0; i < count; i++)
        {
            // Je cherche une position Z valide qui ne chevauche pas les obstacles existants
            float spawnZ = FindValidZPosition();
            
            // Si je n'ai pas trouvé de position valide (chunk saturé)
            if (spawnZ < 0)
            {
                if (_showDebugLogs)
                {
                    Debug.LogWarning($"[ObstacleSpawner] Impossible de trouver une position Z valide pour l'obstacle {i + 1} - Je l'ignore.");
                }
                continue; // Je passe à l'obstacle suivant
            }

            // Je choisis aléatoirement quel type d'obstacle je vais créer
            ObstacleType type = ChooseObstacleType();
            
            // Je crée l'obstacle du type choisi à la position trouvée
            SpawnObstacleByType(chunkRoot, type, spawnZ);
            
            // Je marque cette position Z comme occupée pour éviter de placer un autre obstacle trop près
            _occupiedZones.Add(spawnZ);
        }
    }

    /// <summary>
    /// Je cherche une position Z valide pour placer un obstacle sans chevaucher les autres.
    /// Mon rôle : Garantir qu'il y a toujours assez d'espace entre chaque obstacle.
    /// Je respecte la distance minimale définie dans _minDistanceBetweenObstacles.
    /// </summary>
    /// <returns>Une position Z valide, ou -1 si je n'ai pas trouvé de place après 30 tentatives</returns>
    private float FindValidZPosition()
    {
        int attempts = 0;           // Je compte mes tentatives pour éviter une boucle infinie
        float candidateZ;           // Je stocke la position Z candidate à tester

        do
        {
            // Je tire une position Z aléatoire dans ma plage disponible
            candidateZ = Random.Range(_minZ, _maxZ);
            attempts++;

            // Si j'ai fait trop de tentatives, j'abandonne (le chunk est probablement saturé)
            if (attempts > 30)
            {
                return -1f; // Je retourne -1 pour signaler l'échec
            }

            // Je vérifie si cette position est assez loin de tous les obstacles existants
            bool isValid = true;
            foreach (float occupiedZ in _occupiedZones)
            {
                // Si cette position est trop proche d'un obstacle existant
                if (Mathf.Abs(candidateZ - occupiedZ) < _minDistanceBetweenObstacles)
                {
                    isValid = false; // Cette position n'est pas valide
                    break;           // Je passe à la tentative suivante
                }
            }

            // Si j'ai trouvé une position valide, je la retourne
            if (isValid)
            {
                return candidateZ;
            }

        } while (true); // Je continue jusqu'à trouver une position ou atteindre 30 tentatives
    }

    /// <summary>
    /// Je cherche une position Z valide pour un obstacle inter-voies.
    /// Mon rôle : Appliquer une distance ENCORE PLUS GRANDE car les obstacles inter-voies sont plus difficiles.
    /// Je respecte _minDistanceForInterLane qui est plus grand que _minDistanceBetweenObstacles.
    /// </summary>
    /// <returns>Une position Z valide, ou -1 si aucune position n'est disponible</returns>
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

            // Je vérifie avec une PLUS GRANDE distance pour les obstacles inter-voies
            bool isValid = true;
            foreach (float occupiedZ in _occupiedZones)
            {
                // J'utilise _minDistanceForInterLane au lieu de _minDistanceBetweenObstacles
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
    /// Je choisis aléatoirement quel type d'obstacle je vais créer.
    /// Mon rôle : Assurer une variété dans les patterns d'obstacles pour maintenir l'intérêt du joueur.
    /// Je vérifie d'abord les barrières (rares), puis je choisis parmi les types disponibles.
    /// </summary>
    /// <returns>Le type d'obstacle que je vais spawner</returns>
    private ObstacleType ChooseObstacleType()
    {
        // Je vérifie d'abord si je dois spawner une barrière (selon la probabilité configurée)
        if (_barrierObstacles.Count > 0 && Random.value < _barrierSpawnChance)
        {
            return ObstacleType.Barrier; // Je retourne une barrière (rare mais marquante)
        }

        // Je crée une liste de tous les types d'obstacles disponibles (qui ont des prefabs assignés)
        List<ObstacleType> availableTypes = new List<ObstacleType>();

        // J'ajoute chaque type seulement si j'ai des prefabs pour lui
        if (_singleLaneObstacles.Count > 0)
            availableTypes.Add(ObstacleType.SingleLane);

        if (_leftCenterObstacles.Count > 0)
            availableTypes.Add(ObstacleType.LeftCenter);

        if (_centerRightObstacles.Count > 0)
            availableTypes.Add(ObstacleType.CenterRight);

        if (_leftRightObstacles.Count > 0)
            availableTypes.Add(ObstacleType.LeftRight);

        // Si aucun type n'est disponible (configuration vide), j'affiche une erreur et retourne un défaut
        if (availableTypes.Count == 0)
        {
            Debug.LogWarning("[ObstacleSpawner] Aucun obstacle configuré ! Impossible de générer des obstacles.");
            return ObstacleType.SingleLane;
        }

        // Je choisis aléatoirement un type parmi ceux disponibles
        return availableTypes[Random.Range(0, availableTypes.Count)];
    }

    /// <summary>
    /// Je crée physiquement l'obstacle dans le chunk selon son type.
    /// Mon rôle : Instancier le bon prefab à la bonne position selon le type d'obstacle choisi.
    /// IMPORTANT : Pour SingleLane, je ne bloque qu'UNE voie. Pour les autres, je laisse toujours au moins une voie libre.
    /// </summary>
    /// <param name="chunkRoot">Le transform parent où je vais placer l'obstacle</param>
    /// <param name="type">Le type d'obstacle à créer</param>
    /// <param name="spawnZ">La position Z où je vais placer l'obstacle</param>
    private void SpawnObstacleByType(Transform chunkRoot, ObstacleType type, float spawnZ)
    {
        GameObject prefab = null;       // Je stocke le prefab que je vais instancier
        Vector3 position = Vector3.zero; // Je stocke la position où je vais le placer
        float height = _spawnHeight;     // Je stocke la hauteur (peut changer pour les barrières)

        switch (type)
        {
            case ObstacleType.SingleLane:
                // Je bloque UNE SEULE voie choisie au hasard (les 2 autres restent libres)
                if (_singleLaneObstacles.Count > 0)
                {
                    // Je choisis un prefab aléatoire dans ma liste d'obstacles 1 voie
                    prefab = _singleLaneObstacles[Random.Range(0, _singleLaneObstacles.Count)];
                    
                    // Je crée un tableau des 3 positions X possibles (gauche, centre, droite)
                    float[] lanes = { _leftLaneX, _centerLaneX, _rightLaneX };
                    
                    // Je choisis UNE voie au hasard
                    float selectedLane = lanes[Random.Range(0, 3)];
                    
                    // Je définis la position de mon obstacle
                    position = new Vector3(selectedLane, height, spawnZ);
                    
                    // J'affiche un log de debug pour savoir quelle voie j'ai bloquée
                    if (_showDebugLogs)
                    {
                        string laneName = selectedLane == _leftLaneX ? "Gauche" : selectedLane == _centerLaneX ? "Centre" : "Droite";
                        Debug.Log($"[ObstacleSpawner] Obstacle 1 voie sur {laneName} à Z={spawnZ}");
                    }
                }
                break;

            case ObstacleType.LeftCenter:
                // Je bloque les voies GAUCHE et CENTRE (la voie de DROITE reste libre)
                if (_leftCenterObstacles.Count > 0)
                {
                    // Je choisis un prefab aléatoire
                    prefab = _leftCenterObstacles[Random.Range(0, _leftCenterObstacles.Count)];
                    
                    // Je calcule la position centrale entre la voie gauche et la voie centrale
                    // Cela permet de placer un seul prefab large qui couvre les 2 voies
                    float centerX = (_leftLaneX + _centerLaneX) / 2f;
                    position = new Vector3(centerX, height, spawnZ);
                    
                    if (_showDebugLogs)
                    {
                        Debug.Log($"[ObstacleSpawner] Obstacle 2 voies (Gauche+Centre) à Z={spawnZ} - DROITE LIBRE");
                    }
                }
                break;

            case ObstacleType.CenterRight:
                // Je bloque les voies CENTRE et DROITE (la voie de GAUCHE reste libre)
                if (_centerRightObstacles.Count > 0)
                {
                    prefab = _centerRightObstacles[Random.Range(0, _centerRightObstacles.Count)];
                    
                    // Je calcule la position centrale entre la voie centrale et la voie droite
                    float centerX = (_centerLaneX + _rightLaneX) / 2f;
                    position = new Vector3(centerX, height, spawnZ);
                    
                    if (_showDebugLogs)
                    {
                        Debug.Log($"[ObstacleSpawner] Obstacle 2 voies (Centre+Droite) à Z={spawnZ} - GAUCHE LIBRE");
                    }
                }
                break;

            case ObstacleType.LeftRight:
                // Je bloque les voies GAUCHE et DROITE (la voie du CENTRE reste libre)
                if (_leftRightObstacles.Count > 0)
                {
                    prefab = _leftRightObstacles[Random.Range(0, _leftRightObstacles.Count)];
                    
                    // Pour ce type, je spawne DEUX obstacles séparés (un à gauche, un à droite)
                    // Je crée le premier obstacle sur la voie de GAUCHE
                    GameObject leftObstacle = Instantiate(prefab, chunkRoot);
                    leftObstacle.transform.localPosition = new Vector3(_leftLaneX, height, spawnZ);
                    leftObstacle.transform.localRotation = Quaternion.identity;
                    leftObstacle.tag = "Obstacle"; // Je m'assure qu'il a le bon tag pour les collisions
                    
                    // Je crée le second obstacle sur la voie de DROITE
                    GameObject rightObstacle = Instantiate(prefab, chunkRoot);
                    rightObstacle.transform.localPosition = new Vector3(_rightLaneX, height, spawnZ);
                    rightObstacle.transform.localRotation = Quaternion.identity;
                    rightObstacle.tag = "Obstacle";
                    
                    if (_showDebugLogs)
                    {
                        Debug.Log($"[ObstacleSpawner] Obstacle 2 voies (Gauche+Droite) à Z={spawnZ} - CENTRE LIBRE");
                    }
                    
                    return; // Je quitte ici car j'ai déjà instancié les 2 obstacles
                }
                break;

            case ObstacleType.Barrier:
                // Je crée une barrière haute que le joueur doit éviter en se BAISSANT
                // Les barrières bloquent gauche+droite en HAUTEUR, le centre reste libre au sol
                if (_barrierObstacles.Count > 0)
                {
                    prefab = _barrierObstacles[Random.Range(0, _barrierObstacles.Count)];
                    
                    // J'utilise une hauteur spéciale pour les barrières (plus haute)
                    height = _barrierHeight;
                    
                    // Je spawne DEUX barrières hautes, une de chaque côté
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
                        Debug.Log($"[ObstacleSpawner] Barrière haute à Z={spawnZ} - SE BAISSER AU CENTRE");
                    }
                    
                    return; // Je quitte ici car j'ai déjà instancié les 2 barrières
                }
                break;
        }

        // J'instancie l'obstacle pour les types SingleLane, LeftCenter, CenterRight
        // (LeftRight et Barrier ont déjà été instanciés dans leur case et ont fait return)
        if (prefab != null)
        {
            GameObject obstacle = Instantiate(prefab, chunkRoot);
            obstacle.transform.localPosition = position;
            obstacle.transform.localRotation = Quaternion.identity; // Pas de rotation
            obstacle.tag = "Obstacle"; // Je m'assure qu'il a le bon tag pour la détection de collision
        }
    }

    /// <summary>
    /// Je génère un obstacle inter-voies pour rendre le changement de voie plus difficile.
    /// Mon rôle : Placer des obstacles ENTRE deux voies pour bloquer les déplacements latéraux du joueur.
    /// Ces obstacles forcent le joueur à anticiper ses changements de voie à l'avance.
    /// </summary>
    /// <param name="chunkRoot">Le transform parent où je vais placer l'obstacle</param>
    private void SpawnInterLaneObstacle(Transform chunkRoot)
    {
        // Si je n'ai pas de prefabs d'obstacles inter-voies configurés, je ne fais rien
        if (_interLaneObstacles.Count == 0) return;

        // Je cherche une position Z valide avec une distance ENCORE PLUS GRANDE
        float spawnZ = FindValidZPositionForInterLane();
        
        // Si je n'ai pas trouvé de position valide (pas assez de place)
        if (spawnZ < 0) 
        {
            if (_showDebugLogs)
            {
                Debug.Log("[ObstacleSpawner] Pas assez de place pour un obstacle inter-voies - Je l'ignore.");
            }
            return;
        }

        // Je définis les deux positions X possibles entre les voies
        // -0.92 = entre la voie gauche et la voie centrale
        //  0.92 = entre la voie centrale et la voie droite
        float[] interLanePositions = { -0.92f, 0.92f };
        
        // Je choisis un prefab aléatoire dans ma liste
        GameObject prefab = _interLaneObstacles[Random.Range(0, _interLaneObstacles.Count)];

        if (prefab != null)
        {
            // Je choisis aléatoirement une des deux positions inter-voies
            float randomX = interLanePositions[Random.Range(0, 2)];
            Vector3 localPosition = new Vector3(randomX, _spawnHeight, spawnZ);

            // J'instancie l'obstacle inter-voies
            GameObject obstacle = Instantiate(prefab, chunkRoot);
            obstacle.transform.localPosition = localPosition;
            obstacle.transform.localRotation = Quaternion.identity;
            obstacle.tag = "Obstacle";

            // Je marque cette position Z comme occupée
            _occupiedZones.Add(spawnZ);

            if (_showDebugLogs)
            {
                Debug.Log($"[ObstacleSpawner] Obstacle inter-voies spawné à {localPosition}");
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Je dessine des gizmos dans l'éditeur Unity pour visualiser ma configuration.
    /// Mon rôle : Aider le level designer à voir où se trouvent les voies et les zones de spawn.
    /// Cette méthode n'est appelée que dans l'éditeur Unity, jamais dans le jeu final.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Je dessine les 3 voies de la piste en ROUGE pour les voir facilement
        Gizmos.color = Color.red;
        Vector3 worldPos = transform.position;
        
        // Je dessine la voie de GAUCHE (ligne verticale rouge)
        Gizmos.DrawLine(worldPos + new Vector3(_leftLaneX, 0, 0), 
                       worldPos + new Vector3(_leftLaneX, 0, 50));
        
        // Je dessine la voie du CENTRE (ligne verticale rouge)
        Gizmos.DrawLine(worldPos + new Vector3(_centerLaneX, 0, 0), 
                       worldPos + new Vector3(_centerLaneX, 0, 50));
        
        // Je dessine la voie de DROITE (ligne verticale rouge)
        Gizmos.DrawLine(worldPos + new Vector3(_rightLaneX, 0, 0), 
                       worldPos + new Vector3(_rightLaneX, 0, 50));
        
        // Je dessine la zone de spawn Z en JAUNE (cube fil de fer)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldPos + new Vector3(0, 0, (_minZ + _maxZ) / 2), 
                           new Vector3(4, 2, _maxZ - _minZ));
        
        // Je dessine les zones de distance minimale en VERT (sphères autour de chaque obstacle)
        // Cela permet de visualiser l'espace de sécurité autour de chaque obstacle
        Gizmos.color = Color.green;
        foreach (float z in _occupiedZones)
        {
            Gizmos.DrawWireSphere(worldPos + new Vector3(0, 1, z), _minDistanceBetweenObstacles / 2);
        }
    }
#endif
}