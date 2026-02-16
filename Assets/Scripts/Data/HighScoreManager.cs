using System.Linq;
using UnityEngine;

/// <summary>
/// Je gère les meilleurs scores (top 10).
/// Je peux recharger les scores depuis le JSON à tout moment.
/// </summary>
public class HighScoreManager : Singleton<HighScoreManager>
{
    private int[] _highScores = new int[10]; // Je stocke les 10 meilleurs scores

    // Je donne accès en lecture seule à mes scores
    public int[] HighScores => _highScores;

    // ---------------------------------------------------------
    // INITIALISATION
    // ---------------------------------------------------------
    /// <summary>
    /// Je m'initialise au démarrage du jeu
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // J'initialise le Singleton pour garantir qu'il n'y ait qu'une seule instance de moi
        
        // ✅ CORRECTION 1 : Je me détache de mon parent pour être à la racine
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        // ✅ CORRECTION 2 : Maintenant je peux me rendre persistant
        DontDestroyOnLoad(gameObject); // Je me rends persistant entre les changements de scènes
    }

    /// <summary>
    /// Je charge les scores au démarrage
    /// </summary>
    private void Start()
    {
        LoadHighScores(); // Je charge les scores depuis le fichier JSON
    }

    // ---------------------------------------------------------
    // LOAD (maintenant PUBLIC pour permettre le rechargement)
    // ---------------------------------------------------------
    /// <summary>
    /// Je charge les meilleurs scores depuis le fichier JSON
    /// Cette méthode est publique pour permettre un rechargement à tout moment
    /// </summary>
    public void LoadHighScores()
    {
        // Je vérifie que le SaveSystem existe
        if (SaveSystem.Instance != null)
        {
            // Je demande au SaveSystem de charger les données
            GameData data = SaveSystem.Instance.LoadData();
            
            // Je récupère les scores (ou je crée un tableau vide si null)
            _highScores = data.highScores ?? new int[10];

            // Je compte et j'affiche le nombre de scores valides (supérieurs à 0)
            Debug.Log($"[HighScoreManager] {_highScores.Count(s => s > 0)} scores chargés depuis le JSON.");
        }
        else
        {
            // Si le SaveSystem n'existe pas, j'affiche un avertissement
            Debug.LogWarning("[HighScoreManager] SaveSystem non trouvé, chargement impossible.");
        }
    }

    // ---------------------------------------------------------
    // ADD SCORE
    // ---------------------------------------------------------
    /// <summary>
    /// J'ajoute un nouveau score dans le top 10
    /// </summary>
    /// <param name="newScore">Le nouveau score à ajouter</param>
    public void AddScore(int newScore)
    {
        // Je place le nouveau score à la dernière position (index 9)
        _highScores[9] = newScore;

        // Je trie tous les scores par ordre décroissant (du plus grand au plus petit)
        _highScores = _highScores
            .OrderByDescending(s => s) // Je trie en ordre décroissant
            .ToArray(); // Je convertis le résultat en tableau

        // Je sauvegarde les scores mis à jour
        SaveHighScores();

        // J'affiche un message de confirmation avec le nouveau score et le meilleur score actuel
        Debug.Log($"[HighScoreManager] Nouveau score ajouté : {newScore}. Meilleur score : {_highScores[0]}");
    }

    // ---------------------------------------------------------
    // SAVE
    // ---------------------------------------------------------
    /// <summary>
    /// Je sauvegarde les meilleurs scores dans le fichier JSON
    /// </summary>
    private void SaveHighScores()
    {
        // Je vérifie que le SaveSystem existe
        if (SaveSystem.Instance != null)
        {
            // Je récupère les données actuelles du jeu via le SettingsManager
            GameData data = SettingsManager.Instance.GetCurrentData();
            
            // Je mets à jour les scores dans les données
            data.highScores = _highScores;

            // Je demande au SaveSystem de sauvegarder les données
            SaveSystem.Instance.SaveData(data);
        }
        else
        {
            // Si le SaveSystem n'existe pas, j'affiche un avertissement
            Debug.LogWarning("[HighScoreManager] SaveSystem non trouvé, sauvegarde impossible.");
        }
    }

    // ---------------------------------------------------------
    // CHECK HIGH SCORE
    // ---------------------------------------------------------
    /// <summary>
    /// Je vérifie si un score peut entrer dans le top 10
    /// </summary>
    /// <param name="score">Le score à vérifier</param>
    /// <returns>True si le score peut entrer dans le top 10, False sinon</returns>
    public bool IsHighScore(int score)
    {
        // Je compare le score avec le plus petit score du top 10 (position 9)
        // Si le score est supérieur, il peut entrer dans le top 10
        return score > _highScores[9];
    }

    // ---------------------------------------------------------
    // RANK
    // ---------------------------------------------------------
    /// <summary>
    /// Je calcule le rang d'un score dans le classement
    /// </summary>
    /// <param name="score">Le score dont je veux connaître le rang</param>
    /// <returns>Le rang du score (1 à 10), ou 0 s'il est hors du top 10</returns>
    public int GetScoreRank(int score)
    {
        // Je parcours tous les scores du top 10
        for (int i = 0; i < _highScores.Length; i++)
        {
            // Si le score est supérieur ou égal au score à cette position
            if (score >= _highScores[i])
                return i + 1; // Je retourne le rang (position + 1)
        }

        // Si le score n'est pas dans le top 10, je retourne 0
        return 0; // Hors top 10
    }

#if UNITY_EDITOR
    /// <summary>
    /// Je fournis un menu contextuel pour recharger manuellement les scores en éditeur
    /// Clic droit sur le composant → "Recharger Scores depuis JSON"
    /// </summary>
    [ContextMenu("Recharger Scores depuis JSON")]
    private void ReloadScoresMenu()
    {
        // Je recharge les scores depuis le JSON
        LoadHighScores();
        Debug.Log("[HighScoreManager] Scores rechargés manuellement");
    }

    /// <summary>
    /// Je fournis un menu contextuel pour afficher les scores actuels en éditeur
    /// Clic droit sur le composant → "Afficher Scores Actuels"
    /// </summary>
    [ContextMenu("Afficher Scores Actuels")]
    private void ShowCurrentScores()
    {
        Debug.Log("=== SCORES ACTUELS ===");
        
        // Je parcours tous mes scores
        for (int i = 0; i < _highScores.Length; i++)
        {
            // J'affiche uniquement les scores valides (supérieurs à 0)
            if (_highScores[i] > 0)
            {
                // J'affiche le rang et le score avec un formatage des milliers
                Debug.Log($"#{i + 1}: {_highScores[i]:N0}");
            }
        }
    }
#endif
}