using UnityEngine;
using System.Collections.Generic;

public class LevelUpPanel : MonoBehaviour
{
    public GameObject panelOverlay;
    public Transform skillContainer;       // Parent: LevelUp_Window/SkillContainer
    public SkillOptionUI[] skillOptions;
    public SkillData[] allSkills;

    [SerializeField] private SkillManager playerSkillManager;

    void Awake()
    {
        if (playerSkillManager == null)
            playerSkillManager = FindFirstObjectByType<SkillManager>();
    }



    public void Show()
    {
        panelOverlay.SetActive(true);
        Time.timeScale = 0f; // pause game khi chọn skill

        int optionCount = skillOptions != null ? skillOptions.Length : 0;
        if (optionCount == 0)
        {
            Debug.LogWarning("LevelUpPanel: No skill options found in prefab.");
        }
        else
        {
            int pick = Mathf.Min(3, optionCount);
            List<SkillData> randoms = GetRandomSkills(pick);
            for (int i = 0; i < pick; i++)
            {
                skillOptions[i].Setup(randoms[i], this);
            }
        }
    }

    public void Hide()
    {
        Time.timeScale = 1f;
        Destroy(gameObject);
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
