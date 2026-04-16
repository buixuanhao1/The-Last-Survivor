using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CircleAOEEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    private float pulseTimer;
    private float rippleTimer;
    private Vector3 baseScale;
    private SpriteRenderer rippleRenderer;

    [Header("Visual Settings")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;
    public float alphaMin = 0.2f;
    public float alphaMax = 0.5f;
    public float rippleInterval = 1f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    public void MatchCollider(CircleCollider2D col)
    {
        if (sr == null || sr.sprite == null) return;

        float worldRadius = col.radius * col.transform.lossyScale.x;
        float spriteDiameterLocal = sr.sprite.bounds.size.x;

        float scaleFactor = (worldRadius * 2f) / spriteDiameterLocal;
        baseScale = Vector3.one * scaleFactor;
        transform.localScale = baseScale;
    }
     
    void Update()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;

        float scaleOffset = Mathf.Sin(pulseTimer) * pulseAmount;
        transform.localScale = baseScale * (1f + scaleOffset);

        float alpha = Mathf.PingPong(Time.time * 0.8f, alphaMax - alphaMin) + alphaMin;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        rippleTimer -= Time.deltaTime;
        if (rippleTimer <= 0f)
        {
            StartCoroutine(Ripple());
            rippleTimer = rippleInterval;
        }
    }

    void Start()
    {
        GameObject rippleObj = new GameObject("Ripple");
        rippleObj.transform.SetParent(transform);
        rippleObj.transform.localPosition = Vector3.zero;
        rippleRenderer = rippleObj.AddComponent<SpriteRenderer>();
        rippleRenderer.sprite = sr.sprite;
        rippleRenderer.sortingOrder = sr.sortingOrder - 1;
        rippleObj.SetActive(false);

        // Bắt đầu đếm từ interval đầy đủ — tránh ripple chạy ngay frame đầu tiên
        rippleTimer = rippleInterval;
    }

    void OnDisable()
    {
        // Dừng tất cả coroutine khi AOE bị destroy/disable giữa chừng
        // tránh lỗi "MissingReferenceException" khi Ripple cố access object đã bị hủy
        StopAllCoroutines();
        if (rippleRenderer != null)
            rippleRenderer.gameObject.SetActive(false);
    }

    private IEnumerator Ripple()
    {
        rippleRenderer.gameObject.SetActive(true);

        float t = 0f;
        float duration = 0.6f;
        Vector3 startScale = baseScale * 0.5f;
        Vector3 endScale = baseScale * 1.5f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            rippleRenderer.transform.localScale = Vector3.Lerp(startScale, endScale, p);

            Color c = rippleRenderer.color;
            c.a = Mathf.Lerp(0.3f, 0f, p);
            rippleRenderer.color = c;

            yield return null;
        }

        rippleRenderer.gameObject.SetActive(false);
    }
}
