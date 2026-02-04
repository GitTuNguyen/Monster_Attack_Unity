using Unity.VisualScripting;
using UnityEngine;

public class VampireAxeBehaviour : WeaponBehaviour
{
    public Animator animator;
    private void Awake()
    {
        //Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }
    protected override void Start()
    {
        weaponController = FindFirstObjectByType<VampireAxeController>();
        base.Start();
        //dir = playerController.frontdDir;
        //Rotate(dir);
        if (weaponController.level < weaponController.maxLevel)
        {
            Destroy(gameObject, weaponController.cooldown - weaponController.timeToDestroy > 0 ? weaponController.cooldown - weaponController.timeToDestroy : 0);
        } else 
        {
            animator.SetTrigger("MaxLevel");
        }
    }
    protected override void Move()
    {
        transform.RotateAround(player.transform.position, Vector3.forward, speed*Time.deltaTime);
    }
}
