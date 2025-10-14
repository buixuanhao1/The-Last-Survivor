using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Thông tin Skill")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    public enum SkillType { Attack, Passive, Utility }
    public SkillType type;

    [Header("Thông số Skill")]
    public float value; // ví dụ: damage, tốc độ, giảm hồi chiêu...
}
