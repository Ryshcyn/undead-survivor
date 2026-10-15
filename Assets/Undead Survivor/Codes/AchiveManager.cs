using System;
using System.Collections;
using UnityEngine;

public class AchiveManager : MonoBehaviour
{
    public GameObject[] lockCharacters;
    public GameObject[] unlockCharacters;
    public GameObject uiNotice;

    enum Achive
    {
        UnlockCharacter1,
        UnlockCharacter2
    }
    Achive[] achives;
    WaitForSecondsRealtime wait;
    Coroutine noticeRoutine;

    void Awake()
    {
        achives = (Achive[])Enum.GetValues(typeof(Achive));
        wait = new WaitForSecondsRealtime(5);

        if (!PlayerPrefs.HasKey("MyData"))
        {
            Init();
        }
    }

    void Start()
    {
        uiNotice.SetActive(false);
        UnlockCharacter();
    }

    void Init()
    {
        PlayerPrefs.SetInt("MyData", 1);

        foreach (Achive achive in achives)
        {
            PlayerPrefs.SetInt(achive.ToString(), 0);
        }

        PlayerPrefs.Save();
    }

    // 公开方法：将该方法绑定到 UI Button 的 OnClick 即可重置全部解锁存档
    public void ResetAchive()
    {
        // 清除成就相关的 PlayerPrefs
        foreach (Achive achive in achives)
        {
            PlayerPrefs.DeleteKey(achive.ToString());
        }
        PlayerPrefs.DeleteKey("MyData");
        PlayerPrefs.Save();

        // 重新初始化并刷新角色选择 UI
        Init();
        UnlockCharacter();

        // 复用通知 UI 提示一下（可选）
        AudioManager.instance?.PlaySfx(AudioManager.Sfx.Select);
    }

    void UnlockCharacter()
    {
        for (int index = 0; index < lockCharacters.Length; index++)
        {
            string achiveName = achives[index].ToString();
            bool isUnlock = PlayerPrefs.GetInt(achiveName) == 1;
            lockCharacters[index].SetActive(!isUnlock);
            unlockCharacters[index].SetActive(isUnlock);
        }
    }

    void LateUpdate()
    {
        foreach (Achive achive in achives)
        {
            CheckAchive(achive);
        }
    }

    void CheckAchive(Achive achive)
    {
        bool isAchive = false;

        switch (achive)
        {
            case Achive.UnlockCharacter1:
                // Check conditions for UnlockCharacter1
                isAchive = GameManager.instance.kill >= 1000;
                break;
            case Achive.UnlockCharacter2:
                // Check conditions for UnlockCharacter2
                // 浮点比较容差：避免直接 == 比较带来的精度问题
                isAchive = Mathf.Abs(GameManager.instance.gameTime - GameManager.instance.maxGameTime) < 0.01f;
                break;
        }

        if (isAchive && PlayerPrefs.GetInt(achive.ToString()) == 0)
        {
            PlayerPrefs.SetInt(achive.ToString(), 1);
            PlayerPrefs.Save();

            for (int index = 0; index < uiNotice.transform.childCount; index++)
            {
                bool isActive = index == (int)achive;
                uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);
            }

            UnlockCharacter();

            if (noticeRoutine != null)
            {
                StopCoroutine(noticeRoutine);
            }
            noticeRoutine = StartCoroutine(NoticeRoutine());
        }
    }

    IEnumerator NoticeRoutine()
    {
        uiNotice.SetActive(true);
        AudioManager.instance?.PlaySfx(AudioManager.Sfx.LevelUp);
        yield return wait;
        uiNotice.SetActive(false);
        noticeRoutine = null;
    }
}
