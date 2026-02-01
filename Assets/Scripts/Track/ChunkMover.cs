using UnityEngine;

/// <summary>
/// Déplace le décor (chunks) vers l'arrière pour simuler l'avancement du joueur.
/// Le joueur reste à Z = 0.
/// </summary>
public class ChunkMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _chunksParent;
    private PlayerController _player;

    [Header("Settings")]
    [SerializeField] private bool _showDebugLogs = false;

    private bool _isMoving = true;
    private float _forcedSpeed = -1f; // Utilisé si on veut forcer une vitesse (ex: Tutorial)

    private void Start()
    {
        if (_chunksParent == null)
        {
            GameObject chunksObj = GameObject.Find("ChunksParent");
            if (chunksObj != null)
                _chunksParent = chunksObj.transform;
        }

        _player = FindFirstObjectByType<PlayerController>();

        if (_chunksParent == null)
            Debug.LogError("[ChunkMover] ❌ ChunksParent introuvable !");
        if (_player == null)
            Debug.LogWarning("[ChunkMover] ⚠️ PlayerController introuvable au Start.");
    }

    private void Update()
    {
        if (!_isMoving || _chunksParent == null) return;

        // ✅ CORRECTION : On autorise le mouvement en Playing ET en Tutorial.
        // Avant, on ne vérifiait que Playing, ce qui bloquait le mouvement en Gameplay
        // si l'état n'était pas encore mis à jour au moment du chargement de la scène.
        if (_forcedSpeed < 0 && GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentState;
            if (state != GameState.Playing && state != GameState.Tutorial)
                return;
        }

        // Logique de vitesse : Priorité à la vitesse forcée, sinon vitesse du joueur
        float currentSpeed = 0f;

        if (_forcedSpeed >= 0)
        {
            currentSpeed = _forcedSpeed;
        }
        else if (_player != null)
        {
            currentSpeed = _player.GetCurrentForwardSpeed();
        }

        if (_showDebugLogs) Debug.Log($"[ChunkMover] Moving at: {currentSpeed} m/s");

        float movementZ = -currentSpeed * Time.deltaTime;
        _chunksParent.Translate(0, 0, movementZ, Space.World);
    }

    #region Public API

    /// <summary>
    /// Permet de forcer une vitesse de déplacement (utilisé par le Tutorial).
    /// </summary>
    public void SetSpeed(float speed)
    {
        _forcedSpeed = speed;
        if (_showDebugLogs) Debug.Log($"[ChunkMover] Speed forced to: {speed}");
    }

    /// <summary>
    /// Reprend la vitesse synchronisée avec le joueur.
    /// </summary>
    public void ReleaseForcedSpeed()
    {
        _forcedSpeed = -1f;
    }

    public void StopMovement() => _isMoving = false;
    public void StartMovement() => _isMoving = true;

    public void ResetPosition()
    {
        if (_chunksParent != null) _chunksParent.position = Vector3.zero;
    }

    #endregion
}