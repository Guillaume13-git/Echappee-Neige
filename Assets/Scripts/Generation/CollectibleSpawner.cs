using UnityEngine;

/// <summary>
/// Spawne les collectibles sur un tronçon de piste.
/// Respecte les probabilités du GDD : Sucre d'orge (100%), Cadeau (25%), Boule (50%).
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    [Header("Collectible Prefabs")]
    [SerializeField] private GameObject _painEpicePrefab; // Score
    [SerializeField] private GameObject _sucreOrgePrefab; // Vitesse accélérée
    [SerializeField] private GameObject _cadeauPrefab; // Bouclier
    [SerializeField] private GameObject _bouleNoelPrefab; // -10% menace
    
    [Header("Spawn Positions")]
    [SerializeField] private Transform[] _collectibleSpawnPoints; // 3 positions (une par couloir)
    
    /// <summary>
    /// Génère les collectibles pour ce chunk.
    /// </summary>
    public void SpawnCollectibles(TrackPhase phase)
    {
        // Vérification
        if (_collectibleSpawnPoints.Length < 3)
        {
            Debug.LogWarning("[CollectibleSpawner] Moins de 3 positions de spawn configurées !");
            return;
        }
        
        // Positions utilisées (éviter doublons)
        bool[] positionsUsed = new bool[3];
        
        // 1. Sucre d'orge : toujours présent (100%)
        SpawnCollectible(_sucreOrgePrefab, ref positionsUsed);
        
        // 2. Cadeau : 25% de chance
        if (Random.value <= 0.25f)
        {
            SpawnCollectible(_cadeauPrefab, ref positionsUsed);
        }
        
        // 3. Boule de Noël : 50% de chance
        if (Random.value <= 0.5f)
        {
            SpawnCollectible(_bouleNoelPrefab, ref positionsUsed);
        }
        
        // 4. Pain d'épice : spawn aléatoire supplémentaire (optionnel)
        if (Random.value <= 0.3f && HasAvailablePosition(positionsUsed))
        {
            SpawnCollectible(_painEpicePrefab, ref positionsUsed);
        }
    }
    
    /// <summary>
    /// Spawn un collectible à une position aléatoire disponible.
    /// </summary>
    private void SpawnCollectible(GameObject prefab, ref bool[] positionsUsed)
    {
        if (prefab == null) return;
        
        // Cherche une position libre
        int attempts = 0;
        int randomIndex;
        
        do
        {
            randomIndex = Random.Range(0, 3);
            attempts++;
            if (attempts > 10) return; // Évite boucle infinie
        } while (positionsUsed[randomIndex]);
        
        // Marque la position comme utilisée
        positionsUsed[randomIndex] = true;
        
        // Position avec variation en Z
        Vector3 spawnPos = _collectibleSpawnPoints[randomIndex].position;
        spawnPos.z += Random.Range(10f, 40f);
        
        // Instanciation
        GameObject collectible = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        collectible.tag = "Collectible";
    }
    
    /// <summary>
    /// Vérifie s'il reste des positions disponibles.
    /// </summary>
    private bool HasAvailablePosition(bool[] positionsUsed)
    {
        foreach (bool used in positionsUsed)
        {
            if (!used) return true;
        }
        return false;
    }
}