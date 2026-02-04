using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBehaviour : MonoBehaviour
{
    public Vector2 dir;
    public float speed;
    public float dame;
    public float pierce;
    public WeaponController weaponController;
    public Player player;

    protected virtual void Start()
    {
        TryAssignPlayer();
        speed = weaponController.speed + player.projectileSpeed;
        dame = weaponController.dame + dame * player.increaseDame;
        pierce = weaponController.pierce;
        transform.localScale = Vector3.one * weaponController.projectileScale;
    }

    protected virtual void Update()
    {
        if (!NetHelper.IsServerOrOffline())
            return;
        Move();
    }

    protected virtual void Move()
    {

    }

    protected virtual void Rotate(Vector2 dir)
    {
        Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 360);
    }

    public virtual void OnAttackEnemy()
    {
        pierce--;
    }

    protected void TryAssignPlayer()
    {
        if (Player.LocalPlayer != null)
        {
            player = Player.LocalPlayer;
            return;
        }

        player = FindFirstObjectByType<Player>();
    }

    protected void OnDestroy() 
    {
        weaponController.projectileList.Remove(gameObject);
    }
}
