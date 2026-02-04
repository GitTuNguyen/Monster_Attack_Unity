using UnityEngine;

public class ArrowBehaviour : WeaponBehaviour
{
    private PlayerController playerController;
    private void Awake()
    {
        TryAssignPlayer();
        playerController = FindFirstObjectByType<PlayerController>();
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }
    protected override void Start()
    {
        weaponController = FindFirstObjectByType<ArrowController>();
        base.Start();
        dir = playerController.frontDir;
        Rotate(dir);
        Destroy(gameObject, weaponController.timeToDestroy);
    }
    protected override void Move()
    {
        if (GameStateManager.Instance.isGameOver)
        {
            return;
        }
        transform.Translate(speed * Time.deltaTime * dir, Space.World);
    }
    public override void OnAttackEnemy()
    {
        base.OnAttackEnemy();
        if (pierce <= 0)
        {
            Destroy(gameObject);
        }
    }
}
