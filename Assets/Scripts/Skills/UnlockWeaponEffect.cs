using UnityEngine;

[CreateAssetMenu(menuName = "Game/SkillEffects/Unlock Weapon")]
public class UnlockWeaponEffect : SkillEffect
{
    // Per user request, unlock is disabled. This effect does nothing.
    // Keep the script to avoid missing script errors on existing assets.

    public override void Apply(PlayerContext ctx, int level)
    {
        // No-op
    }
}
