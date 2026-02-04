using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public float maxNumberOfEnemy;
    public List<GameObject> normalMonsterPrefabs = new();
    public List<GameObject> eliteMonsterPrefabs = new();
    public List<GameObject> bossPrefabs = new();
    public GameObject chickenPrefabs;

    [Header("Spawn Settings")]
    public float timeBetweenSpawns;
    public float radiusSpawnerCircle;

    public float spawnEliteMonsterInterval;
    public float spawnBossInterval;
    public float spawnChickenInterval;

    private float timeToSpawnEliteMonster;
    private float timeToSpawnBoss;
    private float timeToSpawnChicken;

    [Header("Runtime")]
    public List<GameObject> enemyList = new();

    private bool isFreezing = false;
    private Player cachedPlayer;

    void Start()
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !NetworkManager.Singleton.IsServer)
        {
            enabled = false;
            return;
        }

        cachedPlayer = GetClosestPlayer();
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !NetworkManager.Singleton.IsServer)
            return;

        timeToSpawnChicken += Time.deltaTime;
        timeToSpawnEliteMonster += Time.deltaTime;
        timeToSpawnBoss += Time.deltaTime;
    }

    private Player GetClosestPlayer()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (players == null || players.Length == 0)
            return null;

        Player closest = null;
        float minDistance = float.MaxValue;

        foreach (var p in players)
        {
            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d < minDistance)
            {
                minDistance = d;
                closest = p;
            }
        }

        return closest;
    }

    private void SpawnEnemy()
    {
        if (enemyList.Count >= maxNumberOfEnemy)
            return;

        if (cachedPlayer == null)
            cachedPlayer = GetClosestPlayer();

        if (cachedPlayer == null)
            return;

        Vector2 basePos = cachedPlayer.transform.position;
        Vector2 spawnPosition =
            basePos +
            Random.insideUnitCircle.normalized * radiusSpawnerCircle +
            new Vector2(Random.Range(0, 5), Random.Range(0, 5));

        GameObject prefabToSpawn = SelectEnemyPrefab();
        if (prefabToSpawn == null)
            return;

        GameObject enemy = Instantiate(
            prefabToSpawn,
            spawnPosition,
            prefabToSpawn.transform.rotation
        );

        enemyList.Add(enemy);

        Enemy enemyController = enemy.GetComponent<Enemy>();
        if (enemyController != null)
            enemyController.SetSpawner(this);

        if (GameModeManager.Mode == GameMode.Multiplayer)
        {
            NetworkObject netObj = enemy.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn();
        }
    }

    private GameObject SelectEnemyPrefab()
    {

        if (timeToSpawnBoss >= spawnBossInterval && bossPrefabs.Count > 0)
        {
            timeToSpawnBoss = 0;
            return bossPrefabs[Random.Range(0, bossPrefabs.Count)];
        }

        if (timeToSpawnEliteMonster >= spawnEliteMonsterInterval && eliteMonsterPrefabs.Count > 0)
        {
            timeToSpawnEliteMonster = 0;
            return eliteMonsterPrefabs[Random.Range(0, eliteMonsterPrefabs.Count)];
        }

        if (timeToSpawnChicken >= spawnChickenInterval && chickenPrefabs != null)
        {
            timeToSpawnChicken = 0;
            return chickenPrefabs;
        }

        if (normalMonsterPrefabs.Count > 0)
        {
            return normalMonsterPrefabs[Random.Range(0, normalMonsterPrefabs.Count)];
        }

        return null;
    }

    private IEnumerator SpawnRoutine()
    {
        while (!GameStateManager.Instance.isGameOver)
        {
            if (!isFreezing)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    public void RemoveEnemyFromList(GameObject enemy)
    {
        enemyList.Remove(enemy);
    }

    public void FreezesAllEnemy(float timeFreeze)
    {
        isFreezing = true;
        StartCoroutine(FreezeRoutine(timeFreeze));
    }

    private IEnumerator FreezeRoutine(float timeFreeze)
    {
        foreach (GameObject enemy in enemyList)
        {
            Enemy enemyController = enemy.GetComponent<Enemy>();
            if (enemyController != null)
            {
                enemyController.Freeze(timeFreeze);
            }
        }

        yield return new WaitForSeconds(timeFreeze);
        isFreezing = false;
    }

    public void DestroyAllEnemy()
    {
        foreach (GameObject enemy in enemyList)
        {
            if (enemy == null)
                continue;

            if (GameModeManager.Mode == GameMode.Multiplayer)
            {
                NetworkObject netObj = enemy.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn(true);
                else
                    Destroy(enemy);
            }
            else
            {
                Destroy(enemy);
            }
        }

        enemyList.Clear();
    }

    public void KillAllEnemy()
    {
        foreach (GameObject enemy in enemyList)
        {
            if (enemy == null)
                continue;

            Enemy enemyController = enemy.GetComponent<Enemy>();
            if (enemyController != null)
            {
                enemyController.Death(true);
            }
        }

        enemyList.Clear();
    }
}
