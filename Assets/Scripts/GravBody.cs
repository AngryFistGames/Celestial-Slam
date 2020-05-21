using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravBody : MonoBehaviour
{
    public GravAttractor attractor;
    private Transform myTransform;
    public Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        myTransform = transform;
        attractor = GameObject.FindGameObjectWithTag("Floor").GetComponent<GravAttractor>();
    }

    // Update is called once per frame
    void Update()
    {
        attractor.Attract(myTransform);
    }
}
