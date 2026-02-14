using UnityEngine;

/// <summary>
/// Définition des types de collectibles. 
/// Note : Si cet Enum existe déjà dans un autre script (comme CollectibleLogic), 
/// tu peux supprimer ce bloc.
/// </summary>
public enum CollectibleType
{
    PainEpice,
    SucreOrge,
    Cadeau,
    BouleDeNoelRouge
}

/// <summary>
/// Je suis le gestionnaire complet des effets visuels d'un collectible.
/// Mon rôle : Créer une identité visuelle unique et immédiatement reconnaissable pour chaque type de collectible.
/// 
/// Ce que je gère :
/// - Système de particules principal (effet unique par type)
/// - Système de particules secondaire (sparkles/accents)
/// - Point Light pulsante avec couleur dédiée
/// - Trail Renderer pour effet de traînée
/// - Scale Pulsation pour effet de "respiration"
/// 
/// Mon emplacement : Je suis attaché à chaque prefab de collectible
/// Mon utilisation : Configuration automatique au Start() selon le type de collectible
/// </summary>
[RequireComponent(typeof(CollectibleVisual))]
public class CollectibleEffects : MonoBehaviour
{
    [Header("🎯 Configuration")]
    [Tooltip("Le type de ce collectible - DOIT correspondre au type dans CollectibleLogic")]
    [SerializeField] private CollectibleType _type = CollectibleType.PainEpice;

    [Header("✨ Particle Systems")]
    [Tooltip("Système de particules principal (effet unique)")]
    [SerializeField] private ParticleSystem _mainParticles;
    
    [Tooltip("Système de particules secondaire (sparkles)")]
    [SerializeField] private ParticleSystem _accentParticles;

    [Header("💡 Light Glow")]
    [Tooltip("J'active ou désactive l'effet de lumière pulsante")]
    [SerializeField] private bool _enableLight = true;
    
    [Tooltip("Référence vers la Point Light (créée automatiquement si null)")]
    [SerializeField] private Light _pointLight;
    
    [Tooltip("Vitesse de pulsation de la lumière")]
    [SerializeField] private float _lightPulseSpeed = 2f;
    
    [Tooltip("Amplitude de pulsation (variation d'intensité)")]
    [SerializeField] private float _lightPulseAmount = 0.5f;

    [Header("🎨 Trail Effect")]
    [Tooltip("J'active ou désactive l'effet de traînée")]
    [SerializeField] private bool _enableTrail = true;
    
    [Tooltip("Référence vers le Trail Renderer (créé automatiquement si null)")]
    [SerializeField] private TrailRenderer _trailRenderer;

    [Header("📏 Scale Pulsation")]
    [Tooltip("J'active ou désactive l'effet de pulsation d'échelle")]
    [SerializeField] private bool _enableScalePulsation = true;
    
    [Tooltip("Vitesse de pulsation de l'échelle")]
    [SerializeField] private float _scalePulseSpeed = 2f;
    
    [Tooltip("Amplitude de pulsation (0.1 = 10% de variation)")]
    [SerializeField] private float _scalePulseAmount = 0.1f;

    [Header("🔧 Debug")]
    [Tooltip("J'affiche des logs détaillés dans la console")]
    [SerializeField] private bool _showDebugLogs = false;

    // Je stocke les valeurs de base pour les pulsations
    private float _baseLightIntensity;
    private Vector3 _baseScale;

    /// <summary>
    /// Au démarrage, j'initialise tous mes effets visuels.
    /// Mon rôle : Créer et configurer automatiquement tous les effets selon le type.
    /// </summary>
    private void Start()
    {
        // Je sauvegarde l'échelle de base pour la pulsation
        _baseScale = transform.localScale;

        // Je cherche automatiquement les Particle Systems s'ils sont enfants
        if (_mainParticles == null)
        {
            ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
            if (systems.Length > 0) _mainParticles = systems[0];
            if (systems.Length > 1) _accentParticles = systems[1];
        }

        // Je configure tous les effets selon le type de collectible
        ConfigureEffectsForType();

        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleEffects] ✓ {gameObject.name} ({_type}) initialisé", this);
        }
    }

    /// <summary>
    /// Je configure tous les effets visuels selon le type de collectible.
    /// Mon rôle : Appliquer des paramètres uniques pour chaque type (couleur, intensité, comportement).
    /// </summary>
    private void ConfigureEffectsForType()
    {
        // Je définis les configurations spécifiques par type
        EffectConfig config = GetConfigForType(_type);

        // J'applique chaque effet avec sa configuration
        if (_enableLight) SetupPointLight(config);
        if (_enableTrail) SetupTrailRenderer(config);
        if (_mainParticles != null) ConfigureMainParticles(config);
        if (_accentParticles != null) ConfigureAccentParticles(config);
    }

    /// <summary>
    /// Je retourne la configuration d'effets pour un type donné.
    /// Mon rôle : Centraliser toutes les configurations dans une seule méthode.
    /// </summary>
    private EffectConfig GetConfigForType(CollectibleType type)
    {
        switch (type)
        {
            case CollectibleType.SucreOrge: // 🍬 Speed Boost - Énergétique Jaune
                return new EffectConfig
                {
                    primaryColor = new Color(1f, 0.9f, 0f),        // Jaune vif
                    secondaryColor = new Color(1f, 0.7f, 0f),      // Orange
                    lightIntensity = 2.5f,
                    lightRange = 3.5f,
                    trailWidth = 0.15f,
                    particleCount = 20,
                    particleSpeed = 1.5f,
                    particleShape = ParticleSystemShapeType.Cone
                };

            case CollectibleType.Cadeau: // 🎁 Shield - Protecteur Bleu
                return new EffectConfig
                {
                    primaryColor = new Color(0f, 0.6f, 1f),        // Bleu cyan
                    secondaryColor = new Color(0.3f, 0.8f, 1f),    // Bleu clair
                    lightIntensity = 3f,
                    lightRange = 4f,
                    trailWidth = 0.2f,
                    particleCount = 25,
                    particleSpeed = 0.8f,
                    particleShape = ParticleSystemShapeType.Sphere
                };

            case CollectibleType.BouleDeNoelRouge: // 🔴 Réduit Menace - Apaisant Vert
                return new EffectConfig
                {
                    primaryColor = new Color(0f, 1f, 0.5f),        // Vert menthe
                    secondaryColor = new Color(0.5f, 1f, 0.7f),    // Vert clair
                    lightIntensity = 2f,
                    lightRange = 3f,
                    trailWidth = 0.12f,
                    particleCount = 15,
                    particleSpeed = 0.5f,
                    particleShape = ParticleSystemShapeType.Hemisphere
                };

            case CollectibleType.PainEpice: // 🍪 Score - Doré Brillant
                return new EffectConfig
                {
                    primaryColor = new Color(1f, 0.7f, 0.2f),      // Doré
                    secondaryColor = new Color(1f, 0.9f, 0.5f),    // Doré clair
                    lightIntensity = 1.5f,
                    lightRange = 2.5f,
                    trailWidth = 0.1f,
                    particleCount = 10,
                    particleSpeed = 0.6f,
                    particleShape = ParticleSystemShapeType.Sphere
                };

            default:
                return new EffectConfig(); // Configuration par défaut
        }
    }

    /// <summary>
    /// Je crée et configure la Point Light pour l'effet de glow.
    /// Mon rôle : Ajouter une lumière colorée pulsante autour du collectible.
    /// </summary>
    private void SetupPointLight(EffectConfig config)
    {
        // Si aucune lumière n'existe, j'en crée une
        if (_pointLight == null)
        {
            GameObject lightObj = new GameObject("PointLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            _pointLight = lightObj.AddComponent<Light>();
        }

        // Je configure la lumière
        _pointLight.type = LightType.Point;
        _pointLight.color = config.primaryColor;
        _pointLight.intensity = config.lightIntensity;
        _pointLight.range = config.lightRange;
        _pointLight.shadows = LightShadows.None;

        // Je sauvegarde l'intensité de base pour la pulsation
        _baseLightIntensity = config.lightIntensity;

        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleEffects] Point Light créée - Color:{config.primaryColor}, Intensity:{config.lightIntensity}");
        }
    }

    /// <summary>
    /// Je crée et configure le Trail Renderer pour l'effet de traînée.
    /// Mon rôle : Ajouter une belle traînée colorée qui suit le collectible.
    /// </summary>
    private void SetupTrailRenderer(EffectConfig config)
    {
        // Si aucun trail n'existe, j'en crée un
        if (_trailRenderer == null)
        {
            _trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }

        // Je configure le trail
        _trailRenderer.time = 0.3f;                                    // Durée de la traînée
        _trailRenderer.startWidth = config.trailWidth;                 // Largeur au début
        _trailRenderer.endWidth = 0f;                                  // Largeur à la fin (0 = pointe)
        _trailRenderer.material = new Material(Shader.Find("Sprites/Default")); // Material simple
        _trailRenderer.material.color = config.primaryColor;

        // Je crée un gradient de couleur (couleur → transparent)
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(config.primaryColor, 0.0f), 
                new GradientColorKey(config.secondaryColor, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        _trailRenderer.colorGradient = gradient;

        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleEffects] Trail Renderer créé - Width:{config.trailWidth}");
        }
    }

    /// <summary>
    /// Je configure le système de particules principal.
    /// Mon rôle : Créer l'effet visuel unique qui définit le collectible.
    /// </summary>
    private void ConfigureMainParticles(EffectConfig config)
    {
        // --- MODULE PRINCIPAL ---
        var main = _mainParticles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(config.primaryColor, config.secondaryColor);
        main.startLifetime = 1.0f;
        main.startSpeed = config.particleSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        // --- MODULE EMISSION ---
        var emission = _mainParticles.emission;
        emission.rateOverTime = config.particleCount;

        // --- MODULE SHAPE ---
        var shape = _mainParticles.shape;
        shape.shapeType = config.particleShape;
        shape.radius = 0.5f;

        // --- MODULE COLOR OVER LIFETIME ---
        var colorOverLifetime = _mainParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        // --- MODULE SIZE OVER LIFETIME ---
        var sizeOverLifetime = _mainParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 0.0f);
        sizeCurve.AddKey(0.5f, 1.0f);
        sizeCurve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

        // --- EFFETS SPÉCIAUX PAR TYPE ---
        ConfigureSpecialEffects(config);

        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleEffects] Particules principales configurées - Count:{config.particleCount}");
        }
    }

    /// <summary>
    /// Je configure des effets spéciaux uniques selon le type de collectible.
    /// Mon rôle : Ajouter des mouvements et comportements spécifiques.
    /// </summary>
    private void ConfigureSpecialEffects(EffectConfig config)
    {
        switch (_type)
        {
            case CollectibleType.SucreOrge: // 🍬 Tourbillon vers le haut
                var velocitySpeed = _mainParticles.velocityOverLifetime;
                velocitySpeed.enabled = true;
                velocitySpeed.y = new ParticleSystem.MinMaxCurve(1.0f);
                velocitySpeed.speedModifier = new ParticleSystem.MinMaxCurve(0.5f);
                break;

            case CollectibleType.Cadeau: // 🎁 Orbite autour
                var velocityShield = _mainParticles.velocityOverLifetime;
                velocityShield.enabled = true;
                velocityShield.orbitalY = new ParticleSystem.MinMaxCurve(1.0f);
                velocityShield.radial = new ParticleSystem.MinMaxCurve(0.3f);
                break;

            case CollectibleType.BouleDeNoelRouge: // 🔴 Descente douce
                var velocityHeal = _mainParticles.velocityOverLifetime;
                velocityHeal.enabled = true;
                velocityHeal.y = new ParticleSystem.MinMaxCurve(-0.5f);
                break;

            case CollectibleType.PainEpice: // 🍪 Sparkles aléatoires
                var noise = _mainParticles.noise;
                noise.enabled = true;
                noise.strength = 0.3f;
                noise.frequency = 1.0f;
                break;
        }
    }

    /// <summary>
    /// Je configure le système de particules d'accent (sparkles).
    /// Mon rôle : Ajouter des petites particules scintillantes pour plus de polish.
    /// </summary>
    private void ConfigureAccentParticles(EffectConfig config)
    {
        var main = _accentParticles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(config.secondaryColor);
        main.startLifetime = 0.5f;
        main.startSpeed = 0.2f;
        main.startSize = 0.05f;
        main.maxParticles = 30;

        var emission = _accentParticles.emission;
        emission.rateOverTime = 5;

        var shape = _accentParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.6f;

        if (_showDebugLogs)
        {
            Debug.Log($"[CollectibleEffects] Particules d'accent configurées");
        }
    }

    /// <summary>
    /// À chaque frame, je mets à jour les effets de pulsation.
    /// Mon rôle : Animer la lumière et l'échelle pour un effet vivant.
    /// </summary>
    private void Update()
    {
        // --- PULSATION DE LA LUMIÈRE ---
        if (_enableLight && _pointLight != null)
        {
            float pulse = Mathf.Sin(Time.time * _lightPulseSpeed);
            _pointLight.intensity = _baseLightIntensity + (pulse * _lightPulseAmount);
        }

        // --- PULSATION DE L'ÉCHELLE ---
        if (_enableScalePulsation)
        {
            float pulse = Mathf.Sin(Time.time * _scalePulseSpeed);
            float scaleFactor = 1f + (pulse * _scalePulseAmount);
            transform.localScale = _baseScale * scaleFactor;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Menu de validation de configuration.
    /// </summary>
    [ContextMenu("🔍 Valider Configuration")]
    private void ValidateConfiguration()
    {
        Debug.Log("========================================");
        Debug.Log($"[CollectibleEffects] VALIDATION - {gameObject.name}");
        Debug.Log("========================================");
        
        Debug.Log($"Type: {_type}");
        Debug.Log($"Main Particles: {(_mainParticles != null ? "✓" : "❌")}");
        Debug.Log($"Accent Particles: {(_accentParticles != null ? "✓" : "❌")}");
        Debug.Log($"Point Light: {(_pointLight != null ? "✓" : "❌")}");
        Debug.Log($"Trail Renderer: {(_trailRenderer != null ? "✓" : "❌")}");
        Debug.Log($"CollectibleVisual: {(GetComponent<CollectibleVisual>() != null ? "✓" : "❌")}");
        
        Debug.Log("========================================");
    }

    /// <summary>
    /// Menu pour créer automatiquement les Particle Systems manquants.
    /// </summary>
    [ContextMenu("🛠️ Créer Particle Systems")]
    private void CreateParticleSystems()
    {
        if (_mainParticles == null)
        {
            GameObject mainObj = new GameObject("MainParticles");
            mainObj.transform.SetParent(transform);
            mainObj.transform.localPosition = Vector3.zero;
            _mainParticles = mainObj.AddComponent<ParticleSystem>();
            Debug.Log("[CollectibleEffects] ✓ Main Particles créé");
        }

        if (_accentParticles == null)
        {
            GameObject accentObj = new GameObject("AccentParticles");
            accentObj.transform.SetParent(transform);
            accentObj.transform.localPosition = Vector3.zero;
            _accentParticles = accentObj.AddComponent<ParticleSystem>();
            Debug.Log("[CollectibleEffects] ✓ Accent Particles créé");
        }

        ConfigureEffectsForType();
    }
#endif

    /// <summary>
    /// Structure qui stocke toutes les configurations d'effets pour un type.
    /// </summary>
    private struct EffectConfig
    {
        public Color primaryColor;
        public Color secondaryColor;
        public float lightIntensity;
        public float lightRange;
        public float trailWidth;
        public int particleCount;
        public float particleSpeed;
        public ParticleSystemShapeType particleShape;
    }
}