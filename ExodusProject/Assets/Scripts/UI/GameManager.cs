using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int totalHumans = 2;
    private int _rescuedCount = 0;
    private HUDManager _hud;
    private WinLoseManager _wl;

    void Start()
    {
        _hud = FindObjectOfType<HUDManager>();
        _wl = FindObjectOfType<WinLoseManager>();
    }

    public void OnHumansDeposited(int count)
    {
        _rescuedCount += count;
        Debug.Log($"[GameManager] {count} humans deposited. Total rescued: {_rescuedCount}/{totalHumans}");

        if (_hud != null)
        {
            _hud.UpdateRescuedCount(_rescuedCount, totalHumans);
        }

        if (_rescuedCount >= totalHumans)
        {
            if (_wl != null)
            {
                Debug.Log("[GameManager] Goal reached! Triggering Win.");
                _wl.TriggerWin();
            }
            else
            {
                Debug.LogError("[GameManager] CRITICAL: WinLoseManager was not found in the scene!");
            }
        }
    }
}