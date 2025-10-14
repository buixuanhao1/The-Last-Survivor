using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Config")]
    [SerializeField] private WeaponData stoneOrbit;   // asset gốc (StoneData.asset)

    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;

    private List<SkillData> acquiredSkills = new List<SkillData>();
    private WeaponData stoneOrbitRuntime; // bản runtime để không sửa asset

    public void AddSkill(SkillData skill)
    {
        if (!acquiredSkills.Contains(skill))
        {
            acquiredSkills.Add(skill);
            Debug.Log("Đã thêm skill: " + skill.skillName);
            ApplySkillEffect(skill);
        }
        else
        {
            // Nếu đã có rồi thì coi như nâng cấp
            ApplySkillEffect(skill);
        }
    }

    private void ApplySkillEffect(SkillData skill)
    {
        if (skill.skillName == "Stone Orbit")
        {
            if (stoneOrbitRuntime == null)
            {
                // Clone runtime để tránh sửa asset gốc
                stoneOrbitRuntime = ScriptableObject.Instantiate(stoneOrbit);
                weaponManager.RegisterWeapon(stoneOrbitRuntime);
                Debug.Log($"Thêm Stone Orbit với {stoneOrbitRuntime.orbitCount} viên đá");
            }
            else
            {
                // Nâng cấp trên bản clone
                stoneOrbitRuntime.orbitCount += 1;
                Debug.Log($"Nâng cấp Stone Orbit lên {stoneOrbitRuntime.orbitCount} viên đá");
            }
        }

        // Sau này có thêm skill khác thì else if thêm ở đây
    }
}
