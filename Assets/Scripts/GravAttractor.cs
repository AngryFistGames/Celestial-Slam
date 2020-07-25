using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravAttractor : MonoBehaviour
{
    Vector3 rot;
    public void Attract(Transform body)
    {
        Vector3 GravityUp;
        Vector3 bodyUp = body.up;
        switch (tag)
        {
            case "Floor":
               GravityUp = new Vector3(0, body.position.y - transform.position.y, 0);
                rot = Vector3.up;
                break;
            case "Cieling":
                GravityUp = new Vector3(0, body.position.y - transform.position.y, 0);
                rot = Vector3.down;
                break;
            case "Left":
                GravityUp = new Vector3(body.position.x - transform.position.x, 0, 0);
                rot = Vector3.right;
                break;
            case "Right":
                GravityUp = new Vector3(body.position.x - transform.position.x, 0, 0);
                rot = Vector3.left;
                break;
            default:
                GravityUp = new Vector3(0, body.position.y - transform.position.y, 0);
                rot = Vector3.up;
                break;
        }
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, rot) * body.rotation;
        body.GetComponent<Rigidbody2D>().AddForce(GravityUp * body.GetComponent<GravBody>().gravity); //general gravity
        body.rotation = Quaternion.Lerp(body.rotation, targetRotation, 50 * Time.fixedDeltaTime); //rotate to gravity
    }

}
