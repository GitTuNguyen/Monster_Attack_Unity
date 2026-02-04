
public class FireballController : WeaponController
{
    private EnemySpawner enemySpawner;
    protected override void Start()
    {
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
        base.Start();        
    }


    protected override void Attack()
    {
        projectileSpawnPosition = player.transform.position;
        if (enemySpawner.enemyList.Count > 0)
        {
            base.Attack();
        }
        AudioManager.Instance.PlaySFX("FireBall");
    }
        
}
