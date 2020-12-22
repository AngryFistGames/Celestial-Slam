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
    public float gripLoss;
    public float GuardBreaking;
    public int charge = 1;
    public bool flinchless = false;
    public int owner;
    public void OnEnable()
    {
        if (!gameObject.GetComponent<Projectile>())
        {
            technique = GetComponentInParent<PlayerScript>().attack;
            if (technique != null)
            {
                isAttacking = true;
                gripLoss = technique.gripLoss * charge;
                priorityPower = technique.priorityPower * GetComponentInParent<PlayerScript>().attackPower;
                damagePower = technique.damagePower * GetComponentInParent<PlayerScript>().attackPower * charge;
                knockback = technique.knockback * charge;
                GuardBreaking = technique.GuardBreaking * charge;
                flinchless = technique.flinchless;
            }
        }
        if (gameObject.GetComponent<Projectile>())
        {
            ammo = GetComponent<Projectile>().ammo;

            if (ammo != null)
            {
                isAttacking = true;
                gripLoss = ammo.gripLoss * charge;
                priorityPower = ammo.priorityPower;
                damagePower = ammo.damagePower * charge;
                knockback = ammo.knockback;
                GuardBreaking = ammo.GuardBreaking;
                flinchless = ammo.flinchless;
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
