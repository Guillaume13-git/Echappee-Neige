using UnityEngine;

/// <summary>
/// Je suis le gestionnaire central de l'affichage des bonus dans l'UI du jeu Échappée-Neige.
/// Mon rôle principal : Faire le lien entre le système de collectibles et l'affichage visuel des bonus actifs.
/// 
/// Mon fonctionnement : 
/// - Je suis un Singleton (une seule instance dans tout le jeu)
/// - Je gère deux types de bonus : Speed Boost (sucre d'orge) et Shield (cadeau)
/// - Je délègue l'affichage réel à des BonusDisplay dédiés
/// 
/// Mon utilisation : D'autres scripts m'appellent via BonusUIManager.Instance.TriggerSpeedBoost() ou TriggerShield()
/// Mon emplacement : Je suis attaché à un GameObject dans la Canvas (souvent nommé "BonusUIManager")
/// </summary>
public class BonusUIManager : MonoBehaviour
{
    // Je m'expose comme instance unique pour être accessible depuis n'importe où dans le code (pattern Singleton)
    // Cela permet aux autres scripts de m'appeler facilement via BonusUIManager.Instance
    public static BonusUIManager Instance { get; private set; }

    // Je stocke la référence vers le composant BonusDisplay qui gère l'affichage du bonus de vitesse (sucre d'orge)
    // Ce BonusDisplay s'occupe de l'animation, du timer circulaire et de la disparition du Speed Boost
    [SerializeField] private BonusDisplay _speedBoostDisplay;
    
    // Je stocke la référence vers le composant BonusDisplay qui gère l'affichage du bouclier (cadeau de Noël)
    // Ce BonusDisplay s'occupe de l'animation et de la disparition du Shield (sans timer circulaire)
    [SerializeField] private BonusDisplay _shieldDisplay;

    /// <summary>
    /// Au réveil (avant Start), je m'initialise comme instance unique (pattern Singleton).
    /// Mon rôle : Garantir qu'une seule instance de BonusUIManager existe dans le jeu.
    /// Si un doublon existe, je le détruis pour éviter les conflits.
    /// </summary>
    private void Awake()
    {
        // Si aucune instance n'existe encore, je deviens l'instance de référence
        // Cela me permet d'être appelé facilement via BonusUIManager.Instance depuis d'autres scripts
        if (Instance == null)
        {
            Instance = this; // Je m'enregistre comme instance unique
            Debug.Log("[BonusUIManager] Instance créée avec succès ✓");
        }
        else
        {
            // Si une instance existe déjà, je me détruis pour éviter les doublons
            Debug.LogWarning("[BonusUIManager] Une instance existe déjà ! Destruction de ce doublon.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Au démarrage du jeu, je vérifie que toutes mes références sont bien configurées.
    /// Mon rôle : Détecter les erreurs de configuration dès le lancement pour éviter les bugs en jeu.
    /// </summary>
    private void Start()
    {
        // Je lance ma vérification complète des références
        ValidateReferences();
    }

    /// <summary>
    /// Je vérifie que toutes les références nécessaires sont bien assignées dans l'Inspector Unity.
    /// Mon rôle : M'assurer que _speedBoostDisplay et _shieldDisplay sont correctement configurés.
    /// J'affiche des logs détaillés pour aider au débogage en cas de problème.
    /// </summary>
    private void ValidateReferences()
    {
        // J'affiche un séparateur visuel dans la console pour faciliter la lecture
        Debug.Log("========================================");
        Debug.Log("[BonusUIManager] 🔍 VALIDATION DES RÉFÉRENCES");
        Debug.Log("========================================");

        bool hasErrors = false; // Je garde en mémoire si j'ai détecté des erreurs

        // ---------------------------------------------------------
        // VÉRIFICATION DU SPEED BOOST DISPLAY
        // ---------------------------------------------------------
        if (_speedBoostDisplay == null)
        {
            // Si la référence n'est pas assignée, j'affiche une erreur critique
            Debug.LogError("[BonusUIManager] ❌ Speed Boost Display n'est PAS assigné dans l'Inspector !", this);
            hasErrors = true;
        }
        else
        {
            // Si la référence est OK, j'affiche ses informations pour vérification
            Debug.Log($"[BonusUIManager] ✓ Speed Boost Display assigné : {_speedBoostDisplay.gameObject.name}");
            Debug.Log($"[BonusUIManager]   └─ Actif au démarrage : {_speedBoostDisplay.gameObject.activeSelf}");
            Debug.Log($"[BonusUIManager]   └─ Parent : {(_speedBoostDisplay.transform.parent != null ? _speedBoostDisplay.transform.parent.name : "NULL")}");
        }

        // ---------------------------------------------------------
        // VÉRIFICATION DU SHIELD DISPLAY
        // ---------------------------------------------------------
        if (_shieldDisplay == null)
        {
            // Si la référence n'est pas assignée, j'affiche une erreur critique
            Debug.LogError("[BonusUIManager] ❌ Shield Display n'est PAS assigné dans l'Inspector !", this);
            hasErrors = true;
        }
        else
        {
            // Si la référence est OK, j'affiche ses informations pour vérification
            Debug.Log($"[BonusUIManager] ✓ Shield Display assigné : {_shieldDisplay.gameObject.name}");
            Debug.Log($"[BonusUIManager]   └─ Actif au démarrage : {_shieldDisplay.gameObject.activeSelf}");
            Debug.Log($"[BonusUIManager]   └─ Parent : {(_shieldDisplay.transform.parent != null ? _shieldDisplay.transform.parent.name : "NULL")}");
            
            // Je vérifie aussi que le parent est actif (sinon le Shield ne pourra jamais s'afficher)
            if (_shieldDisplay.transform.parent != null && !_shieldDisplay.transform.parent.gameObject.activeSelf)
            {
                Debug.LogWarning($"[BonusUIManager] ⚠️ Le parent '{_shieldDisplay.transform.parent.name}' est INACTIF ! Le Shield ne pourra pas s'afficher.");
            }
        }

        // Si tout est OK, j'affiche un message de confirmation
        if (!hasErrors)
        {
            Debug.Log("[BonusUIManager] ✅ Toutes les références sont correctement assignées !");
        }

        Debug.Log("========================================");
    }

    /// <summary>
    /// Je suis appelé quand le joueur ramasse un bonus de vitesse (collectible sucre d'orge).
    /// Mon rôle : Déclencher l'affichage visuel du Speed Boost avec son timer circulaire.
    /// Je délègue l'affichage réel au BonusDisplay dédié au Speed Boost.
    /// </summary>
    /// <param name="duration">La durée pendant laquelle le bonus sera actif (en secondes, typiquement 10s)</param>
    public void TriggerSpeedBoost(float duration)
    {
        // J'affiche un séparateur et un log pour suivre l'activation du bonus
        Debug.Log("========================================");
        Debug.Log($"[BonusUIManager] 🍬 TriggerSpeedBoost appelé avec durée : {duration}s");

        // Je vérifie d'abord que j'ai bien une référence vers le BonusDisplay du Speed Boost
        if (_speedBoostDisplay == null)
        {
            Debug.LogError("[BonusUIManager] ❌ Impossible d'afficher Speed Boost : _speedBoostDisplay est NULL !");
            Debug.LogError("[BonusUIManager] ❌ Vérifie que SpeedBoostUI est bien assigné dans l'Inspector du BonusUIManager !");
            Debug.Log("========================================");
            return; // Je m'arrête ici car je ne peux rien faire sans référence
        }

        // J'affiche l'état AVANT activation pour le débogage
        Debug.Log($"[BonusUIManager] État AVANT activation :");
        Debug.Log($"[BonusUIManager]   └─ GameObject actif : {_speedBoostDisplay.gameObject.activeSelf}");

        // Je demande à mon BonusDisplay dédié d'afficher le bonus de vitesse
        // C'est lui qui va gérer l'animation, le timer et la disparition
        _speedBoostDisplay.ShowBonus(duration);

        // J'affiche l'état APRÈS activation pour vérifier que tout s'est bien passé
        Debug.Log($"[BonusUIManager] État APRÈS activation :");
        Debug.Log($"[BonusUIManager]   └─ GameObject actif : {_speedBoostDisplay.gameObject.activeSelf}");
        Debug.Log("[BonusUIManager] ✓ Speed Boost activé avec succès");
        Debug.Log("========================================");
    }

    /// <summary>
    /// Je suis appelé quand le joueur ramasse un bouclier (collectible cadeau de Noël).
    /// Mon rôle : Déclencher l'affichage visuel du Shield SANS timer circulaire (juste l'icône).
    /// Je délègue l'affichage réel au BonusDisplay dédié au Shield.
    /// IMPORTANT : Si le parent est inactif, je l'active automatiquement pour résoudre le bug d'affichage.
    /// </summary>
    /// <param name="duration">La durée pendant laquelle le bouclier sera actif (en secondes, typiquement 10s)</param>
    public void TriggerShield(float duration)
    {
        // J'affiche un séparateur et un log pour suivre l'activation du bonus
        Debug.Log("========================================");
        Debug.Log($"[BonusUIManager] 🛡️ TriggerShield appelé avec durée : {duration}s");

        // Je vérifie d'abord que j'ai bien une référence vers le BonusDisplay du Shield
        if (_shieldDisplay == null)
        {
            Debug.LogError("[BonusUIManager] ❌ Impossible d'afficher Shield : _shieldDisplay est NULL !");
            Debug.LogError("[BonusUIManager] ❌ Vérifie que ShieldUI est bien assigné dans l'Inspector du BonusUIManager !");
            Debug.Log("========================================");
            return; // Je m'arrête ici car je ne peux rien faire sans référence
        }

        // J'affiche l'état AVANT activation pour le débogage approfondi
        Debug.Log($"[BonusUIManager] 🔍 État AVANT activation :");
        Debug.Log($"[BonusUIManager]   └─ Nom du GameObject : {_shieldDisplay.gameObject.name}");
        Debug.Log($"[BonusUIManager]   └─ GameObject actif : {_shieldDisplay.gameObject.activeSelf}");
        Debug.Log($"[BonusUIManager]   └─ Position : {_shieldDisplay.transform.position}");
        Debug.Log($"[BonusUIManager]   └─ Scale : {_shieldDisplay.transform.localScale}");

        // Je vérifie si le parent existe et s'il est actif (cause fréquente de bug d'affichage)
        if (_shieldDisplay.transform.parent != null)
        {
            Debug.Log($"[BonusUIManager]   └─ Parent : {_shieldDisplay.transform.parent.name}");
            Debug.Log($"[BonusUIManager]   └─ Parent actif : {_shieldDisplay.transform.parent.gameObject.activeSelf}");

            // Si le parent est inactif, je le signale et je l'active automatiquement
            // Cela corrige un bug où le Shield ne s'affichait pas si le parent était désactivé
            if (!_shieldDisplay.transform.parent.gameObject.activeSelf)
            {
                Debug.LogWarning($"[BonusUIManager] ⚠️ Le parent '{_shieldDisplay.transform.parent.name}' était INACTIF !");
                Debug.LogWarning("[BonusUIManager] ⚠️ J'active le parent pour que le Shield puisse s'afficher...");
                _shieldDisplay.transform.parent.gameObject.SetActive(true);
            }
        }

        // Je demande à mon BonusDisplay dédié d'afficher le bouclier
        // C'est lui qui va gérer l'animation et la disparition (SANS timer circulaire)
        _shieldDisplay.ShowBonus(duration);

        // J'affiche l'état APRÈS activation pour vérifier que tout s'est bien passé
        Debug.Log($"[BonusUIManager] 🔍 État APRÈS activation :");
        Debug.Log($"[BonusUIManager]   └─ GameObject actif : {_shieldDisplay.gameObject.activeSelf}");
        Debug.Log($"[BonusUIManager]   └─ IsActive() : {_shieldDisplay.IsActive()}");
        
        Debug.Log("[BonusUIManager] ✓ ShowBonus() appelé sur Shield avec succès");
        Debug.Log("========================================");
    }

    /// <summary>
    /// Je vérifie si le Speed Boost est actuellement actif sur le joueur.
    /// Mon rôle : Permettre aux autres scripts de savoir si le bonus de vitesse est en cours.
    /// Utilisé par exemple par le PlayerController pour appliquer la vitesse boostée.
    /// </summary>
    /// <returns>True si le Speed Boost est actif, False sinon</returns>
    public bool IsSpeedBoostActive()
    {
        // Je vérifie d'abord que la référence existe, puis je demande au BonusDisplay s'il est actif
        return _speedBoostDisplay != null && _speedBoostDisplay.IsActive();
    }

    /// <summary>
    /// Je vérifie si le Shield est actuellement actif sur le joueur.
    /// Mon rôle : Permettre aux autres scripts de savoir si le bouclier protège le joueur.
    /// Utilisé par exemple par le CollisionHandler pour annuler le prochain dégât.
    /// </summary>
    /// <returns>True si le Shield est actif, False sinon</returns>
    public bool IsShieldActive()
    {
        // Je vérifie d'abord que la référence existe, puis je demande au BonusDisplay s'il est actif
        return _shieldDisplay != null && _shieldDisplay.IsActive();
    }

    /// <summary>
    /// Je désactive immédiatement le Shield sans animation.
    /// Mon rôle : Permettre de consommer le bouclier instantanément (par exemple quand il absorbe un coup).
    /// Utilisé par exemple quand le joueur percute un obstacle : le Shield disparaît immédiatement.
    /// </summary>
    public void DeactivateShield()
    {
        // Je vérifie d'abord que le Shield existe et est actif avant de le désactiver
        if (_shieldDisplay != null && _shieldDisplay.IsActive())
        {
            // Je force la désactivation immédiate (sans animation de disparition)
            _shieldDisplay.ForceDeactivate();
            Debug.Log("[BonusUIManager] 🛡️ Shield désactivé manuellement (consommé par un obstacle)");
        }
    }

#if UNITY_EDITOR
    // Les méthodes suivantes ne sont disponibles QUE dans l'éditeur Unity (pas dans le jeu final)
    // Elles permettent de tester les bonus sans avoir à jouer et ramasser des collectibles

    /// <summary>
    /// Menu de test dans l'éditeur Unity pour tester le Speed Boost.
    /// Mon rôle : Permettre au développeur de tester rapidement l'affichage du Speed Boost.
    /// Utilisation : Clic droit sur le composant BonusUIManager dans l'Inspector → "🧪 Test Speed Boost (5s)"
    /// </summary>
    [ContextMenu("🧪 Test Speed Boost (5s)")]
    private void TestSpeedBoost()
    {
        Debug.Log("[BonusUIManager] 🧪 TEST MANUEL : Speed Boost");
        // Je déclenche le Speed Boost pour 5 secondes
        TriggerSpeedBoost(5f);
    }

    /// <summary>
    /// Menu de test dans l'éditeur Unity pour tester le Shield.
    /// Mon rôle : Permettre au développeur de tester rapidement l'affichage du Shield.
    /// Utilisation : Clic droit sur le composant BonusUIManager dans l'Inspector → "🧪 Test Shield (5s)"
    /// </summary>
    [ContextMenu("🧪 Test Shield (5s)")]
    private void TestShield()
    {
        Debug.Log("[BonusUIManager] 🧪 TEST MANUEL : Shield");
        // Je déclenche le Shield pour 5 secondes
        TriggerShield(5f);
    }

    /// <summary>
    /// Menu de diagnostic complet pour déboguer les problèmes d'affichage.
    /// Mon rôle : Afficher TOUTES les informations détaillées sur le Shield Display pour identifier les bugs.
    /// Utilisation : Clic droit sur le composant BonusUIManager dans l'Inspector → "🔍 Diagnostic Complet"
    /// J'affiche : nom, état, tag, layer, positions, scale, parent, composants, etc.
    /// </summary>
    [ContextMenu("🔍 Diagnostic Complet")]
    private void DiagnosticComplet()
    {
        // J'affiche un séparateur visuel
        Debug.Log("========================================");
        Debug.Log("[BonusUIManager] 🔍 DIAGNOSTIC COMPLET");
        Debug.Log("========================================");

        // Je vérifie l'état de base du BonusUIManager
        Debug.Log($"Instance Singleton : {(Instance != null ? "✓ OK" : "❌ NULL")}");
        Debug.Log($"Speed Boost Display : {(_speedBoostDisplay != null ? _speedBoostDisplay.gameObject.name : "❌ NULL")}");
        Debug.Log($"Shield Display : {(_shieldDisplay != null ? _shieldDisplay.gameObject.name : "❌ NULL")}");

        // Si le Shield Display existe, j'affiche TOUTES ses informations détaillées
        if (_shieldDisplay != null)
        {
            Debug.Log($"\n🛡️ Shield Display - Détails Complets :");
            
            // Informations de base du GameObject
            Debug.Log($"  └─ GameObject : {_shieldDisplay.gameObject.name}");
            Debug.Log($"  └─ Actif : {(_shieldDisplay.gameObject.activeSelf ? "✓ OUI" : "❌ NON")}");
            Debug.Log($"  └─ Tag : {_shieldDisplay.gameObject.tag}");
            Debug.Log($"  └─ Layer : {LayerMask.LayerToName(_shieldDisplay.gameObject.layer)}");
            
            // Informations de position et échelle
            Debug.Log($"  └─ Position World (globale) : {_shieldDisplay.transform.position}");
            Debug.Log($"  └─ Position Local (par rapport au parent) : {_shieldDisplay.transform.localPosition}");
            Debug.Log($"  └─ Scale (échelle) : {_shieldDisplay.transform.localScale}");
            
            // Informations sur le parent (crucial pour l'affichage)
            if (_shieldDisplay.transform.parent != null)
            {
                Debug.Log($"  └─ Parent : {_shieldDisplay.transform.parent.name}");
                Debug.Log($"  └─ Parent actif : {(_shieldDisplay.transform.parent.gameObject.activeSelf ? "✓ OUI" : "❌ NON (PROBLÈME !)")}");
            }
            else
            {
                Debug.LogWarning($"  └─ Parent : ❌ AUCUN (le Shield n'a pas de parent !)");
            }

            // Je vérifie le RectTransform (composant UI)
            var rectTransform = _shieldDisplay.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"  └─ RectTransform AnchoredPosition : {rectTransform.anchoredPosition}");
                Debug.Log($"  └─ RectTransform SizeDelta : {rectTransform.sizeDelta}");
            }
            else
            {
                Debug.LogWarning($"  └─ RectTransform : ❌ MANQUANT (requis pour l'UI !)");
            }

            // Je vérifie le composant BonusDisplay (le script qui gère l'affichage)
            var bonusDisplay = _shieldDisplay.GetComponent<BonusDisplay>();
            if (bonusDisplay != null)
            {
                Debug.Log($"  └─ BonusDisplay script : ✓ Présent");
                Debug.Log($"  └─ BonusDisplay.IsActive() : {(bonusDisplay.IsActive() ? "✓ ACTIF" : "❌ INACTIF")}");
            }
            else
            {
                Debug.LogError($"  └─ BonusDisplay script : ❌ MANQUANT ! Le Shield ne peut pas fonctionner sans ce script !");
            }
        }
        else
        {
            Debug.LogError("❌ Shield Display est NULL ! Impossible d'afficher les détails.");
        }

        Debug.Log("========================================");
    }
#endif
}