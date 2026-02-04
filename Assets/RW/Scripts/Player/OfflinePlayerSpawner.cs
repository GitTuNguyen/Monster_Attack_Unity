using UnityEngine;

public class OfflinePlayerSpawner : MonoBehaviour
{
    public GameObject playerControllerPrefab;

    void Start()
    {
        if (GameModeManager.Mode != GameMode.Offline)
            return;

        GameObject pc = Instantiate(playerControllerPrefab, Vector3.zero, Quaternion.identity);

        pc.GetComponent<PlayerController>()
          .SpawnOfflineCharacter();
    }
}
