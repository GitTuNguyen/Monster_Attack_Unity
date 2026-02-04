using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RosaryController : MonoBehaviour
{
    //Kill All Enemies
    private EnemySpawner enemySpawner;

    private void Awake() {
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && enemySpawner != null)
        {
            enemySpawner.KillAllEnemy();
            AudioManager.Instance.PlaySFX("PickUpChicken");
            Destroy(gameObject);
        }
    }
}
