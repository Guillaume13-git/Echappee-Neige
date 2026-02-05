using UnityEngine;

/// <summary>
/// Je suis le gestionnaire central de l'affichage des bonus dans l'UI.
/// Mon rôle est de faire le lien entre le système de collectibles et l'affichage visuel des bonus actifs.
/// </summary>
public class BonusUIManager : MonoBehaviour
{
    // Je m'expose comme instance unique pour être accessible depuis n'importe où dans le code
    public static BonusUIManager Instance { get; private set; }

    // Je stocke la référence vers l'affichage du bonus de vitesse
    [SerializeField] private BonusDisplay _speedBoostDisplay;
    
    // Je stocke la référence vers l'affichage du bouclier
    [SerializeField] private BonusDisplay _shieldDisplay;

    /// <summary>
    /// Au réveil, je m'initialise comme instance unique (pattern Singleton).
    /// </summary>
    private void Awake()
    {
        // Si aucune instance n'existe encore, je deviens l'instance de référence
        // Cela me permet d'être appelé facilement via BonusUIManager.Instance depuis d'autres scripts
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[BonusUIManager] Instance créée avec succès ✓");
        }
        else
        {
            Debug.LogWarning("[BonusUIManager] Une instance existe déjà ! Destruction de ce doublon.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Au démarrage, je vérifie que toutes mes références sont bien assignées.
    /// </summary>
    private void Start()
    {
        ValidateReferences();
    }

    /// <summary>
    /// Je vérifie que toutes les références nécessaires sont bien assignées.
    /// </summary>
    private void ValidateReferences()
    {
        Debug.Log("========================================");
        Debug.Log("[BonusUIManager] 🔍 VALIDATION DES RÉFÉRENCES");
        Debug.Log("========================================");

        bool hasErrors = false;

        // Vérification Speed Boost Display
        if (_speedBoostDisplay == null)
        {
            Debug.LogError("[BonusUIManager] ❌ Speed Boost Display n'est PAS assigné dans l'Inspector !", this);
            hasErrors = true;
        }
        else
        {
            Debug.Log($"[BonusUIManager] ✓ Speed Boost Display assigné : {_speedBoostDisplay.gameObject.name}");
            Debug.Log($"[BonusUIManager]   └─ Actif : {_speedBoostDisplay.gameObject.activeSelf}");
            Debug.Log($"[BonusUIManager]   └─ Parent : {(_speedBoostDisplay.transform.parent != null ? _speedBoostDisplay.transform.parent.name : "NULL")}");
        }

        // Vérification Shield Display
        if (_shieldDisplay == null)
        {
            Debug.LogError("[BonusUIManager] ❌ Shield Display n'est PAS assigné dans l'Inspector !", this);
            hasErrors = true;
        }
        else
        {
            Debug.Log($"[BonusUIManager] ✓ Shield Display assigné : {_shieldDisplay.gameObject.name}");
            Debug.Log($"[BonusUIManager]   └─ Actif : {_shieldDisplay.gameObject.activeSelf}");
            Debug.Log($"[BonusUIManager]   └─ Parent : {(_shieldDisplay.transform.parent != null ? _shieldDisplay.transform.parent.name : "NULL")}");
            
            // Je vérifie que le parent est actif
            if (_shieldDisplay.transform.parent != null && !_shieldDisplay.transform.parent.gameObject.activeSelf)
            {
                Debug.LogWarning($"[BonusUIManager] ⚠️ Le parent '{_shieldDisplay.transform.parent.name}' est INACTIF !");
            }
        }

        if (!hasErrors)
        {
            Debug.Log("[BonusUIManager] ✅ Toutes les références sont correctement assignées !");
        }

        Debug.Log("========================================");
    }

    /// <summary>
    /// Je suis appelé quand le joueur ramasse un bonus de vitesse (sucre d'orge).
    /// Je déclenche alors l'affichage visuel du bonus avec son timer.
    /// </summary>
    /// <param name="duration">La durée pendant laquelle le bonus sera actif (en secondes)</param>
    public void TriggerSpeedBoost(float duration)
    {
        Debug.Log("========================================");
        Debug.Log($"[BonusUIManager] 🍬 TriggerSpeedBoost appelé avec durée : {duration}s");

        if (_speedBoostDisplay == null)
        {
            Debug.LogError("[BonusUIManager] ❌ Impossible d'afficher Speed Boost : _speedBoostDisplay est NULL !");
            Debug.Log("========================================");
            return;
        }

        Debug.Log($"[BonusUIManager] État AVANT activation :");
        Debug.Log($"[BonusUIManager]   └─ GameObject actif : {_speedBoostDisplay.gameObject.activeSelf}");

        // Je demande à mon BonusDisplay dédié d'afficher le bonus de vitesse
        _speedBoostDisplay.ShowBonus(duration);

        Debug.Log($"[BonusUIManager] État APRÈS activation :");
        Debug.Log($"[BonusUIManager]   └─ GameObject actif : {_speedBoostDisplay.gameObject.activeSelf}");
        Debug.Log("[BonusUIManager] ✓ Speed Boost activé avec succès");
        Debug.Log("========================================");
    }

    /// <summary>
    /// Je suis appelé quand le joueur ramasse un bouclier (cadeau de Noël).
    /// Je déclenche alors l'affichage visuel du bouclier avec son timer.
    /// </summary>
    /// <param name="duration">La durée pendant laquelle le bouclier sera actif (en secondes)</param>
    public void TriggerShield(float duration)
    {
        Debug.Log("========================================");
        Debug.Log($"[BonusUIManager] 🛡️ TriggerShield appelé avec durée : {duration}s");

        if (_shieldDisplay == null)
        {
            Debug.LogError("[BonusUIManager] ❌ Impossible d'afficher Shield : _shieldDisplay est NULL !");
            Debug.LogError("[BonusUIManager] ❌ Vérifie que ShieldUI est bien assigné dans l'Inspector du BonusUIManager !");
            Debug.Log("========================================");
            return;
        }

        Debug.Log($"[BonusUIManager] 🔍 État AVANT activation :");
        Debug.Log($"[BonusUIManager]   └─ Nom du GameObject : {_shieldDisplay.gameObject.name}");
        Debug.Log($"[BonusUIManager]   └─ GameObject actif : {_shieldDisplay.gameObject.activeSelf}");
        Debug.Log($"[BonusUIManager]   └─ Position : {_shieldDisplay.transform.position}");
        Debug.Log($"[BonusUIManager]   └─ Scale : {_shieldDisplay.transform.localScale}");

        // Je vérifie si le parent existe et est actif
        if (_shieldDisplay.transform.parent != null)
        {
            Debug.Log($"[BonusUIManager]   └─ Parent : {_shieldDisplay.transform.parent.name}");
            Debug.Log($"[BonusUIManager]   └─ Parent actif : {_shieldDisplay.transform.parent.gameObject.activeSelf}");

            // Si le parent est inactif, je le signale et je l'active
            if (!_shieldDisplay.transform.parent.gameObject.activeSelf)
            {
                Debug.LogWarning($"[BonusUIManager] ⚠️ Le parent '{_shieldDisplay.transform.parent.name}' était INACTIF !");
                Debug.LogWarning("[BonusUIManager] ⚠️ J'active le parent pour que le Shield puisse s'afficher...");
                _shieldDisplay.transform.parent.gameObject.SetActive(true);
            }
        }

        // Je demande à mon BonusDisplay dédié d'afficher le bouclier
        _shieldDisplay.ShowBonus(duration);

        Debug.Log($"[BonusUIManager] 🔍 État APRÈS activation :");
        Debug.Log($"[BonusUIManager]   └─ GameObject actif : {_shieldDisplay.gameObject.activeSelf}");
        Debug.Log($"[BonusUIManager]   └─ IsActive() : {_shieldDisplay.IsActive()}");
        
        Debug.Log("[BonusUIManager] ✓ ShowBonus() appelé sur Shield avec succès");
        Debug.Log("========================================");
    }

    /// <summary>
    /// Je vérifie si le Speed Boost est actuellement actif.
    /// </summary>
    public bool IsSpeedBoostActive()
    {
        return _speedBoostDisplay != null && _speedBoostDisplay.IsActive();
    }

    /// <summary>
    /// Je vérifie si le Shield est actuellement actif.
    /// </summary>
    public bool IsShieldActive()
    {
        return _shieldDisplay != null && _shieldDisplay.IsActive();
    }

    /// <summary>
    /// Je désactive immédiatement le Shield (quand il absorbe un coup par exemple).
    /// </summary>
    public void DeactivateShield()
    {
        if (_shieldDisplay != null && _shieldDisplay.IsActive())
        {
            _shieldDisplay.ForceDeactivate();
            Debug.Log("[BonusUIManager] 🛡️ Shield désactivé manuellement");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Menu de test dans l'éditeur pour tester le Speed Boost.
    /// </summary>
    [ContextMenu("🧪 Test Speed Boost (5s)")]
    private void TestSpeedBoost()
    {
        Debug.Log("[BonusUIManager] 🧪 TEST MANUEL : Speed Boost");
        TriggerSpeedBoost(5f);
    }

    /// <summary>
    /// Menu de test dans l'éditeur pour tester le Shield.
    /// </summary>
    [ContextMenu("🧪 Test Shield (5s)")]
    private void TestShield()
    {
        Debug.Log("[BonusUIManager] 🧪 TEST MANUEL : Shield");
        TriggerShield(5f);
    }

    /// <summary>
    /// Menu de diagnostic complet pour déboguer.
    /// </summary>
    [ContextMenu("🔍 Diagnostic Complet")]
    private void DiagnosticComplet()
    {
        Debug.Log("========================================");
        Debug.Log("[BonusUIManager] 🔍 DIAGNOSTIC COMPLET");
        Debug.Log("========================================");

        Debug.Log($"Instance : {(Instance != null ? "OK" : "NULL")}");
        Debug.Log($"Speed Boost Display : {(_speedBoostDisplay != null ? _speedBoostDisplay.gameObject.name : "NULL")}");
        Debug.Log($"Shield Display : {(_shieldDisplay != null ? _shieldDisplay.gameObject.name : "NULL")}");

        if (_shieldDisplay != null)
        {
            Debug.Log($"\n🛡️ Shield Display Details :");
            Debug.Log($"  └─ GameObject : {_shieldDisplay.gameObject.name}");
            Debug.Log($"  └─ Actif : {_shieldDisplay.gameObject.activeSelf}");
            Debug.Log($"  └─ Tag : {_shieldDisplay.gameObject.tag}");
            Debug.Log($"  └─ Layer : {LayerMask.LayerToName(_shieldDisplay.gameObject.layer)}");
            Debug.Log($"  └─ Position World : {_shieldDisplay.transform.position}");
            Debug.Log($"  └─ Position Local : {_shieldDisplay.transform.localPosition}");
            Debug.Log($"  └─ Scale : {_shieldDisplay.transform.localScale}");
            
            if (_shieldDisplay.transform.parent != null)
            {
                Debug.Log($"  └─ Parent : {_shieldDisplay.transform.parent.name}");
                Debug.Log($"  └─ Parent actif : {_shieldDisplay.transform.parent.gameObject.activeSelf}");
            }

            // Je vérifie les composants
            var rectTransform = _shieldDisplay.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"  └─ RectTransform AnchoredPosition : {rectTransform.anchoredPosition}");
                Debug.Log($"  └─ RectTransform SizeDelta : {rectTransform.sizeDelta}");
            }

            var bonusDisplay = _shieldDisplay.GetComponent<BonusDisplay>();
            if (bonusDisplay != null)
            {
                Debug.Log($"  └─ BonusDisplay script : OK");
                Debug.Log($"  └─ BonusDisplay.IsActive() : {bonusDisplay.IsActive()}");
            }
            else
            {
                Debug.LogError($"  └─ BonusDisplay script : MANQUANT !");
            }
        }

        Debug.Log("========================================");
    }
#endif
}