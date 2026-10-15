using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public float levelTime;

    [Header("# Difficulty Scaling")]
    [Tooltip("难度阶段之间的过渡时间(秒)")]
    public float transitionDuration = 5f;
    [Tooltip("每个阶段同时生成的额外敌人数(随阶段递增)")]
    public int extraSpawnPerLevel = 1;
    [Tooltip("精英敌人生成概率(0-1)，随阶段递增")]
    public float eliteBaseChance = 0.05f;
    [Tooltip("精英敌人属性倍率")]
    public float eliteHealthMult = 3f;
    public float eliteSpeedMult = 0.8f;
    public float eliteSizeMult = 1.4f;

    [Header("# Dynamic Difficulty")]
    [Tooltip("启用基于玩家表现的动态难度调节")]
    public bool enableAdaptive = true;
    [Tooltip("动态难度系数范围")]
    public float adaptiveMin = 0.7f;
    public float adaptiveMax = 1.3f;

    int level; // 当前关卡等级
    float timer;
    float adaptiveFactor = 1f; // 动态难度系数
    float performanceCheckTimer;

    public int Level => level;

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
        levelTime = GameManager.instance.maxGameTime / spawnData.Length;
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
        {
            return;
        }

        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / levelTime), spawnData.Length - 1);

        // 动态难度评估(每5秒)
        if (enableAdaptive)
        {
            performanceCheckTimer += Time.deltaTime;
            if (performanceCheckTimer >= 5f)
            {
                performanceCheckTimer = 0f;
                UpdateAdaptiveDifficulty();
            }
        }

        float currentSpawnTime = GetInterpolatedSpawnTime() * adaptiveFactor;
        if (timer > currentSpawnTime)
        {
            timer = 0;
            Spawn();

            // 高阶段额外生成敌人
            int bonusSpawns = Mathf.FloorToInt(level * extraSpawnPerLevel * 0.5f);
            for (int i = 0; i < bonusSpawns; i++)
            {
                Spawn();
            }
        }
    }

    // 阶段间平滑过渡生成间隔
    float GetInterpolatedSpawnTime()
    {
        if (level >= spawnData.Length - 1)
            return spawnData[spawnData.Length - 1].spawnTime;

        float timeInLevel = GameManager.instance.gameTime - level * levelTime;
        float transitionStart = levelTime - transitionDuration;

        if (timeInLevel < transitionStart)
            return spawnData[level].spawnTime;

        // 在过渡区间内平滑插值到下一阶段
        float t = (timeInLevel - transitionStart) / transitionDuration;
        return Mathf.Lerp(spawnData[level].spawnTime, spawnData[level + 1].spawnTime, t);
    }

    // 基于玩家表现调节难度
    void UpdateAdaptiveDifficulty()
    {
        float healthRatio = GameManager.instance.health / GameManager.instance.maxHealth;

        // 血量低→降低难度，血量高→提升难度
        if (healthRatio < 0.3f)
            adaptiveFactor = Mathf.Min(adaptiveFactor + 0.05f, adaptiveMax); // 生成间隔变长=更简单
        else if (healthRatio > 0.8f)
            adaptiveFactor = Mathf.Max(adaptiveFactor - 0.05f, adaptiveMin); // 生成间隔变短=更难
        else
            adaptiveFactor = Mathf.MoveTowards(adaptiveFactor, 1f, 0.02f); // 回归基准
    }

    void Spawn()
    {
        GameObject enemy = GameManager.instance.pool.Get(0);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        Enemy enemyComp = enemy.GetComponent<Enemy>();

        // 插值计算当前敌人属性
        SpawnData data = GetInterpolatedData();

        // 精英敌人判定
        float eliteChance = eliteBaseChance + level * 0.03f;
        bool isElite = Random.value < eliteChance;

        if (isElite)
        {
            enemyComp.Init(data, true, eliteHealthMult, eliteSpeedMult, eliteSizeMult);
        }
        else
        {
            enemyComp.Init(data);
        }
    }

    // 平滑插值当前敌人属性
    SpawnData GetInterpolatedData()
    {
        if (level >= spawnData.Length - 1)
            return spawnData[spawnData.Length - 1];

        float timeInLevel = GameManager.instance.gameTime - level * levelTime;
        float transitionStart = levelTime - transitionDuration;

        if (timeInLevel < transitionStart)
            return spawnData[level];

        float t = (timeInLevel - transitionStart) / transitionDuration;
        SpawnData a = spawnData[level];
        SpawnData b = spawnData[level + 1];

        SpawnData result = new SpawnData();
        result.spriteType = t < 0.5f ? a.spriteType : b.spriteType;
        result.spawnTime = Mathf.Lerp(a.spawnTime, b.spawnTime, t);
        result.health = Mathf.RoundToInt(Mathf.Lerp(a.health, b.health, t));
        result.speed = Mathf.Lerp(a.speed, b.speed, t);
        return result;
    }
}

// 序列化 生成数据类
[System.Serializable]
public class SpawnData
{
    public int spriteType;
    public float spawnTime;
    public int health;
    public float speed;
}
