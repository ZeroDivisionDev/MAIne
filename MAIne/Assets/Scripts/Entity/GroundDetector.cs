using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{

    public CreatureEntity entity;

    private void OnTriggerStay(Collider other)
    {
        entity.isGrounded = true;
        if (entity.blockFall > 3)
            entity.Damage(Mathf.CeilToInt((entity.blockFall - 3) / 2), Vector3.zero);
        entity.blockFall = 0;
    }

    private void OnTriggerExit(Collider other)
    {
        entity.isGrounded = false;
    }

}
