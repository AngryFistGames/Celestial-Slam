using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    public PlayerScript player;
    public float GuardMeter;
    public List<GameObject> ShieldStars;

    // Start is called before the first frame update
    private void Awake()
    {
        player = GetComponentInParent<PlayerScript>();
        foreach (Transform child in this.gameObject.transform)
        {
            ShieldStars.Add(child.gameObject);
        }
    }

    void OnEnable()
    {
        GuardMeter = player.Guard;
        foreach (GameObject s in ShieldStars)
        {
            s.SetActive(true);
        }
    }
    private void Update()
    {
        GuardMeter = player.Guard;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, 0, 20));
        if (GuardMeter <= 80)
        {
            ShieldStars[0].SetActive(false);
        }
        if (GuardMeter <= 60)
        {
            ShieldStars[1].SetActive(false);
        }
        if (GuardMeter <= 40)
        {
            ShieldStars[2].SetActive(false);
        }
        if (GuardMeter <= 20)
        {
            ShieldStars[3].SetActive(false);
        }
        if (GuardMeter <= 0)
        {
            ShieldStars[4].SetActive(false);
            player.GuardBreak();
          }
    }
}
