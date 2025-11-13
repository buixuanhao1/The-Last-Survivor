using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField] private GameObject equipPanelPrefab;

    public void OnClose()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.mainUIPrefab);
    }

    public void OpenEquip(int slotId, Sprite icon)
    {
        var go = UIManager.Instance.OpenOverlay(equipPanelPrefab);
        var panel = go.GetComponent<EquipPanel>();
        if (panel != null) panel.Setup(slotId, icon);
    }
}
