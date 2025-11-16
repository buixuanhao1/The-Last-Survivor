using UnityEngine;

public enum StatType
{
    MoveSpeedPercent,
    DamagePercent,
    DamageFlat,
    FireRatePercent,
    PickupRangeAdd,
    HealthRegenPerSec
}

[CreateAssetMenu(menuName = "Game/SkillEffects/Stat Modifier")]
public class StatModifierEffect : SkillEffect
{
    public StatType stat;
    public float baseValue = 0f;
    public float perLevel = 0f;

    public override void Apply(PlayerContext ctx, int level)
    {
        if (ctx == null || ctx.stats == null) return;
        float v = baseValue + perLevel * (Mathf.Max(1, level) - 1);
        switch (stat)
        {
            case StatType.MoveSpeedPercent:
                ctx.stats.moveSpeedPercent += v;
                break;
            case StatType.DamagePercent:
                ctx.stats.damagePercent += v;
                break;
            case StatType.DamageFlat:
                ctx.stats.damageFlat += Mathf.RoundToInt(v);
                break;
            case StatType.FireRatePercent:
                ctx.stats.fireRatePercent += v;
                break;
            case StatType.PickupRangeAdd:
                ctx.stats.pickupRangeAdd += v;
                break;
            case StatType.HealthRegenPerSec:
                ctx.stats.healthRegenPerSec += v;
                break;
        }
    }
}
