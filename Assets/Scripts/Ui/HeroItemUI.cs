using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroItemUI : MonoBehaviour
{
    [Header("Tham chiếu UI")]
    public Image icon;           // Ảnh nhân vật
    public TMP_Text label;
    public Image bgImage;        // Nền
    public Button button;

    [Header("Sprite nền")]
    public Sprite unlockedSprite;   // icon_background_vàng
    public Sprite lockedSprite;     // icon_background_xanh

    public Image diamondIcon;
    public TMP_Text priceText;
    public GameObject pricePanle;
    public HeroData data;
    private HeroPanelUI parent;

    public void Setup(HeroData data, HeroPanelUI parent)
    {
        this.data = data;
        this.parent = parent;
        icon.sprite = data.icon;

        label.text = data.isUnlocked ? $"Cấp {data.level}" : "";

        if (pricePanle) pricePanle.SetActive(!data.isUnlocked);
        if (!data.isUnlocked && priceText != null) priceText.text = data.price.ToString();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        parent.ShowHeroDetail(data);
    }

    public void Refresh(string selectedHeroName)
    {
        if (!data.isUnlocked)
        {
            bgImage.sprite = lockedSprite;
            if (pricePanle) pricePanle.SetActive(true);
            return;
        }

        bool isSelected = !string.IsNullOrEmpty(selectedHeroName) && data.heroId == selectedHeroName;
        bgImage.sprite = isSelected ? unlockedSprite : lockedSprite;

        if (pricePanle) pricePanle.SetActive(false);
    }
}
