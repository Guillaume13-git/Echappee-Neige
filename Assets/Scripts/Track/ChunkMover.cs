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
    }

    private void Update()
    {
        if (!_isMoving || _chunksParent == null) return;

        // On ne vérifie le GameManager que si on n'est pas en vitesse forcée (Tuto)
        if (_forcedSpeed < 0 && GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

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

    #region Public API (Fixes Errors)

    /// <summary>
    /// Ajouté pour corriger l'erreur dans TutorialManager.
    /// Permet de forcer la vitesse du décor.
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