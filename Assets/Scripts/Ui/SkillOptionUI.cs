using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillOptionUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descText;
    private SkillData skillData;
    private LevelUpPanel panel;

    public void Setup(SkillData skill, LevelUpPanel parentPanel)
    {
        Debug.Log("Setup skill: " + skill.skillName);
        skillData = skill;
        panel = parentPanel;

        iconImage.sprite = skill.icon;
        nameText.text = skill.skillName;
        descText.text = skill.description;
    }

    public void OnSelect()
    {
        panel.SelectSkill(skillData);
    }
}
