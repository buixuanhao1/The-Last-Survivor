using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HPBar : MonoBehaviour
{
    public PlayerHealth target;
    public Image hpImage; // Image của phần Hp (Type = Filled)

    private void Reset()
    {
        if (target == null)
            target = GetComponentInParent<PlayerHealth>();
        if (hpImage == null)
            hpImage = GetComponentInChildren<Image>(true);
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        if (target == null)
            target = GetComponentInParent<PlayerHealth>();
        Refresh();
    }

    private void Subscribe()
    {
        if (target != null)
            target.onHealthChanged += OnHealthChanged;
    }

    private void Unsubscribe()
    {
        if (target != null)
            target.onHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (hpImage == null || max <= 0) return;
        hpImage.fillAmount = Mathf.Clamp01((float)current / max);
    }

    private void Refresh()
    {
        if (target == null) return;
        OnHealthChanged(target.currentHP, target.maxHP);
    }
}
