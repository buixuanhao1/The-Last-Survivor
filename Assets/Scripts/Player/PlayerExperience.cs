using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExperience : MonoBehaviour
{
    [Header("EXP Settings")]
    public Image expFill;
    public TextMeshProUGUI levelText;
    public GameObject levelUpPanel;

    private int level = 1;
    private float currentExp = 0;
    private float requiredExp = 100;

    void Start()
    {
        UpdateUI();
        levelUpPanel.SetActive(false);
    }

    public void AddExp(float amount)
    {
        currentExp += amount;
        if (currentExp >= requiredExp)
        {
            LevelUp();
        }
        UpdateUI();
    }

    void LevelUp()
    {
        currentExp -= requiredExp;
        level++;
        requiredExp *= 1.2f; // Tăng dần EXP cần

        // Cập nhật UI
        UpdateUI();

        // Hiện bảng chọn skill
        Time.timeScale = 0f; // Pause game
        levelUpPanel.SetActive(true);
    }

    void UpdateUI()
    {
        float progress = currentExp / requiredExp;
        expFill.fillAmount = progress;
        levelText.text = level.ToString();
    }

    // Gọi khi chọn skill
    public void OnSkillChosen()
    {
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }
}
