using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDetector : MonoBehaviour
{

    public CreatureEntity creature;

    private void OnTriggerStay(Collider other)
    {
        creature.Jump();
    }

}
