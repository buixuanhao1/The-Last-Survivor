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
    public GameObject upgradeHeroPrefab;
    public GameObject equipPrefab;
    public GameObject petPrefab;

    private GameObject currentPanel;
    [Header("Overlay Panels")]
    public GameObject settingsPanelPrefab;
    public GameObject rankPanlePrefab;
    
    private readonly List<GameObject> _overlays = new List<GameObject>();

     


    private void Start()
    {
        ShowPanel(loginPanelPrefab);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject OpenOverlay(GameObject overlayPrefab)
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        var ov = Instantiate(overlayPrefab, canvas.transform, false);
        _overlays.Add(ov);
        return ov;
    }

    public void CloseTopOverlay()
    {
        if (_overlays.Count == 0) return;
        var top = _overlays[_overlays.Count - 1];
        _overlays.RemoveAt(_overlays.Count - 1);
        if (top) Destroy(top);
    }

    public void CloseOverlay(GameObject overlayInstance)
    {
        if (_overlays.Contains(overlayInstance))
        {
            _overlays.Remove(overlayInstance);
            Destroy(overlayInstance);
        }
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
        foreach (GameObject overlay in _overlays)
        {
            if (overlay != null)
            {
                Destroy(overlay);
            }
        }
        _overlays.Clear();

        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }
    }



}
