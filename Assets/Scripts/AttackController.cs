using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    public BasicAttack technique;
    public Projectiles ammo;
    public float priorityPower; //hit priority
    public bool isAttacking; //is active
    public float damagePower; //damage dealt upon hit
    public bool SweetSpot; //is this hitbox a sweetspot
    public bool isProjectile;
    public Vector2 knockback;

    public void OnEnable()
    {

        technique = GetComponentInParent<PlayerControl>().attack;
        if (technique != null)
        {
            isAttacking = true;
            priorityPower = technique.priorityPower * GetComponentInParent<PlayerControl>().attackPower;
            damagePower = technique.damagePower * GetComponentInParent<PlayerControl>().attackPower;
            knockback = technique.knockback;
        }
        if (gameObject.GetComponent<Projectile>())
        {
            ammo = GetComponent<Projectile>().ammo;

            if (ammo != null)
            {
                isAttacking = true;
                priorityPower = ammo.priorityPower;
                damagePower = ammo.damagePower;
                knockback = ammo.knockback;
            }
        }
    }

    public void OnDisable()
    {
        if (technique != null)
        {
            technique = null;
            isAttacking = false;
            priorityPower = 0;
            damagePower = 0;
            knockback = Vector2.zero;
        }
        if (technique != null)
        {
            Destroy(this);
       
        }
    }
}
