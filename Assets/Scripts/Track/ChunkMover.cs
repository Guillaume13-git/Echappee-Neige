using UnityEngine;

/// <summary>
/// Déplace le parent de tous les chunks pour simuler le mouvement du joueur.
/// Synchronisé avec PhaseManager pour ajuster la vitesse selon la phase.
/// </summary>
public class ChunkMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f; // Vitesse initiale (Green phase)
    [SerializeField] private Transform _chunksParent;
    
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;
    
    private bool _isMoving = true;
    private float _currentSpeed;
    
    private void Start()
    {
        _currentSpeed = _moveSpeed;
        
        // S'abonner aux changements de phase pour ajuster la vitesse
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
            
            if (_showDebugLogs)
                Debug.Log($"[ChunkMover] Abonné au PhaseManager");
        }
        
        if (_chunksParent == null)
        {
            Debug.LogWarning("[ChunkMover] ChunksParent non assigné ! Le mouvement ne fonctionnera pas.");
        }
        
        if (_showDebugLogs)
            Debug.Log($"[ChunkMover] Initialisé - Vitesse: {_currentSpeed} m/s");
    }
    
    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }
    
    private void Update()
    {
        if (!_isMoving || _chunksParent == null) return;
        
        // Déplacement vers l'arrière (négatif en Z)
        float movement = -_currentSpeed * Time.deltaTime;
        _chunksParent.Translate(0, 0, movement);
    }
    
    /// <summary>
    /// Callback appelé lors du changement de phase.
    /// Ajuste automatiquement la vitesse selon la phase.
    /// </summary>
    private void OnPhaseChanged(TrackPhase newPhase)
    {
        float newSpeed = newPhase switch
        {
            TrackPhase.Green => 5f,
            TrackPhase.Blue => 10f,
            TrackPhase.Red => 15f,
            TrackPhase.Black => 20f,
            _ => 5f
        };
        
        SetSpeed(newSpeed);
        
        if (_showDebugLogs)
            Debug.Log($"[ChunkMover] Phase changée : {newPhase} → Vitesse: {newSpeed} m/s");
    }
    
    /// <summary>
    /// Modifie la vitesse de déplacement.
    /// </summary>
    public void SetSpeed(float speed)
    {
        _currentSpeed = Mathf.Max(0f, speed);
        
        if (_showDebugLogs)
            Debug.Log($"[ChunkMover] Vitesse modifiée : {_currentSpeed} m/s");
    }
    
    /// <summary>
    /// Arrête le mouvement (utile pour pause ou fin de partie).
    /// </summary>
    public void StopMovement()
    {
        _isMoving = false;
        
        if (_showDebugLogs)
            Debug.Log("[ChunkMover] Mouvement arrêté");
    }
    
    /// <summary>
    /// Reprend le mouvement.
    /// </summary>
    public void StartMovement()
    {
        _isMoving = true;
        
        if (_showDebugLogs)
            Debug.Log("[ChunkMover] Mouvement repris");
    }
    
    /// <summary>
    /// Réinitialise la position du parent (nouvelle partie).
    /// </summary>
    public void ResetPosition()
    {
        if (_chunksParent != null)
        {
            _chunksParent.position = Vector3.zero;
            
            if (_showDebugLogs)
                Debug.Log("[ChunkMover] Position réinitialisée");
        }
    }
    
    /// <summary>
    /// Retourne la vitesse actuelle.
    /// </summary>
    public float GetCurrentSpeed()
    {
        return _currentSpeed;
    }
    
    /// <summary>
    /// Vérifie si le mouvement est actif.
    /// </summary>
    public bool IsMoving()
    {
        return _isMoving;
    }
}