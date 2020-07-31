using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    public float hitPoints;
    public int attackCharge;
    public PlayerControl playerSelected;
    public GravAttractor targetTag;
    public bool faceRight;

    // Start is called before the first frame update
    void Start()
    {
        playerSelected = GetComponentInChildren<PlayerControl>(false);    
    }

    // Update is called once per frame
    void Update()
    {
        hitPoints = playerSelected.HP;
        attackCharge = playerSelected.attackCharge;
        targetTag = playerSelected.Target;
        faceRight = playerSelected.faceRight;
    }
}
