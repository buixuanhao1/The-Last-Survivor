using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    public abstract void Apply(PlayerContext ctx, int level);
}
