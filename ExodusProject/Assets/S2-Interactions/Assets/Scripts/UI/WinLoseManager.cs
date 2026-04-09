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
        _timer.StopTimer();
        winPanel.SetActive(true);
        Time.timeScale = 0f;
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