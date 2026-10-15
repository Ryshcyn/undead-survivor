using UnityEngine;

public class Follow : MonoBehaviour
{
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        // 空引用防护：场景中可能尚未存在 Main Camera 或 Player（场景切换瞬间）
        if (Camera.main == null) return;
        if (GameManager.instance == null || GameManager.instance.player == null) return;

        rect.position = Camera.main.WorldToScreenPoint(GameManager.instance.player.transform.position);
    }
}
