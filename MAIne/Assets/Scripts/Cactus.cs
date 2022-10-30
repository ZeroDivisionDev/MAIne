using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        ZombieAI zombie = other.GetComponent<ZombieAI>();
        if (player != null)
        {
            player.Damage(1, Vector2.zero);
        } 
        else if(zombie != null)
        {
            zombie.Damage(1, Vector3.down * 0.5f);
        }
    }
}
