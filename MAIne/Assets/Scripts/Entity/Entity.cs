using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Movement Parameter")]
    public float speed;
    public float airSpeed;
    public float waterSpeed;
    public float jumpForce;
    public float counterForce;
    public float airCounterForce;
    public float waterCounterForce;

    [Header("Situations")]
    public bool isGrounded;
    public bool isSwimming;
    public int blockFall;
    protected int previousY;

    [Header("Health")]
    public int maxHealth;
    public int currentHealth;

    [Header("Combat")]
    public float knockBackForce;

    [HideInInspector]
    public Rigidbody rb;
    public Animator animator;
    [HideInInspector]
    public Vector2 movement;

    protected Vector2Int currentChunkPos;
    protected Vector2Int oldChunkPos;
    protected Chunk currentChunk;

    public void CounterMovement()
    {
        if (isSwimming)
        {
            rb.AddForce(-rb.velocity.x * waterCounterForce, 0, -rb.velocity.z * waterCounterForce);
            blockFall = 0;
        }
        else if (isGrounded)
            rb.AddForce(-rb.velocity.x * counterForce, 0, -rb.velocity.z * counterForce);
        else
            rb.AddForce(-rb.velocity.x * airCounterForce, 0, -rb.velocity.z * airCounterForce);
    }

}
