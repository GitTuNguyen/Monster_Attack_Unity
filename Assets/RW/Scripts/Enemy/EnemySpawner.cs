using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    public float maxNumberOfEnemy;
    public List<GameObject> normalEnemyPrefabs = new List<GameObject>();
    public List<GameObject> bossPrefabs = new List<GameObject>();
    public GameObject chickenPrefabs;
    [Header("Settings")]
    public float timeBetweenSpawns;
    public float radiusSpawnerCircle;
    public float spawnChickenInterval;
    public float timeToSpawnChicken;
    public float spawnBossInterval;
    public float timeToSpawnBoss;
    [HideInInspector]
    public List<GameObject> enemyList = new List<GameObject>();
    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnRoutine());
        timeToSpawnChicken = 0;
        timeToSpawnBoss = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timeToSpawnChicken += Time.deltaTime;
        timeToSpawnBoss += Time.deltaTime;
    }
    private void SpawnEnemy()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
        Vector2 playerPosition = player.transform.position;
        Vector2 spawnPosition = new Vector2(playerPosition.x, playerPosition.y) + Random.insideUnitCircle.normalized * radiusSpawnerCircle + new Vector2(Random.Range(0, 5), Random.Range(0, 5));
        GameObject enemyPrefabs = null;
        if (enemyList.Count < maxNumberOfEnemy)
        {
            enemyPrefabs = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Count)];
            
        }
        if (timeToSpawnChicken >= spawnChickenInterval)
        {
            enemyPrefabs = chickenPrefabs;
            timeToSpawnChicken = 0;
        }

        if (timeToSpawnBoss >= spawnBossInterval)
        {
            enemyPrefabs = bossPrefabs[Random.Range(0, bossPrefabs.Count)];
            timeToSpawnBoss = 0;
        }
        if (enemyPrefabs != null)
        {
            GameObject enemy = Instantiate(enemyPrefabs, spawnPosition, enemyPrefabs.transform.rotation);
            enemyList.Add(enemy);
            enemy.GetComponent<Enemy>().SetSpawner(this);
        }        
    }
    
    private IEnumerator SpawnRoutine()
    {
        while (!GameStateManager.Instance.isGameOver)
        {            
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    public void RemoveEnemyFromList(GameObject enemy)
    {
        enemyList.Remove(enemy);
    }

    public void DestroyAllEnemy()
    {
        foreach (GameObject enemy in enemyList)
        {
            Destroy(enemy);
        }

        enemyList.Clear();
    }
}
