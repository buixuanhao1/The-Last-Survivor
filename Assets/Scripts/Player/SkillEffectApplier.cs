using UnityEngine;

public class SkillEffectApplier : MonoBehaviour
{
    [SerializeField] private PlayerWeaponSystem weaponSystem;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerStats playerStats;

    public void ApplySkill(SkillData data, int level)
    {
        if (data == null) return;

        // build context
        var ctx = new PlayerContext
        {
            weaponSystem = weaponSystem != null ? weaponSystem : GetComponentInParent<PlayerWeaponSystem>(),
            weaponManager = weaponManager != null ? weaponManager : FindFirstObjectByType<WeaponManager>(),
            stats = playerStats != null ? playerStats : FindFirstObjectByType<PlayerStats>()
        };

        if (data.effects == null || data.effects.Count == 0)
        {
            Debug.LogWarning($"Skill {data.skillId} has no effects");
            return;
        }

        foreach (var eff in data.effects)
        {
            if (eff == null) continue;
            eff.Apply(ctx, level);
        }
    }
}
