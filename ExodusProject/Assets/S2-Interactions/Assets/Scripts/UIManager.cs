using UnityEngine;
using UnityEngine.UI;
using TMPro; // Ensure you have TextMeshPro installed

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // Singleton for easy access from other scripts

    [Header("HUD Elements")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI passengerText;
    [SerializeField] private TextMeshProUGUI rescueText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Screens")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    private void Awake()
    {
        Instance = this;
        // Initialize panels as hidden
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    // Called by PlayerHealth.cs
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        healthBar.value = currentHealth / maxHealth;
    }

    // Called by PlayerPickup.cs
    public void UpdatePassengerCount(int current, int max)
    {
        passengerText.text = $"Passengers: {current}/{max}";
    }

    // Called by SafeZone.cs or GameManager.cs
    public void UpdateRescueCount(int rescued, int total)
    {
        rescueText.text = $"Rescued: {rescued}/{total}";
    }

    // Called by GameManager.cs every frame
    public void UpdateTimer(float timeRemaining)
    {
        float minutes = Mathf.FloorToInt(timeRemaining / 60);
        float seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("Time Left: {0:00}:{1:00}", minutes, seconds);
    }

    public void ShowWinScreen()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game
    }

    public void ShowLoseScreen()
    {
        losePanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game
    }
}