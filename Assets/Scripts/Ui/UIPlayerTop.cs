using UnityEngine;
using TMPro;

public class UIPlayerTop : MonoBehaviour
{
    public TMP_Text levelText;
    public TMP_Text energyText;
    public TMP_Text diamondText;
    public TMP_Text goldText;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        var data = UserDataManager.instance.currentData;
        if (data == null) return;
        UserDataManager.instance.UpdateEnergy();

        levelText.text = "Lv. " + data.level;
        energyText.text = $"{data.energy}/40";
        diamondText.text = data.diamond.ToString();
        goldText.text = data.gold.ToString();
    }
}
