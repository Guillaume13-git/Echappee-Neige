using UnityEngine;

/// <summary>
/// Je déplace le décor (chunks) vers l'arrière pour simuler l'avancement du joueur.
/// Le joueur reste fixe à Z = 0, c'est moi qui déplace le monde autour de lui.
/// </summary>
public class ChunkMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _chunksParent;  // Je stocke le parent contenant tous les chunks
    private PlayerController _player;                  // Je stocke la référence au joueur

    [Header("Settings")]
    [SerializeField] private bool _showDebugLogs = false;  // Je stocke si j'affiche les logs de debug

    private bool _isMoving = true;      // Je stocke si je dois déplacer les chunks
    private float _forcedSpeed = -1f;   // Je stocke une vitesse forcée (ex: Tutorial), -1 = désactivé

    /// <summary>
    /// Je récupère mes références au démarrage
    /// </summary>
    private void Start()
    {
        // Si le ChunksParent n'est pas assigné, je le cherche dans la scène
        if (_chunksParent == null)
        {
            GameObject chunksObj = GameObject.Find("ChunksParent");
            if (chunksObj != null)
                _chunksParent = chunksObj.transform;
        }

        // Je cherche le PlayerController dans la scène
        _player = FindFirstObjectByType<PlayerController>();

        // Je vérifie que j'ai trouvé mes références
        if (_chunksParent == null)
            Debug.LogError("[ChunkMover] ❌ ChunksParent introuvable !");
        
        if (_player == null)
            Debug.LogWarning("[ChunkMover] ⚠️ PlayerController introuvable au Start.");
    }

    /// <summary>
    /// Je déplace les chunks à chaque frame
    /// </summary>
    private void Update()
    {
        // Si je ne dois pas bouger ou si je n'ai pas de ChunksParent, je ne fais rien
        if (!_isMoving || _chunksParent == null) return;

        // ---------------------------------------------------------
        // VÉRIFICATION DE L'ÉTAT DU JEU
        // ---------------------------------------------------------
        
        // Je n'autorise le mouvement qu'en Playing ou Tutorial
        // CORRECTION : Avant, je ne vérifiais que Playing, ce qui bloquait le mouvement
        // si l'état n'était pas encore mis à jour au chargement de la scène
        if (_forcedSpeed < 0 && GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentState;
            
            // Si le jeu n'est ni en Playing ni en Tutorial, je ne bouge pas
            if (state != GameState.Playing && state != GameState.Tutorial)
                return;
        }

        // ---------------------------------------------------------
        // CALCUL DE LA VITESSE ACTUELLE
        // ---------------------------------------------------------
        
        // Je détermine quelle vitesse utiliser
        float currentSpeed = 0f;

        // PRIORITÉ 1 : Si une vitesse forcée est active (Tutorial)
        if (_forcedSpeed >= 0)
        {
            // J'utilise la vitesse forcée
            currentSpeed = _forcedSpeed;
        }
        // PRIORITÉ 2 : Sinon, j'utilise la vitesse du joueur
        else if (_player != null)
        {
            // Je récupère la vitesse actuelle du joueur
            // (qui prend en compte la phase, le boost, le chasse-neige, etc.)
            currentSpeed = _player.GetCurrentForwardSpeed();
        }

        // J'affiche la vitesse actuelle si le debug est activé
        if (_showDebugLogs) 
            Debug.Log($"[ChunkMover] Moving at: {currentSpeed} m/s");

        // ---------------------------------------------------------
        // DÉPLACEMENT DES CHUNKS
        // ---------------------------------------------------------
        
        // Je calcule le déplacement en Z pour cette frame
        // Négatif car je déplace les chunks vers l'arrière (le joueur avance visuellement)
        float movementZ = -currentSpeed * Time.deltaTime;
        
        // Je déplace tous les chunks vers l'arrière
        _chunksParent.Translate(0, 0, movementZ, Space.World);
    }

    #region Public API

    /// <summary>
    /// Je permets de forcer une vitesse de déplacement (utilisé par le Tutorial)
    /// </summary>
    /// <param name="speed">La vitesse à forcer en m/s</param>
    public void SetSpeed(float speed)
    {
        // Je mémorise la vitesse forcée
        _forcedSpeed = speed;
        
        if (_showDebugLogs) 
            Debug.Log($"[ChunkMover] Speed forced to: {speed}");
    }

    /// <summary>
    /// Je reprends la vitesse normale synchronisée avec le joueur
    /// </summary>
    public void ReleaseForcedSpeed()
    {
        // Je désactive la vitesse forcée (-1 = désactivé)
        _forcedSpeed = -1f;
    }

    /// <summary>
    /// J'arrête le mouvement des chunks
    /// </summary>
    public void StopMovement() => _isMoving = false;

    /// <summary>
    /// Je reprends le mouvement des chunks
    /// </summary>
    public void StartMovement() => _isMoving = true;

    /// <summary>
    /// Je réinitialise la position du ChunksParent à l'origine
    /// </summary>
    public void ResetPosition()
    {
        if (_chunksParent != null) 
            _chunksParent.position = Vector3.zero;
    }

    #endregion
}