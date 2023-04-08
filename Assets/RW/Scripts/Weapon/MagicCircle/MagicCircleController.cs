using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MagicCircleController : WeaponController
{
    
    private GameObject magicCircle;
    private Player player;
    protected override void Start()
    {
        player = FindObjectOfType<Player>();
        magicCircle = Instantiate(prefab, player.transform.position, Quaternion.identity, player.transform);        
        base.Start();
    }

    protected override void Attack()
    {
        magicCircle.GetComponent<MagicCircleBehaviour>().Attack();
    }
    

}
