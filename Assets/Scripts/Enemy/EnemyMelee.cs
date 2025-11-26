using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyMelee : MonoBehaviour
{
    public int contactDamage = 10;
    public float damageInterval = 0.5f; // mỗi 0.5s mới trừ máu một lần

    private float timer;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        timer += Time.deltaTime;
        if (timer >= damageInterval)
        {
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage);
            timer = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            timer = 0f;
        }
    }
}
