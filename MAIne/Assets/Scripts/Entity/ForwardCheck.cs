using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardCheck : MonoBehaviour
{
    public CreatureEntity creature;
    public LayerMask groundMask;

    private void OnTriggerExit(Collider other)
    {
        if (!Physics.Raycast(transform.position + Vector3.up * 1.1f, Vector3.down, 2.2f, groundMask) && !creature.isSwimming)
        {
            creature.StopMoving();
            DuckAI duck = creature.GetComponent<DuckAI>();
            if (duck != null)
            {
                creature.animator.SetBool("Peck", false);
            }
        }
    }
}
