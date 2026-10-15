using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id; // 武器id
    public int prefabId; // 预制件id
    public float damage; // 伤害
    public int count; // 武器数量
    public float speed; // 旋转速度

    float timer;
    Player player;

    void Awake()
    {
        player = GameManager.instance.player;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isLive)
        {
            return;
        }

        switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;

            default:
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.damage = damage * Character.Damage;
        this.count += count;

        if (id == 0)
            Batch();

        Gear[] gears = player.GetComponentsInChildren<Gear>();
        foreach (Gear gear in gears)
        {
            gear.ApplyGear();
        }
    }

    /// <summary>满级后的溢出升级：按比例提升伤害</summary>
    public void OverflowLevelUp(float damageBonus)
    {
        this.damage *= (1f + damageBonus);

        if (id == 0)
            Batch();
    }

    public void Init(ItemData data)
    {
        // 基础属性
        name = "Weapon " + data.itemId;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;
        // 属性
        id = data.itemId;
        damage = data.baseDamage * Character.Damage;
        count = data.baseCount + Character.Count;

        // 初始化为 -1 以便检测查找失败，避免在 prefabId=0（默认值）时误生成错误预制体
        prefabId = -1;
        for (int index = 0; index < GameManager.instance.pool.prefabs.Length; index++)
        {
            if (data.projectile == GameManager.instance.pool.prefabs[index])
            {
                prefabId = index;
                break;
            }
        }
        if (prefabId == -1)
        {
            string projName = data.projectile != null ? data.projectile.name : "null";
            Debug.LogError("[Weapon] 武器 " + data.itemId + " 的预制体未在 PoolManager 中注册：" + projName);
            return;
        }


        switch (id)
        {
            case 0:
                speed = 150 * Character.WeaponSpeed;
                Batch();
                break;
            default:
                speed = 0.35f * Character.WeaponRate;
                break;
        }

        Gear[] gears = player.GetComponentsInChildren<Gear>();
        foreach (Gear gear in gears)
        {
            gear.ApplyGear();
        }

        // hand set
        Hand hand = player.hands[(int)data.itemType];
        hand.spriter.sprite = data.hand;
        hand.gameObject.SetActive(true);

    }

    // 放置
    void Batch()
    {
        for (int index = 0; index < count; index++)
        {
            Transform bullet;

            if (index < transform.childCount)
            {
                bullet = transform.GetChild(index);
            }
            else
            {
                bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            // 旋转角度
            Vector3 rotVec = Vector3.forward * 360 * index / count;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World); // 根据旋转方向向外移动


            float finalDmg = damage * GameManager.instance.DamageMultiplier;
            bullet.GetComponent<Bullet>().Init(finalDmg, -100, Vector3.zero); // Infinity Per



        }
    }

    void Fire()
    {
        if (!player.scanner.nearsetTarget)
            return;

        // 计算方向
        Vector3 targetPos = player.scanner.nearsetTarget.position;
        Vector3 dir = targetPos - transform.position;
        dir = dir.normalized;

        // 多发射击：count越高，同时射出的子弹越多(带扇形散射)，上限5发
        int bulletCount = Mathf.Clamp(Mathf.FloorToInt(count / 2f) + 1, 1, 5);
        float spreadAngle = 15f; // 每颗子弹间隔角度
        float startAngle = -(bulletCount - 1) * spreadAngle * 0.5f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + i * spreadAngle;
            Vector3 spreadDir = Quaternion.Euler(0, 0, angle) * dir;

            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.position = transform.position;
            bullet.rotation = Quaternion.FromToRotation(Vector3.up, spreadDir);
            float finalDmg = damage * GameManager.instance.DamageMultiplier;
            // 穿透力 = count，保证每颗子弹都能穿过多个敌人
            bullet.GetComponent<Bullet>().Init(finalDmg, count, spreadDir);
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }
}
