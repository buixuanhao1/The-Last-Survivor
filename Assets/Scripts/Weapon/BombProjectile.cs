using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    public WeaponData data;
    private ObjectPool pool;
    private float timer;
    private PlayerStats stats;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float travelTime = 0.8f;
    private float maxHeight = 1.5f;

    public void Init(WeaponData weapon, Vector3 start, Vector3 target, ObjectPool objectPool, float travelSeconds, float peakHeight)
    {
        data = weapon;
        pool = objectPool;
        timer = 0f;
        if (stats == null) stats = PlayerStats.Instance != null ? PlayerStats.Instance : Object.FindFirstObjectByType<PlayerStats>();
        startPos = start;
        targetPos = target;
        travelTime = Mathf.Max(0.05f, travelSeconds);
        maxHeight = Mathf.Max(0f, peakHeight);
        transform.position = startPos;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;

        float t = Mathf.Clamp01(timer / travelTime);
        // parabolic arc: base along line + vertical hump 4h t (1-t)
        Vector3 basePos = Vector3.Lerp(startPos, targetPos, t);
        float height = 4f * maxHeight * t * (1f - t);
        // Use Vector3.forward's cross to get a pseudo 'up' in 2D (Z is constant); we can add on Y
        Vector3 pos = basePos + new Vector3(0f, height, 0f);
        transform.position = pos;

        if (t >= 1f)
        {
            Explode();
        }
    }

    // Do not explode on enemy trigger; only on landing

    private void Explode()
    {
        if (data.explosionPrefab != null)
        {
            var go = GameObject.Instantiate(data.explosionPrefab, transform.position, Quaternion.identity);
            var ex = go.GetComponent<Explosion>();
            if (ex != null)
            {
                int dmg = stats != null ? stats.ComputeDamage(data.damage) : data.damage;
                // Only prepare; Animation Event will call TriggerFromAnimation at the exact frame
                ex.Prepare(dmg, data.explosionRadius);
            }
        }
        pool.Return(gameObject);
    }
}
