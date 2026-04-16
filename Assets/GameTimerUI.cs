using UnityEngine;
using TMPro;

public class GameTimerUI : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    private float elapsedTime = 0f;
    private bool isRunning = true;

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        UpdateTimeText();
    }

    void UpdateTimeText()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        // Format: 1:02
        timeText.text = $"{minutes}:{seconds:D2}";
    }

    // Gọi khi muốn dừng đồng hồ (pause game, end game...)
    public void StopTimer()
    {
        isRunning = false;
    }

    // Reset về 0:00
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimeText();
    }
}
