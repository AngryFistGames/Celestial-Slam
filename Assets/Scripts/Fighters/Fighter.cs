using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fighter", menuName = "Fighters")]
public class Fighter : ScriptableObject
{
    public string fighterName;
    public float HitPoints;
    public float jumpHeight;
    public float speed;
    public float weight;
    public float BaseAttack;
    public float BaseDefense;
    public BasicAttack[] techniques;
}
