using UnityEngine;
using TMPro;

public class ShopPanelUI : MonoBehaviour
{
    [Header("Text hiển thị tài nguyên")]
    public TMP_Text diamondText;
    public TMP_Text goldText;

    [Header("Các nút nhận thưởng")]
    public UnityEngine.UI.Button[] dailyRewardButtons;

    private void OnEnable()
    {
        UpdateUI();
    }

    private void Start()
    {
        // Gán sự kiện cho các nút phần thưởng
        foreach (var btn in dailyRewardButtons)
        {
            btn.onClick.AddListener(() => ClaimReward(btn));
        }
    }

    private void UpdateUI()
    {
        var data = UserDataManager.instance.currentData;
        if (data == null) return;

        diamondText.text = data.diamond.ToString();
        goldText.text = data.gold.ToString();
    }

    private void ClaimReward(UnityEngine.UI.Button btn)
    {
        var data = UserDataManager.instance.currentData;
        if (data == null) return;

        // Ví dụ: phần thưởng +100 gold mỗi lần bấm
        data.gold += 100;
        UserDataManager.instance.SaveUserData(data);

        UpdateUI();
        btn.interactable = false;

        Debug.Log(" Nhận phần thưởng: +100 Gold!");
    }

    public void OnClose()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.mainUIPrefab);
    }
}
