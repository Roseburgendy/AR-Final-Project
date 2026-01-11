using UnityEngine;
using UnityEngine.AI;

namespace LX_Game
{
    /// <summary>
    /// 狗的AI控制器 - 血量系统版本
    /// 从A点跑向B点，被攻击会躲避，5次击中后死亡
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class DogAIController : MonoBehaviour
    {
        [Header("目标点")]
        public Transform pointA; // 起点（猎人位置）
        public Transform pointB; // 终点（目标位置）

        [Header("移动设置")]
        public float moveSpeed = 2f; // 移动速度
        public float rotationSpeed = 5f; // 转向速度
        public float arrivalThreshold = 0.5f; // 判定到达目标的距离阈值

        [Header("战斗设置")]
        public int maxHealth = 5; // 最大血量
        public float detectionRadius = 1.5f; // 检测鸭子攻击的范围
        public float dodgeDistance = 1.5f; // 躲避距离
        public float dodgeDuration = 0.5f; // 躲避动作持续时间
        public float invincibleTime = 0.8f; // 受击后的无敌时间

        [Header("视觉效果")]
        public Renderer dogRenderer; // 狗的渲染器（用于变色）
        public Color hitColor = Color.red; // 被击中时的颜色
        public float hitFlashDuration = 0.3f; // 变色持续时间

        [Header("边界检测")]
        public LX_BoundaryChecker boundaryChecker; // 边界检测器

        private Animator animator;
        private int currentHealth;
        private bool isDead = false;
        private bool reachedB = false;
        private bool canMove = false; // 是否可以移动
        private bool isDodging = false; // 是否正在躲避
        private bool isInvincible = false; // 是否无敌
        private Material dogMaterial;
        private Color originalColor;

        // 动画参数值
        private const int ANIM_IDLE = 0;
        private const int ANIM_WALK = 1;
        private const int ANIM_RUN = 2;
        private const int ANIM_DAMAGE = 6;
        private const int ANIM_DIE = 8;

        void Start()
        {
            animator = GetComponent<Animator>();
            currentHealth = maxHealth;

            // 获取材质用于变色效果
            if (dogRenderer != null)
            {
                dogMaterial = dogRenderer.material;
                originalColor = dogMaterial.color;
            }
            else
            {
                // 尝试自动查找
                dogRenderer = GetComponentInChildren<Renderer>();
                if (dogRenderer != null)
                {
                    dogMaterial = dogRenderer.material;
                    originalColor = dogMaterial.color;
                }
            }

            // 如果没有设置点A，使用当前位置作为点A
            if (pointA == null)
            {
                GameObject pointAObj = new GameObject("Dog_PointA");
                pointAObj.transform.position = transform.position;
                pointA = pointAObj.transform;
            }

            // 自动查找边界检测器
            if (boundaryChecker == null)
            {
                boundaryChecker = FindObjectOfType<LX_BoundaryChecker>();
            }

            // 初始状态：待机，等待开始信号
            SetAnimation(ANIM_IDLE);
            Debug.Log($"狗初始化完成，血量：{currentHealth}/{maxHealth}, canMove={canMove}");
        }

        void Update()
        {
            if (isDead) return;

            // 持续检测是否被鸭子攻击（即使在躲避中也要检测）
            CheckForDuckAttack();

            // 只有在允许移动且不在躲避中时才移动
            if (canMove && !reachedB && !isDodging)
            {
                MoveToPointB();
            }
            else if (isDodging)
            {
                // 躲避中，不进行常规移动
                // Debug.Log($"狗正在躲避中... isDodging={isDodging}");
            }
        }

        /// <summary>
        /// 开始移动（由GameManager调用）
        /// </summary>
        public void StartMoving()
        {
            if (isDead) return;

            canMove = true;
            SetAnimation(ANIM_RUN);
            Debug.Log("狗开始移动！");
        }

        /// <summary>
        /// 向点B移动
        /// </summary>
        void MoveToPointB()
        {
            if (pointB == null) return;

            Vector3 direction = (pointB.position - transform.position);
            direction.y = 0; 
            float distance = direction.magnitude;

            if (distance > arrivalThreshold)
            {
                // 旋转朝向目标点
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // 计算新位置
                Vector3 newPosition = transform.position + direction.normalized * moveSpeed * Time.deltaTime;

                // 边界检测（Kinematic模式下需要手动检测）
                if (boundaryChecker != null)
                {
                    newPosition = boundaryChecker.ClampPosition(newPosition);
                }

                // 移动（Kinematic使用transform）
                transform.position = newPosition;

                // 确保播放跑步动画
                if (!IsAnimationState(ANIM_RUN))
                {
                    SetAnimation(ANIM_RUN);
                    Debug.Log("【MoveToPointB】切换到RUN动画");
                }
            }
            else
            {
                reachedB = true;
                SetAnimation(ANIM_IDLE);
                Debug.Log("狗到达了目标点B！");
            }
        }

        /// <summary>
        /// 检测鸭子的攻击
        /// </summary>
        void CheckForDuckAttack()
        {
            if (isDead || isInvincible) return;

            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            foreach (Collider col in colliders)
            {
                DuckPlayerController duck = col.GetComponent<DuckPlayerController>();
                if (duck != null && duck.IsAttacking())
                {
                    OnHitByDuck(duck.transform.position);
                    break;
                }
            }
        }

        /// <summary>
        /// 被鸭子击中
        /// </summary>
        public void OnHitByDuck(Vector3 attackerPosition)
        {
            if (isDead || isInvincible) return;

            // 减少血量
            currentHealth--;
            Debug.Log($"狗被击中！剩余血量：{currentHealth}/{maxHealth}");

            // 变红效果
            FlashRed();

            // 进入无敌状态
            isInvincible = true;
            Invoke("EndInvincible", invincibleTime);

            // 通知游戏管理器
            LX_GameManager gameManager = FindObjectOfType<LX_GameManager>();
            if (gameManager != null)
            {
                gameManager.OnDogHit();
            }

            // 通知血条UI更新
            LX_DogHealthBar healthBar = FindObjectOfType<LX_DogHealthBar>();
            if (healthBar != null)
            {
                healthBar.UpdateHealth(currentHealth, maxHealth);
            }

            // 检查是否死亡
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 躲避动作
                StartCoroutine(DodgeAway(attackerPosition));
            }
        }

        /// <summary>
        /// 躲避动作协程
        /// </summary>
        System.Collections.IEnumerator DodgeAway(Vector3 attackerPosition)
        {
            isDodging = true;
            Vector3 startPos = transform.position;
            
            Debug.Log($"【狗开始躲避】当前位置: {startPos}, 攻击者位置: {attackerPosition}");

            // 播放受伤动画
            SetAnimation(ANIM_DAMAGE);

            // 计算躲避方向（远离攻击者）
            Vector3 dodgeDirection = (transform.position - attackerPosition).normalized;
            dodgeDirection.y = 0;
            
            Debug.Log($"【狗躲避方向】: {dodgeDirection}, 躲避距离: {dodgeDistance}m, 持续时间: {dodgeDuration}s");

            // 躲避移动
            float elapsed = 0f;
            Vector3 totalMovement = Vector3.zero;
            
            while (elapsed < dodgeDuration)
            {
                Vector3 moveStep = dodgeDirection * (dodgeDistance / dodgeDuration) * Time.deltaTime;
                Vector3 newPosition = transform.position + moveStep;
                
                // 边界检测
                if (boundaryChecker != null)
                {
                    newPosition = boundaryChecker.ClampPosition(newPosition);
                }
                
                Vector3 actualMove = newPosition - transform.position;
                totalMovement += actualMove;
                transform.position = newPosition;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log($"【狗躲避完成】实际移动距离: {totalMovement.magnitude:F2}m, 结束位置: {transform.position}");

            // 躲避结束，恢复状态
            isDodging = false;

            // 如果狗之前到达过B点，检查是否被dodge推开了
            if (reachedB && pointB != null)
            {
                float distanceToB = Vector3.Distance(transform.position, pointB.position);
                if (distanceToB > arrivalThreshold)
                {
                    // 狗被推开了，重置状态让它跑回B点
                    reachedB = false;
                    Debug.Log($"【狗被推离B点】距离: {distanceToB:F2}m，重新前往B点");
                }
                else
                {
                    // 狗还在B点附近，保持待机状态
                    SetAnimation(ANIM_IDLE);
                    Debug.Log($"【狗仍在B点】距离: {distanceToB:F2}m，保持待机");
                    yield break; // 协程中使用 yield break 而不是 return
                }
            }

            // 恢复移动和动画
            if (canMove && !isDead && !reachedB)
            {
                SetAnimation(ANIM_RUN);
                Debug.Log($"【狗恢复移动】canMove={canMove}, isDead={isDead}, reachedB={reachedB}, 当前动画应为RUN");
            }
            else if (!canMove)
            {
                Debug.LogWarning($"【狗无法移动】canMove=false");
            }
        }

        /// <summary>
        /// 结束无敌状态
        /// </summary>
        void EndInvincible()
        {
            isInvincible = false;
        }

        /// <summary>
        /// 变红闪烁效果
        /// </summary>
        void FlashRed()
        {
            if (dogMaterial != null)
            {
                dogMaterial.color = hitColor;
                Invoke("RestoreColor", hitFlashDuration);
            }
        }

        /// <summary>
        /// 恢复原始颜色
        /// </summary>
        void RestoreColor()
        {
            if (dogMaterial != null)
            {
                dogMaterial.color = originalColor;
            }
        }

        /// <summary>
        /// 死亡
        /// </summary>
        void Die()
        {
            if (isDead) return;

            isDead = true;
            canMove = false;
            SetAnimation(ANIM_DIE);
            
            Debug.Log("狗血量归零，死亡！");

            LX_GameManager gameManager = FindObjectOfType<LX_GameManager>();
            if (gameManager != null)
            {
                gameManager.OnDogDied();
            }
        }

        void SetAnimation(int animValue)
        {
            if (animator != null) animator.SetInteger("animation", animValue);
        }

        bool IsAnimationState(int animValue)
        {
            return animator != null && animator.GetInteger("animation") == animValue;
        }

        /// <summary>
        /// 重置狗的状态
        /// </summary>
        public void ResetDog()
        {
            isDead = false;
            isDodging = false;
            isInvincible = false;
            reachedB = false;
            canMove = false;
            currentHealth = maxHealth;
            
            if (pointA != null) transform.position = pointA.position;
            SetAnimation(ANIM_IDLE);

            // 恢复颜色
            if (dogMaterial != null)
            {
                dogMaterial.color = originalColor;
            }

            Debug.Log($"狗已重置，血量：{currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// 被强风吹飞效果（终结技）
        /// </summary>
        public void GetBlownAway(Vector3 direction, float force)
        {
            StartCoroutine(BlowAwayEffect(direction, force));
        }

        /// <summary>
        /// 吹飞效果协程
        /// </summary>
        System.Collections.IEnumerator BlowAwayEffect(Vector3 direction, float force)
        {
            isDead = true;
            canMove = false;
            float duration = 2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // 旋转飞出
                transform.Rotate(Vector3.up, 720f * Time.deltaTime);
                transform.Rotate(Vector3.right, 360f * Time.deltaTime);
                
                // 向远处飞
                transform.position += direction * force * Time.deltaTime;

                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.Log("狗被吹飞了！");
        }

        /// <summary>
        /// 获取当前血量
        /// </summary>
        public int GetCurrentHealth()
        {
            return currentHealth;
        }

        /// <summary>
        /// 获取最大血量
        /// </summary>
        public int GetMaxHealth()
        {
            return maxHealth;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // 绘制躲避范围
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, dodgeDistance);

            if (pointA != null && pointB != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pointA.position, pointB.position);
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(pointA.position, 0.5f);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(pointB.position, arrivalThreshold);
            }
        }
    }
}
