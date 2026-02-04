using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickUpChest : MonoBehaviour
{
    Player player;
    public Image skillImage;
    public float timeToChangeWeaponImage;
    public float timeAfterChangeImage;
    public float timeOpenChest = 3f;
    public float timeAfterOpenChest;
    public bool isOpened;
    public GameObject continueButton;
    private AudioSource openChestMusic;
    private List<SkillController> upgradeableSkillList;
    [SerializeField]
    private SkillController upgradeSkill;
    void Start()
    {
        player = Player.LocalPlayer ?? FindFirstObjectByType<Player>();
        timeAfterChangeImage = 0;
        timeAfterOpenChest = 0;
        isOpened = false;
    }

    private void Update()
    {
        if(openChestMusic != null && openChestMusic.name == "OpenChest")
        {
            if(openChestMusic.isPlaying && timeAfterOpenChest < timeOpenChest)
            {
                timeAfterChangeImage += Time.unscaledDeltaTime;
                timeAfterOpenChest += Time.unscaledDeltaTime;
                Debug.Log("timeAfterChageImage: " + timeAfterChangeImage);

                if (timeAfterChangeImage >= timeToChangeWeaponImage)
                {
                    int randTemp = Random.Range(0, upgradeableSkillList.Count);
                    skillImage.sprite = upgradeableSkillList[randTemp].sprite;
                    timeAfterChangeImage = 0;
                }
            }
            else
            {
                openChestMusic.Stop();
                if (upgradeSkill == null && upgradeableSkillList.Count > 0)
                {
                    int rand = Random.Range(0, upgradeableSkillList.Count);
                    upgradeSkill = upgradeableSkillList[rand];
                    skillImage.sprite = upgradeSkill.sprite;
                    continueButton.SetActive(true);
                }
            }
            
        }
    }

    public void OpenChest()
    {
        if (!isOpened)
        {
            Debug.Log("OpenChest");
            AudioManager.Instance.PlayMusic("OpenChest");
            upgradeableSkillList = UpgradeableWeapon();
            if (openChestMusic == null)
            {
                foreach (var sound in AudioManager.Instance.musicSounds)
                {
                    if (sound.name == "OpenChest" && sound.audioSource.isPlaying)
                    {
                        openChestMusic = sound.audioSource;
                        break;
                    }
                }
            }
            skillImage.gameObject.SetActive(true);
            isOpened = true;
        } else {
            Skip();
        }
    }

    private void Skip()
    {
        timeAfterOpenChest += timeOpenChest;
    }
    public void ContinueGame()
    {
        if (player == null)
        {
            player = Player.LocalPlayer ?? FindFirstObjectByType<Player>();
        }
        if (upgradeSkill != null)
        {
            GameStateManager.Instance.ResumeGame();
            player.UpgradeSkill(upgradeSkill);
            upgradeSkill = null;
            upgradeableSkillList.Clear();
            isOpened = false;
            timeAfterOpenChest = 0;
        }
        
    }
    private List<SkillController> UpgradeableWeapon()
    {
        List<SkillController> upgradeableSkill = new List<SkillController>();
        foreach(var weapon in player.currentWeaponList)
        {
            if(weapon.level < weapon.maxLevel)
            {
                upgradeableSkill.Add(weapon);
            }
        }
        foreach(var passive in player.currentPassiveSkillList)
        {
            if(passive.level < passive.maxLevel)
            {
                upgradeableSkill.Add(passive);
            }
        }
        return upgradeableSkill;
    }

}
