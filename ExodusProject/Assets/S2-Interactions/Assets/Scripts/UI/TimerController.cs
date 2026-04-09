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
        _hud = FindObjectOfType<HUDManager>();
    }

    void Update()
    {
        if (!_running) return;
        _remaining -= Time.deltaTime;
        _remaining = Mathf.Max(0, _remaining);
        _hud.UpdateTimer(_remaining);
        if (_remaining <= 0)
        {
            _running = false;
            FindObjectOfType<WinLoseManager>().TriggerLose("timer");
        }
    }

    public void StopTimer() => _running = false;
    public float GetRemaining() => _remaining;
}