 using UnityEngine;
using UnityEngine.UI;

public class EquipPanel : MonoBehaviour
{
    [SerializeField] private Image iconTarget; 

    private int slotId;

    public void Setup(int id, Sprite icon)
    {
        slotId = id;
        if (iconTarget != null)
        {
            iconTarget.sprite = icon;
            iconTarget.enabled = icon != null;
            // iconTarget.SetNativeSize(); // nếu muốn về kích thước gốc
        }
    }

    public void OnClose()
    {
        UIManager.Instance.CloseOverlay(gameObject);
    }
}
