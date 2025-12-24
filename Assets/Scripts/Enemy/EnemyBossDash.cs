using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyBossDash : MonoBehaviour
{
    [Header("Thiết lập skill lao")]
    public float dashSpeed = 8f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 2.5f;
    public float dashMinDistance = 2f;
    public float dashMaxDistance = 7f;

    [Header("Telegraph (mũi tên báo trước)")]
    public GameObject dashIndicatorPrefab;
    public float telegraphTime = 0.5f;
    public float telegraphLength = 4f;
    public Transform telegraphOrigin;

    private EnemyController controller;
    private Transform player;
    private Animator anim;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDir;

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        player = controller.player;
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        if (isDashing)
        {
            DashUpdate();
        }
        else
        {
            if (dashCooldownTimer > 0f)
                dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownTimer <= 0f)
                TryStartDash();
        }
    }

    void TryStartDash()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < dashMinDistance || dist > dashMaxDistance) return;

        // hướng dash (từ boss -> player)
        dashDir = (player.position - transform.position).normalized;

        controller.canMove = false;
        dashCooldownTimer = dashCooldown;

        if (anim != null) anim.SetTrigger("Dash");

        // === TELEGRAPH INDICATOR ===
        if (dashIndicatorPrefab != null)
        {
            Vector3 origin = telegraphOrigin != null
                ? telegraphOrigin.position
                : transform.position;

            GameObject indicator = Instantiate(
                dashIndicatorPrefab,
                origin,
                Quaternion.identity
            );

            // HƯỚNG THỰC SỰ CỦA MŨI TÊN (đuôi -> đầu)
            // Sprite của bạn đang vẽ ngược, nên dùng -dashDir
            Vector3 arrowDir = -dashDir;

            // quay mũi tên
            indicator.transform.up = arrowDir;

            // scale độ dài
            Vector3 s = indicator.transform.localScale;
            indicator.transform.localScale = new Vector3(s.x, telegraphLength, s.z);

            // lấy chiều dài thật sau khi scale
            SpriteRenderer r = indicator.GetComponent<SpriteRenderer>();
            float realLength = r.bounds.size.y;

            // dịch để ĐUÔI = chân boss (origin)
            indicator.transform.position = origin - arrowDir * (realLength * 0.5f);

            Destroy(indicator, telegraphTime);
        }

        StartCoroutine(StartDashAfterTelegraph());
    }

    System.Collections.IEnumerator StartDashAfterTelegraph()
    {
        yield return new WaitForSeconds(telegraphTime);
        isDashing = true;
        dashTimer = dashDuration;
    }

    void DashUpdate()
    {
        dashTimer -= Time.deltaTime;

        // boss vẫn lao theo dashDir (từ boss -> player)
        transform.position += dashDir * dashSpeed * Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;
            controller.canMove = true;
        }
    }
}
