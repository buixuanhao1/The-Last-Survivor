using UnityEngine;
using TMPro;

public class UserInforUI : MonoBehaviour
{
    [Header("Text hiển thị thông tin")]
    public TMP_Text valueEmail;
    public TMP_Text valueUID;
    public TMP_Text valueLevel;
    public TMP_Text valueGold;
    public TMP_Text valueDiamond;
    public TMP_Text valueEnergy;

    private void OnEnable()
    {
        FillData();
    }

    public void FillData()
    {
        var data = UserDataManager.instance.currentData;
        var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        if (data == null || auth.CurrentUser == null)
        {
            return;
        }

        valueEmail.text = auth.CurrentUser.Email;
        valueUID.text = auth.CurrentUser.UserId;
        valueLevel.text = data.level.ToString();
        valueGold.text = data.gold.ToString();
        valueDiamond.text = data.diamond.ToString();
        valueEnergy.text = $"{data.energy}/40";
    }

    public void OnClose()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.mainUIPrefab);
    }
}
