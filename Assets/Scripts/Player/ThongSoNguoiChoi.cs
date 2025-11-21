using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Runtime modifiers")]
    [Tooltip("+% move speed (0.2 = +20%)")] public float moveSpeedPercent = 0f;
    [Tooltip("+% damage")] public float damagePercent = 0f;
    [Tooltip("+ flat damage")] public int damageFlat = 0;
    [Tooltip("+% fire rate")] public float fireRatePercent = 0f;
    [Tooltip("+ EXP pickup range (world units)")] public float pickupRangeAdd = 0f;
    [Tooltip("+ health regen per second")] public float healthRegenPerSec = 0f;
    [Tooltip("+% projectile travel speed")] public float projectileSpeedPercent = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public float ComputeMoveSpeed(float baseSpeed)
    {
        return baseSpeed * (1f + Mathf.Max(-0.9f, moveSpeedPercent));
    }

    public int ComputeDamage(int baseDamage)
    {
        float basePlus = Mathf.Max(0, baseDamage + damageFlat);
        float mul = 1f + damagePercent;
        return Mathf.Max(0, Mathf.RoundToInt(basePlus * mul));
    }

    public float ComputeFireRate(float baseFireRate)
    {
        // fireRate = shots per second
        return baseFireRate * (1f + fireRatePercent);
    }

    public float ComputeProjectileSpeed(float baseSpeed)
    {
        // Giới hạn giảm tối đa -90% để tránh đảo chiều/đứng yên
        return baseSpeed * (1f + Mathf.Max(-0.9f, projectileSpeedPercent));
    }
}
