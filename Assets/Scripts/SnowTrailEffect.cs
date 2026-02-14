using UnityEngine;

/// <summary>
/// Je gère l'effet de neige/poudreuse qui se soulève au niveau des skis.
/// Mon rôle : Créer un effet visuel immersif qui réagit à la vitesse du joueur.
/// 
/// Ce que je fais :
/// - Je génère des particules de neige au contact des skis avec la piste
/// - J'adapte l'intensité des particules selon la vitesse du joueur
/// - Je crée des traînées de neige différentes pour chaque ski
/// 
/// Mon emplacement : Je suis attaché au GameObject du joueur
/// Mon utilisation : Je détecte automatiquement la vitesse et ajuste les particules
/// </summary>
public class SnowTrailEffect : MonoBehaviour
{
    [Header("🎿 Ski Positions")]
    [Tooltip("Position du ski gauche (en position locale par rapport au joueur)")]
    [SerializeField] private Transform _leftSkiPosition;
    
    [Tooltip("Position du ski droit (en position locale par rapport au joueur)")]
    [SerializeField] private Transform _rightSkiPosition;

    [Header("❄️ Particle Systems")]
    [Tooltip("Système de particules pour le ski gauche (créé automatiquement si null)")]
    [SerializeField] private ParticleSystem _leftSnowParticles;
    
    [Tooltip("Système de particules pour le ski droit (créé automatiquement si null)")]
    [SerializeField] private ParticleSystem _rightSnowParticles;

    [Header("⚙️ Effect Configuration")]
    [Tooltip("Nombre de particules par seconde à vitesse minimale")]
    [SerializeField] private float _minEmissionRate = 5f; 
    
    [Tooltip("Nombre de particules par seconde à vitesse maximale")]
    [SerializeField] private float _maxEmissionRate = 50f;
    
    [Tooltip("Vitesse du joueur en m/s à partir de laquelle l'effet est maximal")]
    [SerializeField] private float _maxSpeedThreshold = 20f;

    [Header("🎨 Visual Settings")]
    [Tooltip("Couleur de la neige (blanc par défaut)")]
    [SerializeField] private Color _snowColor = Color.white;
    
    [Tooltip("Taille des particules de neige")]
    [SerializeField] private Vector2 _particleSizeRange = new Vector2(0.05f, 0.15f);
    
    [Tooltip("Vitesse initiale des particules")]
    [SerializeField] private Vector2 _particleSpeedRange = new Vector2(1f, 3f);

    [Header("🔧 Debug")]
    [Tooltip("J'affiche des logs de debug pour vérifier mon fonctionnement")]
    [SerializeField] private bool _showDebugLogs = false;
    
    [Tooltip("J'affiche la vitesse actuelle et l'intensité des particules")]
    [SerializeField] private bool _showDebugUI = false;

    // Je stocke la référence au Rigidbody ou au script de mouvement pour détecter la vitesse
    private Rigidbody _rigidbody;
    private float _currentSpeed;
    
    // Je stocke les modules d'émission pour les modifier en temps réel
    private ParticleSystem.EmissionModule _leftEmission;
    private ParticleSystem.EmissionModule _rightEmission;

    /// <summary>
    /// Au démarrage, j'initialise mes systèmes de particules de neige.
    /// Mon rôle : Créer automatiquement les particules si elles n'existent pas.
    /// </summary>
    private void Start()
    {
        // Je cherche le Rigidbody pour détecter la vitesse du joueur
        _rigidbody = GetComponent<Rigidbody>();

        // Si les positions des skis ne sont pas définies, je les crée automatiquement
        if (_leftSkiPosition == null)
        {
            _leftSkiPosition = CreateSkiPosition("LeftSkiPosition", new Vector3(-0.2f, 0f, 0.5f));
        }

        if (_rightSkiPosition == null)
        {
            _rightSkiPosition = CreateSkiPosition("RightSkiPosition", new Vector3(0.2f, 0f, 0.5f));
        }

        // Je crée les systèmes de particules s'ils n'existent pas
        if (_leftSnowParticles == null)
        {
            _leftSnowParticles = CreateSnowParticleSystem(_leftSkiPosition, "LeftSnowTrail");
        }

        if (_rightSnowParticles == null)
        {
            _rightSnowParticles = CreateSnowParticleSystem(_rightSkiPosition, "RightSnowTrail");
        }

        // Je récupère les modules d'émission pour pouvoir les modifier en temps réel
        _leftEmission = _leftSnowParticles.emission;
        _rightEmission = _rightSnowParticles.emission;

        if (_showDebugLogs)
        {
            Debug.Log($"[SnowTrailEffect] ✓ Initialisé sur {gameObject.name}");
            Debug.Log($"[SnowTrailEffect] Ski gauche : {_leftSkiPosition.name}");
            Debug.Log($"[SnowTrailEffect] Ski droit : {_rightSkiPosition.name}");
        }
    }

    /// <summary>
    /// Je crée automatiquement une position de ski si elle n'existe pas.
    /// Mon rôle : Simplifier la configuration pour le développeur.
    /// </summary>
    private Transform CreateSkiPosition(string name, Vector3 localPosition)
    {
        GameObject skiPos = new GameObject(name);
        skiPos.transform.SetParent(transform);
        skiPos.transform.localPosition = localPosition;
        skiPos.transform.localRotation = Quaternion.identity;

        if (_showDebugLogs)
        {
            Debug.Log($"[SnowTrailEffect] Position de ski '{name}' créée automatiquement à {localPosition}");
        }

        return skiPos.transform;
    }

    /// <summary>
    /// Je crée un système de particules de neige configuré pour l'effet de ski.
    /// Mon rôle : Générer automatiquement des particules réalistes.
    /// </summary>
    private ParticleSystem CreateSnowParticleSystem(Transform parent, string name)
    {
        // Je crée un nouveau GameObject pour les particules
        GameObject particleObj = new GameObject(name);
        particleObj.transform.SetParent(parent);
        particleObj.transform.localPosition = Vector3.zero;
        particleObj.transform.localRotation = Quaternion.identity;

        // J'ajoute le composant ParticleSystem
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

        // --- MODULE PRINCIPAL ---
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);      // Durée de vie courte (neige qui retombe vite)
        main.startSpeed = new ParticleSystem.MinMaxCurve(_particleSpeedRange.x, _particleSpeedRange.y);
        main.startSize = new ParticleSystem.MinMaxCurve(_particleSizeRange.x, _particleSizeRange.y);
        main.startColor = _snowColor;
        main.gravityModifier = 0.3f;                                           // Légère gravité pour que la neige retombe
        main.simulationSpace = ParticleSystemSimulationSpace.World;            // World space pour que les particules restent en place
        main.maxParticles = 100;

        // --- MODULE EMISSION ---
        var emission = ps.emission;
        emission.rateOverTime = _minEmissionRate;                              // Je commence avec peu de particules

        // --- MODULE SHAPE (Forme d'émission) ---
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;                        // Cône pour simuler la projection de neige
        shape.angle = 25f;                                                      // Angle d'ouverture du cône
        shape.radius = 0.1f;                                                    // Rayon de base du cône
        shape.rotation = new Vector3(-90f, 0f, 0f);                            // Je pointe vers l'arrière/bas

        // --- MODULE COLOR OVER LIFETIME ---
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f),                              // Opaque au début
                new GradientAlphaKey(0.0f, 1.0f)                               // Transparent à la fin (disparaît)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        // --- MODULE SIZE OVER LIFETIME ---
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 1.0f);                                          // Taille normale au début
        sizeCurve.AddKey(1.0f, 0.5f);                                          // Rétrécit en disparaissant
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

        // --- MODULE VELOCITY OVER LIFETIME (Mouvement) ---
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);             // Mouvement latéral aléatoire
        velocity.y = new ParticleSystem.MinMaxCurve(-1.0f, -0.5f);            // Descente progressive
        velocity.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);             // Mouvement avant/arrière aléatoire

        // --- MODULE RENDERER ---
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Je cherche ou crée un material pour les particules
        Material particleMat = Resources.Load<Material>("Default-Particle");
        if (particleMat == null)
        {
            particleMat = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
        }
        renderer.material = particleMat;

        if (_showDebugLogs)
        {
            Debug.Log($"[SnowTrailEffect] Système de particules '{name}' créé");
        }

        return ps;
    }

    /// <summary>
    /// À chaque frame, je calcule la vitesse actuelle et j'ajuste l'intensité des particules.
    /// Mon rôle : Créer un effet dynamique qui réagit au mouvement du joueur.
    /// </summary>
    private void Update()
    {
        // Je calcule la vitesse actuelle du joueur
        CalculateCurrentSpeed();

        // J'ajuste l'intensité des particules selon la vitesse
        UpdateParticleEmission();
    }

    /// <summary>
    /// Je calcule la vitesse actuelle du joueur.
    /// Mon rôle : Détecter la vitesse pour adapter l'effet de neige.
    /// </summary>
    private void CalculateCurrentSpeed()
    {
        if (_rigidbody != null)
        {
            // Je récupère la vitesse depuis le Rigidbody (plus précis)
            _currentSpeed = _rigidbody.linearVelocity.magnitude;
        }
        else
        {
            // Sinon j'estime la vitesse via le ParentController si disponible
            // (Tu peux remplacer ceci par une référence à ton PlayerController)
            _currentSpeed = 10f; // Valeur par défaut pour tester
        }
    }

    /// <summary>
    /// Je mets à jour l'émission de particules selon la vitesse actuelle.
    /// Mon rôle : Plus le joueur va vite, plus la neige se soulève.
    /// </summary>
    private void UpdateParticleEmission()
    {
        // Je calcule un pourcentage de vitesse (0 = arrêt, 1 = vitesse max)
        float speedPercent = Mathf.Clamp01(_currentSpeed / _maxSpeedThreshold);

        // Je calcule le taux d'émission en interpolant entre min et max
        float targetEmissionRate = Mathf.Lerp(_minEmissionRate, _maxEmissionRate, speedPercent);

        // J'applique le même taux d'émission aux deux skis
        _leftEmission.rateOverTime = targetEmissionRate;
        _rightEmission.rateOverTime = targetEmissionRate;

        if (_showDebugLogs && Time.frameCount % 60 == 0) // Log toutes les 60 frames pour éviter le spam
        {
            Debug.Log($"[SnowTrailEffect] Vitesse: {_currentSpeed:F1} m/s | Émission: {targetEmissionRate:F1} particules/s");
        }
    }

    /// <summary>
    /// J'affiche des informations de debug à l'écran si activé.
    /// Mon rôle : Aider le développeur à visualiser la vitesse et l'intensité.
    /// </summary>
    private void OnGUI()
    {
        if (!_showDebugUI) return;

        // Je calcule le pourcentage de vitesse pour l'affichage
        float speedPercent = Mathf.Clamp01(_currentSpeed / _maxSpeedThreshold);

        // J'affiche les infos dans le coin supérieur gauche
        GUILayout.BeginArea(new Rect(10, 150, 300, 100));
        GUILayout.Label($"❄️ Snow Trail Effect", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
        GUILayout.Label($"Vitesse: {_currentSpeed:F1} m/s ({speedPercent * 100:F0}%)");
        GUILayout.Label($"Émission: {_leftEmission.rateOverTime.constant:F1} particules/s");
        GUILayout.EndArea();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Menu de validation de configuration.
    /// </summary>
    [ContextMenu("🔍 Valider Configuration")]
    private void ValidateConfiguration()
    {
        Debug.Log("========================================");
        Debug.Log($"[SnowTrailEffect] VALIDATION - {gameObject.name}");
        Debug.Log("========================================");
        
        Debug.Log($"Ski Gauche Position: {(_leftSkiPosition != null ? "✓" : "❌")}");
        Debug.Log($"Ski Droit Position: {(_rightSkiPosition != null ? "✓" : "❌")}");
        Debug.Log($"Particules Gauche: {(_leftSnowParticles != null ? "✓" : "❌")}");
        Debug.Log($"Particules Droite: {(_rightSnowParticles != null ? "✓" : "❌")}");
        Debug.Log($"Rigidbody: {(_rigidbody != null ? "✓" : "⚠️ Optionnel")}");
        
        Debug.Log($"\nConfiguration:");
        Debug.Log($"  Min Emission: {_minEmissionRate} particules/s");
        Debug.Log($"  Max Emission: {_maxEmissionRate} particules/s");
        Debug.Log($"  Max Speed: {_maxSpeedThreshold} m/s");
        
        Debug.Log("========================================");
    }

    /// <summary>
    /// Menu pour tester l'effet avec différentes vitesses.
    /// </summary>
    [ContextMenu("🧪 Test Effet (Vitesse Max)")]
    private void TestMaxSpeed()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[SnowTrailEffect] Lance le jeu (Play) pour tester !");
            return;
        }

        _currentSpeed = _maxSpeedThreshold;
        UpdateParticleEmission();
        Debug.Log($"[SnowTrailEffect] Test à vitesse maximale : {_maxSpeedThreshold} m/s");
    }

    /// <summary>
    /// Je dessine des gizmos pour visualiser les positions des skis dans l'éditeur.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_leftSkiPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_leftSkiPosition.position, 0.1f);
            Gizmos.DrawLine(_leftSkiPosition.position, _leftSkiPosition.position + Vector3.down * 0.3f);
        }

        if (_rightSkiPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_rightSkiPosition.position, 0.1f);
            Gizmos.DrawLine(_rightSkiPosition.position, _rightSkiPosition.position + Vector3.down * 0.3f);
        }
    }
#endif
}