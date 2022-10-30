using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : CreatureEntity
{
    public LayerMask playerMask;
    public GameObject spawnParticles;
    bool isAttacking = false;

    void Awake()
    {
        InfoOverlay.instance.nbEntity++;
        audioEmitter = GetComponent<AudioEmitter>();
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        oldChunkPos = Vector2Int.one;
        previousY = (int)transform.position.y;
        Instantiate(spawnParticles, transform.position, Quaternion.identity);
    }

    private void OnDestroy()
    {
        if(PlayerController.instance != null && (PlayerController.instance.transform.position-transform.position).magnitude < 50f)
        {
            LevelManager.instance.zombieKilled++;
        }
    }

    void FixedUpdate()
    {
        CheckChunk();
        animator.SetBool("Move", isMoving);
        animator.SetBool("Aggro", isAggro);
        Vector3 playerDistance = PlayerController.instance.transform.position - transform.position;
        if(!isAggro && playerDistance.magnitude< 10f && !PlayerController.instance.dieRotation.enabled && MainGameManager.instance.gamemode != MainGameManager.Gamemode.Immortal)
        {
            StopCoroutine(co);
            isAggro = true;
            isMoving = true;
        } 
        else if(isAggro && (playerDistance.magnitude > 21f || PlayerController.instance.dieRotation.enabled))
        {
            isAggro = false;
            co = StartCoroutine(Move());
            isMoving = false;
            head.transform.localRotation = Quaternion.identity;
        }
        if(isMoving && !isDead)
        {
            if (isAggro)
            {
                direction = new Vector2(playerDistance.x, playerDistance.z).normalized;
                rotation = -Vector2.SignedAngle(Vector2.up, direction);
                /*
                Quaternion rot = Quaternion.FromToRotation(head.forward, PlayerController.instance.headFollow.position - head.position);
                head.localRotation = Quaternion.Lerp(head.localRotation, rot, 0.2f);
                */
                Quaternion rot = Quaternion.LookRotation(PlayerController.instance.transform.position - transform.position);
                head.rotation = Quaternion.Slerp(head.rotation, rot, Time.deltaTime);
                if (!isAttacking && playerDistance.magnitude < 5f)
                {
                    RaycastHit info;
                    if(Physics.Raycast(transform.position + Vector3.up, playerDistance,out info,3.8f,playerMask))
                    {
                        if(info.collider.name == "Player")
                        {
                            isAttacking = true;
                            StartCoroutine(Attack());
                        }
                    }
                        
                }
            }
            Movement();
        }
        else if (isDead)
        {
            armature.rotation = Quaternion.Lerp(armature.rotation, Quaternion.Euler(rotationFall, rotation, -90),0.1f);
        }
        else if (!isMoving && isSwimming)
        {
            if (rb.velocity.y > 3f)
                rb.velocity = new Vector3(rb.velocity.x, 3f, rb.velocity.z);
            else
                rb.AddForce(Vector3.up * 18f);
        }
        CounterMovement();
    }

    IEnumerator Attack()
    {
        animator.SetTrigger("Attack");
        PlayerController.instance.Damage(1 + Random.Range(0, 3),direction);
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    protected new void CheckChunk()
    {
        currentChunkPos = new Vector2Int(Mathf.FloorToInt(transform.position.x / TerrainGenerator.instance.chunkLenght), Mathf.FloorToInt(transform.position.z / TerrainGenerator.instance.chunkLenght)) * TerrainGenerator.instance.chunkLenght;
        if (!oldChunkPos.Equals(currentChunkPos))
        {
            oldChunkPos = currentChunkPos;
            if (TerrainGenerator.instance.chunks.ContainsKey(currentChunkPos))
            {
                currentChunk = TerrainGenerator.instance.chunks[currentChunkPos];
                transform.SetParent(currentChunk.ennemyContainer);
            }
            else
                Destroy(gameObject);
        }
    }

}
