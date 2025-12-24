using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Header("Máu")]
    public int maxHP = 100;
    public int currentHP = 100;

    [Header("Bất tử")]
    public float tgBatTu = 0.2f;   // thời gian bất tử sau khi bị đánh
    private float demBatTu = 0f;

    public System.Action<int, int> onHealthChanged; // (máu hiện tại, máu tối đa)
    public System.Action onDeath;

    private void Awake()
    {
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    private void Update()
    {
        if (demBatTu > 0f)
            demBatTu -= Time.deltaTime;
    }

    public void TakeDamage(int satThuong)
    {
        if (satThuong <= 0) return;
        if (demBatTu > 0f) return; // đang bất tử

        currentHP = Mathf.Max(0, currentHP - satThuong);
        onHealthChanged?.Invoke(currentHP, maxHP);

        demBatTu = tgBatTu;

        if (currentHP <= 0)
            Die();
    }

    public void Heal(int hoiMau)
    {
        if (hoiMau <= 0) return;

        currentHP = Mathf.Min(maxHP, currentHP + hoiMau);
        onHealthChanged?.Invoke(currentHP, maxHP);
    }

    private void Die()
    {
        onDeath?.Invoke();

        var anim = GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("die");

        var player = GetComponent<Player>();
        if (player != null)
            player.enabled = false;
    }
}
