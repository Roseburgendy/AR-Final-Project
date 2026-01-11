using UnityEngine;

namespace LX_Game
{
    /// <summary>
    /// 鸭子玩家控制器
    /// 通过虚拟摇杆控制移动，可以攻击狗
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class DuckPlayerController : MonoBehaviour
    {
        [Header("移动设置")]
        public float moveSpeed = 3f;
        public float rotationSpeed = 10f;

        [Header("攻击设置")]
        public float attackRange = 1.5f; // 攻击范围
        public float attackCooldown = 1f; // 攻击冷却时间
        public KeyCode attackKey = KeyCode.Space; // PC测试用的攻击键

        [Header("摇杆")]
        public VirtualJoystick joystick; // 虚拟摇杆引用

        [Header("边界检测")]
        public LX_BoundaryChecker boundaryChecker; // 边界检测器

        private Animator animator;
        private bool isAttacking = false;
        private float lastAttackTime = 0f;

        // 动画参数值
        private const int ANIM_IDLE = 0;
        private const int ANIM_WALK = 1;
        private const int ANIM_RUN = 2;
        private const int ANIM_JUMP = 3;
        private const int ANIM_ATTACK = 6;

        void Start()
        {
            animator = GetComponent<Animator>();

            // 如果没有设置摇杆，自动查找
            if (joystick == null)
            {
                joystick = FindObjectOfType<VirtualJoystick>();
            }

            // 自动查找边界检测器
            if (boundaryChecker == null)
            {
                boundaryChecker = FindObjectOfType<LX_BoundaryChecker>();
            }

            SetAnimation(ANIM_IDLE);
        }

        void Update()
        {
            if (isAttacking) return;

            // 获取移动输入
            Vector2 input = GetMovementInput();

            if (input.magnitude > 0.1f)
            {
                // 移动
                MoveDuck(input);
            }
            else
            {
                // 停止移动
                SetAnimation(ANIM_IDLE);
            }

            // 检测攻击输入（仅PC测试用）
            if (CanAttack() && Input.GetKeyDown(attackKey))
            {
                Attack();
            }
        }

        /// <summary>
        /// 获取移动输入（摇杆或键盘）
        /// </summary>
        Vector2 GetMovementInput()
        {
            Vector2 input = Vector2.zero;

            // 优先使用虚拟摇杆
            if (joystick != null)
            {
                input = joystick.GetInputDirection();
            }

            // 如果没有摇杆输入，使用键盘输入（用于PC测试）
            if (input.magnitude < 0.1f)
            {
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                input = new Vector2(horizontal, vertical);
            }

            return input;
        }

        /// <summary>
        /// 移动鸭子
        /// </summary>
        void MoveDuck(Vector2 input)
        {
            // 计算移动方向（世界空间）
            Vector3 moveDirection = new Vector3(input.x, 0, input.y).normalized;

            // 计算新位置
            Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

            // 边界检测（Kinematic模式下需要手动检测）
            if (boundaryChecker != null)
            {
                newPosition = boundaryChecker.ClampPosition(newPosition);
            }

            // 移动
            transform.position = newPosition;

            // 转向完全跟随摇杆方向（立即转向，不使用Lerp）
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = targetRotation; // 直接设置旋转，不使用Lerp插值
            }

            // 根据速度设置动画
            if (input.magnitude > 0.5f)
            {
                SetAnimation(ANIM_RUN);
            }
            else
            {
                SetAnimation(ANIM_WALK);
            }
        }

        /// <summary>
        /// 攻击
        /// </summary>
        void Attack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            SetAnimation(ANIM_ATTACK);

            Debug.Log("鸭子发起攻击！");

            // 检测攻击范围内的狗
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);
            foreach (Collider col in colliders)
            {
                DogAIController dog = col.GetComponent<DogAIController>();
                if (dog != null)
                {
                    dog.OnHitByDuck(transform.position); // 传递鸭子的位置用于躲避方向计算
                }
            }

            // 攻击动画持续时间后恢复
            Invoke("EndAttack", 0.6f);
        }

        void EndAttack()
        {
            isAttacking = false;
            SetAnimation(ANIM_IDLE);
        }

        /// <summary>
        /// 是否可以攻击
        /// </summary>
        bool CanAttack()
        {
            return !isAttacking && (Time.time - lastAttackTime) >= attackCooldown;
        }

        /// <summary>
        /// 是否正在攻击
        /// </summary>
        public bool IsAttacking()
        {
            return isAttacking;
        }

        /// <summary>
        /// 触发攻击（供外部调用，如攻击按钮）
        /// </summary>
        public void TriggerAttack()
        {
            if (CanAttack())
            {
                Attack();
            }
        }

        /// <summary>
        /// 设置动画参数
        /// </summary>
        void SetAnimation(int animValue)
        {
            animator.SetInteger("animation", animValue);
        }

        /// <summary>
        /// 播放跳跃动画（供外部调用）
        /// </summary>
        public void PlayJumpAnimation()
        {
            SetAnimation(ANIM_JUMP);
        }

        /// <summary>
        /// 禁用玩家控制
        /// </summary>
        public void DisableControl()
        {
            enabled = false;
        }

        /// <summary>
        /// 启用玩家控制
        /// </summary>
        public void EnableControl()
        {
            enabled = true;
        }

        /// <summary>
        /// 重置鸭子状态
        /// </summary>
        public void ResetDuck(Vector3 startPosition)
        {
            transform.position = startPosition;
            isAttacking = false;
            SetAnimation(ANIM_IDLE);
        }

        void OnDrawGizmosSelected()
        {
            // 绘制攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}

