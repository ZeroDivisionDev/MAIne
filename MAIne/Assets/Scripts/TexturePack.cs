using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Texture Pack", menuName = "TeckturePack")]
public class TexturePack : ScriptableObject
{
    public Texture2D fullTexture;
    public Texture2D playerSkin;
    public Texture2D zombieSkin;
    public Texture foliageTexture;
    public Texture2D[] particlesTexture;
    public Sprite[] itemsSprite;
}
