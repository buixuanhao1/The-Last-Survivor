using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    private int expAmount;
    private Transform player;
    private bool isActive = false;
    private bool isCollected = false;   // Đánh dấu đã bắt đầu hút

    [Header("Magnet Settings")]
    public float attractDistance = 0.5f;   // khoảng cách bắt đầu hút
    public float attractSpeed = 30f;     // hệ số hút (càng cao hút càng nhanh)

    [Header("Offset Effect")]
    public float offsetTime = 0.25f;     // thời gian lùi ra sau
    public float offsetForce = 5f;       // lực lùi

    private Vector3 offsetDir;
    private float offsetTimer;

    public void Init(int amount, Transform playerRef)
    {
        expAmount = amount;
        player = playerRef;
        isActive = true;
        isCollected = false;

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isActive || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Chỉ cần lọt vào phạm vi 1 lần là "khóa" target
        if (!isCollected && dist < attractDistance)
        {
            isCollected = true;

            // setup hiệu ứng lùi
            offsetTimer = offsetTime;
            // offsetDir chỉ ngược hướng player, không random
            offsetDir = (transform.position - player.position).normalized;

        }

        if (isCollected)
        {
            if (offsetTimer > 0)
            {
                // bước 1: bay giật ra sau
                transform.position += offsetDir * offsetForce * Time.deltaTime;
                offsetTimer -= Time.deltaTime;
            }
            else
            {
                // bước 2: hút về player
                // Tốc độ hút = max(tốc độ tối thiểu, tốc độ theo khoảng cách)
                float dynamicSpeed = attractSpeed * dist;
                float finalSpeed = Mathf.Max(dynamicSpeed, 15f); // 15f là tốc độ tối thiểu

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    finalSpeed * Time.deltaTime
                );
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerExperience exp = other.GetComponent<PlayerExperience>();
            if (exp != null)
            {
                exp.AddExp(expAmount);
            }

            // trả orb về pool
            ExpOrbPool.Instance.ReturnToPool(this);
        }
    }
}
