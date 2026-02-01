using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'affichage visuel de la phase actuelle (Nom et Couleur).
/// </summary>
public class PhaseUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _phaseIcon; // Ton cercle
    [SerializeField] private TextMeshProUGUI _phaseText;

    [Header("Phase Settings")]
    [SerializeField] private PhaseDisplayData[] _phasesData;

    private void Start()
    {
        // On attend la fin de la frame pour être sûr que le PhaseManager est prêt
        Invoke(nameof(InitializeUI), 0.1f);
    }

    private void InitializeUI()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnPhaseChanged += UpdatePhaseDisplay;
            UpdatePhaseDisplay(PhaseManager.Instance.CurrentPhase);
            if(gameObject.activeInHierarchy) Debug.Log("[PhaseUI] Initialisation réussie.");
        }
        else
        {
            Debug.LogError("[PhaseUI] PhaseManager introuvable au démarrage !");
        }
    }

    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= UpdatePhaseDisplay;
    }

    public void UpdatePhaseDisplay(TrackPhase phase)
    {
        if (_phasesData == null || _phasesData.Length == 0) return;

        foreach (var data in _phasesData)
        {
            if (data.Phase == phase)
            {
                // ✅ Mise à jour de l'icône (Cercle)
                if (_phaseIcon != null) 
                {
                    _phaseIcon.color = data.PhaseColor;
                    // Force l'Alpha à 1 au cas où l'image est transparente par défaut
                    Color c = _phaseIcon.color;
                    c.a = 1f;
                    _phaseIcon.color = c;
                }

                // ✅ Mise à jour du texte
                if (_phaseText != null)
                {
                    _phaseText.text = data.PhaseName;
                    _phaseText.color = data.PhaseColor;
                }
                
                if (debugLogs) Debug.Log($"[PhaseUI] Affichage mis à jour : {data.PhaseName}");
                return;
            }
        }
    }
    
    [SerializeField] private bool debugLogs = true;
}

[System.Serializable]
public struct PhaseDisplayData
{
    public TrackPhase Phase;
    public string PhaseName;
    public Color PhaseColor;
}