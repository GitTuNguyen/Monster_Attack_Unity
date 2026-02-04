using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Chunks")]
    public List<GameObject> terrainChunks;
    public float checkerRadius;
    public LayerMask terrainMask;
    public GameObject currentChunk;

    [Header("Optimization")]
    public List<GameObject> spawnedChunks = new();
    public float maxDistance;
    public float optimizeCooldownDuration = 1f;

    private Vector3 noTerrainPosition;
    private float optimizeCooldown;

    private Player player;

    void Start()
    {
        if (GameModeManager.Mode == GameMode.Offline)
        {
            player = FindFirstObjectByType<Player>();
        }
        else
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            player = Player.LocalPlayer;
        }
    }

    void Update()
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !NetworkManager.Singleton.IsServer)
            return;

        if (player == null || currentChunk == null)
            return;

        ChunkChecker();
        ChunkOptimize();
    }


    private void ChunkChecker()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 chunkPos = currentChunk.transform.position;

        if (playerPos.x > chunkPos.x)
        {
            CheckAndSpawn("Right");
            CheckAndSpawn("Right Up");
            CheckAndSpawn("Right Down");
        }
        else if (playerPos.x < chunkPos.x)
        {
            CheckAndSpawn("Left");
            CheckAndSpawn("Left Up");
            CheckAndSpawn("Left Down");
        }

        if (playerPos.y > chunkPos.y)
        {
            CheckAndSpawn("Up");
            CheckAndSpawn("Right Up");
            CheckAndSpawn("Left Up");
        }
        else if (playerPos.y < chunkPos.y)
        {
            CheckAndSpawn("Down");
            CheckAndSpawn("Left Down");
            CheckAndSpawn("Right Down");
        }
    }

    private void CheckAndSpawn(string pointName)
    {
        Transform point = currentChunk.transform.Find(pointName);
        if (point == null)
            return;

        if (!Physics2D.OverlapCircle(point.position, checkerRadius, terrainMask))
        {
            noTerrainPosition = point.position;
            SpawnChunk();
        }
    }

    private void SpawnChunk()
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !NetworkManager.Singleton.IsServer)
            return;

        foreach (GameObject chunk in spawnedChunks)
        {
            if (chunk.transform.position == noTerrainPosition)
                return;
        }

        int rand = Random.Range(0, terrainChunks.Count);
        GameObject newChunk = Instantiate(
            terrainChunks[rand],
            noTerrainPosition,
            Quaternion.identity
        );

        spawnedChunks.Add(newChunk);
    }

    private void ChunkOptimize()
    {
        optimizeCooldown -= Time.deltaTime;
        if (optimizeCooldown > 0)
            return;

        optimizeCooldown = optimizeCooldownDuration;

        foreach (GameObject chunk in spawnedChunks)
        {
            float distance = Vector3.Distance(
                player.transform.position,
                chunk.transform.position
            );

            chunk.SetActive(distance <= maxDistance);
        }
    }
}
