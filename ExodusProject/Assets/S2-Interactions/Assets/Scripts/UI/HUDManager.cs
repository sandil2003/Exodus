using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public Slider healthSlider;
    public TextMeshProUGUI passengerText;
    public TextMeshProUGUI rescuedText;
    public GameObject ePromptUI;

    public void UpdateTimer(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60);
        int s = Mathf.FloorToInt(seconds % 60);
        timerText.text = $"{m:00}:{s:00}";
        timerText.color = seconds < 30 ? Color.red : Color.white;
    }

    public void UpdateHealth(int current, int max)
    {
        healthSlider.value = (float)current / max;
    }

    public void UpdatePassengerCount(int count)
    {
        passengerText.text = $"Passengers: {count}/2";
    }

    public void UpdateRescuedCount(int count, int total)
    {
        rescuedText.text = $"Rescued: {count}/{total}";
    }

    public void ShowEPrompt(bool show)
    {
        ePromptUI.SetActive(show);
    }
}