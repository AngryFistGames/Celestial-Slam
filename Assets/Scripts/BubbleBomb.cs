using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleBomb : MonoBehaviour
{
    public int launchSize;

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.GetComponent<GravBody>().attractor = transform.GetComponentInParent<PlayerTracker>().targetTag;
        launchSize = GetComponentInParent<PlayerTracker>().attackCharge;
        if (GetComponentInParent<PlayerTracker>() != null && launchSize > 0)
        {
            transform.localScale = new Vector2(launchSize * 0.2f, launchSize * 0.2f);
        }
        if (launchSize <= 0)
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
