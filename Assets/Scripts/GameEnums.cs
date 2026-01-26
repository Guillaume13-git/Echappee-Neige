/// <summary>
/// Énumérations centrales du jeu Échappée-Neige.
/// CE FICHIER EST LA SEULE SOURCE DE VÉRITÉ POUR TOUS LES ENUMS.
/// EMPLACEMENT : Assets/Scripts/Core/GameEnums.cs
/// </summary>

/// <summary>
/// États du jeu.
/// </summary>
public enum GameState
{
    MainMenu,
    Tutorial,
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

/// <summary>
/// Type de chunk selon son usage.
/// </summary>
public enum ChunkType
{
    Tutorial,   // Chunks simples pour le tutorial
    Normal,     // Chunks standard
    Transition, // Chunks de transition entre phases
    Boss        // Chunks spéciaux (futurs updates)
}

/// <summary>
/// Difficulté du chunk.
/// </summary>
public enum ChunkDifficulty
{
    Easy,
    Medium,
    Hard,
    VeryHard,
    Extreme
}