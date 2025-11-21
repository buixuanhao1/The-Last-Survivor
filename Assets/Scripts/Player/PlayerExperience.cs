using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExperience : MonoBehaviour
{
    [Header("EXP Settings")]
    public Image expFill;                   // Thanh EXP
    public TextMeshProUGUI levelText;       // Text hiển thị level
    public GameObject levelUpPanelPrefab;   // Panel chọn skill (prefab)
    public Transform uiParent;              // Chỗ gắn UI (Canvas) khi Instantiate

    [Header("EXP Config")]
    public float baseRequiredExp = 100f;    // EXP cần ban đầu
    public float expMultiplier = 1.2f;      // EXP tăng theo cấp

    private int level = 1;
    private float currentExp = 0;
    private float requiredExp;

    void Start()
    {
        requiredExp = baseRequiredExp;
        UpdateUI();
    }


    public void AddExp(float amount)
    {
        currentExp += amount;

        while (currentExp >= requiredExp)
        {
            currentExp -= requiredExp;
            LevelUp();
        }

        UpdateUI();
    }


    void LevelUp()
    {
        level++;
        requiredExp *= expMultiplier;
        UpdateUI();

        if (levelUpPanelPrefab != null)
        {
            if (FindFirstObjectByType<LevelUpPanel>() == null)
            {
                GameObject instance = uiParent != null
                    ? Instantiate(levelUpPanelPrefab, uiParent)
                    : Instantiate(levelUpPanelPrefab);

                // Find LevelUpPanel even if it's on a child of the prefab root
                LevelUpPanel panel = instance.GetComponent<LevelUpPanel>();
                if (panel == null)
                    panel = instance.GetComponentInChildren<LevelUpPanel>(true);
                if (panel != null)
                {
                    panel.Show();
                }
            }
        }
    }


    void UpdateUI()
    {
        if (expFill != null)
            expFill.fillAmount = currentExp / requiredExp;

        if (levelText != null)
            levelText.text = level.ToString();
    }
}
