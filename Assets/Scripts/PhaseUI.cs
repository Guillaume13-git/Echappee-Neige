using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Je gère l'affichage visuel de la phase actuelle de la piste dans l'interface utilisateur.
/// Mon rôle : afficher le nom et la couleur de la phase (Verte, Bleue, Rouge, Noire) au joueur.
/// </summary>
public class PhaseUI : MonoBehaviour
{
    [Header("References")]
    // Je stocke la référence vers l'image qui représente visuellement la phase (un cercle coloré)
    [SerializeField] private Image _phaseIcon;
    
    // Je stocke la référence vers le texte qui affiche le nom de la phase
    [SerializeField] private TextMeshProUGUI _phaseText;

    [Header("Phase Settings")]
    // Je stocke un tableau de données qui associe chaque phase à son nom et sa couleur
    [SerializeField] private PhaseDisplayData[] _phasesData;

    /// <summary>
    /// Au démarrage, je m'initialise avec un léger délai pour m'assurer que le PhaseManager est prêt.
    /// </summary>
    private void Start()
    {
        // J'attends 0.1 seconde avant de m'initialiser pour garantir que le PhaseManager existe
        // Cela évite les problèmes de timing où je chercherais le PhaseManager avant qu'il ne soit créé
        Invoke(nameof(InitializeUI), 0.1f);
    }

    /// <summary>
    /// Je m'initialise en me connectant au PhaseManager et en affichant la phase actuelle.
    /// </summary>
    private void InitializeUI()
    {
        // Je vérifie que le PhaseManager existe
        if (PhaseManager.Instance != null)
        {
            // Je m'abonne à l'événement de changement de phase pour être notifié automatiquement
            PhaseManager.Instance.OnPhaseChanged += UpdatePhaseDisplay;
            
            // J'affiche immédiatement la phase actuelle au démarrage
            UpdatePhaseDisplay(PhaseManager.Instance.CurrentPhase);
            
            // Je log mon initialisation réussie si mon GameObject est actif
            if(gameObject.activeInHierarchy) Debug.Log("[PhaseUI] Initialisation réussie.");
        }
        else
        {
            // Si le PhaseManager n'existe pas, je signale une erreur critique
            Debug.LogError("[PhaseUI] PhaseManager introuvable au démarrage !");
        }
    }

    /// <summary>
    /// Avant d'être détruit, je me désabonne proprement des événements pour éviter les erreurs.
    /// </summary>
    private void OnDestroy()
    {
        // Je me désabonne de l'événement si le PhaseManager existe encore
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdatePhaseDisplay;
    }

    /// <summary>
    /// Je mets à jour l'affichage visuel de la phase (couleur et nom).
    /// Cette méthode est appelée automatiquement quand la phase change.
    /// </summary>
    /// <param name="phase">La nouvelle phase à afficher</param>
    public void UpdatePhaseDisplay(TrackPhase phase)
    {
        // Je vérifie que j'ai bien des données de phases configurées
        if (_phasesData == null || _phasesData.Length == 0) return;

        // Je cherche dans mes données la phase qui correspond à celle reçue en paramètre
        foreach (var data in _phasesData)
        {
            if (data.Phase == phase)
            {
                // ✅ Je mets à jour l'icône (le cercle coloré)
                if (_phaseIcon != null) 
                {
                    // J'applique la couleur de la phase à mon icône
                    _phaseIcon.color = data.PhaseColor;
                    
                    // Je force l'alpha à 1 pour m'assurer que l'icône est bien visible
                    // (au cas où l'image serait transparente par défaut dans l'inspecteur)
                    Color c = _phaseIcon.color;
                    c.a = 1f;
                    _phaseIcon.color = c;
                }

                // ✅ Je mets à jour le texte
                if (_phaseText != null)
                {
                    // J'affiche le nom de la phase (par exemple "Piste Verte")
                    _phaseText.text = data.PhaseName;
                    
                    // J'applique aussi la couleur de la phase au texte pour la cohérence visuelle
                    _phaseText.color = data.PhaseColor;
                }
                
                // Je log le changement si les logs de debug sont activés
                if (debugLogs) Debug.Log($"[PhaseUI] Affichage mis à jour : {data.PhaseName}");
                
                // J'ai trouvé et affiché la phase, je peux sortir de la boucle
                return;
            }
        }
    }
    
    // Je permets d'activer/désactiver mes logs de debug depuis l'inspecteur
    [SerializeField] private bool debugLogs = true;
}

/// <summary>
/// Je suis une structure de données qui associe une phase à son affichage visuel.
/// Je regroupe : la phase (enum), son nom affiché (string), et sa couleur (Color).
/// </summary>
[System.Serializable]
public struct PhaseDisplayData
{
    public TrackPhase Phase;      // Je stocke la phase concernée (Green, Blue, Red, Black)
    public string PhaseName;      // Je stocke le nom à afficher (ex: "Piste Verte")
    public Color PhaseColor;      // Je stocke la couleur associée (ex: #3AA435 pour le vert)
}