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
        Destroy(parentBubble, .01f);
    }
}
