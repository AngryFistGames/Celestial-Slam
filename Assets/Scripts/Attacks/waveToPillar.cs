using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waveToPillar : MonoBehaviour
{
    public GameObject pillar;
    public float advancing;
    string characterName = "Cancer";
  
    public void OnTriggerEnter2D(Collider2D collision)
    {
      if (!collision.CompareTag(GetComponentInParent<PlayerTracker>().targetTag.gameObject.tag))
        {
            if ((collision.gameObject.tag == "Player" && (!collision.gameObject.GetComponent<PlayerScript>().dodging) && (collision.gameObject.name != characterName + GetComponentInParent<PlayerTracker>().playerNumber)))
            {
                if (collision.GetComponent<PlayerScript>().isBlocking == false)
                {
                    switch (GetComponentInParent<PlayerTracker>().targetTag.gameObject.tag)
                    {
                        case "Floor":
                            Instantiate(pillar, new Vector2(transform.position.x + advancing, transform.position.y - 0.6f), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                            break;
                        case "Cieling":
                            Instantiate(pillar, new Vector2(transform.position.x - advancing, transform.position.y + 0.6f), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                            break;
                        case "Left":
                            Instantiate(pillar, new Vector2(transform.position.x - 0.6f, transform.position.y - advancing), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                            break;
                        case "Right":
                            Instantiate(pillar, new Vector2(transform.position.x + 0.6f, transform.position.y + advancing), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                            break;

                        default:
                            Instantiate(pillar, new Vector2(transform.position.x, transform.position.y - 0.6f), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                            break;
                    }
                            Destroy(this.gameObject);
                }
            }
            else
            {
                switch (GetComponentInParent<PlayerTracker>().targetTag.gameObject.tag)
                {
                    case "Floor":
                        Instantiate(pillar, new Vector2(transform.position.x, transform.position.y - 0.6f), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        break;
                    case "Cieling":
                        Instantiate(pillar, new Vector2(transform.position.x, transform.position.y + 0.6f), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        break;
                    case "Left":
                        Instantiate(pillar, new Vector2(transform.position.x - 0.6f, transform.position.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        break;
                    case "Right":
                        Instantiate(pillar, new Vector2(transform.position.x + 0.6f, transform.position.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        break;

                    default:
                        Instantiate(pillar, new Vector2(transform.position.x, transform.position.y - 0.6f), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        break;
                }
                Destroy(this.gameObject);
            }
        } 
    }
}
