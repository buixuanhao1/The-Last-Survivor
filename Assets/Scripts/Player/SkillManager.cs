using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkillEffectApplier effectApplier;

    // Lưu level theo skillId
    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();

    public int GetLevel(SkillData skill)
    {
        if (skill == null) return 0;
        return skillLevels.TryGetValue(skill.skillId, out var lv) ? lv : 0;
    }

    public void AddSkill(SkillData skill)
    {
        if (skill == null) return;

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
    }
}
