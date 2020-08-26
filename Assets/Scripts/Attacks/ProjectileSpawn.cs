using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawn : MonoBehaviour
{
    public List<GameObject> projectiles;
    public bool faceRight;

    // Start is called before the first frame update
    void Start()
    {
        faceRight = GetComponentInParent<PlayerTracker>().faceRight;
        foreach (GameObject i in projectiles)
        {
            switch (GetComponentInParent<PlayerTracker>().targetTag.gameObject.tag)
            {
                case "Floor":
                    {
                        if (faceRight)
                        {
                            Instantiate<GameObject>(i, new Vector2(transform.position.x + i.gameObject.GetComponent<Projectile>().launchingPoint.x, transform.position.y + i.gameObject.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                        if (!faceRight)
                        {
                            Instantiate<GameObject>(i, new Vector2(transform.position.x - i.gameObject.GetComponent<Projectile>().launchingPoint.x, transform.position.y + i.gameObject.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                    }
                    break;
                case "Cieling":
                    {
                        if (faceRight)
                        {
                            Instantiate<GameObject>(i, new Vector2(transform.position.x - i.gameObject.GetComponent<Projectile>().launchingPoint.x, transform.position.y - i.gameObject.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                        if (!faceRight)
                        {
                            Instantiate<GameObject>(i, new Vector2(transform.position.x + i.gameObject.GetComponent<Projectile>().launchingPoint.x, transform.position.y - i.gameObject.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                    }
                    break;
                case "Right":
                    {
                        if (faceRight)
                        {
                            Instantiate<GameObject>(i, new Vector2(transform.position.x + i.gameObject.GetComponent<Projectile>().launchingPoint.y, transform.position.y + i.gameObject.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                        if (!faceRight)
                        {
                            Instantiate<GameObject>(i, new Vector2(transform.position.x - i.gameObject.GetComponent<Projectile>().launchingPoint.y, transform.position.y - i.gameObject.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                    }
                    break;
                case "Left":
                    {
                        if (faceRight)
                        {
                            Instantiate<GameObject>(i, new Vector2(transform.position.x - i.gameObject.GetComponent<Projectile>().launchingPoint.y, transform.position.y - i.gameObject.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                        if (!faceRight)
                        {
                            Instantiate<GameObject>(i, new Vector2(transform.position.x + i.gameObject.GetComponent<Projectile>().launchingPoint.y, transform.position.y + i.gameObject.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                    }
                    break;
            }
        }
        Destroy(this, .01f);
    }
}
