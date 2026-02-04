using UnityEngine;

public class LightningBehaviour : WeaponBehaviour
{    
    public LayerMask enemyLayerMask;
    [SerializeField]
    private float baseArea;
    [SerializeField]
    private float currentArea;
    private Animator animator;
    private Collider2D[] enemyInsideArea;
    // Start is called before the first frame update
    protected override void Start()
    {
        weaponController = FindFirstObjectByType<LightningController>();
        base.Start();
        currentArea = baseArea * weaponController.projectileScale;
        enemyInsideArea = Physics2D.OverlapCircleAll(transform.position, currentArea, enemyLayerMask);
        foreach (Collider2D collision in enemyInsideArea)
        {
            collision.GetComponent<Enemy>().LoseHP(weaponController.dame);
        }
        animator = transform.GetChild(0).GetComponent<Animator>();
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }

}
