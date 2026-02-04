using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrologionController : MonoBehaviour
{
    //Freezes all enemies
    private EnemySpawner enemySpawner;
    public float timeFreeze = 3f;

    private void Awake() {
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && enemySpawner != null)
        {
            enemySpawner.FreezesAllEnemy(timeFreeze);
            AudioManager.Instance.PlaySFX("PickUpChicken");
            Destroy(gameObject);
        }
    }
}
