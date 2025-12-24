using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [Header("Motion")]
    public float riseDistance = 60f;   
    public float lifeTime = 0.8f;      

    [Header("Fade")]
    [Range(0f, 1f)] public float fadeStart = 0.25f; 

    TextMeshProUGUI txt;
    RectTransform rect;

    Vector2 startPos;
    Color baseColor;
    float t;

    void Awake()
    {
        txt = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        t = 0f;
        startPos = rect.anchoredPosition;

        baseColor = txt.color;
        baseColor.a = 1f;
        txt.color = baseColor;
    }

    public void Setup(int damage, bool crit = false)
    {
        txt.text = damage.ToString();

        if (crit)
        {
            txt.fontSize = 56;
            txt.color = new Color(1f, 0.25f, 0.25f, 1f);
        }
        else
        {
            txt.fontSize = 42;
            txt.color = Color.white;
        }

        baseColor = txt.color;
        baseColor.a = 1f;
        txt.color = baseColor;
    }

    void Update()
    {
        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / lifeTime);

        float eased = 1f - Mathf.Pow(1f - p, 3f); 
        rect.anchoredPosition = startPos + Vector2.up * (riseDistance * eased);

        float fadeP = Mathf.InverseLerp(fadeStart, 1f, p); 
        Color c = baseColor;
        c.a = 1f - fadeP;
        txt.color = c;
    }
}
