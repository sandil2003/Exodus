using UnityEngine;

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

    void Awake()
    {
        Instance = this;

        // Fix the NullReferenceException by finding the player automatically
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerPickup == null) playerPickup = FindObjectOfType<PlayerPickup>();
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

        // Send the timer value to the UIManager instead of handling text here
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateTimer(gameTime);
    }

    void UpdateUI()
    {
        if (UIManager.Instance == null) return;

        // Send counts to the UIManager
        UIManager.Instance.UpdateRescueCount(rescuedHumans, totalHumans);
        
        if (playerPickup != null)
            UIManager.Instance.UpdatePassengerCount(playerPickup.passengerCount, 2);
    }

    void CheckGameState()
    {
        // Safety check to prevent crash if playerHealth is still null
        if (playerHealth == null) return;

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
        if (UIManager.Instance != null) UIManager.Instance.ShowWinScreen();
        Time.timeScale = 0f;
    }

    void LoseGame()
    {
        gameOver = true;
        Debug.Log("YOU LOSE");
        if (UIManager.Instance != null) UIManager.Instance.ShowLoseScreen();
        Time.timeScale = 0f;
    }
}