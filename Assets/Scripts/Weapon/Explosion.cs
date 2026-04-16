using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.6f;
    private float timer = 0f;
    private bool triggered = false;
    private int pendingDamage = 0;
    private float pendingRadius = 0f;

    public void Trigger(int damage, float radius)
    {
        if (triggered) return;
        triggered = true;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            var enemy = hits[i].GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    public void Prepare(int damage, float radius)
    {
        pendingDamage = damage;
        pendingRadius = radius;
    }

    public void TriggerFromAnimation()
    {
        Trigger(pendingDamage, pendingRadius);
    }

    private void Update()
    {
        if (!triggered) return;
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
