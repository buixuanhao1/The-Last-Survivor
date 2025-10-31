using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject mainUI;
    public GameObject shopPanel;
    public GameObject heroPanle;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Show(GameObject panel)
    {
        HideAll();
        panel.SetActive(true);
    }

    public void HideAll()
    {
        loginPanel.SetActive(false);
        mainUI.SetActive(false);
        shopPanel.SetActive(false);
        heroPanle.SetActive(false);
    }

    public void ShowShopPanel(GameObject panel)
    {
        HideAll();
        shopPanel.SetActive(true);
    }

    public void ShowHeroPanel(GameObject panel)
    {
        HideAll();
        heroPanle.SetActive(true);
    }

}
