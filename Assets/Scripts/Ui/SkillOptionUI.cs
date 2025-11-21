using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillOptionUI : MonoBehaviour
{
    [Header("Ui refs")]
    [SerializeField] private Image frameImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text descText;

    [Header("Frame sprites")]
    [SerializeField] private Sprite weaponFrame;
    [SerializeField] private Sprite otherFrame;

    private SkillData skillData;
    private LevelUpPanel panel;

    public void Setup(SkillData skill, LevelUpPanel parentPanel, int curentLevel)
    {
        skillData = skill;
        panel = parentPanel;

        iconImage.sprite = skill.icon;
        nameText.text = skill.skillName;
        descText.text = skill.description;

        frameImage.sprite = (skill.type == SkillData.SkillType.Weapon) 
            ? weaponFrame : otherFrame;

        if(curentLevel <= 0)
        {
            levelText.text = "Mới";
        }
        else
        {
            int nextLevel = Mathf.Min(curentLevel +1, skill.maxLevel);
            levelText.text = "Cấp " + nextLevel;
        }
    }

    public void OnSelect()
    {
        panel.SelectSkill(skillData);
    }
}
