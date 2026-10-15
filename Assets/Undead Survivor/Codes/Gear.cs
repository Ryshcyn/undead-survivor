using UnityEngine;

public class Gear : MonoBehaviour
{
    const float BaseSpinSpeed = 150f;
    const float BaseFireInterval = 0.35f;
    const float BaseMoveSpeed = 3f;

    public ItemData.ItemType type;
    public float rate;

    public void Init(ItemData data)
    {
        // 基础设置
        name = "Gear " + data.itemId;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;

        // property set
        type = data.itemType;
        rate = data.damages[0];
        ApplyGear();
    }

    public void LevelUp(float rate)
    {
        this.rate = rate;
        ApplyGear();
    }

    public void ApplyGear()
    {
        switch (type)
        {
            case ItemData.ItemType.Glove:
                RateUp();
                break;
            case ItemData.ItemType.Shoe:
                SpeedUp();
                break;
        }
    }

    void RateUp()
    {
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            switch (weapon.id)
            {
                case 0:
                    weapon.speed = Character.WeaponSpeed * BaseSpinSpeed * (1f + rate);
                    break;

                default:
                    // 射速下限0.1秒，防止过快导致卡顿
                    float fireInterval = Character.WeaponRate * BaseFireInterval * (1f - rate);
                    weapon.speed = Mathf.Max(fireInterval, 0.1f);
                    break;
            }
        }
    }

    void SpeedUp()
    {
        float speed = 3 * Character.Speed;
        GameManager.instance.player.speed = BaseMoveSpeed * (1f + rate);
    }
}
