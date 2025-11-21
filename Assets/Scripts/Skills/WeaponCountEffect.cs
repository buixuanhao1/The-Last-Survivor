using UnityEngine;

[CreateAssetMenu(menuName = "Game/SkillEffects/Weapon Count")]
public class WeaponCountEffect : SkillEffect
{
    public WeaponId weapon;
    public int baseCount = 1;
    public int perLevel = 1;

    public override void Apply(PlayerContext ctx, int level)
    {
        if (ctx == null || ctx.weaponSystem == null) return;
        int clampedLevel = Mathf.Max(1, level);
        int targetAtLevel = Mathf.Max(0, baseCount + perLevel * (clampedLevel - 1));
        int prevAtLevel = clampedLevel > 1 ? Mathf.Max(0, baseCount + perLevel * (clampedLevel - 2)) : 0;
        int delta = targetAtLevel - prevAtLevel; // tăng thêm ở level hiện tại

        switch (weapon)
        {
            case WeaponId.Shuriken:
                ctx.weaponSystem.SetShurikenCount(ctx.weaponSystem.shurikenCount + delta);
                break;
            case WeaponId.Tarot:
                ctx.weaponSystem.SetTarotCount(ctx.weaponSystem.tarotCount + delta);
                break;
            case WeaponId.StoneOrbit:
                ctx.weaponSystem.SetStoneCount(ctx.weaponSystem.stoneCount + delta);
                break;
            case WeaponId.Sword:
                ctx.weaponSystem.SetSwordCount(ctx.weaponSystem.swordCount + delta);
                break;
            case WeaponId.Knife:
                ctx.weaponSystem.SetKnifeCount(ctx.weaponSystem.knifeCount + delta);
                break;
        }

        // Kích hoạt ngay: đặt cooldown về 0 để bắn/spawn ở frame kế tiếp
        if (ctx.weaponManager != null)
        {
            ctx.weaponManager.ResetCooldown(weapon);
        }
    }
}
