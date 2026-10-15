using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanRange;
    public LayerMask targetLayer;
    public RaycastHit2D[] targets;
    public Transform nearsetTarget;

    // 性能优化：不必每 FixedUpdate 都扫描，按固定间隔扫描即可
    const float SCAN_INTERVAL = 0.1f;
    float scanTimer;

    void FixedUpdate()
    {
        scanTimer += Time.fixedDeltaTime;
        if (scanTimer < SCAN_INTERVAL) return;
        scanTimer = 0f;

        targets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, targetLayer);
        nearsetTarget = GetNearest();
    }

    Transform GetNearest()
    {
        Transform result = null;
        float diff = 100;
        foreach (RaycastHit2D target in targets)
        {
            Vector3 myPos = transform.position;
            Vector3 targetPos = target.transform.position;
            float curDiff = Vector3.Distance(myPos, targetPos);

            if (curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }

        return result;
    }

}
