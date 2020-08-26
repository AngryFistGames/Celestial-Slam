using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Destroy : MonoBehaviour
{
    void SelfDestruct()
    {
        Destroy(this.gameObject);
    }
}
