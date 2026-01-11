using UnityEngine;

public class DuckController : MonoBehaviour
{
    [Header("位置参考点 (直接拖入物体)")]
    public Transform startTarget;  // 拖入 A 物体
    public Transform endTarget;    // 拖入 B 物体

    [Header("缩放设置")]
    public Vector3 scaleA = new Vector3(1, 1, 1);
    public Vector3 scaleB = new Vector3(2, 2, 2);

    [Header("运动设置")]
    public float speed = 0.01f;
    [Range(0, 1)] public float progress = 0f;
    public bool isMoving = true;

    void Update()
    {
        // 安全检查：确保你已经拖入了物体，否则报错
        if (startTarget == null || endTarget == null) return;

        if (isMoving)
        {
            // 计算进度
            progress += Time.deltaTime * speed;
            progress = Mathf.Clamp01(progress);

            // 使用拖入物体的 position 进行插值
            transform.position = Vector3.Lerp(startTarget.position, endTarget.position, progress);

            // 同时处理缩放
            transform.localScale = Vector3.Lerp(scaleA, scaleB, progress);

            if (progress >= 1f) isMoving = false;
        }
    }
}