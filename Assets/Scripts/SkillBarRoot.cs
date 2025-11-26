using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillBarRoot : MonoBehaviour
{
    [Header("Active (weapon) slots")]
    [SerializeField] private SkillBarSlot[] activeSlots;   

    [Header("Passive slots")]
    [SerializeField] private SkillBarSlot[] passiveSlots; 

    // map skillId -> slot để lần sau up level không chiếm ô mới
    private Dictionary<string, SkillBarSlot> slotBySkillId = new();

    public void OnSkillLeveledUp(SkillData data, int newLevel)
    {
        if (data == null) return;

        // xem đã có slot cho skill này chưa
        if (!slotBySkillId.TryGetValue(data.skillId, out var slot))
        {

            // chưa có → lấy slot trống tuỳ loại
            SkillBarSlot[] pool = (data.type == SkillData.SkillType.Weapon)
                ? activeSlots
                : passiveSlots;

            slot = FindEmptySlot(pool);
            if (slot == null)
            {
                Debug.Log("SkillBarRoot: hết slot cho " + data.skillName);
                return;
            }

            slotBySkillId[data.skillId] = slot;

            slot.SetIcon(data.icon);
        }

        slot.SetLevel(newLevel);
    }

    private SkillBarSlot FindEmptySlot(SkillBarSlot[] pool)
    {
        foreach (var s in pool)
        {
            Debug.Log(s.HasSkill);
            if (!s.HasSkill)
                return s;
        }
        return null;
    }
}
