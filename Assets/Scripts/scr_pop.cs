using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_pop : MonoBehaviour
{
    public GameObject parentBubble;

    // Start is called before the first frame update
    void Start()
    {
        parentBubble = GetComponentInParent<BubbleBomb>().gameObject;
    }

   public void Pop()
    {
        parentBubble.GetComponent<Projectile>().directionSpeed = 0;
        Destroy(parentBubble, .01f);
    }
}
