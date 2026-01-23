/// <summary>
/// Énumérations centrales du jeu.
/// CE FICHIER EST LA SEULE SOURCE DE VÉRITÉ POUR LES ENUMS.
/// </summary>

/// <summary>
/// États du jeu.
/// </summary>
public enum GameState
{
    MainMenu,
    Tutorial,      // ⭐ AJOUTÉ
    Playing,
    Paused,
    GameOver
}

/// <summary>
/// Phases de la piste (couleurs) selon le GDD.
/// </summary>
public enum TrackPhase
{
    Green,  // Verte : 5 m/s, 0-60s
    Blue,   // Bleue : 10 m/s, 60-120s
    Red,    // Rouge : 15 m/s, 120-180s
    Black   // Noire : 20 m/s, 180s+
}