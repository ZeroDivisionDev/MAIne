using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckAI : CreatureEntity
{
    private void OnEnable()
    {
        InfoOverlay.instance.nbEntity++;
        StartCoroutine(CheckWater());
        co = StartCoroutine(Move());
        StartCoroutine(IdleSound());
        StartCoroutine(Pecking());
    }

    void FixedUpdate()
    {
        CheckChunk();
        animator.SetBool("Move", isMoving);
        animator.SetBool("Aggro", isAggro);
        Vector3 playerDistance = PlayerController.instance.transform.position - transform.position;
        if(playerDistance.magnitude < 7f && isInvincible && !isAggro)
        {
            isAggro = true;
            isMoving = true;
            StartCoroutine(RunAway());
        }
        if (isMoving && !isDead)
        {
            if (isAggro)
            {
                direction = new Vector2(-playerDistance.x, -playerDistance.z).normalized;
                rotation = -Vector2.SignedAngle(Vector2.up, direction);
            }
            Movement();
        }
        else if (isDead)
        {
            armature.rotation = Quaternion.Lerp(armature.rotation, Quaternion.Euler(rotationFall, rotation, -90), 0.1f);
        }
        else if (!isMoving && isSwimming)
        {
            animator.SetBool("Peck", false);
            if (rb.velocity.y > 3f)
                rb.velocity = new Vector3(rb.velocity.x, 3f, rb.velocity.z);
            else
                rb.AddForce(Vector3.up * 18f);
        }
        CounterMovement();
    }

    IEnumerator RunAway()
    {
        speed += 40;
        waterSpeed += 10;
        yield return new WaitForSeconds(7f);
        speed -= 40;
        waterSpeed -= 10;
        isAggro = false;
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