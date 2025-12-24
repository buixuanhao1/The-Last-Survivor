using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroConfig
{
    public string id;                           // "Naruto", "Tanjiro"
    public Sprite sprite;                       // sprite đứng của hero
    public RuntimeAnimatorController animator;  // animator controller
    public int baseHP = 100;
    public int baseDamage = 10;
    public float baseMoveSpeed = 3f;
}

public class HeroInitializer : MonoBehaviour
{
    public List<HeroConfig> heroes = new List<HeroConfig>();

    private void Start()
    {
        var dataMgr = UserDataManager.instance;
        if (dataMgr == null || dataMgr.currentData == null)
        {
            Debug.LogWarning("HeroInitializer: chưa có UserData, dùng hero đầu tiên trong list.");
            if (heroes.Count > 0)
                ApplyHeroConfig(heroes[0]);
            return;
        }

        string heroId = dataMgr.currentData.selectedHero;
        if (string.IsNullOrEmpty(heroId))
            heroId = "Naruto";

        HeroConfig cfg = heroes.Find(h => h.id == heroId);
        if (cfg == null)
        {
            Debug.LogWarning("HeroInitializer: không tìm thấy config cho " + heroId + ", dùng hero đầu tiên.");
            if (heroes.Count > 0)
                cfg = heroes[0];
        }

        if (cfg != null)
            ApplyHeroConfig(cfg);
    }

    private void ApplyHeroConfig(HeroConfig cfg)
    {
        // 1) Đổi sprite + animator
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && cfg.sprite != null)
            sr.sprite = cfg.sprite;

        var anim = GetComponent<Animator>();
        if (anim != null && cfg.animator != null)
            anim.runtimeAnimatorController = cfg.animator;

        // 2) Stat cơ bản
        var health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.maxHP = cfg.baseHP;
            health.currentHP = cfg.baseHP;
        }

        Debug.Log("Đã apply hero: " + cfg.id);
    }
}
