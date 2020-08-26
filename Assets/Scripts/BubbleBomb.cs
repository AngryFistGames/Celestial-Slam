using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleBomb : MonoBehaviour
{
    public int launchSize;
    public Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        gameObject.GetComponent<GravBody>().attractor = transform.GetComponentInParent<PlayerTracker>().targetTag;
        launchSize = GetComponentInParent<PlayerTracker>().attackCharge;
        if (GetComponentInParent<PlayerTracker>() != null && launchSize > 0)
        {
            transform.localScale = new Vector2(launchSize * 0.2f, launchSize * 0.2f);
            GetComponent<Projectile>().directionSpeed = (GetComponent<Projectile>().directionSpeed * launchSize);
        }
        if (launchSize <= 0)
        {
            Destroy(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("HitBox"))
        {
            if (GetComponent<Projectiles>().priorityPower <= collision.gameObject.GetComponent<Projectiles>().priorityPower)
            {
                anim.SetTrigger("done");
            }
                    }
        
    }
}
