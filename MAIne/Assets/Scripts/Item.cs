using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public PlayerController.ItemID id;
    public bool isBlock;
    [ConditionalField("isBlock")] public bool isNotCube;
    [ConditionalField("isBlock")] public BlockType type;
    [ConditionalField("isBlock")] public float reloadSpeed;
    public bool isHeal;
    [ConditionalField("isHeal")] public int heal;
    public Sprite sprite;
    public GameObject mesh;
    public int maxStack;
    public int damage;
}
