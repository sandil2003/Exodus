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
        if (timerText == null) return;

        // Force the object to be active so you can see it
        if (!timerText.gameObject.activeInHierarchy)
        {
            timerText.gameObject.SetActive(true);
        }

        int m = Mathf.FloorToInt(seconds / 60);
        int s = Mathf.FloorToInt(seconds % 60);
        string timeStr = $"{m:00}:{s:00}";
        
        // Use SetText (Best practice for TMP)
        timerText.SetText(timeStr);
        
        // Use a high-contrast color (Yellow) so it doesn't blend with white backgrounds
        timerText.color = seconds < 30 ? Color.red : Color.yellow;
        
        // Log the EXACT object name to help you find it in the Hierarchy
        Debug.Log($"[Timer Fix] Updating Object: '{timerText.gameObject.name}' with time: {timeStr}");
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
        if (ePromptUI != null)
        {
            ePromptUI.SetActive(show);
        }
    }
}