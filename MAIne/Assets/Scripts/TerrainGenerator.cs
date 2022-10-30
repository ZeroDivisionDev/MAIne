using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine.Profiling;

public class TerrainGenerator : MonoBehaviour
{

    public enum BiomeType { Grassland, Desert}

    public static TerrainGenerator instance;

    [Header("Chunk Parameters")]
    public int chunkLenght = 16;
    public int chunkHeight = 100;
    public int baseLevel = 50;
    public int waterLevel = 60;

    [Header("Other parmaeters")]
    public int renderDistance = 5;

    public Transform player;
    public GameObject chunk;
    public GameObject blockParticles;

    public int maxGen = 64;
    public float waitTime = 0.0001f;
    public bool needRegenerate = false;

    [HideInInspector]
    public Dictionary<Vector2Int, Chunk> chunks;
    //Dictionary<Vector2Int, Chunk> toGenerate;

    float offsetX;
    float offsetZ;

    Vector2Int currentChunkPos;
    Vector2Int oldChunkPos;

    Dictionary<Vector2Int, Chunk> toGenerate;
    bool isGenerate = false;
    List<TileTexture> allTileTextures;

    private void Awake()
    {
        renderDistance = MainGameManager.instance.settings.renderDistance;
        UnityEngine.Random.InitState(MainGameManager.instance.seed + 69);
        instance = this;
        chunks = new Dictionary<Vector2Int, Chunk>();
        //Generate offset for noise
        offsetX = UnityEngine.Random.Range(0f, 9999f);
        offsetZ = UnityEngine.Random.Range(0f, 9999f);
        allTileTextures = InitializeTileTextures();
    }

    void Start()
    {
        //Application.targetFrameRate = 60;
        //QualitySettings.vSyncCount = 0;
        InfoOverlay.instance.gameObject.SetActive(false);
        //Initialize variables
        oldChunkPos = new Vector2Int(99, 99);
        currentChunkPos = new Vector2Int(0, 0);
        toGenerate = new Dictionary<Vector2Int, Chunk>();
        LoadTerrain();
        //Build first chunk
        //AddChunk(0, 0);
    }

    void FixedUpdate()
    {
        //Keep trace of player position and generate/enable or Destroy/disable chunks according to the render distance
        currentChunkPos = new Vector2Int(Mathf.FloorToInt(player.position.x / chunkLenght), Mathf.FloorToInt(player.position.z / chunkLenght)) * chunkLenght;

        if ((!oldChunkPos.Equals(currentChunkPos) && toGenerate.Count == 0) || needRegenerate) // Player has moved to another chunk
        {
            needRegenerate = false;
            //Debug.Log("Changing chunk x" + xPos + "  z " + zPos);
            oldChunkPos = currentChunkPos;
            int posZ = currentChunkPos.y - chunkLenght * renderDistance;
            for (int z = 0; z < renderDistance * 2 + 1; z++)
            {
                int posX = currentChunkPos.x - chunkLenght * renderDistance;
                for (int x = 0; x < renderDistance * 2 + 1; x++)
                {
                    Vector2Int pos = new Vector2Int(posX, posZ);
                    if (!chunks.ContainsKey(pos) && !toGenerate.ContainsKey(pos))
                    {
                        //Debug.Log("Adding X " + posX + "    Z " + posZ);
                        AddChunk(posX, posZ);
                    }
                    else if (chunks.ContainsKey(pos))
                    {
                        if (chunks[pos].isLoadedFromSave)
                        {
                            chunks[pos].isLoadedFromSave = false;
                            toGenerate.Add(pos, chunks[pos]);
                        }
                        chunks[pos].gameObject.SetActive(true);
                        if(LevelManager.instance.isLoading)
                            LevelManager.instance.UpadteBar(1);
                    }
                    posX += chunkLenght;
                }
                posZ += chunkLenght;
            }

            List<Vector2Int> toDestroy = new List<Vector2Int>();
            int nbChunk = 0;
            foreach (KeyValuePair<Vector2Int, Chunk> ch in chunks)
            {
                if (!ch.Value.isModified && (Mathf.Abs(ch.Key.x - player.position.x) > chunkLenght * (renderDistance + 6) || Mathf.Abs(ch.Key.y - player.position.z) > chunkLenght * (renderDistance + 6)))
                {
                    Destroy(ch.Value.gameObject);
                    toDestroy.Add(ch.Key);
                }
                else if (Mathf.Abs(ch.Key.x - player.position.x) > chunkLenght * (renderDistance + 3) || Mathf.Abs(ch.Key.y - player.position.z) > chunkLenght * (renderDistance + 3))
                {
                    ch.Value.gameObject.SetActive(false);
                }
                if (ch.Value.gameObject.activeSelf)
                    nbChunk++;
            }
            InfoOverlay.instance.nbChunk = nbChunk;
            foreach(Vector2Int v in toDestroy)
            {
                chunks.Remove(v);
            }
        }
        if (!isGenerate && toGenerate.Count > 0)
        {
            isGenerate = true;
            StartCoroutine(Generate());
        }
    }

    public void SetRenderDistance(int r)
    {
        renderDistance = r;
        oldChunkPos = Vector2Int.one;
    }

    void AddChunk(int posX, int posZ)
    {
        GameObject c = Instantiate(chunk, new Vector3(posX, 0, posZ), Quaternion.identity);
        c.transform.SetParent(gameObject.transform);
        Chunk ch = c.GetComponent<Chunk>();
        chunks.Add(new Vector2Int(posX, posZ), ch);
        toGenerate.Add(new Vector2Int(posX, posZ),ch);
    }

    public BlockType AddBlock(Vector3 hitPosition, BlockType blockType)
    {
        BlockType currentBlock;
        Vector2Int modifyChunkPos = new Vector2Int(Mathf.FloorToInt(hitPosition.x / chunkLenght), Mathf.FloorToInt(hitPosition.z / chunkLenght)) * chunkLenght;
        Chunk ch = chunks[modifyChunkPos];
        
        if (blockType == BlockType.Air || Chunk.IsFlowerOrWeed(blockType) || !Physics.CheckBox(new Vector3(Mathf.Floor(hitPosition.x) + 0.5f, Mathf.Floor(hitPosition.y) + 0.5f, Mathf.Floor(hitPosition.z) + 0.5f), new Vector3(0.49f,0.49f,0.49f), Quaternion.identity, LayerMask.GetMask("Player","Entity")))
        {
            //Debug.Log("Building block");
            int x = (int)(hitPosition.x - modifyChunkPos.x);
            int y = (int)hitPosition.y;
            int z = (int)(hitPosition.z - modifyChunkPos.y);
            currentBlock = ch.blockMap[y + (x + z * chunkLenght) * chunkHeight];
            if (currentBlock == BlockType.Bedrock)
                return BlockType.Bedrock;
            else if (Chunk.IsFlowerOrWeed(blockType) && !Chunk.CanBuildFlower(ch.blockMap[y - 1 + (x + z * chunkLenght) * chunkHeight]))
                return BlockType.Bedrock;
            ch.blockMap[y + (x + z * chunkLenght) * chunkHeight] = blockType;
            ch.isModified = true;
            if (currentBlock != BlockType.Air && currentBlock != BlockType.Water)
            {
                ParticleSystemRenderer par = Instantiate(blockParticles, new Vector3(modifyChunkPos.x + x + 0.5f, y + 0.8f, modifyChunkPos.y + z + 0.5f), Quaternion.identity).GetComponent<ParticleSystemRenderer>();
                par.sharedMaterial = MainGameManager.instance.particlesMat[(int)currentBlock-1];
                PlayBlockDestroy(currentBlock);
                BlockType upBlock = ch.blockMap[y + 1 + (x + z * chunkLenght) * chunkHeight];
                if (Chunk.IsFlowerOrWeed(upBlock))
                {
                    ch.blockMap[y + 1 + (x + z * chunkLenght) * chunkHeight] = BlockType.Air;
                    ParticleSystemRenderer parFlower = Instantiate(blockParticles, new Vector3(modifyChunkPos.x + x + 0.5f, y + 1.8f, modifyChunkPos.y + z + 0.5f), Quaternion.identity).GetComponent<ParticleSystemRenderer>();
                    parFlower.sharedMaterial = MainGameManager.instance.particlesMat[(int)upBlock-1];
                    PlayBlockDestroy(upBlock);
                }

            }
            if(blockType != BlockType.Air)
            {
                PlayBlockPlacement(blockType);
            }
            if (!toGenerate.ContainsKey(modifyChunkPos))
            {
                GenerateChunk(x, z, modifyChunkPos);
                toGenerate.Add(modifyChunkPos, ch);
            }
        }
        else
        {
            return BlockType.Dirt;
        }
        if(currentBlock == BlockType.Spawner)
        {
            ch.StopSpawning();
        }
        return currentBlock;
    }

    private void PlayBlockPlacement(BlockType currentBlock)
    {
        if (currentBlock == BlockType.Dirt)
        {
            AudioManager.instance.Play("Dirt");
        }
        else if (currentBlock == BlockType.Grass)
        {
            AudioManager.instance.Play("Grass");
        }
        else if (currentBlock == BlockType.Leaves)
        {
            AudioManager.instance.Play("Leaves");
        }
        else if (currentBlock == BlockType.Wood)
        {
            AudioManager.instance.Play("Wood");
        }
        else if (currentBlock == BlockType.Sand)
        {
            AudioManager.instance.Play("Sand");
        }
        else if (Chunk.IsFlowerOrWeed(currentBlock))
        {
            AudioManager.instance.Play("Flower");
        }
        else if (currentBlock == BlockType.Cactus)
        {
            AudioManager.instance.Play("Cactus");
        }
        else
        {
            AudioManager.instance.Play("Stone");
        }
    }

    private void PlayBlockDestroy(BlockType currentBlock)
    {
        int r = UnityEngine.Random.Range(1, 3);
        if (currentBlock == BlockType.Dirt)
        {
            AudioManager.instance.Play("Dirt"+r);
        }
        else if (currentBlock == BlockType.Grass)
        {
            AudioManager.instance.Play("Grass"+r);
        }
        else if (currentBlock == BlockType.Leaves)
        {
            AudioManager.instance.Play("Leaves"+r);
        }
        else if (currentBlock == BlockType.Spawner)
        {
            AudioManager.instance.Play("Spawner"+r);
        }
        else if (currentBlock == BlockType.Wood)
        {
            AudioManager.instance.Play("Wood"+r);
        }
        else if (currentBlock == BlockType.Sand)
        {
            AudioManager.instance.Play("Sand");
        }
        else if (Chunk.IsFlowerOrWeed(currentBlock))
        {
            AudioManager.instance.Play("Flower"+r);
        }
        else if (currentBlock == BlockType.Cactus)
        {
            AudioManager.instance.Play("Cactus" + r);
        }
        else
        {
            AudioManager.instance.Play("Stone"+r);
        }
    }

    //When modifying a chunk, we check if we should also update neighbour chunks
    private void GenerateChunk(int x, int z, Vector2Int modifyChunkPos)
    {
        if(x == 0)
        {
            Vector2Int leftChunk = new Vector2Int(modifyChunkPos.x - chunkLenght, modifyChunkPos.y);
            if (!toGenerate.ContainsKey(leftChunk))
            {
                toGenerate.Add(leftChunk, chunks[leftChunk]);
            }
        }
        else if(x == chunkLenght - 1)
        {
            Vector2Int rightChunk = new Vector2Int(modifyChunkPos.x + chunkLenght, modifyChunkPos.y);
            if (!toGenerate.ContainsKey(rightChunk))
            {
                toGenerate.Add(rightChunk, chunks[rightChunk]);
            }
        }

        if (z == 0)
        {
            Vector2Int frontChunk = new Vector2Int(modifyChunkPos.x, modifyChunkPos.y - chunkLenght);
            if (!toGenerate.ContainsKey(frontChunk))
            {
                toGenerate.Add(frontChunk, chunks[frontChunk]);
            }
        }
        else if(z == chunkLenght - 1)
        {
            Vector2Int backChunk = new Vector2Int(modifyChunkPos.x, modifyChunkPos.y + chunkLenght);
            if (!toGenerate.ContainsKey(backChunk))
            {
                toGenerate.Add(backChunk, chunks[backChunk]);
            }
        }
    }

    NativeArray<BlockType> GetChunkMap(Vector2Int chunkPos)
    {
        if (!chunks.ContainsKey(chunkPos))
            return new NativeArray<BlockType>(0, Allocator.TempJob);
        return new NativeArray<BlockType>(chunks[chunkPos].blockMap, Allocator.TempJob);
    }

    /*
    public void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
    */

    //Construct the tile textures in the same order as the blocktype enum
    List<TileTexture> InitializeTileTextures()
    {
        List<TileTexture> tileTextures = new List<TileTexture>();
        //Grass
        tileTextures.Add(TileTexture.GrassSide);
        tileTextures.Add(TileTexture.Grass);
        tileTextures.Add(TileTexture.Dirt);
        //Dirt
        tileTextures.Add(TileTexture.Dirt);
        tileTextures.Add(TileTexture.Dirt);
        tileTextures.Add(TileTexture.Dirt);
        //Stone
        tileTextures.Add(TileTexture.Stone);
        tileTextures.Add(TileTexture.Stone);
        tileTextures.Add(TileTexture.Stone);
        //Wood
        tileTextures.Add(TileTexture.WoodSide);
        tileTextures.Add(TileTexture.WoodTop);
        tileTextures.Add(TileTexture.WoodTop);
        //Sand
        tileTextures.Add(TileTexture.Sand);
        tileTextures.Add(TileTexture.Sand);
        tileTextures.Add(TileTexture.Sand);
        //Leaves
        tileTextures.Add(TileTexture.Leaves);
        tileTextures.Add(TileTexture.Leaves);
        tileTextures.Add(TileTexture.Leaves);
        //Spawner
        tileTextures.Add(TileTexture.Spawner);
        tileTextures.Add(TileTexture.Spawner);
        tileTextures.Add(TileTexture.Spawner);
        //Weed (there is no tile for the grass/flower because they are not cubes so we aren't suppose to use them)
        tileTextures.Add(TileTexture.Bedrock);
        tileTextures.Add(TileTexture.Stone);
        tileTextures.Add(TileTexture.Grass);
        //Rose
        tileTextures.Add(TileTexture.Bedrock);
        tileTextures.Add(TileTexture.Stone);
        tileTextures.Add(TileTexture.Grass);
        //Dandelion
        tileTextures.Add(TileTexture.Bedrock);
        tileTextures.Add(TileTexture.Stone);
        tileTextures.Add(TileTexture.Grass);
        //Cornflower
        tileTextures.Add(TileTexture.Bedrock);
        tileTextures.Add(TileTexture.Stone);
        tileTextures.Add(TileTexture.Grass);
        //Cactus
        tileTextures.Add(TileTexture.CactusSide);
        tileTextures.Add(TileTexture.CactusTop);
        tileTextures.Add(TileTexture.CactusBottom);
        //DeadBush
        tileTextures.Add(TileTexture.Bedrock);
        tileTextures.Add(TileTexture.Stone);
        tileTextures.Add(TileTexture.Grass);
        //Water
        tileTextures.Add(TileTexture.Water);
        tileTextures.Add(TileTexture.Water);
        tileTextures.Add(TileTexture.Water);
        //Bedrock
        tileTextures.Add(TileTexture.Bedrock);
        tileTextures.Add(TileTexture.Bedrock);
        tileTextures.Add(TileTexture.Bedrock);

        return tileTextures;
    }

    //Run some jobs to  generate the  chunk and construct its mesh
    IEnumerator Generate()
    {
        //ClearLog();
        List<ChunkJob> chunkJobs = new List<ChunkJob>();
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.Temp);
        List<Chunk> modifyChunks = new List<Chunk>();
        int i = 0;
        //Create and run some jobs
        while(toGenerate.Count > 0 && i < maxGen)
        {
            //Profiler.BeginSample("Creating and doing jobs");
            Chunk ch = toGenerate.Values.Last();
            //Chunk ch = toGenerate[i];
            NativeArray<BlockType> blockTypes;
            if (ch.blockMap.Length > 0)
            {
                blockTypes = new NativeArray<BlockType>(ch.blockMap, Allocator.TempJob);
            } else
            {
                blockTypes = new NativeArray<BlockType>(chunkLenght * chunkHeight * chunkLenght, Allocator.TempJob);
            }
            NativeArray<BlockType> frontChunk = GetChunkMap(new Vector2Int((int)ch.transform.position.x, (int)(ch.transform.position.z - chunkLenght)));
            NativeArray<BlockType> backChunk = GetChunkMap(new Vector2Int((int)ch.transform.position.x, (int)(ch.transform.position.z + chunkLenght)));
            NativeArray<BlockType> leftChunk = GetChunkMap(new Vector2Int((int)(ch.transform.position.x - chunkLenght), (int)ch.transform.position.z));
            NativeArray<BlockType> rightChunk = GetChunkMap(new Vector2Int((int)(ch.transform.position.x + chunkLenght), (int)ch.transform.position.z));
            NativeList<Vector3> verticesJob = new NativeList<Vector3>(Allocator.TempJob);
            NativeList<int> trianglesJob = new NativeList<int>(Allocator.TempJob);
            NativeList<Vector2> uvJob = new NativeList<Vector2>(Allocator.TempJob);
            NativeList<Vector3> verticesLJob = new NativeList<Vector3>(Allocator.TempJob);
            NativeList<int> trianglesLJob = new NativeList<int>(Allocator.TempJob);
            NativeList<Vector2> uvLJob = new NativeList<Vector2>(Allocator.TempJob);
            NativeList<Vector3> verticesFJob = new NativeList<Vector3>(Allocator.TempJob);
            NativeList<int> trianglesFJob = new NativeList<int>(Allocator.TempJob);
            NativeList<Vector2> uvFJob = new NativeList<Vector2>(Allocator.TempJob);
            NativeList<Vector3> verticesWJob = new NativeList<Vector3>(Allocator.TempJob);
            NativeList<int> trianglesWJob = new NativeList<int>(Allocator.TempJob);
            NativeList<Vector2> uvWJob = new NativeList<Vector2>(Allocator.TempJob);
            NativeArray<int3> spawnerPos = new NativeArray<int3>(2, Allocator.TempJob);
            NativeList<Vector3> foliagePos = new NativeList<Vector3>(Allocator.TempJob);
            NativeList<Vector3> foliageRot = new NativeList<Vector3>(Allocator.TempJob);
            NativeList<Vector3> cactusPos = new NativeList<Vector3>(Allocator.TempJob);
            NativeArray<TileTexture> tileTextures = new NativeArray<TileTexture>(allTileTextures.ToArray(),Allocator.TempJob);

            //Pass some data to the job
            ChunkJob job = new ChunkJob
            {
                blockTypes = blockTypes,
                frontChunk = frontChunk,
                backChunk = backChunk,
                leftChunk = leftChunk,
                rightChunk = rightChunk,
                vertices = verticesJob,
                triangles = trianglesJob,
                uv = uvJob,
                verticesLeaves = verticesLJob,
                trianglesLeaves = trianglesLJob,
                uvLeaves = uvLJob,
                verticesFlower = verticesFJob,
                trianglesFlower = trianglesFJob,
                uvFlower = uvFJob,
                verticesWater = verticesWJob,
                trianglesWater = trianglesWJob,
                uvWater = uvWJob,
                spawnerPosition = spawnerPos,
                foliagePosition = foliagePos,
                foliageRotation = foliageRot,
                cactusPosition = cactusPos,
                chunkLenght = chunkLenght,
                chunkHeight = chunkHeight,
                baseLevel = baseLevel,
                waterLevel = waterLevel,
                offsetX = offsetX + ch.transform.position.x,
                offsetZ = offsetZ + ch.transform.position.z,
                xPosition = ch.transform.position.x,
                zPosition = ch.transform.position.z,
                isGenerate = ch.isGenerate,
                rand = new Unity.Mathematics.Random((uint)(Mathf.PerlinNoise(offsetX * 500 + ch.transform.position.x, offsetZ * 500 + ch.transform.position.z) * 10000)),
                tileTextures = tileTextures,
            };
            //We run the job
            JobHandle jobHandle = job.Schedule();
            chunkJobs.Add(job);
            jobHandles.Add(jobHandle);
            modifyChunks.Add(ch);
            i++;
            toGenerate.Remove(new Vector2Int((int)ch.transform.position.x, (int)ch.transform.position.z));
        }
        //We wait for the parallel jobs to finish
        JobHandle.CompleteAll(jobHandles);
        jobHandles.Dispose();

        //We get the data back and construct the mesh
        for (int k = 0; k < modifyChunks.Count; k++)
        {
            Chunk ch = modifyChunks[k];
            ch.blockMap = new BlockType[chunkLenght * chunkHeight * chunkLenght];
            chunkJobs[k].blockTypes.CopyTo(ch.blockMap);
            chunkJobs[k].blockTypes.Dispose();
            chunkJobs[k].frontChunk.Dispose();
            chunkJobs[k].backChunk.Dispose();
            chunkJobs[k].leftChunk.Dispose();
            chunkJobs[k].rightChunk.Dispose();
            chunkJobs[k].tileTextures.Dispose();

            Vector3[] vertices = chunkJobs[k].vertices.ToArray();
            int[] triangles = chunkJobs[k].triangles.ToArray(); ;
            Vector2[] uv = chunkJobs[k].uv.ToArray();
            chunkJobs[k].vertices.Dispose();
            chunkJobs[k].triangles.Dispose();
            chunkJobs[k].uv.Dispose();

            Vector3[] verticesLeaves = chunkJobs[k].verticesLeaves.ToArray();
            int[] trianglesLeaves = chunkJobs[k].trianglesLeaves.ToArray(); ;
            Vector2[] uvLeaves = chunkJobs[k].uvLeaves.ToArray();
            chunkJobs[k].verticesLeaves.Dispose();
            chunkJobs[k].trianglesLeaves.Dispose();
            chunkJobs[k].uvLeaves.Dispose();

            Vector3[] verticesFlower = chunkJobs[k].verticesFlower.ToArray();
            int[] trianglesFlower = chunkJobs[k].trianglesFlower.ToArray(); ;
            Vector2[] uvFlower = chunkJobs[k].uvFlower.ToArray();
            chunkJobs[k].verticesFlower.Dispose();
            chunkJobs[k].trianglesFlower.Dispose();
            chunkJobs[k].uvFlower.Dispose();

            Vector3[] verticesWater = chunkJobs[k].verticesWater.ToArray();
            int[] trianglesWater = chunkJobs[k].trianglesWater.ToArray(); ;
            Vector2[] uvWater = chunkJobs[k].uvWater.ToArray();
            chunkJobs[k].verticesWater.Dispose();
            chunkJobs[k].trianglesWater.Dispose();
            chunkJobs[k].uvWater.Dispose();

            int3 s1 = chunkJobs[k].spawnerPosition[0];
            if (ch.spawnerLocation == Vector3Int.zero)
                ch.spawnerLocation = new Vector3Int(s1.x,s1.y,s1.z);
            int3 s2 = chunkJobs[k].spawnerPosition[1];
            if (ch.spawnDuckLocation == Vector3Int.zero)
                ch.spawnDuckLocation = new Vector3Int(s2.x, s2.y, s2.z);
            chunkJobs[k].spawnerPosition.Dispose();

            Vector3[] foliagePosition = chunkJobs[k].foliagePosition.ToArray();
            Vector3[] foliageRotation = chunkJobs[k].foliageRotation.ToArray();
            chunkJobs[k].foliagePosition.Dispose();
            chunkJobs[k].foliageRotation.Dispose();

            Vector3[] cactusPosition = chunkJobs[k].cactusPosition.ToArray();
            chunkJobs[k].cactusPosition.Dispose();

            Mesh mesh = new Mesh();
            //mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            ch.GetComponent<MeshFilter>().mesh = mesh;
            ch.GetComponent<MeshCollider>().sharedMesh = mesh;

            Mesh meshLeaves = new Mesh();
            //meshLeaves.Clear();
            meshLeaves.vertices = verticesLeaves;
            meshLeaves.uv = uvLeaves;
            meshLeaves.triangles = trianglesLeaves;
            meshLeaves.RecalculateNormals();
            ch.leavesMeshFilter.mesh = meshLeaves;
            ch.leavesMeshCollider.sharedMesh = meshLeaves;

            Mesh meshFlower = new Mesh();
            //meshFlower.Clear();
            meshFlower.vertices = verticesFlower;
            meshFlower.uv = uvFlower;
            meshFlower.triangles = trianglesFlower;
            meshFlower.RecalculateNormals();
            ch.flowerMeshFilter.mesh = meshFlower;
            ch.flowerMeshCollider.sharedMesh = meshFlower;

            Mesh meshWater = new Mesh();
            //meshFlower.Clear();
            meshWater.vertices = verticesWater;
            meshWater.uv = uvWater;
            meshWater.triangles = trianglesWater;
            meshWater.RecalculateNormals();
            ch.waterMeshFilter.mesh = meshWater;
            ch.waterMeshCollider.sharedMesh = meshWater;

            ch.SetFoliage(foliagePosition, foliageRotation);
            ch.SetCactus(cactusPosition);
            ch.SpawnAnimals();
            ch.isGenerate = true;
            ch.StartSpawning();

            if (LevelManager.instance.isLoading)
                LevelManager.instance.UpadteBar(1);
        }
        //Profiler.EndSample();
        yield return new WaitForSeconds(waitTime);
        isGenerate = false;
    }

    void LoadTerrain()
    {
        //float time = Time.realtimeSinceStartup;
        ES3.CacheFile(MainGameManager.instance.worldName + "/world.save");
        ES3Settings cacheSetting = new ES3Settings(ES3.Location.Cache);
        foreach (string key in ES3.GetKeys(MainGameManager.instance.worldName + "/world.save"))
        {
            if (key[0] == 'C')
            {
                string[] split = key.Split('_');
                int x = Int32.Parse(split[1]);
                int y = Int32.Parse(split[2]);
                GameObject c = Instantiate(chunk, new Vector3(x, 0, y), Quaternion.identity);
                c.transform.SetParent(gameObject.transform);
                Chunk ch = c.GetComponent<Chunk>();
                chunks.Add(new Vector2Int(x, y), ch);
                ch.isGenerate = true;
                ch.isModified = true;
                ch.isLoadedFromSave = true;
                ch.blockMap = ES3.Load<BlockType[]>(key, MainGameManager.instance.worldName + "/world.save",cacheSetting);
            }
        }
        //Debug.Log(Time.realtimeSinceStartup - time);
    }

    private void OnDestroy()
    {
        SaveTerrain();
    }

    private void OnApplicationQuit()
    {
        SaveTerrain();
    }

    void SaveTerrain()
    {
        //float time = Time.realtimeSinceStartup;
        ES3Settings cacheSetting = new ES3Settings(ES3.Location.Cache);
        foreach (KeyValuePair<Vector2Int, Chunk> c in chunks)
        {
            if (c.Value.isModified && !c.Value.isLoadedFromSave)
            {
                ES3.Save("C:_" + c.Key.x + "_" + c.Key.y, c.Value.blockMap, MainGameManager.instance.worldName + "/world.save", cacheSetting);
            }
        }
        ES3.StoreCachedFile(MainGameManager.instance.worldName + "/world.save");
        //Debug.Log(Time.realtimeSinceStartup - time);
    }
}





[BurstCompile]
public struct ChunkJob : IJob
{
    public NativeArray<BlockType> blockTypes;

    [ReadOnly] public NativeArray<BlockType> frontChunk;
    [ReadOnly] public NativeArray<BlockType> backChunk;
    [ReadOnly] public NativeArray<BlockType> leftChunk;
    [ReadOnly] public NativeArray<BlockType> rightChunk;

    public NativeList<Vector3> vertices;
    public NativeList<int> triangles;
    public NativeList<Vector2> uv;
    public NativeList<Vector3> verticesLeaves;
    public NativeList<int> trianglesLeaves;
    public NativeList<Vector2> uvLeaves;
    public NativeList<Vector3> verticesFlower;
    public NativeList<int> trianglesFlower;
    public NativeList<Vector2> uvFlower;
    public NativeList<Vector3> verticesWater;
    public NativeList<int> trianglesWater;
    public NativeList<Vector2> uvWater;
    public NativeArray<int3> spawnerPosition;
    public NativeList<Vector3> foliagePosition;
    public NativeList<Vector3> foliageRotation;
    public NativeList<Vector3> cactusPosition;
    public int chunkLenght;
    public int chunkHeight;
    public int baseLevel;
    public int waterLevel;
    public float offsetX;
    public float offsetZ;
    public float xPosition;
    public float zPosition;
    public bool isGenerate;
    public Unity.Mathematics.Random rand;

    public NativeArray<TileTexture> tileTextures;

    bool isGrassLand;

    public void Execute()
    {
        isGrassLand = true;
        //Debug.Log("Executing job");
        if (!isGenerate)
        {
            PopulateMap();
        }
        UpdateMap();
    }

    void PopulateMap()
    {
        //Profiler.BeginSample("Populate Map");
        for (int z = 0; z < chunkLenght; z++)
        {
            for (int x = 0; x < chunkLenght; x++)
            {
                TerrainGenerator.BiomeType biomeType;
                int sample = GenerateOneNoise(x + (int)offsetX, z + (int)offsetZ, out biomeType) + baseLevel;
                float caveMask = noise.cnoise(new Vector2((x + (int)offsetX) / 200f, (z + (int)offsetZ) / 200f)) + .7f;
                int bedrockLevel = (int)math.ceil((noise.cnoise(new Vector2((x + offsetX) / 0.005f, (z + offsetZ) / 0.005f)) + 1f) * 2.5);
                int height = math.max(sample + 1, waterLevel - 1);
                int xzIndex = (x + z * chunkLenght) * chunkHeight;
                for (int y = 0; y <= height; y++)
                {
                    //Profiler.BeginSample("Generate Block");
                    blockTypes[y + xzIndex] = GenerateBlock(x, y, z, sample, caveMask, bedrockLevel, true, biomeType);
                    //Profiler.EndSample();
                }
                if (biomeType == TerrainGenerator.BiomeType.Desert || sample < waterLevel - 3)
                    isGrassLand = false;
            }
        }
        GenerateTree();
        GenerateSpawner();
        //Profiler.EndSample();
    }

    //Generate a height and a biome for a x z coordinate in the world
    int GenerateOneNoise(int x, int z , out TerrainGenerator.BiomeType biome)
    {
        //Profiler.BeginSample("Generate one noise");
        float simplex1 = noise.cnoise(new Vector2(x / 35f, z / 35f)) * 6f; //Global terrain small scale (a bit of variation)
        float simplex2 = noise.cnoise(new Vector2(x / 500f, z / 500f)) * 20f; //Global terrain at big scale
        float simplex3 = (noise.snoise(new Vector2(x / 100f, z / 100f))+1) * 8f; //some montain at medium scale
        float cellular = 0f;
        float biomeNoise = noise.cnoise(new Vector2((x + 100) / 300f, (z - 420) / 300f)) + 1f;
        if (biomeNoise > 1.2f)
        {
            cellular = (noise.cellular(new Vector2(x / 40f, z / 80f)).y + 0.1f) *25f;
            biome = TerrainGenerator.BiomeType.Desert;
        }
        else if (biomeNoise > 0.9f)
        {
            cellular = (noise.cellular(new Vector2(x / 40f, z / 80f)).y + 0.1f) * 25f * (biomeNoise - 0.9f) * 3.3f;
            biome = TerrainGenerator.BiomeType.Grassland;
        }
        else
        {
            biome = TerrainGenerator.BiomeType.Grassland;
        }
        //Profiler.EndSample();
        return (int)(simplex1 + simplex2 + simplex3 + cellular);
    }

    bool GenerateNoiseCave(int x, int y, int z, float caveMask)
    {
        if (caveMask > 1f)
            return false;
        //Profiler.BeginSample("Generate noise cave");
        float caveNoise1 = noise.cnoise(new Vector3(x / 15f, y / 15f, z / 15f));
        //Profiler.EndSample();
        return (caveNoise1 > math.max(caveMask,0.2));
    }

    //Return a BlockType at x y z coordinates (A lot of branchment for the world generation)
    BlockType GenerateBlock(int x, int y, int z, int sample, float caveMask, int bedrockLevel, bool isBuilding, TerrainGenerator.BiomeType biomeType = TerrainGenerator.BiomeType.Grassland)
    {
        if (y < bedrockLevel) // Generate BedRock
        {
            return BlockType.Bedrock;
        }
        else if (y <= sample && GenerateNoiseCave(x + (int)offsetX, y, z + (int)offsetZ, caveMask)) // Generate cave below or at ground level
        {
            return BlockType.Air;
        }
        else if (y <= sample - 3) //Generate blocks far below the ground (Stone)
        {
            return BlockType.Stone;
        }
        else if (y < sample && y > sample - 3) //Block just 3 blocks below the ground
        {
            if (biomeType == TerrainGenerator.BiomeType.Desert)
                return BlockType.Sand;
            else if (!GenerateNoiseCave(x + (int)offsetX, y + 1, z + (int)offsetZ, caveMask)) //the block above is not part of a cave(Dirt/ Sand)
                return BlockType.Dirt;
            else
                return BlockType.Grass;
        }
        else if (y == sample) // Block at ground level
        {
            if (y < waterLevel) // Below water level (Dirt / Sand)
            {
                if (rand.NextFloat() > 0.9f)
                    return BlockType.Dirt;
                else
                    return BlockType.Sand;
            }
            else if (biomeType == TerrainGenerator.BiomeType.Desert) // Above water level
                return BlockType.Sand;
            else
                return BlockType.Grass;
        }
        else if (y < waterLevel) // Block below water level
        {
            return BlockType.Water;
        }
        else if (isBuilding) // Block just above the ground (Vegetation)
        {
            if (y == sample + 1)
                return GenerateFlower(x, y, z, biomeType);
            else //Debug block
                return BlockType.Leaves;
        }
        else
            return BlockType.Air;

    }

    BlockType GenerateFlower(int x, int y, int z, TerrainGenerator.BiomeType biomeType)
    {
        if (biomeType == TerrainGenerator.BiomeType.Grassland)
        {
            if (blockTypes[y - 1 + (x + z * chunkLenght) * chunkHeight] != BlockType.Grass)
                return BlockType.Air;
            float r = rand.NextFloat();
            if (r > 0.05f)
            {
                return BlockType.Air;
            }
            else if (r > 0.009f)
            {
                return BlockType.Weed;
            }
            else if (r > 0.005f)
            {
                return BlockType.Dandelion;
            }
            else if (r > 0.002f)
            {
                return BlockType.Cornflower;
            }
            else
            {
                return BlockType.Rose;
            }
        }
        else
        {
            if (blockTypes[y - 1 + (x + z * chunkLenght) * chunkHeight] != BlockType.Sand)
                return BlockType.Air;
            float r = rand.NextFloat();
            if (r > 0.003f)
            {
                return BlockType.Air;
            }
            else if (r > 0.0008f)
            {
                return BlockType.Deadbush;
            }
            else //For the cactus we also add two on top
            {
                blockTypes[y + 1 + (x + z * chunkLenght) * chunkHeight] = BlockType.Cactus;
                blockTypes[y + 2 + (x + z * chunkLenght) * chunkHeight] = BlockType.Cactus;
                return BlockType.Cactus;
            }
        }
    }

    void GenerateTree()
    {
        float noiseValue = noise.snoise(new Vector2(offsetX / 300f, offsetZ / 300f));
        int nbTree = 0;
        if (noiseValue > 0f)
        {
            nbTree = (int)math.floor(noiseValue * 8f * rand.NextFloat(0.66f, 1.5f));
        }
        else if (rand.NextFloat() > 0.96f)
        {
            nbTree = 1;
        }
        if(nbTree > 0)
        {
            NativeArray<int2> pos = new NativeArray<int2>(nbTree, Allocator.Temp);
            for (int i = 0; i < nbTree; i++)
            {
                pos[i] = new int2(rand.NextInt(2, 14), rand.NextInt(2, 14));
            }

            foreach (int2 p in pos)
            {
                int x = p[0];
                int z = p[1];
                TerrainGenerator.BiomeType biomeType;
                int y = GenerateOneNoise(x + (int)offsetX, z + (int)offsetZ,out biomeType) + baseLevel;
                if (biomeType != TerrainGenerator.BiomeType.Grassland)
                    return;
                if (blockTypes[y + (x + z * chunkLenght) * chunkHeight] == BlockType.Grass && y > waterLevel)
                {
                    //Generate Leaves
                    for (int i = 4; i < 6; i++)
                    {
                        GenerateLeaves(x, y + i, z, 2);
                    }
                    GenerateLeaves(x, y + 6, z, 1);
                    GenerateLeaves(x, y + 7, z, 1, false);

                    //Generate Wood
                    for (int j = 1; j < 7; j++)
                    {
                        blockTypes[(y + j) + (x + z * chunkLenght) * chunkHeight] = BlockType.Wood;
                    }
                }
            }
            pos.Dispose();
        }
    }

    void GenerateLeaves(int x, int y, int z, int lim, bool corners = true)
    {
        for (int i = -lim; i < lim+1; i++)
        {
            for (int j = -lim; j < lim+1; j++)
            {
                //If it is the block in the corners
                bool c = (i == -lim && j == -lim) || (i == -lim && j == lim) || (i == lim && j == -lim) || (i == lim && j == lim);
                if (!c || (corners && rand.NextBool()))
                {
                    blockTypes[y + ((x+i) + (z+j) * chunkLenght) * chunkHeight] = BlockType.Leaves;
                } 
            }
        }
    }

    void GenerateSpawner()
    { 
        //Zombie Spawner
        spawnerPosition[0] = int3.zero;
        float noiseValue1 = noise.snoise(new Vector2((offsetX+1234) / 50f, (offsetZ-1234) / 50f));
        TerrainGenerator.BiomeType biomeType;
        int sample = GenerateOneNoise(chunkLenght/2 + (int)offsetX, chunkLenght/2 + (int)offsetZ, out biomeType) + baseLevel;
        if(noiseValue1 > 0.8f)
        {
            if (blockTypes[sample + (chunkLenght / 2 + (chunkLenght / 2) * chunkLenght) * chunkHeight] != BlockType.Air && 
                blockTypes[sample + 1 + (chunkLenght / 2 + (chunkLenght / 2) * chunkLenght) * chunkHeight] == BlockType.Air)
            {
                blockTypes[sample + 1 + (chunkLenght / 2 + (chunkLenght / 2) * chunkLenght) * chunkHeight] = BlockType.Spawner;
                spawnerPosition[0] = new int3(chunkLenght / 2, sample + 1, chunkLenght / 2);
            }
        }

        //Duck Spawn
        spawnerPosition[1] = int3.zero;
        float noiseValue2 = noise.snoise(new Vector2((offsetX + 42) / 10f, (offsetZ - 69) / 10f));
        if (isGrassLand && noiseValue2 > 0.8f)
        {
            spawnerPosition[1] = new int3(chunkLenght / 2, sample + 1, chunkLenght / 2);
        }
    }

    void UpdateMap()
    {
        //Profiler.BeginSample("Update Map");
        for (int z = 0; z < chunkLenght; z++)
        {
            for (int x = 0; x < chunkLenght; x++)
            {
                int xzIndex = (x + z * chunkLenght) * chunkHeight;

                for (int y = 0; y < chunkHeight; y++)
                {
                    BlockType blockType = blockTypes[y + xzIndex];
                    if (blockType != BlockType.Air)
                    {
                        if (blockType != BlockType.Weed && blockType != BlockType.Deadbush && !IsFlower(blockType))
                        {
                            CheckSideBlock(blockType, x, y, z);
                        }
                        else
                        {
                            BuildWeed(blockType, x, y, z);
                        }
                    }
                }
            }
        }
        //Profiler.EndSample();
    }

    bool IsFlower(BlockType blockType)
    {
        return blockType == BlockType.Rose || blockType == BlockType.Cornflower || blockType == BlockType.Dandelion;
    }

    void BuildWeed(BlockType blockType, int x, int y, int z)
    {
        //Profiler.BeginSample("Build weed");
        float scale = 0.1f;
        for (int i = 0; i < 2; i++)
        {
            /*
            float centerx = rand.NextFloat(0.35f, 0.65f);
            float centerz = rand.NextFloat(0.35f, 0.65f);*/
            int nbFlower = verticesFlower.Length;

            /*
            verticesFlower.Add(new Vector3(x + centerx - 0.5f, y, z + centerz - 0.5f));
            verticesFlower.Add(new Vector3(x + centerx - 0.5f, y + 1, z + centerz - 0.5f));
            verticesFlower.Add(new Vector3(x + centerx + 0.5f, y + 1, z + centerz + 0.5f));
            verticesFlower.Add(new Vector3(x + centerx + 0.5f, y, z + centerz + 0.5f));
            */
            for (int j = 0; j < 2; j++)
            {
                if(blockType == BlockType.Weed || blockType == BlockType.Deadbush)
                {
                    verticesFlower.Add(new Vector3(x + scale + i * (1 - 2 * scale), y, z + scale));
                    verticesFlower.Add(new Vector3(x + scale + i * (1 - 2 * scale), y + 1 - scale, z + scale));
                    verticesFlower.Add(new Vector3(x + 1 - scale - i * (1 - 2 * scale), y + 1 - scale, z + 1 - scale));
                    verticesFlower.Add(new Vector3(x + 1 - scale - i * (1 - 2 * scale), y, z + 1 - scale));
                    if (blockType == BlockType.Weed)
                    {
                        uvFlower.Add(new Vector2(0.125f, 0.25f));
                        uvFlower.Add(new Vector2(0.125f, 0.375f));
                        uvFlower.Add(new Vector2(0.25f, 0.375f));
                        uvFlower.Add(new Vector2(0.25f, 0.25f));
                    }
                    else
                    {
                        uvFlower.Add(new Vector2(0f, 0.25f));
                        uvFlower.Add(new Vector2(0f, 0.375f));
                        uvFlower.Add(new Vector2(0.125f, 0.375f));
                        uvFlower.Add(new Vector2(0.125f, 0.25f));
                    }
                }
                else
                {
                    verticesFlower.Add(new Vector3(x + 0.3f + i * 0.4f, y, z + 0.3f));
                    verticesFlower.Add(new Vector3(x + 0.3f + i * 0.4f, y + 0.7f, z + 0.3f));
                    verticesFlower.Add(new Vector3(x + 0.7f - i * 0.4f, y + 0.7f, z + 0.7f));
                    verticesFlower.Add(new Vector3(x + 0.7f - i * 0.4f, y, z + 0.7f));
                    if (blockType == BlockType.Rose)
                    {
                        //xOffset = x +- 0.125/4 (0.03125)
                        //yOffset = y - (0.125/16)*11 (0.0875)
                        uvFlower.Add(new Vector2(0.28125f, 0.25f));
                        uvFlower.Add(new Vector2(0.28125f, 0.3375f));
                        uvFlower.Add(new Vector2(0.34375f, 0.3375f));
                        uvFlower.Add(new Vector2(0.34375f, 0.25f));
                    }
                    else if (blockType == BlockType.Dandelion)
                    {
                        uvFlower.Add(new Vector2(0.40625f, 0.25f));
                        uvFlower.Add(new Vector2(0.40625f, 0.3375f));
                        uvFlower.Add(new Vector2(0.46875f, 0.3375f));
                        uvFlower.Add(new Vector2(0.46875f, 0.25f));
                    }
                    else //Cornflower
                    {
                        uvFlower.Add(new Vector2(0.53125f, 0.25f));
                        uvFlower.Add(new Vector2(0.53125f, 0.3375f));
                        uvFlower.Add(new Vector2(0.59375f, 0.3375f));
                        uvFlower.Add(new Vector2(0.59375f, 0.25f));
                    }
                }
            }
            trianglesFlower.Add(nbFlower + 0);
            trianglesFlower.Add(nbFlower + 1);
            trianglesFlower.Add(nbFlower + 2);
            trianglesFlower.Add(nbFlower + 0);
            trianglesFlower.Add(nbFlower + 2);
            trianglesFlower.Add(nbFlower + 3);
            trianglesFlower.Add(nbFlower + 6);
            trianglesFlower.Add(nbFlower + 5);
            trianglesFlower.Add(nbFlower + 4);
            trianglesFlower.Add(nbFlower + 7);
            trianglesFlower.Add(nbFlower + 6);
            trianglesFlower.Add(nbFlower + 4);
        }
        //Profiler.EndSample();
    }

    //Return True if x y z is a BlockType.Air
    BlockType CheckBlockType(int x, int y, int z)
    {
        //Profiler.BeginSample("Check Block Type");
        TerrainGenerator.BiomeType biomeType;
        int sample = GenerateOneNoise(x + (int)offsetX, z + (int)offsetZ, out biomeType) + baseLevel;
        float caveMask = noise.cnoise(new Vector2((x + (int)offsetX) / 200f, (z + (int)offsetZ) / 200f)) + .7f;
        int bedrockLevel = (int)math.ceil((noise.cnoise(new Vector2((x + offsetX) / 0.005f, (z + offsetZ) / 0.005f)) + 1f) * 2.5);
        //Profiler.EndSample();
        return GenerateBlock(x, y, z, sample, caveMask, bedrockLevel, false, biomeType);
    }

    //Return true if you want to render a tile
    bool BlockTypeRender(BlockType blockType, BlockType sideBlock)
    {
        return (sideBlock == BlockType.Air) || (blockType != BlockType.Water && sideBlock == BlockType.Water) || (sideBlock == BlockType.Leaves && blockType != BlockType.Leaves) || (sideBlock == BlockType.Spawner)
                || (sideBlock == BlockType.Weed) || (sideBlock ==BlockType.Rose) || (sideBlock == BlockType.Cornflower) || (sideBlock == BlockType.Dandelion) || (sideBlock == BlockType.Cactus) || (blockType == BlockType.Cactus)
                || (sideBlock == BlockType.Deadbush);
    }

    void CheckSideBlock(BlockType blockType, int x, int y, int z)
    {
        //Profiler.BeginSample("Check side block");
        if (blockType == BlockType.Cactus)
            cactusPosition.Add(new Vector3(x + xPosition, y, z + zPosition));

        TileTexture sideTexture = tileTextures[((int)blockType - 1) * 3];
        //Check front
        if (z - 1 >= 0)
        {
            BlockType sideBlock = blockTypes[y + (x + (z - 1) * chunkLenght) * chunkHeight];
            if (BlockTypeRender(blockType, sideBlock))
            {
                BuildTile(Side.Front, sideTexture, x, y, z);
            }
        }
        //First condition for if the neighbour chunk exists and second one if it doesn't exist yet
        else if ((frontChunk.Length > 0 && BlockTypeRender(blockType, frontChunk[y + (x + (chunkLenght - 1) * chunkLenght) * chunkHeight])) || (frontChunk.Length == 0 && BlockTypeRender(blockType, CheckBlockType(x, y, z - 1))))
        {
            BuildTile(Side.Front, sideTexture, x, y, z);
        }

        //Check left
        if (x - 1 >= 0)
        {
            BlockType sideBlock = blockTypes[y + ((x - 1) + z * chunkLenght) * chunkHeight];
            if (BlockTypeRender(blockType, sideBlock))
            {
                BuildTile(Side.Left, sideTexture, x, y, z);
            }
        }
        else if ((leftChunk.Length > 0 && BlockTypeRender(blockType, leftChunk[y + ((chunkLenght - 1) + z * chunkLenght) * chunkHeight])) || (leftChunk.Length == 0 && BlockTypeRender(blockType, CheckBlockType(x - 1, y, z))))
        {
            BuildTile(Side.Left, sideTexture, x, y, z);
        }

        //Check back
        if (z + 1 < chunkLenght)
        {
            BlockType sideBlock = blockTypes[y + (x + (z + 1) * chunkLenght) * chunkHeight];
            if (BlockTypeRender(blockType, sideBlock))
            {
                BuildTile(Side.Back, sideTexture, x, y, z);
            }
        }
        else if ((backChunk.Length > 0 && BlockTypeRender(blockType, backChunk[y + x * chunkHeight])) || (backChunk.Length == 0 && BlockTypeRender(blockType, CheckBlockType(x, y, z + 1))))
        {
            BuildTile(Side.Back, sideTexture, x, y, z);
        }

        //Check right
        if (x + 1 < chunkLenght)
        {
            BlockType sideBlock = blockTypes[y + ((x + 1) + z * chunkLenght) * chunkHeight];
            if (BlockTypeRender(blockType, sideBlock))
            {
                BuildTile(Side.Right, sideTexture, x, y, z);
            }
        }
        else if ((rightChunk.Length > 0 && BlockTypeRender(blockType, rightChunk[y + z * chunkLenght * chunkHeight])) || (rightChunk.Length == 0 && BlockTypeRender(blockType, CheckBlockType(x + 1, y, z))))
        {
            BuildTile(Side.Right, sideTexture, x, y, z);
        }

        //Check top
        if (y + 1 < chunkHeight)
        {
            BlockType sideBlock = blockTypes[(y + 1) + (x + z * chunkLenght) * chunkHeight];
            if (BlockTypeRender(blockType, sideBlock) || (blockType == BlockType.Water && sideBlock != BlockType.Water))
            {
                BuildTile(Side.Top, tileTextures[((int)blockType - 1) * 3 + 1], x, y, z);
            }
        }
        else
        {
            BuildTile(Side.Top, tileTextures[((int)blockType - 1) * 3 + 1], x, y, z);
        }

        //Check bottom
        if (y - 1 >= 0)
        {
            BlockType sideBlock = blockTypes[(y - 1) + (x + z * chunkLenght) * chunkHeight];
            if (BlockTypeRender(blockType, sideBlock))
            {
                BuildTile(Side.Bottom, tileTextures[((int)blockType - 1) * 3 + 2], x, y, z);
            }
        }
        //Profiler.EndSample();
    }

    void FoliageRotation(Side side)
    {
        switch (side)
        {
            case Side.Top:
                foliageRotation.Add(new Vector3(0, 45, 0));
                foliageRotation.Add(new Vector3(0, -45, 0));
                break;

            case Side.Bottom:
                foliageRotation.Add(new Vector3(180, 45, 0));
                foliageRotation.Add(new Vector3(180, -45, 0));
                break;

            case Side.Front:
                foliageRotation.Add(new Vector3(45, 90, -90));
                foliageRotation.Add(new Vector3(-45, 90, -90));
                break;

            case Side.Back:
                foliageRotation.Add(new Vector3(45, 90, 90));
                foliageRotation.Add(new Vector3(-45, 90, 90));
                break;

            case Side.Left:
                foliageRotation.Add(new Vector3(45, 0, 90));
                foliageRotation.Add(new Vector3(-45, 0, 90));
                break;

            case Side.Right:
                foliageRotation.Add(new Vector3(45, 0, -90));
                foliageRotation.Add(new Vector3(-45, 0, -90));
                break;
        }
    }

    void BuildTile(Side side, TileTexture texture, int x, int y, int z)
    {
        //Profiler.BeginSample("Build tile");
        float blockHeight = 1f;
        float blockIn = 0f;
        if (texture == TileTexture.Water && blockTypes[(y + 1) + (x + z * chunkLenght) * chunkHeight] != BlockType.Water)
            blockHeight = 0.9f;
        else if (texture == TileTexture.CactusSide || texture == TileTexture.CactusTop || texture == TileTexture.CactusBottom)
            blockIn = 0.0625f;
        Vector3 v1 = Vector3.zero;
        Vector3 v2 = Vector3.zero;
        Vector3 v3 = Vector3.zero;
        Vector3 v4 = Vector3.zero;

        //Create vertices according to the Side
        switch (side)
        {
            case Side.Front:
                v1 = new Vector3(x, y, z + blockIn);
                v2 = new Vector3(x, y + blockHeight, z + blockIn);
                v3 = new Vector3(x + 1, y + blockHeight, z + blockIn);
                v4 = new Vector3(x + 1, y, z + blockIn);
                break;

            case Side.Left:
                v1 = new Vector3(x + blockIn, y, z + 1);
                v2 = new Vector3(x + blockIn, y + blockHeight, z + 1);
                v3 = new Vector3(x + blockIn, y + blockHeight, z);
                v4 = new Vector3(x + blockIn, y, z);
                break;

            case Side.Back:
                v1 = new Vector3(x + 1, y, z + 1 - blockIn);
                v2 = new Vector3(x + 1, y + blockHeight, z + 1 - blockIn);
                v3 = new Vector3(x, y + blockHeight, z + 1 - blockIn);
                v4 = new Vector3(x, y, z + 1 - blockIn);
                break;

            case Side.Right:
                v1 = new Vector3(x + 1 - blockIn, y, z);
                v2 = new Vector3(x + 1 - blockIn, y + blockHeight, z);
                v3 = new Vector3(x + 1 - blockIn, y + blockHeight, z + 1);
                v4 = new Vector3(x + 1 - blockIn, y, z + 1);
                break;

            case Side.Top:
                v1 = new Vector3(x + blockIn, y + blockHeight, z + blockIn);
                v2 = new Vector3(x + blockIn, y + blockHeight, z + 1 - blockIn);
                v3 = new Vector3(x + 1 - blockIn, y + blockHeight, z + 1 - blockIn);
                v4 = new Vector3(x + 1 - blockIn, y + blockHeight, z + blockIn);
                break;

            case Side.Bottom:
                v1 = new Vector3(x + blockIn, y, z + 1 - blockIn);
                v2 = new Vector3(x + blockIn, y, z + blockIn);
                v3 = new Vector3(x + 1 - blockIn, y, z + blockIn);
                v4 = new Vector3(x + 1 - blockIn, y, z + 1 - blockIn);
                break;
        }
        /*
        Tableau tableau = blah;

        uv.Add(new Vector2(tableau[texture].val1, ))
        */
        if (texture == TileTexture.Grass)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0f, 0f));
            uv.Add(new Vector2(0f, 0.125f));
            uv.Add(new Vector2(0.125f, 0.125f));
            uv.Add(new Vector2(0.125f, 0f));
        }
        else if (texture == TileTexture.Stone)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0f, 0.125f));
            uv.Add(new Vector2(0f, 0.25f));
            uv.Add(new Vector2(0.125f, 0.25f));
            uv.Add(new Vector2(0.125f, 0.125f));
        }
        else if (texture == TileTexture.GrassSide)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.125f, 0f));
            uv.Add(new Vector2(0.125f, 0.125f));
            uv.Add(new Vector2(0.25f, 0.125f));
            uv.Add(new Vector2(0.25f, 0f));
        }
        else if (texture == TileTexture.Sand)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.5f, 0.125f));
            uv.Add(new Vector2(0.5f, 0.25f));
            uv.Add(new Vector2(0.625f, 0.25f));
            uv.Add(new Vector2(0.625f, 0.125f));
        }
        else if (texture == TileTexture.Water)
        {
            AddVerticesAndTrianglesWater(v1, v2, v3, v4);
            uvWater.Add(new Vector2(0f, 0f));
            uvWater.Add(new Vector2(0f, 1f));
            uvWater.Add(new Vector2(1f, 1f));
            uvWater.Add(new Vector2(1f, 0f));
        }
        else if (texture == TileTexture.Dirt)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.25f, 0f));
            uv.Add(new Vector2(0.25f, 0.125f));
            uv.Add(new Vector2(0.375f, 0.125f));
            uv.Add(new Vector2(0.375f, 0f));
        }
        else if (texture == TileTexture.Bedrock)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.625f, 0.125f));
            uv.Add(new Vector2(0.625f, 0.25f));
            uv.Add(new Vector2(0.75f, 0.25f));
            uv.Add(new Vector2(0.75f, 0.125f));
        }
        else if (texture == TileTexture.Leaves)
        {
            AddVerticesAndTrianglesLeaves(v1, v2, v3, v4, side);
            uvLeaves.Add(new Vector2(0.25f, 0.125f));
            uvLeaves.Add(new Vector2(0.25f, 0.25f));
            uvLeaves.Add(new Vector2(0.375f, 0.25f));
            uvLeaves.Add(new Vector2(0.375f, 0.125f));
        }
        else if (texture == TileTexture.WoodSide)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.375f, 0f));
            uv.Add(new Vector2(0.375f, 0.125f));
            uv.Add(new Vector2(0.5f, 0.125f));
            uv.Add(new Vector2(0.5f, 0f));
        }
        else if (texture == TileTexture.WoodTop)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.5f, 0f));
            uv.Add(new Vector2(0.5f, 0.125f));
            uv.Add(new Vector2(0.625f, 0.125f));
            uv.Add(new Vector2(0.625f, 0f));
        }
        else if (texture == TileTexture.Spawner)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.375f, 0.125f));
            uv.Add(new Vector2(0.375f, 0.25f));
            uv.Add(new Vector2(0.5f, 0.25f));
            uv.Add(new Vector2(0.5f, 0.125f));
        }
        else if (texture == TileTexture.CactusSide)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.625f, 0f));
            uv.Add(new Vector2(0.625f, 0.125f));
            uv.Add(new Vector2(0.75f, 0.125f));
            uv.Add(new Vector2(0.75f, 0f));
        }
        else if (texture == TileTexture.CactusTop)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.75f, 0f));
            uv.Add(new Vector2(0.75f, 0.125f));
            uv.Add(new Vector2(0.875f, 0.125f));
            uv.Add(new Vector2(0.875f, 0f));
        }
        else if (texture == TileTexture.CactusBottom)
        {
            AddVerticesAndTriangles(v1, v2, v3, v4);
            uv.Add(new Vector2(0.875f, 0f));
            uv.Add(new Vector2(0.875f, 0.125f));
            uv.Add(new Vector2(1f, 0.125f));
            uv.Add(new Vector2(1f, 0f));
        }
        //Profiler.EndSample();
    }

    void AddVerticesAndTriangles(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        triangles.Add(0 + vertices.Length);
        triangles.Add(1 + vertices.Length);
        triangles.Add(2 + vertices.Length);
        triangles.Add(0 + vertices.Length);
        triangles.Add(2 + vertices.Length);
        triangles.Add(3 + vertices.Length);

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);

    }

    void AddVerticesAndTrianglesLeaves(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Side side)
    {
        trianglesLeaves.Add(0 + verticesLeaves.Length);
        trianglesLeaves.Add(1 + verticesLeaves.Length);
        trianglesLeaves.Add(2 + verticesLeaves.Length);
        trianglesLeaves.Add(0 + verticesLeaves.Length);
        trianglesLeaves.Add(2 + verticesLeaves.Length);
        trianglesLeaves.Add(3 + verticesLeaves.Length);

        verticesLeaves.Add(v1);
        verticesLeaves.Add(v2);
        verticesLeaves.Add(v3);
        verticesLeaves.Add(v4);
        foliagePosition.Add((v1 + v3) / 2 + new Vector3(xPosition, 0, zPosition));
        foliagePosition.Add((v1 + v3) / 2 + new Vector3(xPosition, 0, zPosition));
        FoliageRotation(side);

    }

    void AddVerticesAndTrianglesWater(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        trianglesWater.Add(0 + verticesWater.Length);
        trianglesWater.Add(1 + verticesWater.Length);
        trianglesWater.Add(2 + verticesWater.Length);
        trianglesWater.Add(0 + verticesWater.Length);
        trianglesWater.Add(2 + verticesWater.Length);
        trianglesWater.Add(3 + verticesWater.Length);

        verticesWater.Add(v1);
        verticesWater.Add(v2);
        verticesWater.Add(v3);
        verticesWater.Add(v4);
    }

}
