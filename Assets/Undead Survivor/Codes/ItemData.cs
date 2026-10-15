using UnityEngine;
using UnityEngine.Serialization;

// 创建菜单
[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/ItemData")]
public class ItemData : ScriptableObject
{
    public enum ItemType { Melee, Range, Glove, Shoe, Heal }

    [Header("# Item Info")]
    public ItemType itemType;
    [FormerlySerializedAs("itemID")]
    public int itemId;
    public string itemName;
    [TextArea]
    public string itemDesc; // 物品描述
    public Sprite itemIcon;


    [Header("# Level Data")]
    public float baseDamage; // 基础伤害
    public int baseCount; // 基础数量
    public float[] damages; // 每级伤害
    public int[] counts; // 每级数量


    [Header("# Weapon")]
    public GameObject projectile;
    public Sprite hand;
}
