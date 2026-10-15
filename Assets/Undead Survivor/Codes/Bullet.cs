using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int per; //穿透力
    float lifeTimer;
    const float MaxLifeTime = 5f; // 子弹最大存活时间

    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        lifeTimer = 0f;
    }

    void Update()
    {
        if (per == -100) return; // 旋转武器不计时

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= MaxLifeTime)
        {
            if (rigid)
                rigid.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }

    public void Init(float damage, int per, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;
        lifeTimer = 0f;

        if (rigid && per >= 0)
        {
            rigid.linearVelocity = dir * 22f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        if (per == -100)
            return;

        per--;

        if (per < 0)
        {
            if (rigid)
                rigid.linearVelocity = Vector2.zero;

            gameObject.SetActive(false);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area") || per == -100)
            return;

        gameObject.SetActive(false);
    }
}
