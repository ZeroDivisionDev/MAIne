using System.Collections;
using UnityEngine;
using UnityEditor;

public enum BlockType { Air ,Grass, Dirt, Stone, Wood, Sand, Leaves, Spawner, Weed, Rose, Dandelion, Cornflower, Cactus, Deadbush, Water, Bedrock}
public enum TileTexture { Grass, Dirt, GrassSide, Stone, WoodSide, WoodTop, Sand, Leaves, Spawner, Water, CactusSide, CactusTop, CactusBottom, Bedrock }
public enum Side { Front, Left, Back, Right, Top, Bottom }

public class Chunk : MonoBehaviour
{

    public MeshFilter leavesMeshFilter;
    public MeshCollider leavesMeshCollider;
    public MeshFilter flowerMeshFilter;
    public MeshCollider flowerMeshCollider;
    public MeshFilter waterMeshFilter;
    public MeshCollider waterMeshCollider;
    public Transform ennemyContainer;
    public Transform creatureContainer;
    public GameObject zombiePrefab;
    public GameObject duckPrefab1;
    public GameObject duckPrefab2;
    public Transform cactusContainer;
    public GameObject cactusBox;
    public GameObject spawnerParticlesPrefab;

    [HideInInspector]
    public BlockType[] blockMap;
    public Vector3Int spawnerLocation;
    public Vector3Int spawnDuckLocation;


    public bool isGenerate = false;
    public bool isModified = false;
    public bool isLoadedFromSave = false;
    public Mesh foliageMesh;
    public Material foliageMat;

    public Material shaderMat;
    Coroutine co;
    bool isSpawning = false;
    ComputeBuffer meshTransformBuffer;
    Bounds bounds;
    Matrix4x4[] foliageTransform;
    GameObject spawnerParticles;

    public void StartSpawning()
    {
        if(isGenerate && spawnerLocation.magnitude != 0 && gameObject.activeSelf && !isSpawning)
        {
            isSpawning = true;
            co = StartCoroutine(SpawnZombie());
            if (spawnerParticles == null)
                spawnerParticles = Instantiate(spawnerParticlesPrefab, spawnerLocation + Vector3.one * 0.5f + transform.position, Quaternion.identity, transform);
        }
    }

    public void SpawnAnimals()
    {
        if(!isGenerate && spawnDuckLocation.magnitude != 0 && gameObject.activeSelf)
        {
            //Debug.Log("Spawning duck");
            int rSpawn = Random.Range(2, 6);
            for (int i = 0; i < rSpawn; i++)
            {
                Vector3 spawnPos = FindSpawnPosition(spawnDuckLocation, 11f);
                if (spawnPos != Vector3.zero)
                {
                    float r = Random.value;
                    if (r<.5f)
                        Instantiate(duckPrefab1, spawnPos, Quaternion.identity, creatureContainer);
                    else
                        Instantiate(duckPrefab2, spawnPos, Quaternion.identity, creatureContainer);
                }
            }
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        spawnerLocation = Vector3Int.zero;
        if (co != null)
            StopCoroutine(co);
        if (spawnerParticles != null)
            Destroy(spawnerParticles);
    }

    private void OnEnable()
    {
        StartSpawning();
        if (foliageTransform != null)
            CreateBuffer();
    }

    private void OnDisable()
    {
        isSpawning = false;
        if (co != null)
            StopCoroutine(co);
        
        if (meshTransformBuffer != null)
        {
            meshTransformBuffer.Dispose();
            meshTransformBuffer = null;
        }
    }

    IEnumerator SpawnZombie()
    {
        while (true)
        {
            if (ennemyContainer.childCount < 1) //If there is more than 1 ennemy on the chunk, we don't spaww more
            {
                int rSpawn = Random.Range(1, 3);
                for (int i = 0; i < rSpawn; i++)
                {
                    Vector3 spawnPos = FindSpawnPosition(spawnerLocation, 4f);
                    if (spawnPos != Vector3.zero)
                        Instantiate(zombiePrefab, spawnPos, Quaternion.identity, ennemyContainer);
                }
            }
            float r = Random.Range(12f, 48f);
            yield return new WaitForSeconds(r);
        }
    }

    Vector3 FindSpawnPosition(Vector3Int pos, float dist)
    {
        Vector3 spawnPos = Vector3.zero;
        Vector3 spawnerPos = pos + Vector3.one * 0.5f + transform.position;

        int i = 0;
        while (spawnPos == Vector3.zero && i<10)
        {
            Vector2 spawnDir = Random.insideUnitCircle.normalized * Random.Range(0f, dist);
            RaycastHit hitInfo;
            if(Physics.Raycast(spawnerPos + new Vector3(spawnDir.x,4f,spawnDir.y),Vector3.down,out hitInfo, 8f, LayerMask.GetMask("Block")))
            {
                if(!Physics.CheckBox(hitInfo.point+Vector3.up,new Vector3(0.3f,0.5f,0.3f),Quaternion.identity, LayerMask.GetMask("Block")))
                {
                    spawnPos = hitInfo.point;
                }
            }
            i++;
        }
        return spawnPos;
    }
        
    public void SetFoliage(Vector3[] pos, Vector3[] rot)
    {
        foliageTransform = null;
        if (meshTransformBuffer != null)
        {
            meshTransformBuffer.Dispose();
            meshTransformBuffer = null;
        }
        if (pos.Length == 0)
            return;
        shaderMat = new Material(foliageMat);
        bounds = new Bounds(transform.position + new Vector3(8, 50, 8), new Vector3(100, 100, 100));
        foliageTransform = new Matrix4x4[pos.Length];
        for (int i = 0; i < pos.Length; i++)
        {
            foliageTransform[i] = Matrix4x4.TRS(pos[i], Quaternion.Euler(rot[i]), Vector3.one);
        }
        CreateBuffer();
    }

    public void SetCactus(Vector3[] cactusPosition)
    {
        foreach (Transform child in cactusContainer)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < cactusPosition.Length; i++)
        {
            Instantiate(cactusBox, cactusPosition[i], Quaternion.identity, cactusContainer);
        }
    }

    void CreateBuffer()
    {
        meshTransformBuffer = new ComputeBuffer(foliageTransform.Length, sizeof(float) * 4 * 4);
        meshTransformBuffer.SetData(foliageTransform);
        shaderMat.SetBuffer("_Transform", meshTransformBuffer);
    }

    private void Update()
    {
        if (meshTransformBuffer == null)
            return;
        Graphics.DrawMeshInstancedProcedural(foliageMesh, 0, shaderMat, bounds, meshTransformBuffer.count);
    }

    public static bool IsFlowerOrWeed(BlockType blockType)
    {
        return blockType == BlockType.Weed || blockType == BlockType.Rose || blockType == BlockType.Dandelion || blockType == BlockType.Cornflower || blockType == BlockType.Deadbush;
    }

    public static bool CanBuildFlower(BlockType blockType)
    {
        return blockType == BlockType.Grass || blockType == BlockType.Dirt || blockType == BlockType.Sand;
    }

}
