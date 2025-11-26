using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillBarSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;   // icon skill
    [SerializeField] private TMP_Text levelText; // text "1", "2"... (nếu có)

    public bool HasSkill => iconImage.sprite != null;

    public void SetIcon(Sprite sprite)
    {
        if (sprite != null)
        {
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = sprite;
        }
        else
        {
            iconImage.gameObject.SetActive(false);
            iconImage.sprite=null;
        }
    }

    public void SetLevel(int level)
    {
        if (levelText == null) return;

        if (level <= 0)
        {
            levelText.gameObject.SetActive(false);

        }
        else
        {
            levelText.gameObject.SetActive(true);
            levelText.text = level.ToString();
        }
    }
}
