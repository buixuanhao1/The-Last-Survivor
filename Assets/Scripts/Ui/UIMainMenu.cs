using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMainMenu : MonoBehaviour
{
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
        SceneManager.LoadScene("GamePlay");

    }

}
