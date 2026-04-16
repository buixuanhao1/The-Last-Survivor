using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HeroPanelUI : MonoBehaviour
{
    [Header("Danh sách Hero")]
    public Transform heroListParent;
    public GameObject heroItemPrefab;

    [Header("Chi tiết Hero")]
    public Image heroDetailImage;
    public TMP_Text heroNameText;
    public Button btnSelect;        // Chỉ dùng 1 nút duy nhất
    public TMP_Text btnSelectText;  // Text trên nút (đổi giữa “Mua” / “Chọn”)
    public Button btnClose;

    private List<HeroData> heroes = new List<HeroData>();
    private List<HeroItemUI> heroItems = new List<HeroItemUI>();
    private HeroData selectedHero;

    private void Start()
    {
        btnClose.onClick.AddListener(OnClose);
        btnSelect.onClick.AddListener(OnSelectOrBuy);
        LoadHeroes();

        SyncHeroUnlockStatus();
        PopulateHeroList();
        RefreshAll(); 
    }

    private void LoadHeroes()
    {
        heroes = new List<HeroData>(Resources.LoadAll<HeroData>("Heroes"));
    }

    private void PopulateHeroList()
    {
        foreach (Transform child in heroListParent)
            Destroy(child.gameObject);

        heroItems.Clear();

        foreach (var hero in heroes)
        {
            var obj = Instantiate(heroItemPrefab, heroListParent);
            var itemUI = obj.GetComponent<HeroItemUI>();
            itemUI.Setup(hero, this);
            heroItems.Add(itemUI);
        }
    }

    public void ShowHeroDetail(HeroData hero)
    {
        selectedHero = hero;
        heroDetailImage.sprite = hero.icon;
        heroNameText.text = hero.heroName;

        btnSelectText.text = hero.isUnlocked ? "Chọn" : "Mua";
    }

    private void OnSelectOrBuy()
    {
        if (selectedHero == null) return;

        var data = UserDataManager.instance.currentData;
        if (data == null) return;

        if (!selectedHero.isUnlocked)
        {
            if (data.diamond >= selectedHero.price)
            {
                data.diamond -= selectedHero.price;
                if (!data.unlockedHeroes.Contains(selectedHero.heroId))
                    data.unlockedHeroes.Add(selectedHero.heroId);

                selectedHero.isUnlocked = true;
                UserDataManager.instance.SaveUserData(data);

                btnSelectText.text = "Chọn"; // cập nhật lại text
                Debug.Log($"Đã mua {selectedHero.name}");

                RefreshAll();
            }
            else
            {
                Debug.Log("Không đủ kim cương!");
            }
            return;
        }

        data.selectedHero = selectedHero.heroId;
        UserDataManager.instance.SaveUserData(data);
        Debug.Log($"Đã chọn hero: {selectedHero.name}");

        RefreshAll();

    }

    private void SyncHeroUnlockStatus()
    {
        var data = UserDataManager.instance.currentData;
        if (data == null) return;

        foreach (var hero in heroes)
        {
            hero.isUnlocked = data.unlockedHeroes.Contains(hero.heroId);
        }
    }

    private void OnClose()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.mainUIPrefab);
    }

    private void RefreshAll()
    {
        var data = UserDataManager.instance.currentData;
        string selectedName = data != null ? data.selectedHero : null;

        foreach (var item in heroItems)
        {
            item.Refresh(selectedName);
        }
    }

    public void OpenHeroDetails()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.detailsPrefab);
    }
}
