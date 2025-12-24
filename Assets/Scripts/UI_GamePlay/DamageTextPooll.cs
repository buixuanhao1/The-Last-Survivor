using System.Collections.Generic;
using UnityEngine;

public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private GameObject damageTextPrefab; // kéo prefab Text(TMP) vào đây
    [SerializeField] private string canvasName = "Canvas_UI";

    [Header("Pool")]
    [SerializeField] private int preloadCount = 40;

    private readonly Queue<DamageText> pool = new();
    private Transform canvasUI;
    private Camera cam;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam = Camera.main;

        var canvasObj = GameObject.Find(canvasName);
        if (canvasObj != null) canvasUI = canvasObj.transform;

        Preload();
    }

    void Preload()
    {
        if (damageTextPrefab == null || canvasUI == null) return;

        for (int i = 0; i < preloadCount; i++)
        {
            var dt = CreateNew();
            Return(dt);
        }
    }

    DamageText CreateNew()
    {
        GameObject go = Instantiate(damageTextPrefab, canvasUI);
        go.SetActive(false);

        var dt = go.GetComponent<DamageText>();
        if (dt == null) dt = go.AddComponent<DamageText>(); // phòng khi bạn quên gắn
        return dt;
    }

    DamageText Get()
    {
        if (pool.Count > 0) return pool.Dequeue();
        return CreateNew();
    }

    void Return(DamageText dt)
    {
        dt.gameObject.SetActive(false);
        pool.Enqueue(dt);
    }

    public void Show(Vector3 worldPos, int damage, bool crit = false)
    {
        if (damageTextPrefab == null || canvasUI == null) return;

        var dt = Get();
        var rect = dt.GetComponent<RectTransform>();
        var canvasRect = canvasUI.GetComponent<RectTransform>();

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            Camera.main,   
            out localPos
        );

        rect.anchoredPosition = localPos;

        dt.gameObject.SetActive(true);
        dt.transform.SetAsLastSibling();
        dt.Setup(damage, crit);

        StartCoroutine(ReturnWhenDone(dt));
    }


    System.Collections.IEnumerator ReturnWhenDone(DamageText dt)
    {
        // chờ đúng lifetime (nhẹ nhất)
        float t = dt.lifeTime;
        yield return new WaitForSeconds(t);

        // nếu object vẫn active (tránh trường hợp chết enemy/scene đổi)
        if (dt != null) Return(dt);
    }
}
