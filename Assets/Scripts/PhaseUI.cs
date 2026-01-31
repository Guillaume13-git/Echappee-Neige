using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'affichage visuel de la phase actuelle (Nom et Couleur)
/// </summary>
public class PhaseUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _phaseIcon;
    [SerializeField] private TextMeshProUGUI _phaseText;

    [Header("Phase Settings")]
    [SerializeField] private PhaseDisplayData[] _phasesData;

    private void Start()
    {
        if (PhaseManager.Instance != null)
        {
            // On s'abonne à l'événement de changement de phase
            PhaseManager.Instance.OnPhaseChanged += UpdatePhaseDisplay;
            
            // Initialisation immédiate au lancement
            UpdatePhaseDisplay(PhaseManager.Instance.CurrentPhase);
        }
        else
        {
            Debug.LogWarning("[PhaseUI] PhaseManager.Instance est introuvable !");
        }
    }

    private void OnDestroy()
    {
        // Toujours se désabonner pour éviter les fuites de mémoire
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdatePhaseDisplay;
    }

    private void UpdatePhaseDisplay(TrackPhase phase)
    {
        if (_phasesData == null || _phasesData.Length == 0) return;

        // On parcourt les réglages pour trouver la phase correspondante
        foreach (var data in _phasesData)
        {
            if (data.Phase == phase)
            {
                // Mise à jour de l'icône
                if (_phaseIcon != null) 
                    _phaseIcon.color = data.PhaseColor;

                // Mise à jour du texte (Contenu ET Couleur pour plus de style)
                if (_phaseText != null)
                {
                    _phaseText.text = data.PhaseName;
                    _phaseText.color = data.PhaseColor; // Optionnel : le texte prend aussi la couleur
                }
                
                return; // On a trouvé, on arrête la boucle
            }
        }
    }
}

[System.Serializable]
public struct PhaseDisplayData
{
    public TrackPhase Phase;
    public string PhaseName;
    public Color PhaseColor;
}