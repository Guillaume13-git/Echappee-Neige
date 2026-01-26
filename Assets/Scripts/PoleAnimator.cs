using UnityEngine;

/// <summary>
/// Anime les bâtons de ski.
/// Attach à PoleVisuals.
/// </summary>
public class PoleAnimator : MonoBehaviour
{
    [Header("Pole References")]
    [SerializeField] private Transform _poleLeft;
    [SerializeField] private Transform _poleRight;
    
    [Header("Animation Settings")]
    [SerializeField] private float _swingAmount = 5f;
    [SerializeField] private float _swingSpeed = 4f;
    
    private PlayerController _player;
    private float _swingOffset = 0f;
    
    private void Start()
    {
        _player = GetComponentInParent<PlayerController>();
    }
    
    private void Update()
    {
        if (_player == null) return;
        
        // Mouvement alterné des bâtons
        _swingOffset += Time.deltaTime * _swingSpeed;
        float swing = Mathf.Sin(_swingOffset) * _swingAmount;
        
        // Appliquer rotation
        if (_poleLeft != null)
        {
            Vector3 leftRot = _poleLeft.localEulerAngles;
            leftRot.x = 15 + swing;
            _poleLeft.localEulerAngles = leftRot;
        }
        
        if (_poleRight != null)
        {
            Vector3 rightRot = _poleRight.localEulerAngles;
            rightRot.x = 15 - swing;
            _poleRight.localEulerAngles = rightRot;
        }
    }
}