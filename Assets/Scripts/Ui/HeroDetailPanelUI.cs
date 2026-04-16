using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeroDetailPanelUI : MonoBehaviour
{
    [Header("Top bar resources")]
    public TMP_Text diamondText;
    public TMP_Text goldText;

    [Header("Buttons")]
    public Button btnClose;

    private void Awake()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(OnClose);
    }

    private void OnEnable()
    {
        UpdateResourceUI();
    }

    public void UpdateResourceUI()
    {
        var mgr = UserDataManager.instance;
        if (mgr == null) return;
        var data = mgr.currentData;
        if (data == null) return;

        if (diamondText != null) diamondText.text = data.diamond.ToString();
        if (goldText != null) goldText.text = data.gold.ToString();
    }

    public void OnClose()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.mainUIPrefab);
    }
}
