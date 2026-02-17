using UnityEngine;

/// <summary>
/// Je suis responsable de l'initialisation de chaque chunk de piste.
/// Mon rôle est de déclencher la génération procédurale des obstacles et collectibles au bon moment.
/// Je suis attaché à la racine de chaque prefab de chunk.
/// 
/// ✅ CORRECTION CRITIQUE :
/// Je résous le problème de timing où les objets spawnaient avec un parent incorrect.
/// En utilisant OnEnable(), je m'assure que le chunk a déjà été correctement parenté avant de générer son contenu.
/// </summary>
public class ChunkInitializer : MonoBehaviour
{
    [Header("Spawners")]
    // Je garde la référence vers le spawner d'obstacles qui va générer les sapins, rochers, etc.
    [SerializeField] private ObstacleSpawner _obstacleSpawner;
    
    // Je garde la référence vers le spawner de collectibles qui va générer les bonus
    [SerializeField] private CollectibleSpawner _collectibleSpawner;

    [Header("Settings")]
    // Je détermine si je dois spawner automatiquement à l'activation du chunk
    [SerializeField] private bool _spawnOnEnable = true;

    // Je garde en mémoire si j'ai déjà généré le contenu pour éviter les doublons
    private bool _hasSpawned = false;

    /// <summary>
    /// Quand je suis activé, je lance la génération du contenu du chunk.
    /// À ce moment, le ChunkSpawner a déjà fait SetParent() et SetActive(true),
    /// donc ma hiérarchie est stable et correcte.
    /// </summary>
    private void OnEnable()
    {
        // ✅ Point crucial : à ce stade, transform.parent est stabilisé et correct
        // Je ne spawne que si c'est autorisé et que je ne l'ai pas déjà fait
        if (_spawnOnEnable && !_hasSpawned)
        {
            InitializeChunk();
        }
    }

    /// <summary>
    /// Je déclenche la génération procédurale du contenu du chunk.
    /// C'est ma fonction principale qui orchestre tout le processus de spawn.
    /// </summary>
    public void InitializeChunk()
    {
        // Si j'ai déjà spawné, je ne refais rien (protection contre les appels multiples)
        if (_hasSpawned) return;

        // Je détermine dans quelle phase de jeu nous sommes (Verte, Bleue, Rouge, Noire)
        // Cela influence la difficulté et le type d'obstacles que je vais générer
        TrackPhase currentPhase = TrackPhase.Green;
        if (PhaseManager.Instance != null)
        {
            currentPhase = PhaseManager.Instance.CurrentPhase;
        }

        // Je log des informations de debug pour valider que ma hiérarchie est correcte
        // Très utile pour tracer les problèmes de parentage
        Debug.Log($"[ChunkInitializer] Initialisation de {gameObject.name} | Parent: {transform.parent?.name ?? "None"}");

        // Je demande au spawner d'obstacles de générer les obstacles selon la phase actuelle
        if (_obstacleSpawner != null)
        {
            _obstacleSpawner.SpawnObstacles(currentPhase);
        }

        // Je demande au spawner de collectibles de générer les bonus selon la phase actuelle
        if (_collectibleSpawner != null)
        {
            _collectibleSpawner.SpawnCollectibles(currentPhase);
        }

        // Je marque que j'ai terminé le spawn pour éviter de le refaire
        _hasSpawned = true;
    }

    /// <summary>
    /// Je nettoie le chunk pour le préparer à être recyclé (Object Pooling).
    /// Mon rôle est de détruire tous les obstacles et collectibles que j'ai générés.
    /// </summary>
    public void ResetChunk()
    {
        // 1. Je nettoie les enfants potentiels des spawners (par précaution)
        if (_obstacleSpawner != null) DestroySpawnedChildren(_obstacleSpawner.transform);
        if (_collectibleSpawner != null) DestroySpawnedChildren(_collectibleSpawner.transform);

        // 2. Je nettoie les objets directement attachés à mon chunk (via les Tags)
        // C'est ici que la majorité des objets seront détruits car ils sont parentés à moi
        foreach (Transform child in transform)
        {
            // Je détruis uniquement les obstacles et collectibles, pas les spawners ou autres composants fixes
            if (child.CompareTag("Obstacle") || child.CompareTag("Collectible"))
            {
                Destroy(child.gameObject);
            }
        }

        // Je réinitialise mon flag pour permettre un nouveau spawn lors de la prochaine activation
        _hasSpawned = false;
    }

    /// <summary>
    /// Je détruis les enfants d'un spawner tout en préservant les points de spawn.
    /// Les SpawnPoints sont des marqueurs de position que je veux garder.
    /// </summary>
    /// <param name="spawnerTransform">Le transform du spawner dont je dois nettoyer les enfants</param>
    private void DestroySpawnedChildren(Transform spawnerTransform)
    {
        // Je parcours tous les enfants du spawner
        foreach (Transform child in spawnerTransform)
        {
            // Je saute les SpawnPoints car ce sont des éléments permanents du prefab
            if (child.name.Contains("SpawnPoint")) continue;
            
            // Je détruis tout le reste (les objets que j'ai générés)
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Quand je suis désactivé, je me nettoie pour être prêt à être réutilisé.
    /// C'est important pour le système de pooling de chunks.
    /// </summary>
    private void OnDisable()
    {
        // Je me prépare pour ma prochaine réutilisation en me nettoyant complètement
        ResetChunk();
    }
}