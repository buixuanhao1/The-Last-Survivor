using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.6f; // how long the explosion effect stays
    private float timer = 0f;
    private bool triggered = false;

    // Optional: If you want to drive destruction by animation event, you can call Finish() from Animation
    public void Trigger(int damage, float radius)
    {
        if (triggered) return;
        triggered = true;
        // Deal damage instantly in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            var enemy = hits[i].GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        // Animator (if present) will play automatically; we just wait for lifeTime
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
        // For visualization in editor when testing
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
