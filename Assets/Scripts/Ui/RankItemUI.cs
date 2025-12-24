using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RankItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI txtRank;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtLevel;
    public TextMeshProUGUI txtExp;
    public Image img;

    public Image avatarImage;

    // thêm isSelf
    public void Setup(int rank, string playerName, int level, int exp,
                      bool isSelf = false, Sprite avatarSprite = null)
    {
        if (txtRank != null) txtRank.text = rank.ToString();
        if (txtName != null) txtName.text = playerName;
        if (txtLevel != null) txtLevel.text = "Lv. " + level;
        if (txtExp != null) txtExp.text = exp.ToString();

        if (avatarImage != null && avatarSprite != null)
            avatarImage.sprite = avatarSprite;

        if (img != null)
        {
            if (isSelf)
            {
                img.color = new Color(0.93f, 0.27f, 0.27f); // đỏ nhạt
            }           
        }
    }
}
