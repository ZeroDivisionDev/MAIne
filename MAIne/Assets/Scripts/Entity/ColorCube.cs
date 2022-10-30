using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorCube : MonoBehaviour
{
    List<Vector2> uv;

    public void ApplyUV(BlockType blockType)
    {
        uv = new List<Vector2>();

        List<TileTexture> allTexture = new List<TileTexture>();
        if (blockType == BlockType.Grass)
        {
            allTexture = new List<TileTexture>()
            {
                TileTexture.GrassSide,TileTexture.Grass, TileTexture.Dirt
            };
        }
        else if (blockType == BlockType.Dirt)
        {
            allTexture = new List<TileTexture>()
            {
                TileTexture.Dirt,TileTexture.Dirt,TileTexture.Dirt
            };
        }
        else if (blockType == BlockType.Stone)
        {
            allTexture = new List<TileTexture>()
            {
                TileTexture.Stone,TileTexture.Stone,TileTexture.Stone
            };
        }
        else if (blockType == BlockType.Wood)
        {
            allTexture = new List<TileTexture>()
            {
                TileTexture.WoodSide,TileTexture.WoodTop,TileTexture.WoodTop
            };
        }
        else if (blockType == BlockType.Sand)
        {
            allTexture = new List<TileTexture>()
            {
                TileTexture.Sand,TileTexture.Sand,TileTexture.Sand
            };
        }
        else if (blockType == BlockType.Leaves)
        {
            allTexture = new List<TileTexture>()
            {
                TileTexture.Leaves,TileTexture.Leaves,TileTexture.Leaves
            };
        }
        else if (blockType == BlockType.Cactus)
        {
            allTexture = new List<TileTexture>()
            {
                TileTexture.CactusSide,TileTexture.CactusTop,TileTexture.CactusBottom
            };
        }

        //Applying the side
        ApplyOneFace(allTexture[0]);
        ApplyOneFace(allTexture[0]);
        ApplyOneFace(allTexture[0]);
        ApplyOneFace(allTexture[0]);
        //Applying Top
        ApplyOneFace(allTexture[1]);
        //Applying bottom
        ApplyOneFace(allTexture[2]);

        GetComponent<MeshFilter>().mesh.uv = uv.ToArray();
    }

    private void ApplyOneFace(TileTexture texture)
    {
        if (texture == TileTexture.Grass)
        {
            uv.Add(new Vector2(0f, 0f));
            uv.Add(new Vector2(0f, 0.125f));
            uv.Add(new Vector2(0.125f, 0.125f));
            uv.Add(new Vector2(0.125f, 0f));
        }
        else if (texture == TileTexture.Stone)
        {
            uv.Add(new Vector2(0f, 0.125f));
            uv.Add(new Vector2(0f, 0.25f));
            uv.Add(new Vector2(0.125f, 0.25f));
            uv.Add(new Vector2(0.125f, 0.125f));
        }
        else if (texture == TileTexture.GrassSide)
        {
            uv.Add(new Vector2(0.125f, 0f));
            uv.Add(new Vector2(0.125f, 0.125f));
            uv.Add(new Vector2(0.25f, 0.125f));
            uv.Add(new Vector2(0.25f, 0f));
        }
        else if (texture == TileTexture.Sand)
        {
            uv.Add(new Vector2(0.5f, 0.125f));
            uv.Add(new Vector2(0.5f, 0.25f));
            uv.Add(new Vector2(0.625f, 0.25f));
            uv.Add(new Vector2(0.625f, 0.125f));
        }
        else if (texture == TileTexture.Dirt)
        {
            uv.Add(new Vector2(0.25f, 0f));
            uv.Add(new Vector2(0.25f, 0.125f));
            uv.Add(new Vector2(0.375f, 0.125f));
            uv.Add(new Vector2(0.375f, 0f));
        }
        else if (texture == TileTexture.Bedrock)
        {
            uv.Add(new Vector2(0.625f, 0.125f));
            uv.Add(new Vector2(0.625f, 0.25f));
            uv.Add(new Vector2(0.75f, 0.25f));
            uv.Add(new Vector2(0.75f, 0.125f));
        }
        else if (texture == TileTexture.Leaves)
        {
            uv.Add(new Vector2(0.125f, 0.125f));
            uv.Add(new Vector2(0.125f, 0.25f));
            uv.Add(new Vector2(0.25f, 0.25f));
            uv.Add(new Vector2(0.25f, 0.125f));
        }
        else if (texture == TileTexture.WoodSide)
        {
            uv.Add(new Vector2(0.375f, 0f));
            uv.Add(new Vector2(0.375f, 0.125f));
            uv.Add(new Vector2(0.5f, 0.125f));
            uv.Add(new Vector2(0.5f, 0f));
        }
        else if (texture == TileTexture.WoodTop)
        {
            uv.Add(new Vector2(0.5f, 0f));
            uv.Add(new Vector2(0.5f, 0.125f));
            uv.Add(new Vector2(0.625f, 0.125f));
            uv.Add(new Vector2(0.625f, 0f));
        }
        else if (texture == TileTexture.Spawner)
        {
            uv.Add(new Vector2(0.375f, 0.125f));
            uv.Add(new Vector2(0.375f, 0.25f));
            uv.Add(new Vector2(0.5f, 0.25f));
            uv.Add(new Vector2(0.5f, 0.125f));
        }
        else if (texture == TileTexture.CactusSide)
        {
            uv.Add(new Vector2(0.625f, 0f));
            uv.Add(new Vector2(0.625f, 0.125f));
            uv.Add(new Vector2(0.75f, 0.125f));
            uv.Add(new Vector2(0.75f, 0f));
        }
        else if (texture == TileTexture.CactusTop)
        {
            uv.Add(new Vector2(0.75f, 0f));
            uv.Add(new Vector2(0.75f, 0.125f));
            uv.Add(new Vector2(0.875f, 0.125f));
            uv.Add(new Vector2(0.875f, 0f));
        }
        else if (texture == TileTexture.CactusBottom)
        {
            uv.Add(new Vector2(0.875f, 0f));
            uv.Add(new Vector2(0.875f, 0.125f));
            uv.Add(new Vector2(1f, 0.125f));
            uv.Add(new Vector2(1f, 0f));
        }
    }
}
