using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckMenu : CreatureEntity
{
    private void OnEnable()
    {
        co = StartCoroutine(Move());
        StartCoroutine(IdleSound());
        StartCoroutine(Pecking());
    }

    private void OnDisable()
    {
        isMoving = false;
    }

    void FixedUpdate()
    {
        animator.SetBool("Move", isMoving);
      
        if (isMoving && !isDead)
        {
            Movement();
        }
        else if (isDead)
        {
            armature.rotation = Quaternion.Lerp(armature.rotation, Quaternion.Euler(rotationFall, rotation, -90), 0.1f);
        }
        CounterMovement();
    }

    IEnumerator Pecking()
    {
        while (true)
        {
            animator.SetBool("Peck", true);
            float waitTime1 = Random.Range(2f, 6f);
            yield return new WaitForSeconds(waitTime1);
            animator.SetBool("Peck", false);
            float waitTime2 = Random.Range(8f, 16f);
            yield return new WaitForSeconds(waitTime2);
        }
    }
}
