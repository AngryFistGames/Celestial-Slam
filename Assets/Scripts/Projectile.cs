using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Projectiles ammo;
    public Animator anim;
    public Vector2 direction;
    public Vector2 _direction;
    public bool rikochet;
    public Vector2 bounceAngle;
    float timer = 0f;
    public float shotRange; //how long the projectile is active in the scene
    [SerializeField]
    string characterName;
    public Vector2 launchingPoint;
    public Rigidbody2D rb;
    [SerializeField]
    BoxCollider2D bc;
    public bool rightFace;
    string floor;
    public float directionSpeed;

    private void Awake()
    {
        launchingPoint = ammo.launchPoint;
    }
    void OnEnable()
    {
        bc = GetComponent<BoxCollider2D>();
        rightFace = GetComponentInParent<PlayerTracker>().faceRight;
        StartCoroutine(EnableCollider());
        direction = ammo.trajectory;
        rikochet = ammo.doesRikochet;
        if (rikochet)
        {
            bounceAngle = ammo.rikochetAngle;
        }
        floor = transform.GetComponentInParent<PlayerTracker>().targetTag.gameObject.tag;
    }
    void Update()
    {
        if (rightFace)
        {
            switch (floor)
            {
                case "Floor":
                    _direction = new Vector2(direction.x * Time.deltaTime * directionSpeed, direction.y * Time.deltaTime * directionSpeed);
                    break;
                case "Cieling":
                    _direction = new Vector2(-direction.x * Time.deltaTime * directionSpeed, -direction.y * Time.deltaTime * directionSpeed);
                    break;
                case "Left":
                    _direction = new Vector2(-direction.y * Time.deltaTime * directionSpeed, -direction.x * Time.deltaTime * directionSpeed);
                    break;
                case "Right":
                    _direction = new Vector2(direction.y * Time.deltaTime * directionSpeed, direction.x * Time.deltaTime * directionSpeed);
                    break;
                default:
                    _direction = direction * Time.deltaTime;
                    break;
            }
        }
            if (!rightFace)
            {
                switch (floor)
                {
                    case "Floor":
                        _direction = new Vector2(-direction.x * Time.deltaTime * directionSpeed, direction.y * Time.deltaTime * directionSpeed);
                        break;
                    case "Cieling":
                        _direction = new Vector2(direction.x * Time.deltaTime * directionSpeed, -direction.y * Time.deltaTime * directionSpeed);
                        break;
                    case "Left":
                    _direction = new Vector2(direction.y * Time.deltaTime * directionSpeed, direction.x * Time.deltaTime * directionSpeed);
                        break;
                    case "Right":
                    _direction = new Vector2(-direction.y * Time.deltaTime * directionSpeed, -direction.x * Time.deltaTime * directionSpeed);
                        break;
                    default:
                    _direction = Vector2.up  * Time.deltaTime;
                        break;
                }
            }
        rb.velocity = _direction;
        timer += Time.deltaTime;
        if (timer >= shotRange)
        {
            if (anim != null)
            {
                anim.SetTrigger("done");
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }


            public void OnCollisionEnter2D(Collision2D collision)
            {
                if ((collision.gameObject.tag == "Floor" || collision.gameObject.tag == "Cieling" || collision.gameObject.tag == "Left" || collision.gameObject.tag == "Right") && (!collision.gameObject.CompareTag(floor)) )
                {
                    if (rikochet)
                    {
                floor = collision.gameObject.tag;
                //Vector2 wallContact = collision.contacts[0].normal;
                //direction = Vector2.Reflect(bounceAngle, wallContact).normalized * Time.deltaTime;
                //_direction = direction * directionSpeed;
                    }
                    if (!rikochet)
                    {
                        anim.SetTrigger("done");
                    }
                }
                if (collision.gameObject.tag == "Player" && (!collision.gameObject.GetComponent<PlayerControl>().dodging) && (collision.gameObject.GetComponent<PlayerControl>().GetInstanceID() != GetComponentInParent<PlayerTracker>().gameObject.GetComponentInChildren<PlayerControl>(false).GetInstanceID()))
                {
                    anim.SetTrigger("done");
                }
            }
            IEnumerator EnableCollider()
            {
                yield return new WaitForSeconds(0.4f);
                bc.enabled = true; //enable the collider
            }

            void RangeOut()
            {
                Destroy(this.gameObject);
            }
        
    }