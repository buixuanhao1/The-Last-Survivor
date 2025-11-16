using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Thông tin Skill")]
    public string skillId;      // ví dụ: "KnifeCount", "TarotCount", "StoneOrbit", "SwordCount"
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    public enum SkillType { Weapon, Stat, Utility }
    public SkillType type;

    [Header("Cấp tối đa")]
    public int maxLevel = 5;

    [Header("Effects (SO)")]
    public List<SkillEffect> effects = new List<SkillEffect>();
}
