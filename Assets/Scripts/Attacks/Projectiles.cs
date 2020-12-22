using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Basic Attack", menuName = "Attacks/Projectile")]
public class Projectiles : ScriptableObject
{
    public Vector2 trajectory;
    public float damagePower;
    public float priorityPower;
    public Vector2 knockback;
    public float GuardBreaking;
    public float gripLoss;
    public float charge;
    public bool doesRikochet;
    public Vector2 rikochetAngle;
    public Vector2 launchPoint;
    public bool flinchless;
    public string launcher;
}
