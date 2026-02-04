using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public Image skillImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI detaildText;
    public SkillController optionSkill;
    public string optionSkillName;

    public void SetData(SkillController skillController)
    {
        optionSkill = skillController;
        optionSkillName = optionSkill != null ? optionSkill.skillName : string.Empty;
        skillImage.sprite = optionSkill.sprite;
        nameText.text = optionSkill.skillName;
        int nextLevel = optionSkill.level + 1;
        levelText.text = "Level " + nextLevel.ToString();
        detaildText.text = optionSkill.getDescriptionNextLevel();
    }

    public void SetDataFromInfo(SkillOptionInfo info, Sprite sprite)
    {
        optionSkill = null;
        optionSkillName = info.SkillName.ToString();
        skillImage.sprite = sprite;
        nameText.text = optionSkillName;
        levelText.text = "Level " + info.NextLevel.ToString();
        detaildText.text = info.Description.ToString();
    }
}
