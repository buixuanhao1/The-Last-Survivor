using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;
    private Transform player;
    private SpriteRenderer rbSprite;
    private int currentHP;

    private void Awake()
    {
        rbSprite = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHP = enemyData.baseHP;

        // Gán sprite từ data
        GetComponent<SpriteRenderer>().sprite = enemyData.sprite;
    }

    void Update()
    {
        if (player == null) return;

        // Di chuyển về phía player
        Vector2 dir = (player.position - transform.position).normalized;
        if (dir.x < 0)
        {
            rbSprite.flipX = true;
        }else if (dir.x > 0)
        {
            rbSprite.flipX=false;
        }
        transform.position += (Vector3)dir * enemyData.moveSpeed * Time.deltaTime;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        // TODO: drop exp orb ở đây
        Destroy(gameObject);
    }
}
