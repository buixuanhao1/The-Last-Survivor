using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkillEffectApplier effectApplier;
    private SkillBarRoot skillBar;
    // Lưu level theo skillId
    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    [SerializeField] private SkillData[] allSkills;


    public int GetLevel(SkillData skill)
    {
        if (skill == null) return 0;
        return skillLevels.TryGetValue(skill.skillId, out var lv) ? lv : 0;
    }
    public Dictionary<string, int> GetAllSkillLevels()
    {
        return new Dictionary<string, int>(skillLevels);
    }
    public void AddSkill(SkillData skill)
    {
        if (skill == null) return;
        // Nếu chưa gán thì thử tìm lại tại đây
        if (skillBar == null)
        {
            skillBar = FindFirstObjectByType<SkillBarRoot>(FindObjectsInactive.Include);
            if (skillBar == null)
                Debug.Log("SkillManager: vẫn không tìm thấy SkillBarRoot trong AddSkill");
            else
                Debug.Log("SkillManager: tìm thấy SkillBarRoot trong AddSkill");
        }
        int current = GetLevel(skill);
        int newLevel = Mathf.Clamp(current + 1, 1, Mathf.Max(1, skill.maxLevel));
        skillLevels[skill.skillId] = newLevel;

        Debug.Log($"Chọn skill {skill.skillName} → level {newLevel}");

        if (effectApplier != null)
        {
            effectApplier.ApplySkill(skill, newLevel);
        }
        else
        {
            Debug.LogWarning("Thiếu SkillEffectApplier trong SkillManager");
        }

        if (skillBar != null)
        {
            skillBar.OnSkillLeveledUp(skill, newLevel);
        }

        
    }

    public SkillData GetSkillDataById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var s in allSkills)
        {
            if (s != null && s.skillId == id)
                return s;
        }
        return null;
    }

}
