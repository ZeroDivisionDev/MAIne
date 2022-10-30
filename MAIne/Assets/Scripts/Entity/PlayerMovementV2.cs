using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MilkShake;

public class PlayerMovementV2 : Entity
{
    /*[SerializeField]
    public BoxCollider groundCheck;*/
    [SerializeField]
    public float waterUpForce;

    public GameObject waterParticles;

    Vector2 direction;
    Inputs inputs;
    PlayerController playerController;
    bool jumpButton;
    bool previousSwimming = false;
    bool canPlayWater = true;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        inputs = playerController.inputs;
        inputs.Player.Movement.performed += context => movement = context.ReadValue<Vector2>();
        //inputs.Player.Movement.canceled += context => movement = Vector2.zero;
        inputs.Player.Jump.performed += context => jumpButton = true;
        inputs.Player.Jump.canceled += context => jumpButton = false;
    }

    private void OnEnable()
    {
        previousY = (int)transform.position.y;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        oldChunkPos = Vector2Int.one;
    }

    void FixedUpdate()
    {
        CounterMovement();
        if (playerController.dieRotation.enabled)
            return;
        if (!movement.Equals(Vector2.zero))
        {
            float radians = -playerController.cam.transform.parent.localEulerAngles.y * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            direction = new Vector2(cos * movement.x - sin * movement.y, sin * movement.x + cos * movement.y);
            rb.position += Vector3.up * 0.0001f; // A weird workaround to a bug where the player's box collider is blocked by moving between two mesh colliders that are on the same level
        }
        else
        {
            direction = Vector2.zero;
        }
        if (isSwimming)
        {
            if (rb.velocity.y < -18f)
                rb.velocity = new Vector3(rb.velocity.x, -17f, rb.velocity.z);
            else if (rb.velocity.y > 3f)
                rb.velocity = new Vector3(rb.velocity.x, 3f, rb.velocity.z);
            if (rb.velocity.y < 0f)
                rb.AddForce(direction.x * waterSpeed, 9f - rb.velocity.y * 3f, direction.y * waterSpeed);
            else
                rb.AddForce(direction.x * waterSpeed, 9f, direction.y * waterSpeed);
        }
        else if (isGrounded)
            rb.AddForce(direction.x * speed, 0f, direction.y * speed);
        else
            rb.AddForce(direction.x * airSpeed, 0f, direction.y * airSpeed);

        animator.SetFloat("Velocity", movement.magnitude);


        currentChunkPos = new Vector2Int(Mathf.FloorToInt(transform.position.x / TerrainGenerator.instance.chunkLenght), Mathf.FloorToInt(transform.position.z / TerrainGenerator.instance.chunkLenght)) * TerrainGenerator.instance.chunkLenght;
        if (currentChunkPos != oldChunkPos && TerrainGenerator.instance.chunks.ContainsKey(currentChunkPos))
        {
            oldChunkPos = currentChunkPos;
            currentChunk = TerrainGenerator.instance.chunks[currentChunkPos];
        }
        if(currentChunk != null && transform.position.y < 128)
        {
            int x = (int)(transform.position.x - currentChunkPos.x);
            int y = (int)transform.position.y;
            int z = (int)(transform.position.z - currentChunkPos.y);
            int blockPos = y + (x + z * TerrainGenerator.instance.chunkLenght) * TerrainGenerator.instance.chunkHeight;
            isSwimming = currentChunk.blockMap[blockPos] == BlockType.Water || currentChunk.blockMap[blockPos + 1] == BlockType.Water;
            if(isSwimming && !previousSwimming) //Entering water
            {
                Instantiate(waterParticles, new Vector3(transform.position.x, y + 1, transform.position.z), Quaternion.identity);
                if (canPlayWater)
                {
                    canPlayWater = false;
                    StartCoroutine(PlayWaterSound(true));
                }
            }
            else if (!isSwimming && previousSwimming) //Leaving water
            {
                if (canPlayWater)
                {
                    canPlayWater = false;
                    StartCoroutine(PlayWaterSound(false));
                }
            }
            previousSwimming = isSwimming;
            blockFall += Mathf.Clamp(previousY - y, 0, 50);
            previousY = y;
        }
        if (jumpButton)
            Jump();

        if (rb.position.y < -50)
        {
            playerController.Damage(5, Vector2.zero);
        }
    }


    IEnumerator PlayWaterSound(bool isEntering)
    {
        if (isEntering)
        {
            if (blockFall > 3)
                AudioManager.instance.Play("WaterFall");
            else
                AudioManager.instance.Play("WaterEnter");
        }
        else
        {
            if (Random.value < 0.5f)
                AudioManager.instance.Play("WaterLeave1");
            else
                AudioManager.instance.Play("WaterLeave2");
        }
        yield return new WaitForSeconds(2f);
        canPlayWater = true;
    }

    void Jump()
    {
        if (isSwimming)
        {
            rb.AddForce(Vector3.up * waterUpForce,ForceMode.Force);
        }
        else if (isGrounded && Mathf.Abs(rb.velocity.y)<0.1f)
        {
            isGrounded = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

}
