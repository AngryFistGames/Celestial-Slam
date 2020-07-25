using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Projectiles ammo;
    public Vector2 direction;
    public bool rikochet;
    public Vector2 bounceAngle;
    float timer = 0f;
    public float shotRange; //how long the projectile is active in the scene
    [SerializeField]
    string characterName;
    public float directionSpeed;

    [SerializeField]
    BoxCollider2D bc;


    void OnEnable()
    {
        StartCoroutine(EnableCollider());
        direction = ammo.trajectory;
        rikochet = ammo.doesRikochet;
        if (rikochet)
        {
            bounceAngle = ammo.rikochetAngle;
        }
    }
    void Update()
    {
        if (direction == Vector2.right) //if the projectile goes right
        {
            
        }
        if (direction == Vector2.left) //if the projectile goes left
        {

        }
        if (direction == Vector2.up) //if the projectile goes up
        {

        }
        if (direction == Vector2.down) //if the projectile goes down
        {

        }

        timer += Time.deltaTime;
        if (timer >= shotRange)
        {
            Destroy(this.gameObject);
        }
    }


    IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(0.2f);
            bc.enabled = true; //enable the collider
    }

    void RangeOut()
    {
        Destroy(this.gameObject);
    }
}
