using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Basic Attack", menuName = "Attacks/Basic")]
public class BasicAttack : ScriptableObject
{
    public string attackName;
    public string animationName;
    public float damagePower;
    public float priorityPower;
    public Vector2 knockback;
    public bool chargable;
    public float recharge = 0.5f;
    public GameObject projectile;

}
