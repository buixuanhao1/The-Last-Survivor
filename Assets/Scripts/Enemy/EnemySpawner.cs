using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhaseConfig
{
    public float startTime;
    public EnemyType[] addTypes;        // CHỈ khai báo quái thêm ở phase này
    public float spawnInterval = 2f;    // rải rác
}

[System.Serializable]
public class BurstEntry
{
    public EnemyType type;              // quái bạn muốn spawn trong wave này
    public int count = 10;              // số lượng quái đó
}

[System.Serializable]
public class WaveBurst
{
    public float time;                  // mốc thời gian nổ wave
    public BurstEntry[] entries;        // custom quái cho wave
    [HideInInspector] public bool done;
}

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public float spawnRadius = 10f;

    [Header("Phases (Additive)")]
    public PhaseConfig[] phases;        // phase 0: mặc định, phase sau: thêm quái

    [Header("Burst Waves (Custom)")]
    public WaveBurst[] bursts;          // mỗi wave tự set quái và số lượng

    [Header("Boss")]
    public EnemyType bossType;          // kéo EnemyType boss vào đây
    public float bossSpawnTime = 170f;  // giây thứ bao nhiêu spawn boss
    public bool stopSpawningAfterBoss = false; // nếu bật: spawn boss xong thì dừng spawn quái thường

    [Header("Win UI")]
    public GameObject winPanelPrefab;   // Prefab UI Win Game

    [Header("Match Time")]
    public float gameDuration = 180f;

    float elapsed;
    float timer;

    // cache list gộp để không tạo rác GC mỗi frame
    readonly List<EnemyType> activeTypes = new List<EnemyType>(32);
    int cachedPhaseIndex = -1; // để chỉ rebuild list khi phase đổi

    bool bossSpawned;
    GameObject bossInstance;
    bool winShown;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Use default spawner configuration; do not override by map
    }

    void Update()
    {
        if (player == null) return;

        elapsed += Time.deltaTime;

        // 0) Kiểm tra win: phải chạy TRƯỚC khi check gameDuration
        // Nếu boss bị giết sau khi hết giờ, win UI vẫn phải hiện
        if (bossSpawned && !winShown && bossInstance == null)
        {
            ShowWinUI();
            winShown = true;
            return;
        }

        // Hết thời gian → dừng spawn nhưng vẫn cho phép win check ở trên
        if (elapsed >= gameDuration) return;

        // 1) Spawn Boss (1 lần)
        if (!bossSpawned && bossType != null && bossType.prefab != null && elapsed >= bossSpawnTime)
        {
            bossSpawned = true;
            bossInstance = SpawnSpecific(bossType);

            if (stopSpawningAfterBoss)
                return; // dừng spawn quái thường
        }

        // 2) Phase additive -> build list quái đang được phép spawn
        int phaseIndex = GetPhaseIndex(elapsed);
        if (phaseIndex < 0) return;

        if (phaseIndex != cachedPhaseIndex)
        {
            cachedPhaseIndex = phaseIndex;
            RebuildActiveTypes(phaseIndex);
        }

        if (activeTypes.Count == 0) return;

        // 3) Spawn rải rác theo interval của phase hiện tại
        float interval = Mathf.Max(0.05f, phases[phaseIndex].spawnInterval);
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            SpawnRandomFrom(activeTypes);
        }

        // 4) Burst theo mốc thời gian (custom quái cho từng wave)
        if (bursts != null)
        {
            for (int i = 0; i < bursts.Length; i++)
            {
                if (bursts[i].done) continue;

                if (elapsed >= bursts[i].time)
                {
                    bursts[i].done = true;
                    SpawnCustomBurst(bursts[i]);
                }
            }
        }
    }

    bool IsTypeAllowed(EnemyType type)
    {
        if (type == null) return false;
        if (MapSelection.Instance == null || MapSelection.Instance.Selected == null) return true; // no restriction
        var list = MapSelection.Instance.Selected.enemyTypes;
        if (list == null || list.Count == 0) return true; // if map not configured, don't block
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == type) return true;
        }
        return false;
    }

    // NOTE: ApplyMapConfig() đã được thay thế bởi IsTypeAllowed().
    // Map restriction hiện được filter trực tiếp trong RebuildActiveTypes()
    // thay vì override phases[0].addTypes. Hàm này không còn cần thiết.

    int GetPhaseIndex(float t)
    {
        int idx = -1;
        if (phases == null) return idx;

        // phases phải set theo startTime tăng dần
        for (int i = 0; i < phases.Length; i++)
        {
            if (t >= phases[i].startTime) idx = i;
            else break;
        }
        return idx;
    }

    void RebuildActiveTypes(int uptoIndex)
    {
        activeTypes.Clear();
        if (phases == null) return;

        for (int i = 0; i <= uptoIndex; i++)
        {
            var add = phases[i].addTypes;
            if (add == null) continue;

            for (int k = 0; k < add.Length; k++)
            {
                var e = add[k];
                if (e == null) continue;

                // tránh add trùng + chỉ thêm loại quái được phép trên map này
                if (!activeTypes.Contains(e) && IsTypeAllowed(e))
                    activeTypes.Add(e);
            }
        }
    }

    void SpawnRandomFrom(List<EnemyType> types)
    {
        if (types == null || types.Count == 0) return;
        var type = types[Random.Range(0, types.Count)];
        SpawnSpecific(type);
    }

    void SpawnCustomBurst(WaveBurst burst)
    {
        if (burst == null || burst.entries == null) return;

        foreach (var e in burst.entries)
        {
            if (e == null || e.type == null || e.type.prefab == null) continue;

            int n = Mathf.Max(1, e.count);
            for (int i = 0; i < n; i++)
            {
                SpawnSpecific(e.type);
            }
        }
    }

    GameObject SpawnSpecific(EnemyType type)
    {
        if (type == null || type.prefab == null) return null;

        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector2 spawnPos = (Vector2)player.position + dir * spawnRadius;

        GameObject enemy = Instantiate(type.prefab, spawnPos, Quaternion.identity);

        var controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.enemyData = type.data;
            controller.player = player; // khỏi FindTag lại
        }
        return enemy;
    }

    void ShowWinUI()
    {
        if (winPanelPrefab == null) return;
        Time.timeScale = 0f;
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Instantiate(winPanelPrefab, canvas.transform, false);
        }
        else
        {
            Instantiate(winPanelPrefab);
        }
    }
}
