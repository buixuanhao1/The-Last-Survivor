using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text soundStateText;


    private bool soundOn;

    void Awake()
    {
        soundOn = PlayerPrefs.GetInt("sound_on", 1) == 1;
        UpdateSoundText();
        AudioListener.pause = !soundOn;
    }

    public void OnToggleSound()
    {

    }

    public void OnOpenLanguage()
    {
    }

    public void OnClose()
    {
        UIManager.Instance.CloseOverlay(gameObject);
        Debug.Log("Close....");
    }

    public void OnLogout()
    {
        try
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }
        catch
        {
            Debug.LogWarning("Chưa khởi tạo Firebase hoặc lỗi khi SignOut.");
        }

        UIManager.Instance.HideAll();
        UIManager.Instance.ShowPanel(UIManager.Instance.loginPanelPrefab);
    }

    private void UpdateSoundText()
    {
        if (soundStateText != null)
            soundStateText.text = soundOn ? "Bật" : "Tắt";
    }
}
