using UnityEngine;

public class Result : MonoBehaviour
{
    public GameObject[] titles;

    public void Lose()
    {
        titles[0].SetActive(true);
        titles[1].SetActive(false);
    }

    public void Win()
    {
        titles[0].SetActive(false);
        titles[1].SetActive(true);
    }

    public void OnClickRetry()
    {
        GameManager.instance.GameRetry();
    }

    public void OnClickReselect()
    {
        GameManager.instance.BackToCharSelect();
    }

    public void OnClickQuit()
    {
        GameManager.instance.QuitGame();
    }
}
