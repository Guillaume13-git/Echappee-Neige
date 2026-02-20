using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Je gère l'effet visuel de givre/neige sur les bords de l'écran.
/// Mon rôle : Créer un feedback visuel immersif qui montre le danger qui se rapproche.
/// 
/// Selon le GDD :
/// - Plus la menace augmente (avalanche se rapproche), plus l'effet est intense
/// - À 80%+ de menace, l'écran devient critique avec un effet très marqué
/// - Les 4 côtés de l'écran deviennent neigeux/givrés
/// 
/// Mon emplacement : Je suis attaché au Canvas/HUD
/// Mon utilisation : Je m'abonne automatiquement au ThreatManager
/// </summary>
public class ScreenEdgeEffect : MonoBehaviour
{
    [Header("📐 UI Setup")]
    [Tooltip("Je crée automatiquement l'image overlay si elle n'existe pas")]
    [SerializeField] private Image _overlayImage;
    
    [Tooltip("Le Canvas parent (récupéré automatiquement si null)")]
    [SerializeField] private Canvas _canvas;

    [Header("🎨 Visual Settings")]
    [Tooltip("Couleur de l'effet de givre (blanc semi-transparent)")]
    [SerializeField] private Color _edgeColor = new Color(1f, 1f, 1f, 0f); // Blanc transparent par défaut
    
    [Tooltip("Intensité maximale de l'effet (alpha) à 100% de menace")]
    [SerializeField] private float _maxAlpha = 0.6f;
    
    [Tooltip("Texture/Sprite pour l'effet de givre (vignette)")]
    [SerializeField] private Sprite _vignetteSprite;

    [Header("⚙️ Effect Behavior")]
    [Tooltip("À partir de quel % de menace l'effet commence à apparaître")]
    [SerializeField] private float _effectStartThreshold = 40f;
    
    [Tooltip("À partir de quel % de menace l'effet devient critique (rouge)")]
    [SerializeField] private float _criticalThreshold = 80f;
    
    [Tooltip("Vitesse de transition de l'effet (lerp speed)")]
    [SerializeField] private float _transitionSpeed = 2f;

    [Header("🎨 Color Progression")]
    [Tooltip("Couleur à 0-40% de menace (transparent)")]
    [SerializeField] private Color _lowThreatColor = new Color(1f, 1f, 1f, 0f);
    
    [Tooltip("Couleur à 40-80% de menace (blanc givré)")]
    [SerializeField] private Color _mediumThreatColor = new Color(0.9f, 0.95f, 1f, 0.3f);
    
    [Tooltip("Couleur à 80-100% de menace (rouge critique)")]
    [SerializeField] private Color _highThreatColor = new Color(1f, 0.3f, 0.3f, 0.6f);

    [Header("💨 Animation")]
    [Tooltip("J'anime l'effet avec un pulse en mode critique")]
    [SerializeField] private bool _enablePulseEffect = true;
    
    [Tooltip("Vitesse du pulse en mode critique")]
    [SerializeField] private float _pulseSpeed = 3f;
    
    [Tooltip("Intensité du pulse (variation d'alpha)")]
    [SerializeField] private float _pulseIntensity = 0.15f;

    [Header("🔧 Debug")]
    [SerializeField] private bool _showDebugLogs = true;
    [SerializeField] private bool _showDebugUI = false;

    // Je stocke l'état actuel
    private float _currentThreatPercent = 0f;
    private Color _targetColor;
    private bool _isInitialized = false;

    /// <summary>
    /// Au démarrage, je me configure automatiquement.
    /// </summary>
    private void Start()
    {
        if (_showDebugLogs)
        {
            Debug.Log("========================================");
            Debug.Log("[ScreenEdgeEffect] 🎬 Initialisation...");
        }

        // Je récupère le Canvas parent si non assigné
        if (_canvas == null)
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        // Je crée l'overlay image si elle n'existe pas
        if (_overlayImage == null)
        {
            CreateOverlayImage();
        }
        else
        {
            // Je configure l'image existante
            ConfigureOverlayImage();
        }

        // Je m'abonne au ThreatManager pour recevoir les mises à jour
        if (ThreatManager.Instance != null)
        {
            ThreatManager.Instance.OnThreatChanged += OnThreatChanged;
            
            if (_showDebugLogs)
            {
                Debug.Log("[ScreenEdgeEffect] ✓ Abonné au ThreatManager");
            }
        }
        else
        {
            Debug.LogError("[ScreenEdgeEffect] ❌ ThreatManager.Instance est NULL !");
        }

        _isInitialized = true;

        if (_showDebugLogs)
        {
            Debug.Log("[ScreenEdgeEffect] ✓ Initialisation terminée");
            Debug.Log("========================================");
        }
    }

    /// <summary>
    /// ✅ NOUVEAU : Je crée automatiquement l'image overlay pour l'effet.
    /// </summary>
    private void CreateOverlayImage()
    {
        if (_showDebugLogs) Debug.Log("[ScreenEdgeEffect] 📐 Création de l'overlay image...");

        // Je crée un GameObject pour l'overlay
        GameObject overlayObj = new GameObject("ScreenEdgeOverlay");
        overlayObj.transform.SetParent(transform);

        // J'ajoute le composant Image
        _overlayImage = overlayObj.AddComponent<Image>();

        // Je configure le RectTransform pour couvrir tout l'écran
        RectTransform rt = _overlayImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;        // Coin bas-gauche
        rt.anchorMax = Vector2.one;         // Coin haut-droit
        rt.offsetMin = Vector2.zero;        // Pas d'offset
        rt.offsetMax = Vector2.zero;        // Pas d'offset
        rt.pivot = new Vector2(0.5f, 0.5f); // Centre

        // Je configure l'image
        ConfigureOverlayImage();

        if (_showDebugLogs) Debug.Log("[ScreenEdgeEffect]   ✓ Overlay créé");
    }

    /// <summary>
    /// Je configure l'image overlay avec les bons paramètres.
    /// </summary>
    private void ConfigureOverlayImage()
    {
        // Je désactive le raycast (l'overlay ne doit pas bloquer les clics)
        _overlayImage.raycastTarget = false;

        // Je définis la couleur initiale (transparent)
        _overlayImage.color = _lowThreatColor;

        // Si j'ai un sprite de vignette, je l'utilise
        if (_vignetteSprite != null)
        {
            _overlayImage.sprite = _vignetteSprite;
        }
        else
        {
            // Sinon je crée une vignette procédurale simple
            // (Unity n'a pas de sprite par défaut pour ça, donc je laisse blanc)
            _overlayImage.color = _lowThreatColor;
        }

        // Je m'assure que l'overlay est au premier plan (dernier enfant = dessus)
        _overlayImage.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Quand je suis détruit, je me désabonne proprement.
    /// </summary>
    private void OnDestroy()
    {
        if (ThreatManager.Instance != null)
        {
            ThreatManager.Instance.OnThreatChanged -= OnThreatChanged;
        }
    }

    /// <summary>
    /// Je reçois les mises à jour de menace du ThreatManager.
    /// </summary>
    private void OnThreatChanged(float threatPercent)
    {
        _currentThreatPercent = threatPercent;

        // Je calcule la couleur cible selon le niveau de menace
        CalculateTargetColor(threatPercent);
    }

    /// <summary>
    /// Je calcule la couleur cible selon le pourcentage de menace.
    /// </summary>
    private void CalculateTargetColor(float threatPercent)
    {
        if (threatPercent < _effectStartThreshold)
        {
            // Zone verte : Pas d'effet (transparent)
            _targetColor = _lowThreatColor;
        }
        else if (threatPercent < _criticalThreshold)
        {
            // Zone orange : Effet blanc givré progressif (40-80%)
            float t = (threatPercent - _effectStartThreshold) / (_criticalThreshold - _effectStartThreshold);
            _targetColor = Color.Lerp(_lowThreatColor, _mediumThreatColor, t);
        }
        else
        {
            // Zone rouge : Effet rouge critique (80-100%)
            float t = (threatPercent - _criticalThreshold) / (100f - _criticalThreshold);
            _targetColor = Color.Lerp(_mediumThreatColor, _highThreatColor, t);
        }
    }

    /// <summary>
    /// À chaque frame, j'interpole vers la couleur cible et j'anime l'effet.
    /// </summary>
    private void Update()
    {
        if (!_isInitialized || _overlayImage == null) return;

        // J'interpole vers la couleur cible
        Color currentColor = _overlayImage.color;
        _overlayImage.color = Color.Lerp(currentColor, _targetColor, Time.deltaTime * _transitionSpeed);

        // Si je suis en mode critique, j'ajoute un pulse
        if (_enablePulseEffect && _currentThreatPercent >= _criticalThreshold)
        {
            ApplyPulseEffect();
        }
    }

    /// <summary>
    /// ✅ NOUVEAU : J'applique un effet de pulse en mode critique.
    /// </summary>
    private void ApplyPulseEffect()
    {
        // Je calcule un pulse sinusoïdal
        float pulse = Mathf.Sin(Time.time * _pulseSpeed) * _pulseIntensity;

        // J'ajoute le pulse à l'alpha actuel
        Color color = _overlayImage.color;
        color.a = Mathf.Clamp01(_targetColor.a + pulse);
        _overlayImage.color = color;
    }

    /// <summary>
    /// Je réinitialise l'effet (appelé au début d'une nouvelle partie).
    /// </summary>
    public void ResetEffect()
    {
        if (_overlayImage != null)
        {
            _overlayImage.color = _lowThreatColor;
        }

        _currentThreatPercent = 0f;
        _targetColor = _lowThreatColor;

        if (_showDebugLogs)
        {
            Debug.Log("[ScreenEdgeEffect] 🔄 Effet réinitialisé");
        }
    }

    /// <summary>
    /// J'affiche des informations de debug à l'écran.
    /// </summary>
    private void OnGUI()
    {
        if (!_showDebugUI) return;

        GUILayout.BeginArea(new Rect(10, 300, 300, 120));
        
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
        
        GUILayout.Label($"🌫️ Screen Edge Effect", titleStyle);
        GUILayout.Label($"Menace: {_currentThreatPercent:F1}%", labelStyle);
        GUILayout.Label($"Alpha: {_overlayImage?.color.a:F2}", labelStyle);
        
        string zone = _currentThreatPercent < _effectStartThreshold ? "Sûr" : 
                      _currentThreatPercent < _criticalThreshold ? "Attention" : "CRITIQUE";
        GUILayout.Label($"Zone: {zone}", labelStyle);
        
        GUILayout.EndArea();
    }

#if UNITY_EDITOR
    [ContextMenu("🔍 Diagnostic")]
    private void Diagnostic()
    {
        Debug.Log("========================================");
        Debug.Log("[ScreenEdgeEffect] DIAGNOSTIC");
        Debug.Log("========================================");
        
        Debug.Log($"✓ Canvas: {(_canvas != null ? _canvas.name : "❌ NULL")}");
        Debug.Log($"✓ Overlay Image: {(_overlayImage != null ? _overlayImage.name : "❌ NULL")}");
        Debug.Log($"✓ ThreatManager: {(ThreatManager.Instance != null ? "OK" : "❌ NULL")}");
        
        Debug.Log($"\nConfiguration:");
        Debug.Log($"  Seuil début: {_effectStartThreshold}%");
        Debug.Log($"  Seuil critique: {_criticalThreshold}%");
        Debug.Log($"  Alpha max: {_maxAlpha}");
        Debug.Log($"  Pulse: {(_enablePulseEffect ? "ACTIVÉ" : "DÉSACTIVÉ")}");
        
        Debug.Log("========================================");
    }

    [ContextMenu("🧪 Test Effet Critique")]
    private void TestCriticalEffect()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[ScreenEdgeEffect] Lance le jeu pour tester !");
            return;
        }

        // Je simule une menace à 90%
        OnThreatChanged(90f);
        Debug.Log("[ScreenEdgeEffect] 🚨 Test effet critique (90% menace)");
    }

    [ContextMenu("🧪 Test Reset")]
    private void TestReset()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[ScreenEdgeEffect] Lance le jeu pour tester !");
            return;
        }

        ResetEffect();
        Debug.Log("[ScreenEdgeEffect] 🔄 Reset testé");
    }
#endif
}