using System.IO;
using UnityEngine;

/// <summary>
/// Je suis le système de sauvegarde/chargement en JSON.
/// Je gère les paramètres, les meilleurs scores, et la persistance entre sessions.
/// </summary>
public class SaveSystem : Singleton<SaveSystem>
{
    [Header("Save Settings")]
    [SerializeField] private string _saveFileName = "echappeeNeige_data.json"; // Je stocke le nom du fichier de sauvegarde

    // Je calcule le chemin complet du fichier de sauvegarde
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, _saveFileName);


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    /// <summary>
    /// Je m'initialise au démarrage du jeu
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // J'initialise le Singleton pour garantir qu'il n'y ait qu'une seule instance de moi
        DontDestroyOnLoad(gameObject); // Je me rends persistant entre les changements de scènes
    }


    // ---------------------------------------------------------
    // SAVE
    // ---------------------------------------------------------
    /// <summary>
    /// Je sauvegarde les données du jeu dans un fichier JSON
    /// </summary>
    /// <param name="data">Les données que je dois sauvegarder</param>
    public void SaveData(GameData data)
    {
        try
        {
            // Je convertis les données en format JSON avec une mise en forme lisible
            string json = JsonUtility.ToJson(data, true); // prettyPrint = true pour la lisibilité
            
            // J'écris le JSON dans le fichier
            File.WriteAllText(SaveFilePath, json);

            // Je confirme que la sauvegarde s'est bien passée
            Debug.Log($"[SaveSystem] Données sauvegardées : {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            // Si quelque chose ne va pas, j'affiche l'erreur
            Debug.LogError($"[SaveSystem] Erreur de sauvegarde : {e.Message}");
        }
    }


    // ---------------------------------------------------------
    // LOAD
    // ---------------------------------------------------------
    /// <summary>
    /// Je charge les données du jeu depuis le fichier JSON
    /// </summary>
    /// <returns>Les données chargées ou les données par défaut si le fichier n'existe pas</returns>
    public GameData LoadData()
    {
        // Je vérifie si le fichier de sauvegarde existe
        if (!File.Exists(SaveFilePath))
        {
            // Si le fichier n'existe pas, je crée des données par défaut
            Debug.Log("[SaveSystem] Aucune sauvegarde trouvée, création des données par défaut.");
            return CreateDefaultData();
        }

        try
        {
            // Je lis le contenu du fichier JSON
            string json = File.ReadAllText(SaveFilePath);
            
            // Je convertis le JSON en objet GameData
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Je confirme que le chargement s'est bien passé
            Debug.Log($"[SaveSystem] Données chargées : {SaveFilePath}");
            return data;
        }
        catch (System.Exception e)
        {
            // Si quelque chose ne va pas, j'affiche l'erreur et je retourne des données par défaut
            Debug.LogError($"[SaveSystem] Erreur de chargement : {e.Message}");
            return CreateDefaultData();
        }
    }


    // ---------------------------------------------------------
    // DEFAULT DATA
    // ---------------------------------------------------------
    /// <summary>
    /// Je crée des données par défaut pour un nouveau joueur
    /// </summary>
    /// <returns>Les données par défaut du jeu</returns>
    private GameData CreateDefaultData()
    {
        // Je retourne un nouvel objet GameData avec les valeurs par défaut
        return new GameData
        {
            musicVolume = 0.5f,      // Je mets le volume de la musique à 50%
            sfxVolume = 0.5f,        // Je mets le volume des effets sonores à 50%
            showTutorial = true,     // J'active le tutoriel pour les nouveaux joueurs
            highScores = new int[10] // Je crée un tableau vide pour les 10 meilleurs scores
        };
    }


    // ---------------------------------------------------------
    // DELETE SAVE
    // ---------------------------------------------------------
    /// <summary>
    /// Je supprime le fichier de sauvegarde
    /// </summary>
    public void DeleteSaveFile()
    {
        // Je vérifie si le fichier existe avant de le supprimer
        if (File.Exists(SaveFilePath))
        {
            // Je supprime le fichier
            File.Delete(SaveFilePath);
            
            // Je confirme la suppression
            Debug.Log("[SaveSystem] Sauvegarde supprimée.");
        }
    }


    // ---------------------------------------------------------
    // CHECK SAVE
    // ---------------------------------------------------------
    /// <summary>
    /// Je vérifie si un fichier de sauvegarde existe
    /// </summary>
    /// <returns>True si le fichier existe, False sinon</returns>
    public bool SaveFileExists()
    {
        // Je retourne true si le fichier existe, false sinon
        return File.Exists(SaveFilePath);
    }
}


// ---------------------------------------------------------
// STRUCTURE DES DONNÉES
// ---------------------------------------------------------
/// <summary>
/// Je représente la structure des données sauvegardées du jeu.
/// Je contiens tous les paramètres et scores qui doivent persister entre les sessions.
/// </summary>
[System.Serializable]
public class GameData
{
    public float musicVolume = 0.5f;  // Je stocke le volume de la musique (0 à 1)
    public float sfxVolume = 0.5f;    // Je stocke le volume des effets sonores (0 à 1)

    public bool showTutorial = true;  // Je stocke si le tutoriel doit être affiché

    public int[] highScores = new int[10]; // Je stocke les 10 meilleurs scores du jeu
}