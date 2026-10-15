using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public ItemData data;
    public int level;
    public Weapon weapon;
    public Gear gear;

    Image icon;
    Text textLevel;
    Text textName;
    Text textDesc;

    void Awake()
    {
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.itemIcon;

        Text[] texts = GetComponentsInChildren<Text>();
        textLevel = texts[0];
        textName = texts[1];
        textDesc = texts[2];
        textName.text = data.itemName;
        textDesc.text = data.itemDesc;
    }

    void OnEnable()
    {
        textLevel.text = "Lv." + (level + 1);

        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                if (level < data.damages.Length)
                    textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100, data.counts[level]);
                else
                    textDesc.text = "伤害 +25%";
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (level < data.damages.Length)
                    textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100);
                else
                    textDesc.text = "效果 +10%";
                break;
            default:
                textDesc.text = string.Format(data.itemDesc);
                break;
        }
    }



    public void OnClick()
    {
        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                if (level == 0)
                {
                    GameObject newWeapon = new GameObject();
                    weapon = newWeapon.AddComponent<Weapon>();
                    weapon.Init(data);
                }
                else if (level < data.damages.Length)
                {
                    float nextDamage = data.baseDamage;
                    int nextCount = 0;

                    nextDamage += data.baseDamage * data.damages[level];
                    nextCount += data.counts[level];

                    weapon.LevelUp(nextDamage, nextCount);
                }
                else
                {
                    // 溢出升级：每次+25%当前伤害
                    weapon.OverflowLevelUp(0.25f);
                }
                level++;
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (level == 0)
                {
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data);
                }
                else if (level < data.damages.Length)
                {
                    float nextRate = data.damages[level];
                    gear.LevelUp(nextRate);
                }
                else
                {
                    // 溢出升级：效果+10%，上限80%
                    gear.LevelUp(Mathf.Min(gear.rate + 0.1f, 0.8f));
                }
                level++;
                break;
            case ItemData.ItemType.Heal:
                GameManager.instance.health = GameManager.instance.MaxHealthWithLevel;
                break;
        }

        // 不再禁用按钮，允许无限升级
    }
}
