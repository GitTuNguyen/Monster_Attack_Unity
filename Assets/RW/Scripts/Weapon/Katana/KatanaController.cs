using System.Collections.Generic;
using UnityEngine;

public class KatanaController : WeaponController
{
    private PlayerController playerController;
    public List<Transform> spawnPosition;
    private int spawnPositionIndex;
    private int i;
    protected override void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        transform.parent = player.transform;
        spawnPositionIndex = 0;
        base.Start();
    }

    private void Update()
    {
        if (playerController.moveDir.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        } else if (playerController.moveDir.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    protected override void Attack()
    {
        projectileSpawnPosition = spawnPosition[spawnPositionIndex].transform.position;
        var projectile = Instantiate(prefab, projectileSpawnPosition, spawnPosition[spawnPositionIndex].transform.rotation, spawnPosition[spawnPositionIndex].transform);
        projectileList.Add(projectile);
        spawnPositionIndex++;
        if (spawnPositionIndex == spawnPosition.Count || i == amount - 1)
        {
            spawnPositionIndex = 0;
        }
    }

}
