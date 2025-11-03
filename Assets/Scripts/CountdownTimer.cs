using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Text
using TMPro; // If using TextMeshPro, use this instead

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private float timeRemaining = 10f; // Initial time for the countdown
    [SerializeField] private bool timerIsRunning = false;
    [SerializeField] private GameManager gameManager;

    // Use TextMeshProUGUI for TextMeshPro, or Text for legacy UI Text
    public TextMeshProUGUI timerText; // Assign this in the Inspector

    void Start()
    {
        timerIsRunning = true; // Start the timer automatically
    }

    void Update()
    {
        if (!gameManager.IsPaused())
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime; // Decrease time by the time since last frame
                UpdateTimerDisplay(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;

                // Add any actions to perform when the timer finishes
                gameManager.SetGameStateGameOver();
            }
        }
    }

    void UpdateTimerDisplay(float currentTime)
    {
        currentTime += 1; // To account for floor rounding, display time as if it's counting down to 1 instead of 0

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // You can add methods to start, pause, or reset the timer if needed
    public void StartTimer()
    {
        timerIsRunning = true;
    }

    public void StopTimer()
    {
        timerIsRunning = false;
    }

    public void ResetTimer(float newTime)
    {
        timeRemaining = newTime;
        timerIsRunning = false;
        UpdateTimerDisplay(timeRemaining);
    }
}