using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float gameTime = 180f;
    public int totalHumans = 4;

    [Header("Game State")]
    public int rescuedHumans = 0;
    public bool gameOver = false;

    [Header("References")]
    public PlayerHealth playerHealth;
    public PlayerPickup playerPickup;

    [Header("UI")]
    public Text timerText;
    public Text rescueText;
    public Text passengerText;
    public GameObject winPanel;
    public GameObject losePanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    void Update()
    {
        if (gameOver) return;

        UpdateTimer();
        UpdateUI();
        CheckGameState();
    }

    void UpdateTimer()
    {
        gameTime -= Time.deltaTime;

        if (gameTime <= 0)
        {
            gameTime = 0;
            LoseGame();
        }

        float minutes = Mathf.FloorToInt(gameTime / 60);
        float seconds = Mathf.FloorToInt(gameTime % 60);

        if (timerText != null)
            timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    void UpdateUI()
    {
        if (rescueText != null)
            rescueText.text = "Rescued: " + rescuedHumans + " / " + totalHumans;

        if (passengerText != null)
            passengerText.text = "Passengers: " + playerPickup.passengerCount;
    }

    void CheckGameState()
    {
        if (rescuedHumans >= totalHumans)
        {
            WinGame();
        }

        if (playerHealth.currentHealth <= 0)
        {
            LoseGame();
        }
    }

    public void AddRescuedHumans(int amount)
    {
        rescuedHumans += amount;
    }

    void WinGame()
    {
        gameOver = true;
        Debug.Log("YOU WIN");

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    void LoseGame()
    {
        gameOver = true;
        Debug.Log("YOU LOSE");

        if (losePanel != null)
            losePanel.SetActive(true);

        Time.timeScale = 0f;
    }
}