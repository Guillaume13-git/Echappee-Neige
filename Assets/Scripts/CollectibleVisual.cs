using UnityEngine; 

public class CollectibleVisual : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _floatAmplitude = 0.15f;
    [SerializeField] private float _floatFrequency = 2f;

    private Vector3 _startPos;

    void Start() 
    {
        // Utilise transform (minuscule)
        _startPos = transform.position; 
    }

    void Update()
    {
        // 1. Rotation Continue (R majuscule à Rotate)
        transform.Rotate(Vector3.up * _rotationSpeed * Time.deltaTime);

        // 2. Petit mouvement de haut en bas (Lévitation)
        // Utilise .y (minuscule) pour la coordonnée
        float newY = _startPos.y + Mathf.Sin(Time.time * _floatFrequency) * _floatAmplitude;
        
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}