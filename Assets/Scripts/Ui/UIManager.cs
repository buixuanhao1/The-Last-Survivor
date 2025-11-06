using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Panels")]
    public GameObject loginPanelPrefab;
    public GameObject mainUIPrefab;
    public GameObject shopPanelPrefab;
    public GameObject heroPanelPrefab;
    public GameObject profilePanelPrefab;
    private GameObject currentPanel;
    private void Start()
    {
        ShowPanel(loginPanelPrefab);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowPanel(GameObject panelPrefab)
    {
        if (currentPanel != null)
            Destroy(currentPanel);

        Canvas canvas = FindFirstObjectByType<Canvas>();
        currentPanel = Instantiate(panelPrefab, canvas.transform, false);
    }


    public void HideAll()
    {
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }
    }

    

}
