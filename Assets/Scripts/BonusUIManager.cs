using UnityEngine;

public class BonusUIManager : MonoBehaviour
{
    public static BonusUIManager Instance { get; private set; }

    [SerializeField] private BonusDisplay _speedBoostDisplay;
    [SerializeField] private BonusDisplay _shieldDisplay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Appelé quand on ramasse un Speed Boost
    public void TriggerSpeedBoost(float duration)
    {
        _speedBoostDisplay.ShowBonus(duration);
    }

    // Appelé quand on ramasse un Bouclier
    public void TriggerShield(float duration)
    {
        _shieldDisplay.ShowBonus(duration);
    }
}