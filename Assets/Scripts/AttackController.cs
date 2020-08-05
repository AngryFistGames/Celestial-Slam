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
    public float GuardBreaking;
    public int charge = 1;

    public void OnEnable()
    {
        if (!gameObject.GetComponent<Projectile>())
        {
            technique = GetComponentInParent<PlayerControl>().attack;
            if (technique != null)
            {
                isAttacking = true;
                priorityPower = technique.priorityPower * GetComponentInParent<PlayerControl>().attackPower;
                damagePower = technique.damagePower * GetComponentInParent<PlayerControl>().attackPower * charge;
                knockback = technique.knockback * charge;
                GuardBreaking = technique.GuardBreaking * charge;
            }
        }
        if (gameObject.GetComponent<Projectile>())
        {
            ammo = GetComponent<Projectile>().ammo;

            if (ammo != null)
            {
                isAttacking = true;
                priorityPower = ammo.priorityPower;
                damagePower = ammo.damagePower * charge;
                knockback = ammo.knockback;
                GuardBreaking = ammo.GuardBreaking;
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
            charge = 1;
        }
        if (ammo != null)
        {
            Destroy(this);
       
        }
    }
}
