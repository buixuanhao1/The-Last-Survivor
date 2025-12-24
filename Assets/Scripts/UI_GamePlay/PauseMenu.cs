using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PanelPause : MonoBehaviour
{
    [Header("UI Root")]
    public GameObject panelOverlay;        // UI Pause Panel

    [Header("Skills UI (Kỹ năng đã chọn)")]
    [SerializeField] private SkillBarRoot skillBarRoot;
    [SerializeField] private SkillManager playerSkillManager;

    private void Awake()
    {
        if (playerSkillManager == null)
            playerSkillManager = FindFirstObjectByType<SkillManager>();

        if (skillBarRoot == null)
            skillBarRoot = GetComponentInChildren<SkillBarRoot>(true);
    }

  
    public void Show()
    {
        panelOverlay.SetActive(true);
        Time.timeScale = 0f;

        RefreshSkillDisplay();
    }

    public void Hide()
    {
        Time.timeScale = 1f;
        panelOverlay.SetActive(false);
    }

    public void OnClick_Continue()
    {
        Hide();
    }

    public void OnClick_Quit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

 

    private void RefreshSkillDisplay()
    {
        if (skillBarRoot == null || playerSkillManager == null)
            return;

        skillBarRoot.ResetAllSlots();

        Dictionary<string, int> allSkills = playerSkillManager.GetAllSkillLevels();

        foreach (var pair in allSkills)
        {
            string skillId = pair.Key;
            int level = pair.Value;
            SkillData data = playerSkillManager.GetSkillDataById(skillId);
            if (data == null) continue;
            skillBarRoot.OnSkillLeveledUp(data, level);
        }
    }
}
