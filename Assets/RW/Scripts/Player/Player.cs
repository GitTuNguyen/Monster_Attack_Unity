using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; private set; }

    public CharacterStats characterStats;
    public List<WeaponController> allWeaponList;
    public List<WeaponController> currentWeaponList;
    public List<PassivesSkillController> allPassiveSkillList;
    public List<PassivesSkillController> currentPassiveSkillList;
    public CharacterAnimationController animationController;
    public GameObject pickUpArea;
    private PlayerController playerController;
    [Header("UI")]
    public HealthBar healthBar;
    public GameObject floatingTextPrefabs;
    [Header("Stats")]
    public int playerLevel;
    [SerializeField]
    private float currentHealth;
    [SerializeField]
    private float maxHealth;
    [SerializeField]
    private float playerSpeed;
    [HideInInspector]
    public float currentExp;
    [SerializeField]
    private float maxExp;
    [SerializeField]
    private float pickUpRadius;
    public float weaponScale;
    [SerializeField]
    private float dodgeRate;
    public float bonusAmountProjectile;
    [SerializeField]
    private float bonusHealing;
    [SerializeField]
    private float takeHitInterval;
    private int maxNumberOfWeapon = 6;
    private int maxNumberOfPassive = 6;
    [HideInInspector]
    public int amountWeaponSelectableWhenLvUp = 3;
    [SerializeField]
    private bool isTakeHitInterval;
    private float timeAfterTakeHit;
    private float remainingExp;
    private bool isLevelingUp = false;

    [Header("Passive Skill Bonus")]
    public float reduceCooldown;
    public float increaseDame;
    public float armor;
    public float amountProjectile;
    public float moveSpeed;
    public float projectileSpeed;
    public float bonusMaxHealth;
    public float bonusExperience;

    [Header("Network Stats")]
    public NetworkVariable<float> NetCurrentHealth = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> NetMaxHealth = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> NetCurrentExp = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> NetMaxExp = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> NetLevel = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool IsOffline => GameModeManager.Mode == GameMode.Offline;
    private bool IsServerAuthority => GameModeManager.Mode == GameMode.Multiplayer && IsServer;
    private bool IsLocalPlayerInstance => IsOffline || IsOwner;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            LocalPlayer = this;

        playerController = GetComponentInParent<PlayerController>();

        if (GameModeManager.Mode != GameMode.Multiplayer)
            return;

        NetCurrentHealth.OnValueChanged += OnNetStatsChanged;
        NetMaxHealth.OnValueChanged += OnNetStatsChanged;
        NetCurrentExp.OnValueChanged += OnNetStatsChanged;
        NetMaxExp.OnValueChanged += OnNetStatsChanged;
        NetLevel.OnValueChanged += OnNetLevelChanged;

        if (IsServer)
        {
            InitializeRuntime();
            SyncNetStats();
        }

        if (IsLocalPlayerInstance)
        {
            AudioManager.Instance.PlayMusic("ThemeMusic");
            RefreshLocalUIFromNet();
        }
    }

    public override void OnNetworkDespawn()
    {
        NetCurrentHealth.OnValueChanged -= OnNetStatsChanged;
        NetMaxHealth.OnValueChanged -= OnNetStatsChanged;
        NetCurrentExp.OnValueChanged -= OnNetStatsChanged;
        NetMaxExp.OnValueChanged -= OnNetStatsChanged;
        NetLevel.OnValueChanged -= OnNetLevelChanged;

        if (LocalPlayer == this)
            LocalPlayer = null;
    }

    void Start()
    {
        if (IsOffline)
        {
            AudioManager.Instance.PlayMusic("ThemeMusic");
            playerController = GetComponentInParent<PlayerController>();
            InitializeRuntime();
        }
    }

    void Update()
    {
        if (!GameStateManager.Instance.isGameOver)
        {
            if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
                return;

            Move();
            if (isTakeHitInterval)
            {
                timeAfterTakeHit += Time.deltaTime;
                if (timeAfterTakeHit >= takeHitInterval)
                {
                    isTakeHitInterval = false;
                    timeAfterTakeHit = 0;
                }
            }
        }
    }

    private void InitializeRuntime()
    {
        SetCharacterDefaultStats();
        UpgradeWeapon(characterStats.defaultWeapon);

        timeAfterTakeHit = 0;
        maxNumberOfWeapon = 6;
        amountWeaponSelectableWhenLvUp = 3;
        remainingExp = 0;
        isTakeHitInterval = false;

        if (IsOffline)
        {
            UIManager.Instance.UpdateLeveTextlUI(playerLevel);
            healthBar.SetMaxHeath(maxHealth);
            healthBar.SetCurrentHeath(currentHealth);
            UIManager.Instance.SetMaxEXP(maxExp);
            UIManager.Instance.SetCurrentEXP(currentExp);
        }
    }

    private void SyncNetStats()
    {
        if (!IsServerAuthority)
            return;

        NetCurrentHealth.Value = currentHealth;
        NetMaxHealth.Value = maxHealth;
        NetCurrentExp.Value = currentExp;
        NetMaxExp.Value = maxExp;
        NetLevel.Value = playerLevel;
    }

    private void RefreshLocalUIFromNet()
    {
        if (!IsLocalPlayerInstance)
            return;

        UIManager.Instance.UpdateLeveTextlUI(NetLevel.Value);
        healthBar.SetMaxHeath(NetMaxHealth.Value);
        healthBar.SetCurrentHeath(NetCurrentHealth.Value);
        UIManager.Instance.SetMaxEXP(NetMaxExp.Value);
        UIManager.Instance.SetCurrentEXP(NetCurrentExp.Value);
    }

    private void OnNetStatsChanged(float oldValue, float newValue)
    {
        RefreshLocalUIFromNet();
    }

    private void OnNetLevelChanged(int oldValue, int newValue)
    {
        RefreshLocalUIFromNet();
    }

    private void Move()
    {
        if (!playerController)
            playerController = GetComponentInParent<PlayerController>();

        if (!playerController)
            return;

        Vector2 dir = GameModeManager.Mode == GameMode.Offline
            ? playerController.moveDir
            : playerController.ServerMoveDir;

        transform.Translate(dir * playerSpeed * Time.deltaTime);
    }



    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }


    public void LoseHP(int dmg)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        float takeHitRate = Random.Range(0f, 100f);
        if (takeHitRate > dodgeRate)
        {
            if (!isTakeHitInterval && currentHealth != 0)
            {
                dmg = dmg - (int)armor > 0 ? dmg - (int)armor : 0;
                if (currentHealth > dmg)
                {
                    ShowFloatingText(dmg, true);
                    animationController.TakeHitAnimation();
                    currentHealth -= dmg;
                    isTakeHitInterval = true;
                    AudioManager.Instance.PlaySFX("PlayerTakeHit");
                }
                else
                {
                    Death();
                }
                if (IsOffline)
                    healthBar.SetCurrentHeath(currentHealth);

                SyncNetStats();
            }
        }
    }

    private void ShowFloatingText(float number, bool isLoseHp)
    {
        if (floatingTextPrefabs)
        {
            var text = Instantiate(floatingTextPrefabs, transform.position, Quaternion.identity, transform);
            if (isLoseHp)
            {
                text.GetComponent<TextMeshPro>().text = $"-{number}";
                text.GetComponent<TextMeshPro>().color = Color.red;
            }
            else
            {
                text.GetComponent<TextMeshPro>().text = $"+{number}";
                text.GetComponent<TextMeshPro>().color = Color.green;
            }
        }
    }

    public void SetPassiveBonus(PassivesSkillStats passiveStat)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        reduceCooldown += passiveStat.reduceCooldown;
        increaseDame += passiveStat.increaseDame;
        armor += passiveStat.armor;
        amountProjectile += passiveStat.amountProjectile;
        moveSpeed += passiveStat.moveSpeed;
        projectileSpeed += passiveStat.projectileSpeed;
        bonusMaxHealth += passiveStat.maxHealth;
        bonusExperience += passiveStat.bonusExperience;

        playerSpeed += moveSpeed;
        maxHealth += maxHealth * bonusMaxHealth;
        RefreshWeapontController();
        SyncNetStats();
    }

    private void RefreshWeapontController()
    {
        foreach (var weapon in currentWeaponList)
        {
            weapon.SetStats(weapon.level);
        }
    }

    public void PickUpChest()
    {
        if (!IsLocalPlayerInstance)
            return;

        Array.Find(AudioManager.Instance.musicSounds, musicSound => musicSound.name == "ThemeMusic").audioSource.Stop();
        GameStateManager.Instance.StopGame();
        UIManager.Instance.pickUpChestUI.gameObject.SetActive(true);
    }

    public void GainHP(int health)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        health += (int)bonusHealing;
        ShowFloatingText(health, false);
        if (currentHealth + health > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += health;
        }
        if (IsOffline)
            healthBar.SetCurrentHeath(currentHealth);

        SyncNetStats();
    }

    public void GainEXP(float exp, bool isRemainingExp = false)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        if (!isRemainingExp)
        {
            exp += exp * bonusExperience;
        }
        if (isLevelingUp)
        {
            remainingExp += exp;
        }
        else if (currentExp + exp >= maxExp)
        {
            remainingExp = currentExp + exp - maxExp;
            isLevelingUp = true;
            UpLevel();
        }
        else
        {
            currentExp += exp;
            remainingExp = 0;
            if (IsOffline)
            {
                UIManager.Instance.levelUpUI.gameObject.SetActive(false);
                GameStateManager.Instance.ResumeGame();
            }
        }
        if (IsOffline)
            UIManager.Instance.SetCurrentEXP(currentExp);

        SyncNetStats();
    }

    public void UpLevel()
    {
        if (GameModeManager.Mode == GameMode.Multiplayer)
        {
            playerLevel++;
            UpdateStatsLevelUp();
            currentExp = maxExp;
            SyncNetStats();

            List<SkillController> selectable = SelectableWeaponToUpgrade();
            if (selectable.Count > 0)
            {
                SkillOptionInfo[] options = BuildSkillOptions(selectable);
                ShowLevelUpOptionsToOwner(options);
            }
            else
            {
                currentExp = 0;
                isLevelingUp = false;
                SyncNetStats();
            }
            return;
        }

        Debug.Log("level up " + playerLevel);
        GameStateManager.Instance.StopGame();
        AudioManager.Instance.PlaySFX("LevelUp");
        playerLevel++;
        UpdateStatsLevelUp();

        healthBar.SetMaxHeath(maxHealth);

        currentExp = maxExp;
        UIManager.Instance.SetCurrentEXP(currentExp);
        UIManager.Instance.SetMaxEXP(maxExp);
        UIManager.Instance.levelUpUI.SetLevelUp(SelectableWeaponToUpgrade());
        UIManager.Instance.levelUpUI.gameObject.SetActive(true);
    }

    private void ShowLevelUpOptionsToOwner(SkillOptionInfo[] options)
    {
        if (options == null || options.Length == 0)
            return;

        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { OwnerClientId }
            }
        };
        ShowLevelUpOptionsClientRpc(options, rpcParams);
    }

    [ClientRpc]
    private void ShowLevelUpOptionsClientRpc(SkillOptionInfo[] options, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner)
            return;

        UIManager.Instance.levelUpUI.SetLevelUpNetwork(this, options);
        UIManager.Instance.levelUpUI.gameObject.SetActive(true);
    }

    [ClientRpc]
    private void CloseLevelUpClientRpc(ClientRpcParams rpcParams = default)
    {
        if (!IsOwner)
            return;

        UIManager.Instance.levelUpUI.Close();
    }

    [ServerRpc]
    public void RequestUpgradeSkillServerRpc(string skillName)
    {
        if (string.IsNullOrEmpty(skillName))
            return;

        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { OwnerClientId }
            }
        };
        CloseLevelUpClientRpc(rpcParams);

        SkillController skill = FindSkillByName(skillName);
        if (skill == null)
            return;

        UpgradeSkill(skill);
    }

    private SkillController FindSkillByName(string skillName)
    {
        if (string.IsNullOrEmpty(skillName))
            return null;

        foreach (var weapon in currentWeaponList)
        {
            if (weapon != null && weapon.skillName == skillName)
                return weapon;
        }

        foreach (var passive in currentPassiveSkillList)
        {
            if (passive != null && passive.skillName == skillName)
                return passive;
        }

        foreach (var weapon in allWeaponList)
        {
            if (weapon != null && weapon.skillName == skillName)
                return weapon;
        }

        foreach (var passive in allPassiveSkillList)
        {
            if (passive != null && passive.skillName == skillName)
                return passive;
        }

        return null;
    }

    private SkillOptionInfo[] BuildSkillOptions(List<SkillController> selectable)
    {
        List<SkillOptionInfo> list = new List<SkillOptionInfo>();
        foreach (var skill in selectable)
        {
            if (skill == null)
                continue;

            bool isOwned = currentWeaponList.Contains(skill as WeaponController) || currentPassiveSkillList.Contains(skill as PassivesSkillController);
            int nextLevel = isOwned ? skill.level + 1 : 1;
            string desc = GetDescriptionFor(skill, nextLevel);
            list.Add(new SkillOptionInfo(skill.skillName, desc, nextLevel, skill.skillType == SkillType.Weapon));
        }
        return list.ToArray();
    }

    private string GetDescriptionFor(SkillController skill, int nextLevel)
    {
        if (skill is WeaponController weapon)
        {
            int idx = Mathf.Clamp(nextLevel - 1, 0, weapon.stats.Count - 1);
            return weapon.stats[idx].description;
        }
        if (skill is PassivesSkillController passive)
        {
            int idx = Mathf.Clamp(nextLevel - 1, 0, passive.stats.Count - 1);
            return passive.stats[idx].description;
        }
        return string.Empty;
    }

    private void Death()
    {
        currentHealth = 0;
        AudioManager.Instance.PlayMusic("GameOver");
        animationController.DeathAnimation();
        GameStateManager.Instance.GameOver();
        SyncNetStats();
    }

    public void UpgradeWeapon(WeaponController weaponController)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        bool isAvailable = false;
        foreach (WeaponController weapon in currentWeaponList)
        {
            if (weapon == weaponController)
            {
                weapon.Upgrade();
                isAvailable = true;
                break;
            }
        }
        if (!isAvailable)
        {
            WeaponController newWeapon = Instantiate(weaponController, transform.position, Quaternion.identity);
            newWeapon.SetOwner(this);
            newWeapon.SetStats(1);
            currentWeaponList.Add(newWeapon);
            if (IsOffline)
                UIManager.Instance.UpdateInventoryUI(newWeapon.sprite);
        }

        SyncNetStats();
    }

    public void UpgradePassiveSkill(PassivesSkillController passivesSkillController)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        bool isAvailable = false;
        foreach (PassivesSkillController passive in currentPassiveSkillList)
        {
            if (passive == passivesSkillController)
            {
                passive.Upgrade();
                isAvailable = true;
                break;
            }
        }
        if (!isAvailable)
        {
            PassivesSkillController newPassiveSkill = Instantiate(passivesSkillController, transform.position, Quaternion.identity);
            newPassiveSkill.SetOwner(this);
            newPassiveSkill.SetStats(1);
            currentPassiveSkillList.Add(newPassiveSkill);
            if (IsOffline)
                UIManager.Instance.UpdateInventoryUI(newPassiveSkill.sprite, false);
        }

        SyncNetStats();
    }

    public void UpgradeSkill(SkillController skill)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        if (skill == null)
            return;

        if (skill.skillType == SkillType.Weapon)
        {
            WeaponController weapon = skill as WeaponController;
            if (weapon != null)
            {
                UpgradeWeapon(weapon);
            }
        }
        else
        {
            PassivesSkillController passive = skill as PassivesSkillController;
            if (passive != null)
            {
                UpgradePassiveSkill(passive);
            }
        }

        if (IsOffline)
        {
            AudioManager.Instance.PlaySFX("PowerUp");
            UIManager.Instance.UpdateLeveTextlUI(playerLevel);
            UIManager.Instance.SetMaxEXP(maxExp);
        }

        currentExp = 0;
        if (IsOffline)
            UIManager.Instance.SetCurrentEXP(currentExp);

        isLevelingUp = false;
        if (remainingExp > 0)
        {
            GainEXP(remainingExp, true);
        }
        else
        {
            if (IsOffline)
            {
                UIManager.Instance.levelUpUI.gameObject.SetActive(false);
                GameStateManager.Instance.ResumeGame();
            }
        }

        SyncNetStats();
    }

    public List<SkillController> SelectableWeaponToUpgrade()
    {
        List<SkillController> selectableSkill = new List<SkillController>();
        List<SkillController> pickedSkillList = new List<SkillController>();

        selectableSkill.AddRange(currentWeaponList);
        if (currentWeaponList.Count < maxNumberOfWeapon)
        {
            List<SkillController> tempAllWeaponList = new List<SkillController>(allWeaponList);
            foreach (var weapon in currentWeaponList)
            {
                tempAllWeaponList.Remove(tempAllWeaponList.FirstOrDefault(c => c.skillName == weapon.skillName));
            }
            selectableSkill.AddRange(tempAllWeaponList);
        }

        selectableSkill.AddRange(currentPassiveSkillList);
        if (currentPassiveSkillList.Count < maxNumberOfPassive)
        {
            List<SkillController> tempAllPassiveList = new List<SkillController>(allPassiveSkillList);
            foreach (var passive in currentPassiveSkillList)
            {
                tempAllPassiveList.Remove(tempAllPassiveList.FirstOrDefault(c => c.skillName == passive.skillName));
            }
            selectableSkill.AddRange(tempAllPassiveList);
        }

        int amountWeaponsMaxLv = 0;
        foreach (WeaponController weapon in currentWeaponList)
        {
            if (weapon.level == weapon.maxLevel)
            {
                amountWeaponsMaxLv++;
                selectableSkill.Remove(selectableSkill.FirstOrDefault(c => c.skillName == weapon.skillName));
            }
        }

        Debug.Log("amountWeaponsMaxLv " + amountWeaponsMaxLv);

        int amountPassiveSkillMaxLv = 0;
        foreach (PassivesSkillController passive in currentPassiveSkillList)
        {
            if (passive.level == passive.maxLevel)
            {
                amountPassiveSkillMaxLv++;
                selectableSkill.Remove(selectableSkill.FirstOrDefault(c => c.skillName == passive.skillName));
            }
        }
        Debug.Log("amountPassiveSkillMaxLv " + amountPassiveSkillMaxLv);

        if (selectableSkill.Count > 0)
        {
            int numberSelectable = Math.Min(amountWeaponSelectableWhenLvUp, selectableSkill.Count);
            for (int i = 0; i < numberSelectable; i++)
            {
                bool isSelected = false;
                while (!isSelected)
                {
                    int rand = Random.Range(0, selectableSkill.Count);
                    bool isAlreadyPicked = false;
                    foreach (SkillController skill in pickedSkillList)
                    {
                        if (selectableSkill[rand].skillName == skill.skillName)
                        {
                            isAlreadyPicked = true;
                            break;
                        }
                    }
                    if (isAlreadyPicked)
                    {
                        continue;
                    }
                    pickedSkillList.Add(selectableSkill[rand]);
                    isSelected = true;
                }
            }
        }
        return pickedSkillList;
    }

    public void SetCharacterDefaultStats()
    {
        playerLevel = 1;
        maxHealth = characterStats.defaultMaxHealth + characterStats.bonusMaxHealth;
        currentHealth = maxHealth;
        maxExp = characterStats.defaultMaxExp;
        currentExp = 0;
        playerSpeed = characterStats.defaultSpeed + characterStats.bonusSpeed;
        pickUpRadius = characterStats.defaultPickUpRadius + characterStats.bonusPickUpRadius;
        weaponScale = characterStats.defaultWeaponSize + characterStats.bonusWeaponSize;
        dodgeRate = characterStats.defaultDodgeRate + characterStats.bonusDodgeRate;
        bonusAmountProjectile = characterStats.bonusAmountProjectile;
        bonusHealing = characterStats.bonusHealing;

        pickUpArea.transform.localScale = new Vector3(pickUpRadius, pickUpRadius, pickUpRadius);
    }

    private void UpdateStatsLevelUp()
    {
        float tempMaxHealth = maxHealth;
        maxHealth += characterStats.healthForLevelUpStep;
        currentHealth = (currentHealth / tempMaxHealth) * maxHealth;
        maxExp += characterStats.expForLevelUpStep;
    }

    public void ResetGame()
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        transform.position = Vector3.zero;
        foreach (WeaponController weapon in currentWeaponList)
        {
            Destroy(weapon);
        }
        currentWeaponList.Clear();

        SetCharacterDefaultStats();

        if (IsOffline)
        {
            UIManager.Instance.ResetInventory();
            UIManager.Instance.UpdateLeveTextlUI(playerLevel);
            healthBar.SetMaxHeath(maxHealth);
            healthBar.SetCurrentHeath(currentHealth);
            UIManager.Instance.SetMaxEXP(maxExp);
            UIManager.Instance.SetCurrentEXP(0);
        }

        UpgradeWeapon(characterStats.defaultWeapon);
        animationController.ResetGame();
        SyncNetStats();
    }
}
