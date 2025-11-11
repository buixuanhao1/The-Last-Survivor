using UnityEngine;
using UnityEngine.UI;

public class SlotButton : MonoBehaviour
{
    [SerializeField] private EquipmentUI owner;
    [SerializeField] private int slotId;         // 0..5
    [SerializeField] private Image iconSource;   

    public void OnClick()
    {
        Sprite icon = iconSource != null ? iconSource.sprite : null;
        owner.OpenEquip(slotId, icon);
    }
}
