using UnityEngine;

public class PetUI : MonoBehaviour
{
    public void OnClose()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.mainUIPrefab);
    }
}
