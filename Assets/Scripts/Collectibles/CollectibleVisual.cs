using UnityEngine;

/// <summary>
/// Gère uniquement les effets visuels des collectibles (rotation, lévitation).
/// VERSION CORRIGÉE - Utilise localPosition pour que les collectibles suivent leur parent
/// </summary>
public class CollectibleVisual : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 50f;
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;

    [Header("Lévitation")]
    [SerializeField] private bool _enableLevitation = true;
    [SerializeField] private float _levitationAmplitude = 0.3f;
    [SerializeField] private float _levitationSpeed = 2f;

    private Vector3 _startLocalPosition;  // ✅ CHANGÉ : Position LOCALE au lieu de globale
    private float _levitationTimer;

    private void Start()
    {
        // ✅ CRITIQUE : Sauvegarder la position LOCALE au démarrage
        _startLocalPosition = transform.localPosition;
        _levitationTimer = Random.Range(0f, 2f * Mathf.PI); // Démarrage aléatoire
    }

    private void Update()
    {
        // Rotation continue (autour du centre local, pas de problème)
        transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime, Space.Self);

        // ✅ CORRECTION : Lévitation en utilisant localPosition
        if (_enableLevitation)
        {
            _levitationTimer += Time.deltaTime * _levitationSpeed;
            float yOffset = Mathf.Sin(_levitationTimer) * _levitationAmplitude;
            
            // ✅ IMPORTANT : Utiliser localPosition pour suivre le parent (chunk)
            transform.localPosition = _startLocalPosition + new Vector3(0, yOffset, 0);
        }
    }
}