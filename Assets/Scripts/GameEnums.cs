/// <summary>
/// Je regroupe toutes les énumérations centrales du jeu Échappée-Neige.
/// JE SUIS LA SEULE SOURCE DE VÉRITÉ POUR TOUS LES ENUMS DU PROJET.
/// Mon emplacement : Assets/Scripts/Core/GameEnums.cs
/// 
/// Mon rôle : Centraliser toutes les définitions d'enums pour éviter les duplications
/// et garantir la cohérence dans tout le code.
/// </summary>

/// <summary>
/// Je définis les différents états possibles du jeu.
/// J'aide à gérer les transitions entre les écrans et les modes de jeu.
/// </summary>
public enum GameState
{
    MainMenu,   // Je représente l'écran du menu principal
    Tutorial,   // Je représente la phase d'apprentissage pour les nouveaux joueurs
    Playing,    // Je représente l'état où le jeu est actif et le joueur joue
    Paused,     // Je représente l'état où le jeu est mis en pause
    GameOver    // Je représente l'écran de fin de partie (victoire ou défaite)
}

/// <summary>
/// Je définis les phases de la piste selon le code couleur des pistes de ski (GDD).
/// Chaque phase représente un niveau de difficulté croissant avec une vitesse différente.
/// Mon rôle : Déterminer la vitesse du joueur, la difficulté des obstacles, et l'apparence visuelle.
/// </summary>
public enum TrackPhase
{
    Green,  // Je représente la piste verte : 5 m/s, durée 0-60s (facile, début de partie)
    Blue,   // Je représente la piste bleue : 10 m/s, durée 60-120s (moyenne difficulté)
    Red,    // Je représente la piste rouge : 15 m/s, durée 120-180s (difficile)
    Black   // Je représente la piste noire : 20 m/s, durée 180s+ (très difficile, fin de partie)
}

/// <summary>
/// Je définis les différents types de chunks selon leur usage dans le jeu.
/// Mon rôle : Permettre au système de génération de choisir le bon type de chunk selon le contexte.
/// </summary>
public enum ChunkType
{
    Tutorial,   // Je représente les chunks simplifiés utilisés pour apprendre les mécaniques au joueur
    Normal,     // Je représente les chunks standards du jeu, les plus fréquents
    Transition, // Je représente les chunks de transition entre deux phases de piste (changement de couleur)
    Boss        // Je représente les chunks spéciaux pour d'éventuelles mises à jour futures (événements spéciaux)
}

/// <summary>
/// Je définis les niveaux de difficulté des chunks.
/// Mon rôle : Permettre une graduation fine de la difficulté au sein d'une même phase.
/// Par exemple, une piste bleue peut contenir des chunks Easy, Medium ou Hard.
/// </summary>
public enum ChunkDifficulty
{
    Easy,       // Je représente les chunks faciles avec peu d'obstacles espacés
    Medium,     // Je représente les chunks moyens avec une densité d'obstacles modérée
    Hard,       // Je représente les chunks difficiles avec beaucoup d'obstacles et des patterns complexes
    VeryHard,   // Je représente les chunks très difficiles avec des patterns serrés
    Extreme     // Je représente les chunks extrêmes réservés aux joueurs expérimentés (piste noire tardive)
}