using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseManager : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject losePanel;
    private TimerController _timer;

    void Start() => _timer = FindObjectOfType<TimerController>();

    public void TriggerWin()
    {
        if (_timer != null) _timer.StopTimer();
        
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("WinLoseManager: Win Panel is NOT assigned in the Inspector!");
        }
    }

    public void TriggerLose(string reason)
    {
        losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}