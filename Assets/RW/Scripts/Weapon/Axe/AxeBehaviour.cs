using UnityEngine;

public class AxeBehaviour : WeaponBehaviour
{
    private Rigidbody2D rb;
    public float thrust = 1f;
    public float gravity = 2.5f;
    private Vector2 mFallingDir;
    private bool mIsFalling = false;
    private void Awake()
    {
        TryAssignPlayer();
        rb = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), transform.GetComponent<Collider2D>());
    }
    protected override void Start()
    {
        weaponController = FindFirstObjectByType<AxeController>();
        base.Start();
        dir = new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized;
        mFallingDir = new Vector2(0, -1f).normalized;
        thrust = Random.Range(5f, 7f);
        //rb?.AddForce(dir * thrust);
        //Rotate(dir);
        Destroy(gameObject, weaponController.timeToDestroy);
    }

    protected void FixedUpdate()
    {
        if(thrust < 0f)
        {
            mIsFalling = true;
        }

        if (!mIsFalling)
        {
            rb?.AddForce(dir * thrust * weaponController.speed);
            thrust -= gravity;
        } else 
        {
            thrust += gravity;
            rb?.AddForce(mFallingDir * thrust * weaponController.speed);            
        }
    }

    protected override void Move()
    {
        // if (!GameStateManager.Instance.isGameOver)
        // {
        //     transform.Translate(speed * Time.deltaTime * dir, Space.World);
        // }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }
}
