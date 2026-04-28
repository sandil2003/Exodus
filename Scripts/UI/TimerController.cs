using UnityEngine;

public class TimerController : MonoBehaviour
{
    public float totalTime = 180f;
    private float _remaining;
    private HUDManager _hud;
    private bool _running = true;

    void Start()
    {
        _remaining = totalTime;
        FindHUD();
        
        // Initial update so it doesn't wait a frame
        if (_hud != null) _hud.UpdateTimer(_remaining);
    }

    private void FindHUD()
    {
        if (_hud == null)
        {
            _hud = FindObjectOfType<HUDManager>();
        }
    }

    void Update()
    {
        if (!_running) return;

        _remaining -= Time.deltaTime;
        
        if (_remaining <= 0)
        {
            _remaining = 0;
            _running = false;
            
            var wl = FindObjectOfType<WinLoseManager>();
            if (wl != null) wl.TriggerLose("timer");
        }

        if (_hud != null)
        {
            _hud.UpdateTimer(_remaining);
        }
    }

    public void StopTimer() => _running = false;
    public float GetRemaining() => _remaining;
}