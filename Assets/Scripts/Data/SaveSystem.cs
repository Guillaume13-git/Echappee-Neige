using System.IO;
using UnityEngine;

/// <summary>
/// Système de sauvegarde/chargement en JSON.
/// Gère les paramètres, les meilleurs scores, et la persistance entre sessions.
/// </summary>
public class SaveSystem : Singleton<SaveSystem>
{
    [Header("Save Settings")]
    [SerializeField] private string _saveFileName = "echappeeNeige_data.json";

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, _saveFileName);


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    protected override void Awake()
    {
        base.Awake(); // ← Initialise le Singleton
        DontDestroyOnLoad(gameObject);
    }


    // ---------------------------------------------------------
    // SAVE
    // ---------------------------------------------------------
    public void SaveData(GameData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true); // prettyPrint
            File.WriteAllText(SaveFilePath, json);

            Debug.Log($"[SaveSystem] Données sauvegardées : {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Erreur de sauvegarde : {e.Message}");
        }
    }


    // ---------------------------------------------------------
    // LOAD
    // ---------------------------------------------------------
    public GameData LoadData()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.Log("[SaveSystem] Aucune sauvegarde trouvée, création des données par défaut.");
            return CreateDefaultData();
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            Debug.Log($"[SaveSystem] Données chargées : {SaveFilePath}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Erreur de chargement : {e.Message}");
            return CreateDefaultData();
        }
    }


    // ---------------------------------------------------------
    // DEFAULT DATA
    // ---------------------------------------------------------
    private GameData CreateDefaultData()
    {
        return new GameData
        {
            musicVolume = 0.5f,
            sfxVolume = 0.5f,
            showTutorial = true,
            highScores = new int[10]
        };
    }


    // ---------------------------------------------------------
    // DELETE SAVE
    // ---------------------------------------------------------
    public void DeleteSaveFile()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("[SaveSystem] Sauvegarde supprimée.");
        }
    }


    // ---------------------------------------------------------
    // CHECK SAVE
    // ---------------------------------------------------------
    public bool SaveFileExists()
    {
        return File.Exists(SaveFilePath);
    }
}


// ---------------------------------------------------------
// STRUCTURE DES DONNÉES
// ---------------------------------------------------------
[System.Serializable]
public class GameData
{
    public float musicVolume = 0.5f;
    public float sfxVolume = 0.5f;

    public bool showTutorial = true;

    public int[] highScores = new int[10];
}