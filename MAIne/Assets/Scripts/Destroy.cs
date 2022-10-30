using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{

    public float destroyTime = 5f;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyObject", destroyTime);
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
