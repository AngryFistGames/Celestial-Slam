using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTracker : MonoBehaviour
{
    public float shrinkTimerMax = 1f;

    public bool defeated = false;
    public Image barImage;
    public Image damageBar;
    public float hitPoints;
    public int attackCharge;
    public PlayerScript playerSelected;
    public GravAttractor targetTag;
    public bool faceRight;
    public int playerNumber;
    public float shrinkTimer;
    [SerializeField] float shrinkSpeed = .1f;

    // Start is called before the first frame update
    void Start()
    {
        playerSelected = GetComponentInChildren<PlayerScript>(false);
        SetHealth(1f);
        damageBar.fillAmount = barImage.fillAmount;
    }

    // Update is called once per frame
    void Update()
    {
        shrinkTimer -= Time.deltaTime;
        if (shrinkTimer < 0)
        {
          if   (barImage.fillAmount < damageBar.fillAmount)
                {
                damageBar.fillAmount -= shrinkSpeed * Time.deltaTime;
            }
        }
        hitPoints = playerSelected.HP - playerSelected.damage;
        if (hitPoints <= 0)
        {
            defeated = true;
        }
        attackCharge = playerSelected.attackCharge;
        targetTag = playerSelected.Target;
        faceRight = playerSelected.faceRight;
    }

    public float GetHealthNormalized()
    {
        return (float)hitPoints / playerSelected.HP;
    }

    public void SetHealth(float hpPercentage)
    {
        barImage.fillAmount = hpPercentage;
    }

    public void FlipProjectiles()
    {
        if (GetComponentInChildren<scr_Flip>() != null)
        {
            GetComponentInChildren<scr_Flip>().rend.flipX = !GetComponentInChildren<scr_Flip>().rend.flipX;
        }
    }
  
}
