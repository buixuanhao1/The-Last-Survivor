using UnityEngine;
using System.Collections.Generic;

public class LevelUpPanel : MonoBehaviour
{
    public GameObject panelOverlay;
    public SkillOptionUI[] skillOptions;
    public SkillData[] allSkills;

    [SerializeField] private SkillManager playerSkillManager;

    void Start()
    {
        if (panelOverlay != null)
            panelOverlay.SetActive(false);
    }

    public void Show()
    {
        panelOverlay.SetActive(true);

        List<SkillData> randoms = GetRandomSkills(3);
        for (int i = 0; i < skillOptions.Length; i++)
        {
            skillOptions[i].Setup(randoms[i], this);
        }

        Time.timeScale = 0f; // pause game khi chọn skill
    }

    public void Hide()
    {
        panelOverlay.SetActive(false);
        Time.timeScale = 1f;
    }

    public void SelectSkill(SkillData chosen)
    {

        if (playerSkillManager != null)
            playerSkillManager.AddSkill(chosen);

        Hide();
    }

    private List<SkillData> GetRandomSkills(int count)
    {
        List<SkillData> copy = new List<SkillData>(allSkills);
        List<SkillData> result = new List<SkillData>();

        count = Mathf.Min(count, copy.Count);

        for (int i = 0; i < count; i++)
        {
            int r = Random.Range(0, copy.Count);
            result.Add(copy[r]);
            copy.RemoveAt(r);
        }

        return result;
    }
}
