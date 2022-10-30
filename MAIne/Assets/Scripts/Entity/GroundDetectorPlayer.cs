using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetectorPlayer : MonoBehaviour
{

    public PlayerMovementV2 entity;
    bool isPlaying = false;

    //When the player is on the ground
    private void OnTriggerStay(Collider other)
    {
        //Play movement/fall sound
        if (entity.blockFall > 3 || !isPlaying && entity.movement.magnitude>0.1f)
        {
            RaycastHit hitInfo;
            //A few raycasts to find the block type where the player is
            if(Physics.Raycast(entity.transform.position + Vector3.up, Vector3.down, out hitInfo, 1.9f, LayerMask.GetMask("Block"))
               || Physics.Raycast(entity.transform.position + new Vector3(0.6f,1f,0.6f),Vector3.down,out hitInfo, 1.9f, LayerMask.GetMask("Block"))
               || Physics.Raycast(entity.transform.position + new Vector3(-0.6f, 1f, 0.6f), Vector3.down, out hitInfo, 1.9f, LayerMask.GetMask("Block"))
               || Physics.Raycast(entity.transform.position + new Vector3(-0.6f, 1f, -0.6f), Vector3.down, out hitInfo, 1.9f, LayerMask.GetMask("Block"))
               || Physics.Raycast(entity.transform.position + new Vector3(-0.6f, 1f, -0.6f), Vector3.down, out hitInfo, 1.9f, LayerMask.GetMask("Block")))
            {
                isPlaying = true;
                Vector2Int modifyChunkPos = new Vector2Int(Mathf.FloorToInt(hitInfo.point.x / TerrainGenerator.instance.chunkLenght), Mathf.FloorToInt(hitInfo.point.z / TerrainGenerator.instance.chunkLenght)) * TerrainGenerator.instance.chunkLenght;
                BlockType b = TerrainGenerator.instance.chunks[modifyChunkPos].blockMap[(int)(hitInfo.point.y - 0.5f) + ((int)(hitInfo.point.x - modifyChunkPos.x) + (int)(hitInfo.point.z - modifyChunkPos.y) * TerrainGenerator.instance.chunkLenght) * TerrainGenerator.instance.chunkHeight];
                if (entity.blockFall > 3)
                {
                    PlayerController.instance.Damage(Mathf.CeilToInt((entity.blockFall - 3) / 2f), Vector3.zero);
                    StartCoroutine(PlayWalkingSound(b , true));
                }
                else
                    StartCoroutine(PlayWalkingSound(b, false));
            }
            //box.size = new Vector3(box.size.x, 0.01f, box.size.z);
        }
        //Reset the fall and player is grounded
        entity.blockFall = 0;
        entity.isGrounded = true;
    }

    //Handle the sound to play
    IEnumerator PlayWalkingSound(BlockType b, bool fall)
    {
        if (b == BlockType.Dirt)
        {
            if (fall)
                AudioManager.instance.Play("DirtFall");
            else
                AudioManager.instance.Play("DirtWalk");
        }
        else if (b == BlockType.Grass || b == BlockType.Cactus)
        {
            if (fall)
                AudioManager.instance.Play("GrassFall");
            else
                AudioManager.instance.Play("GrassWalk");
        }
        else if (b == BlockType.Wood)
        {
            if (fall)
                AudioManager.instance.Play("WoodFall");
            else
                AudioManager.instance.Play("WoodWalk");
        }
        else if (b == BlockType.Sand)
        {
            if (fall)
                AudioManager.instance.Play("SandFall");
            else
                AudioManager.instance.Play("SandWalk");
        }
        else if (b == BlockType.Leaves)
        {
            if (fall)
                AudioManager.instance.Play("LeavesFall");
            else
                AudioManager.instance.Play("LeavesWalk");
        }
        else
        {
            if (fall)
                AudioManager.instance.Play("StoneFall");
            else
                AudioManager.instance.Play("StoneWalk");
        }
        yield return new WaitForSeconds(0.35f);
        isPlaying = false;
    }

    private void OnTriggerExit(Collider other)
    {
        entity.isGrounded = false;
    }

}
