using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Push : MonoBehaviour
{
    public Entity entity;

    private void OnTriggerStay(Collider other)
    {
        if (PlayerController.instance == null)
            return;
        if (PlayerController.instance.dieRotation == null)
            return;
        Vector3 dir1 = other.transform.position - transform.position;
        float mag = Mathf.Pow(dir1.x, 2) + Mathf.Pow(dir1.z, 2);
        if(!PlayerController.instance.dieRotation.enabled)
            other.GetComponent<Rigidbody>().AddForce(new Vector3(dir1.x,0,dir1.z).normalized  / (mag * 5 + 0.01f) * 10f,ForceMode.Acceleration);
        if (mag == 0)
        {
            other.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1f,1f),0, Random.Range(-1f, 1f))/5f, ForceMode.Acceleration);
        }
    }
}
