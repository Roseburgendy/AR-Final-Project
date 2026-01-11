using UnityEngine;

namespace LX_Game
{
    /// <summary>
    /// 实时边界检测器 - 支持随物体移动的边界
    /// 强行将坐标限制在移动的 BoxCollider 范围内
    /// </summary>
    public class LX_BoundaryChecker : MonoBehaviour
    {
        [Header("边界设置")]
        [Tooltip("作为活动范围的 Box Collider（可以随父物体移动）")]
        public BoxCollider boundaryCollider;

        [Tooltip("边界内缩距离（米），防止角色完全贴在边缘")]
        public float boundaryPadding = 0.1f;

        [Header("调试")]
        public bool showDebugInfo = true;

        /// <summary>
        /// 实时获取并限制位置（核心方法）
        /// </summary>
        public Vector3 ClampPosition(Vector3 targetPosition)
        {
            if (boundaryCollider == null) return targetPosition;

            // 【实时检测核心】：每帧获取最新的 Bounds
            // bounds 属性获取的是该 Collider 在世界空间中的最新 AABB 包围盒
            Bounds currentBounds = boundaryCollider.bounds;

            // 计算考虑了 Padding 后的实时限制区间
            float minX = currentBounds.min.x + boundaryPadding;
            float maxX = currentBounds.max.x - boundaryPadding;
            float minZ = currentBounds.min.z + boundaryPadding;
            float maxZ = currentBounds.max.z - boundaryPadding;

            // 安全性检查：防止 Padding 过大导致 min > max
            if (minX > maxX) minX = maxX = currentBounds.center.x;
            if (minZ > maxZ) minZ = maxZ = currentBounds.center.z;

            // 强行锁定坐标
            Vector3 clampedPos = targetPosition;
            clampedPos.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            clampedPos.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
            
            // 如果需要 Y 轴也跟随边界（例如在斜坡上），可以取消下行注释
            // clampedPos.y = Mathf.Clamp(targetPosition.y, currentBounds.min.y, currentBounds.max.y);

            return clampedPos;
        }

        /// <summary>
        /// 辅助方法：获取移动后的安全位置
        /// </summary>
        public Vector3 GetSafePosition(Vector3 currentPosition, Vector3 moveVector)
        {
            return ClampPosition(currentPosition + moveVector);
        }

        void OnDrawGizmos()
        {
            if (boundaryCollider == null) return;

            // 实时在 Scene 窗口绘制“缩水”后的安全活动区
            Bounds b = boundaryCollider.bounds;
            Vector3 safeSize = new Vector3(
                Mathf.Max(0, b.size.x - boundaryPadding * 2),
                b.size.y,
                Mathf.Max(0, b.size.z - boundaryPadding * 2)
            );

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(b.center, safeSize);
            
            // 绘制原始边界
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawCube(b.center, b.size);
        }
    }
}