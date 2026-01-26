using UnityEngine;

/// <summary>
/// Anime légèrement les skis pendant le mouvement.
/// Attach à SkiVisuals.
/// </summary>
public class SkiAnimator : MonoBehaviour
{
    [Header("Ski References")]
    [SerializeField] private Transform _skiLeft;
    [SerializeField] private Transform _skiRight;
    
    [Header("Animation Settings")]
    [SerializeField] private float _tiltAmount = 2f; // Angle de balancement
    [SerializeField] private float _tiltSpeed = 3f;
    
    private PlayerController _player;
    private float _tiltOffset = 0f;
    
    private void Start()
    {
        _player = GetComponentInParent<PlayerController>();
    }
    
    private void Update()
    {
        if (_player == null) return;
        
        // Balancement subtil pendant le mouvement
        _tiltOffset += Time.deltaTime * _tiltSpeed;
        float tilt = Mathf.Sin(_tiltOffset) * _tiltAmount;
        
        // Appliquer aux skis
        if (_skiLeft != null)
        {
            Vector3 leftRot = _skiLeft.localEulerAngles;
            leftRot.z = -tilt;
            _skiLeft.localEulerAngles = leftRot;
        }
        
        if (_skiRight != null)
        {
            Vector3 rightRot = _skiRight.localEulerAngles;
            rightRot.z = tilt;
            _skiRight.localEulerAngles = rightRot;
        }
    }
}