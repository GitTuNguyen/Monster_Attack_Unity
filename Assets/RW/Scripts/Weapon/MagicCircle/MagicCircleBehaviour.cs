using UnityEngine;

public class MagicCircleBehaviour : WeaponBehaviour
{    
    [SerializeField]
    private float timeAttack;
    [SerializeField]
    private float radiusOfDamage;
    [SerializeField]
    private float defaultRadius;

    public LayerMask layerMask;
    // Start is called before the first frame update
    protected override void Start()
    {
        weaponController = FindFirstObjectByType<MagicCircleController>();
        defaultRadius = transform.GetComponent<CircleCollider2D>().radius;
        Debug.Log("(start)weaponController.attackDuration = " + weaponController.attackDuration);
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (radiusOfDamage != defaultRadius * weaponController.projectileScale)
        {
            radiusOfDamage = defaultRadius * weaponController.projectileScale;
            transform.localScale = new Vector3(weaponController.projectileScale, weaponController.projectileScale, weaponController.projectileScale);    
        }                    
    }
    public void Attack()
    {
        timeAttack = 0;
        if (weaponController == null)
        {
            weaponController = FindFirstObjectByType<MagicCircleController>();
        }
        while (timeAttack < weaponController.attackDuration)
        {            
            Collider2D[] enemyInsideAttackArea = Physics2D.OverlapCircleAll(transform.position, radiusOfDamage, layerMask);
            foreach (Collider2D collision in enemyInsideAttackArea)
            {
                collision.GetComponent<Enemy>().LoseHP(dame);

            }
            timeAttack += Time.unscaledDeltaTime;
        }
    }
}
