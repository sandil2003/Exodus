using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int totalHumans = 5;
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
        _hud.UpdateRescuedCount(_rescuedCount, totalHumans);
        if (_rescuedCount >= totalHumans)
            _wl.TriggerWin();
    }
}