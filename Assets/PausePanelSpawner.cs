using UnityEngine;

public class PausePanelSpawner : MonoBehaviour
{
    [Header("Prefab Pause Panel")]
    public PanelPause pausePanelPrefab;

    [Header("Parent UI (Canvas)")]
    public Transform uiParent; 

    private PanelPause instance;

    public void OnClickPauseButton()
    {
        if (instance == null)
        {
            instance = Instantiate(pausePanelPrefab, uiParent);
        }

        instance.Show();
    }
}
