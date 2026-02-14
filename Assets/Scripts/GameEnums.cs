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
    MainMenu,   // Je représente l'écran du menu principal où le joueur peut démarrer une partie ou accéder aux options
    Tutorial,   // Je représente la phase d'apprentissage pour les nouveaux joueurs qui découvrent les mécaniques
    Playing,    // Je représente l'état où le jeu est actif et le joueur skie en évitant les obstacles
    Paused,     // Je représente l'état où le jeu est mis en pause, le temps est figé et le joueur peut reprendre ou quitter
    GameOver    // Je représente l'écran de fin de partie (victoire ou défaite) avec affichage du score final
}

/// <summary>
/// Je définis les phases de la piste selon le code couleur des pistes de ski (GDD).
/// Chaque phase représente un niveau de difficulté croissant avec une vitesse différente.
/// Mon rôle : Déterminer la vitesse du joueur, la difficulté des obstacles, et l'apparence visuelle.
/// Je suis utilisé par le ChunkSpawner pour choisir les bons prefabs et par le PlayerController pour ajuster la vitesse.
/// </summary>
public enum TrackPhase
{
    Green,  // Je représente la piste verte : vitesse de 5 m/s, durée 0-60s (facile, début de partie, peu d'obstacles)
    Blue,   // Je représente la piste bleue : vitesse de 10 m/s, durée 60-120s (moyenne difficulté, obstacles modérés)
    Red,    // Je représente la piste rouge : vitesse de 15 m/s, durée 120-180s (difficile, beaucoup d'obstacles)
    Black   // Je représente la piste noire : vitesse de 20 m/s, durée 180s+ (très difficile, obstacles maximum, fin de partie)
}

/// <summary>
/// Je définis les différents types de chunks selon leur usage dans le jeu.
/// Mon rôle : Permettre au système de génération de choisir le bon type de chunk selon le contexte.
/// Je suis utilisé par le ChunkSpawner pour différencier les chunks de tutoriel des chunks normaux.
/// </summary>
public enum ChunkType
{
    Tutorial,   // Je représente les chunks simplifiés utilisés pour apprendre les mécaniques au joueur (pas d'obstacles dangereux)
    Normal,     // Je représente les chunks standards du jeu, les plus fréquents, avec obstacles et collectibles
    Transition, // Je représente les chunks de transition entre deux phases de piste (changement de couleur, effet visuel spécial)
    Boss        // Je représente les chunks spéciaux pour d'éventuelles mises à jour futures (événements spéciaux, défis uniques)
}

/// <summary>
/// Je définis les niveaux de difficulté des chunks.
/// Mon rôle : Permettre une graduation fine de la difficulté au sein d'une même phase.
/// Par exemple, une piste bleue peut contenir des chunks Easy, Medium ou Hard pour varier le challenge.
/// Je suis utilisé par l'ObstacleSpawner pour déterminer le nombre et la complexité des obstacles à générer.
/// </summary>
public enum ChunkDifficulty
{
    Easy,       // Je représente les chunks faciles avec peu d'obstacles espacés (1-2 obstacles simples, beaucoup d'espace)
    Medium,     // Je représente les chunks moyens avec une densité d'obstacles modérée (2-3 obstacles, espacement normal)
    Hard,       // Je représente les chunks difficiles avec beaucoup d'obstacles et des patterns complexes (3-4 obstacles, patterns serrés)
    VeryHard,   // Je représente les chunks très difficiles avec des patterns serrés et des obstacles rapprochés (4-5 obstacles)
    Extreme     // Je représente les chunks extrêmes réservés aux joueurs expérimentés (5-6 obstacles, piste noire tardive, patterns très complexes)
}

/// <summary>
/// Je définis les différents types d'obstacles que le joueur peut rencontrer.
/// Mon rôle : Déterminer quelle(s) voie(s) de la piste est/sont bloquée(s) par un obstacle.
/// Je garantis toujours qu'au moins une voie reste libre pour que le joueur puisse passer.
/// Je suis utilisé par l'ObstacleSpawner pour créer des patterns de gameplay variés et stratégiques.
/// </summary>
public enum ObstacleType
{
    SingleLane,    // Je représente un obstacle qui bloque UNE SEULE voie (gauche, centre OU droite) - les 2 autres voies sont libres
    LeftCenter,    // Je représente un obstacle qui bloque les voies GAUCHE et CENTRE - la voie de DROITE est libre
    CenterRight,   // Je représente un obstacle qui bloque les voies CENTRE et DROITE - la voie de GAUCHE est libre
    LeftRight,     // Je représente un obstacle qui bloque les voies GAUCHE et DROITE - la voie du CENTRE est libre
    Barrier        // Je représente une barrière haute qui bloque gauche+droite - le joueur doit se BAISSER au centre pour passer
}