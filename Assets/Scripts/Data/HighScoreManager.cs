using System.Linq;
using UnityEngine;

/// <summary>
/// Gère les meilleurs scores (top 10).
/// Tri automatique, sauvegarde persistante.
/// </summary>
public class HighScoreManager : Singleton<HighScoreManager>
{
    private int[] _highScores = new int[10];

    public int[] HighScores => _highScores;


    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    protected override void Awake()
    {
        base.Awake(); // ← Initialise le Singleton
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadHighScores();
    }


    // ---------------------------------------------------------
    // LOAD
    // ---------------------------------------------------------
    private void LoadHighScores()
    {
        if (SaveSystem.Instance != null)
        {
            GameData data = SaveSystem.Instance.LoadData();
            _highScores = data.highScores ?? new int[10];

            Debug.Log($"[HighScoreManager] {_highScores.Count(s => s > 0)} scores chargés.");
        }
        else
        {
            Debug.LogWarning("[HighScoreManager] SaveSystem non trouvé, chargement impossible.");
        }
    }


    // ---------------------------------------------------------
    // ADD SCORE
    // ---------------------------------------------------------
    public void AddScore(int newScore)
    {
        _highScores[9] = newScore;

        _highScores = _highScores
            .OrderByDescending(s => s)
            .ToArray();

        SaveHighScores();

        Debug.Log($"[HighScoreManager] Nouveau score ajouté : {newScore}. Meilleur score : {_highScores[0]}");
    }


    // ---------------------------------------------------------
    // SAVE
    // ---------------------------------------------------------
    private void SaveHighScores()
    {
        if (SaveSystem.Instance != null)
        {
            GameData data = SettingsManager.Instance.GetCurrentData();
            data.highScores = _highScores;

            SaveSystem.Instance.SaveData(data);
        }
        else
        {
            Debug.LogWarning("[HighScoreManager] SaveSystem non trouvé, sauvegarde impossible.");
        }
    }


    // ---------------------------------------------------------
    // CHECK HIGH SCORE
    // ---------------------------------------------------------
    public bool IsHighScore(int score)
    {
        return score > _highScores[9];
    }


    // ---------------------------------------------------------
    // RANK
    // ---------------------------------------------------------
    public int GetScoreRank(int score)
    {
        for (int i = 0; i < _highScores.Length; i++)
        {
            if (score >= _highScores[i])
                return i + 1;
        }

        return 0; // Hors top 10
    }
}