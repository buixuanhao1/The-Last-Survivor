using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;
    public GameObject expOrbPrefab; //prefab ExpOrb
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
        // Drop orb từ pool
        ExpOrbPool.Instance.SpawnOrb(transform.position, enemyData.expDrop);
        Destroy(gameObject);
    }
    public void Knockback(Vector3 hitSource, float force)
    {
        Vector2 knockDir = (transform.position - hitSource).normalized;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(knockDir * force, ForceMode2D.Impulse);
        StartCoroutine(ResetVelocity(rb));
    }
    private System.Collections.IEnumerator ResetVelocity(Rigidbody2D rb)
    {
        yield return new WaitForSeconds(0.1f); // knockback 
        rb.linearVelocity = Vector2.zero;
    }

}
