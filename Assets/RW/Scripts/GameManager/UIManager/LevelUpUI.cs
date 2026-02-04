using System.Collections.Generic;
using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    private Player player;
    public List<OptionUI> optionOptionUIList;
    public SkillController skillSelected;
    private string selectedSkillName;
    private bool usingNetworkData;

    public void SetLevelUp(List<SkillController> selectableList)
    {
        usingNetworkData = false;
        skillSelected = null;
        selectedSkillName = null;

        if (selectableList.Count == 0)
        {
            return;
        }

        for (int i = 0; i < optionOptionUIList.Count; i++)
        {
            optionOptionUIList[i].gameObject.SetActive(true);
            if (i < selectableList.Count)
            {
                optionOptionUIList[i].SetData(selectableList[i]);
            }
            else
            {
                optionOptionUIList[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetLevelUpNetwork(Player owner, SkillOptionInfo[] options)
    {
        usingNetworkData = true;
        player = owner;
        skillSelected = null;
        selectedSkillName = null;

        if (options == null || options.Length == 0)
            return;

        for (int i = 0; i < optionOptionUIList.Count; i++)
        {
            if (i < options.Length)
            {
                SkillOptionInfo info = options[i];
                Sprite sprite = FindSkillSprite(info.SkillName.ToString());
                optionOptionUIList[i].SetDataFromInfo(info, sprite);
                optionOptionUIList[i].gameObject.SetActive(true);
            }
            else
            {
                optionOptionUIList[i].gameObject.SetActive(false);
            }
        }
    }

    private Sprite FindSkillSprite(string skillName)
    {
        if (player == null)
            return null;

        foreach (var weapon in player.allWeaponList)
        {
            if (weapon != null && weapon.skillName == skillName)
                return weapon.sprite;
        }
        foreach (var passive in player.allPassiveSkillList)
        {
            if (passive != null && passive.skillName == skillName)
                return passive.sprite;
        }
        return null;
    }

    public void SetSkillSelect(OptionUI option)
    {
        skillSelected = option.optionSkill;
        selectedSkillName = option.optionSkillName;
    }

    public void UpgradeSkill()
    {
        if (player == null)
        {
            player = Player.LocalPlayer ?? FindFirstObjectByType<Player>();
        }

        if (GameModeManager.Mode == GameMode.Multiplayer)
        {
            if (!string.IsNullOrEmpty(selectedSkillName))
            {
                player.RequestUpgradeSkillServerRpc(selectedSkillName);
            }
            return;
        }

        if (skillSelected != null)
        {
            AudioManager.Instance.PlaySFX("PowerUp");
            player.UpgradeSkill(skillSelected);
        }
    }

    public void Close()
    {
        usingNetworkData = false;
        skillSelected = null;
        selectedSkillName = null;
        gameObject.SetActive(false);
    }
}
