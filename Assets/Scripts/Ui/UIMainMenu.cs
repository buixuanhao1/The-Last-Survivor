using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMainMenu : MonoBehaviour
{
    private void Start()
    {
        ApplyLoginStateUI();
    }

    private void OnEnable()
    {
        ApplyLoginStateUI();
    }

    private void ApplyLoginStateUI()
    {
        bool loggedIn = FirebaseAuth.DefaultInstance != null &&
                        FirebaseAuth.DefaultInstance.CurrentUser != null;

        if (loggedIn)
        {
            UIManager.Instance.ShowPanel(UIManager.Instance.mainUIPrefab);
        }
        else
        {
            
            Debug.Log("Chưa đăng nhập -> cần hiển thị Login Panel");
        }
    }
    public void SelectMap(MapConfig map)
    {
        if (MapSelection.Instance == null)
        {
            Debug.LogWarning("MapSelection instance not found in scene. Please add MapSelection to the Main Menu scene.");
            return;
        }
        MapSelection.Instance.Selected = map;
    }

    public void OpenUserProfile()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.profilePanelPrefab);
    }

    public void OpenShop()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.shopPanelPrefab);
    }

    public void OpenHero()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.heroPanelPrefab);
    }
    public void OpenSetting()
    {
        UIManager.Instance.OpenOverlay(UIManager.Instance.settingsPanelPrefab);

    }

    public void OpenRankPanle()
    {
        UIManager.Instance.OpenOverlay(UIManager.Instance.rankPanlePrefab);
    }

    public void OpenUpgradeHeroPanel()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.upgradeHeroPrefab);
    }

    public void OpenPetUIPanel()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.petPrefab);
    }

    public void LoadGamePlay()
    {
        bool loggedIn = FirebaseAuth.DefaultInstance != null &&
                        FirebaseAuth.DefaultInstance.CurrentUser != null;

        if (!loggedIn)
        {
            UIManager.Instance.ShowPanel(UIManager.Instance.loginPanelPrefab);
            return;
        }

        SceneManager.LoadScene("GamePlay");
    }

}
