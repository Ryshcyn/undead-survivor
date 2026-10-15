using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // 单例模式

    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 5 * 60f; // 5分钟

    [Header("# Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 3, 5, 8, 12, 18, 25, 35, 50, 70, 100, 135, 175, 220, 280, 350, 430, 520, 620, 740, 880 };

    [Header("# Growth Per Level")]
    [Tooltip("每级伤害加成(8%=0.08)")]
    public float damagePerLevel = 0.08f;
    [Tooltip("每级最大生命加成")]
    public float healthPerLevel = 5f;
    [Tooltip("每秒自然回血量(基础)")]
    public float baseRegen = 0f;
    [Tooltip("每级回血增量")]
    public float regenPerLevel = 0.15f;

    /// <summary>全局伤害倍率，随等级成长</summary>
    public float DamageMultiplier => 1f + level * damagePerLevel;

    /// <summary>当前最大生命值(含等级加成)</summary>
    public float MaxHealthWithLevel => maxHealth + level * healthPerLevel;

    [Header("# Game Object")]
    public PoolManager pool;
    public Player player;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public GameObject enemyCleaner;
    public float victoryCleanerDelay = 0.35f;

    [Header("# UI Panels")]
    public GameObject uiMainMenu;       // 主菜单面板(标题+开始按钮)
    public GameObject uiCharSelect;     // 角色选择面板
    public GameObject uiHUD;            // 游戏中HUD

    void Awake()
    {
        instance = this;
    }

    /// <summary>主菜单点击"开始游戏"按钮</summary>
    public void OnClickStart()
    {
        uiMainMenu.SetActive(false);
        uiCharSelect.SetActive(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    /// <summary>选角界面点击"返回"按钮</summary>
    public void OnClickBackToMenu()
    {
        uiCharSelect.SetActive(false);
        uiMainMenu.SetActive(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    /// <summary>选择角色并开始游戏(角色按钮调用)</summary>
    public void GameStart(int id)
    {
        playerId = id;
        health = maxHealth;

        uiCharSelect.SetActive(false);
        uiHUD.SetActive(true);

        player.gameObject.SetActive(true);
        SetEnemyCleanerActive(false);
        uiLevelUp.Select(playerId % 2);
        Resume();

        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;
        SetEnemyCleanerActive(false);
        ClearAllEnemies();
        yield return new WaitForSeconds(0.5f);
        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }
    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        SetEnemyCleanerActive(true);

        yield return new WaitForSeconds(victoryCleanerDelay);

        ClearAllEnemies();
        SetEnemyCleanerActive(false);

        yield return new WaitForSeconds(0.15f);
        uiResult.gameObject.SetActive(true);
        uiResult.Win();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }

    void ClearAllEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                enemy.gameObject.SetActive(false);
            }
        }
    }

    void SetEnemyCleanerActive(bool isActive)
    {
        if (enemyCleaner != null)
        {
            enemyCleaner.SetActive(isActive);
        }
    }

    public void GameRetry()
    {
        PlayerPrefs.SetInt("BackToCharSelect", 1);
        PlayerPrefs.SetInt("RetryWithId", playerId);
        SceneManager.LoadScene(0);
    }

    /// <summary>返回角色选择界面(重载场景，显示选角)</summary>
    public void BackToCharSelect()
    {
        PlayerPrefs.SetInt("BackToCharSelect", 1);
        SceneManager.LoadScene(0);
    }

    /// <summary>退出游戏</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Start()
    {
        // 如果是从结算界面返回选角
        if (PlayerPrefs.GetInt("BackToCharSelect", 0) == 1)
        {
            PlayerPrefs.SetInt("BackToCharSelect", 0);

            // Retry：用上次的角色直接开始
            int retryId = PlayerPrefs.GetInt("RetryWithId", -1);
            if (retryId >= 0)
            {
                PlayerPrefs.SetInt("RetryWithId", -1);
                if (uiMainMenu != null) uiMainMenu.SetActive(false);
                if (uiCharSelect != null) uiCharSelect.SetActive(false);
                GameStart(retryId);
            }
            else
            {
                // 重新选角
                if (uiMainMenu != null) uiMainMenu.SetActive(false);
                if (uiCharSelect != null) uiCharSelect.SetActive(true);
            }
        }
    }

    void Update()
    {
        if (!isLive)
        {
            return;
        }

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictory();
        }

        // 自然回血
        float regen = baseRegen + level * regenPerLevel;
        if (regen > 0)
        {
            health = Mathf.Min(health + regen * Time.deltaTime, MaxHealthWithLevel);
        }
    }

    public void GetExp()
    {
        if (!isLive)
        {
            return;
        }
        exp++;

        if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;
            exp = 0;
            // 升级回复30%最大生命(不超过上限)
            health = Mathf.Min(health + MaxHealthWithLevel * 0.3f, MaxHealthWithLevel);
            uiLevelUp.show();
        }
    }

    public void Stop()
    {
        isLive = false;
        SetEnemyCleanerActive(false);
        Time.timeScale = 0;
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
    }


}
