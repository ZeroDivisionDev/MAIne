using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureEntity : Entity
{
    public Transform armature;
    public Transform groundCheck;
    public LayerMask groundMask;
    public Material hitMat;
    public SkinnedMeshRenderer skin;
    public Transform head;
    public GameObject dieParticles;
    public GameObject waterParticles;
    public Vector2 idleSoundInterval;
    public Material skinMat;

    protected AudioEmitter audioEmitter;
    protected int health;
    protected bool isMoving = false;
    protected bool isAggro = false;
    protected bool isInvincible = false;
    protected bool isDead = false;
    protected Vector2 direction;
    protected float rotation;
    protected float rotationFall;
    protected Coroutine co;
    protected bool previousSwimming = false;

    void Awake()
    {
        audioEmitter = GetComponent<AudioEmitter>();
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        oldChunkPos = Vector2Int.one;
        previousY = (int)transform.position.y;
    }

    private void OnEnable()
    {
        InfoOverlay.instance.nbEntity++;
        StartCoroutine(CheckWater());
        co = StartCoroutine(Move());
        StartCoroutine(IdleSound());
    }

    private void OnDisable()
    {
        InfoOverlay.instance.nbEntity--;
        isMoving = false;
    }

    protected void CheckChunk()
    {
        currentChunkPos = new Vector2Int(Mathf.FloorToInt(transform.position.x / TerrainGenerator.instance.chunkLenght), Mathf.FloorToInt(transform.position.z / TerrainGenerator.instance.chunkLenght)) * TerrainGenerator.instance.chunkLenght;
        if (!oldChunkPos.Equals(currentChunkPos))
        {
            oldChunkPos = currentChunkPos;
            if (TerrainGenerator.instance.chunks.ContainsKey(currentChunkPos))
            {
                currentChunk = TerrainGenerator.instance.chunks[currentChunkPos];
                transform.SetParent(currentChunk.creatureContainer);
            }
            else
                Destroy(gameObject);
        }
    }

    protected void Movement()
    {
        if (isSwimming)
        {
            if (rb.velocity.y < -18f)
                rb.velocity = new Vector3(rb.velocity.x, -17f, rb.velocity.z);
            else if (rb.velocity.y > 3f)
                rb.velocity = new Vector3(rb.velocity.x, 3f, rb.velocity.z);
            if (rb.velocity.y < 3f)
                rb.AddForce(direction.x * waterSpeed, 18f, direction.y * waterSpeed);
        }
        else if (isGrounded)
            rb.AddForce(direction.x * speed, 0f, direction.y * speed);
        else
            rb.AddForce(direction.x * airSpeed, 0f, direction.y * airSpeed);
        float rotationDir = Mathf.MoveTowardsAngle(armature.rotation.eulerAngles.y, rotation, 7f);
        armature.rotation = Quaternion.Euler(0, rotationDir, 0);
    }

    protected IEnumerator IdleSound()
    {
        while (true)
        {
            float rTime = Random.Range(idleSoundInterval.x, idleSoundInterval.y);
            yield return new WaitForSeconds(rTime);
            int r = Random.Range(1, 3);
            if (!isDead)
                audioEmitter.Play("Idle" + r);
        }
    }

    protected IEnumerator Move()
    {
        while (true)
        {
            float waitTime = Random.Range(1f, 5f);
            yield return new WaitForSeconds(waitTime);
            int i = 0;
            do
            {
                i++;
                direction = Random.insideUnitCircle.normalized;
            } while ((CheckForward() || CheckUp() || CheckFall()) && i < 6);
            //Debug.Log(direction.x + " " + direction.y);
            if (i < 6)
            {
                rotation = -Vector2.SignedAngle(Vector2.up, direction);
                isMoving = true;
            }
            float moveTime = Random.Range(4f, 10f);
            yield return new WaitForSeconds(moveTime);
            StopMoving();
        }
    }

    protected IEnumerator CheckWater()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.06f);
            int x = (int)(transform.position.x - currentChunkPos.x);
            int y = (int)transform.position.y;
            int z = (int)(transform.position.z - currentChunkPos.y);
            int blockPos = y + (x + z * TerrainGenerator.instance.chunkLenght) * TerrainGenerator.instance.chunkHeight;
            if (blockPos > 0 && blockPos < currentChunk.blockMap.Length - 1)
                isSwimming = currentChunk.blockMap[blockPos] == BlockType.Water || currentChunk.blockMap[blockPos + 1] == BlockType.Water;
            if(isSwimming && !previousSwimming && (transform.position - PlayerController.instance.transform.position).magnitude < 30f) //Entering
                Instantiate(waterParticles, new Vector3(transform.position.x, y + 1, transform.position.z), Quaternion.identity);
            previousSwimming = isSwimming;
            blockFall += Mathf.Clamp(previousY - y, 0, 50);
            previousY = y;
        }
    }

    protected bool CheckForward()
    {
        //Debug.DrawLine(transform.position + Vector3.up * 1.9f, transform.position + Vector3.up * 1.9f + new Vector3(direction.x, 0, direction.y), Color.blue, 2f);
        return Physics.Raycast(transform.position + Vector3.up * 1.9f, new Vector3(direction.x, 0, direction.y), 1f, groundMask);
    }

    protected bool CheckUp()
    {
        //return Physics.Raycast(transform.position + Vector3.up * 1.9f, transform.up, 0.5f, groundMask) && Physics.Raycast(transform.position + Vector3.up * 0.5f, new Vector3(direction.x, 0, direction.y), 1f, groundMask);
        return Physics.Raycast(head.position, transform.up, 0.5f, groundMask) && Physics.Raycast(transform.position + Vector3.up * 0.5f, new Vector3(direction.x, 0, direction.y), 1f, groundMask);
    }

    //Return true if there is a gap in the given direction
    protected bool CheckFall()
    {
        //Debug.DrawLine(transform.position + new Vector3(direction.x, 0.1f, direction.y), transform.position + new Vector3(direction.x, 0.1f, direction.y) + Vector3.down * 1.5f, Color.blue, 2f);
        return !Physics.Raycast(transform.position + new Vector3(direction.x / 3, 0.1f, direction.y / 3), Vector3.down, 1.8f, groundMask) && !isSwimming;
    }

    public void StopMoving()
    {
        if (!isAggro)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            isMoving = false;
        }
    }

    public void Jump()
    {
        if (!isSwimming && isGrounded && isMoving && Mathf.Abs(rb.velocity.y) < 0.03f && !isDead)
        {
            if (!isAggro && (CheckForward() || CheckUp()))
            {
                StopMoving();
            }
            else
            {
                isGrounded = false;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    public void Damage(int d, Vector3 dir)
    {
        if (!isInvincible)
        {
            isInvincible = true;
            audioEmitter.Play("Damage");
            health = Mathf.Clamp(health - d, 0, maxHealth);
            if (isGrounded && Mathf.Abs(rb.velocity.y) < 0.03f)
                rb.AddForce(new Vector3(dir.x, 4f + dir.y, dir.z), ForceMode.Impulse);
            else
                rb.AddForce(new Vector3(dir.x, 1f + dir.y, dir.z), ForceMode.Impulse);
            if (health == 0)
            {
                isDead = true;
                isMoving = false;
                animator.speed = 0f;
                rotation = armature.rotation.eulerAngles.y + 90;
                int r = (int)Mathf.Sign(Random.value - 0.5f);
                rotationFall = -90 + 90 * r;
            }
            StartCoroutine(TakeDamage());
        }
    }

    IEnumerator TakeDamage()
    {
        skin.materials = new Material[] { skinMat, hitMat };
        yield return new WaitForSeconds(0.25f);
        if (!isDead)
        {
            skin.materials = new Material[] { skinMat };
            isInvincible = false;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            if (MainGameManager.instance.gamemode != MainGameManager.Gamemode.Immortal)
                PlayerController.instance.AddItem(PlayerController.ItemID.Steak, Random.Range(1, 3));
            Instantiate(dieParticles, transform.position, Quaternion.identity);
            Instantiate(dieParticles, head.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
