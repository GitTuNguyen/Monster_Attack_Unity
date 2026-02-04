using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class WeaponController : SkillController
{
    public List<WeaponStats> stats;
    public List<Object> projectileList;
    public int dame;
    public float speed;
    public int pierce;
    public int amount;
    public float projectileInterval;
    public float projectileScale;
    public float attackDuration;
    public float cooldown;
    public float timeToDestroy;
    public float radius;
    public float bonusWeaponScale;
    public int bonusAmount = 0;
    public Vector3 projectileSpawnPosition;


    protected override void Start()
    {
        base.Start();
        maxLevel = stats.Count;
        skillType = stats[0].skillType;

        if (NetHelper.IsServerOrOffline())
            StartCoroutine(AttackRoutine());
    }


    public override void SetStats(int level)
    {
        dame = stats[level - 1].projectileDMG;
        speed = stats[level - 1].projectileSpeed;
        pierce = stats[level - 1].pierce;
        amount = stats[level - 1].amount + (int)player.amountProjectile + (int)player.bonusAmountProjectile;
        projectileInterval = stats[level - 1].projectileInterval;
        projectileScale = stats[level - 1].projectileScale + player.weaponScale;
        attackDuration = stats[level - 1].attackDuration;
        cooldown = stats[level - 1].cooldown - cooldown * player.reduceCooldown;
        timeToDestroy = stats[level - 1].timeToDestroy;
        radius = stats[level - 1].radius;
        Reset();
    }

    public override string getDescriptionNextLevel()
    {
        return stats[level].description;
    }

    protected virtual void Attack()
    {
        if (!NetHelper.IsServerOrOffline())
            return;

        GameObject projectile = Instantiate(prefab, projectileSpawnPosition, Quaternion.identity);

        var weaponBehaviour = projectile.GetComponent<WeaponBehaviour>();
        weaponBehaviour.weaponController = this;
        weaponBehaviour.player = player;

        if (GameModeManager.Mode == GameMode.Multiplayer)
        {
            var netObj = projectile.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError($"{prefab.name} missing NetworkObject");
                Destroy(projectile);
                return;
            }
            netObj.Spawn();
        }

        projectileList.Add(projectile);
    }



    protected virtual IEnumerator AttackRoutine()
    {
        while (!GameStateManager.Instance.isGameOver)
        {
            for (int i = 0; i < amount; i++)
            {
                Attack();
                yield return new WaitForSeconds(projectileInterval);
            }
            yield return new WaitForSeconds(cooldown);
        }
    }

    public void Reset()
    {
        if (!NetHelper.IsServerOrOffline())
            return;

        if (projectileList.Count > 0)
        {
            StopAllCoroutines();
            foreach (var projectile in projectileList)
                Destroy(projectile);

            projectileList.Clear();
            StartCoroutine(AttackRoutine());
        }
    }
}
