using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerExperience : MonoBehaviour
{
    [Header("EXP Settings")]
    public Image expFill;                   // Thanh EXP
    public TextMeshProUGUI levelText;       // Text hiển thị level
    public GameObject levelUpPanel;         // Panel chọn skill
    public Transform skillContainer;        // Chỗ chứa các button skill
    public GameObject skillButtonPrefab;    // Prefab button skill

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

    /// <summary>
    /// Thêm EXP cho người chơi
    /// </summary>
    public void AddExp(float amount)
    {
        currentExp += amount;

        // Cho phép lên nhiều cấp trong một lần cộng EXP
        while (currentExp >= requiredExp)
        {
            currentExp -= requiredExp;
            LevelUp();
        }

        UpdateUI();
    }

    /// <summary>
    /// Khi lên cấp
    /// </summary>
    void LevelUp()
    {
        level++;
        requiredExp *= expMultiplier;

        UpdateUI();

        // Mở panel chọn skill
        if (levelUpPanel != null)
        {
            LevelUpPanel panel = levelUpPanel.GetComponent<LevelUpPanel>();
            if (panel != null)
            {
                panel.Show();
            }
        }
    }


    /// <summary>
    /// Update thanh EXP và text level
    /// </summary>
    void UpdateUI()
    {
        if (expFill != null)
            expFill.fillAmount = currentExp / requiredExp;

        if (levelText != null)
            levelText.text = level.ToString();
    }

    /// <summary>
    /// Hiện 3 skill random trong panel
    /// </summary>
    void ShowRandomSkills()
    {
        // Xóa button cũ
        foreach (Transform child in skillContainer)
        {
            Destroy(child.gameObject);
        }

        // Giả sử mình có 10 skill (ID từ 0 -> 9)
        List<int> allSkills = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            allSkills.Add(i);
        }

        // Random ra 3 skill
        for (int i = 0; i < 3; i++)
        {
            int randIndex = Random.Range(0, allSkills.Count);
            int skillId = allSkills[randIndex];
            allSkills.RemoveAt(randIndex);

            GameObject btnObj = Instantiate(skillButtonPrefab, skillContainer);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = "Skill " + skillId;

            int chosenId = skillId; // tránh lỗi lambda
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnSkillChosen(chosenId));
        }
    }

    /// <summary>
    /// Gọi khi chọn skill
    /// </summary>
    public void OnSkillChosen(int skillId)
    {
        Debug.Log("Chose skill: " + skillId);

        // TODO: gửi skillId sang WeaponManager/SkillManager để xử lý
        // FindObjectOfType<WeaponManager>().AddSkill(skillId);

        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        Time.timeScale = 1f; // resume game 
    }
}
