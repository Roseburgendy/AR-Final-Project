using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LX_Game
{
    /// <summary>
    /// 攻击按钮控制器
    /// 用于手机端的攻击操作（精简版：仅处理逻辑）
    /// </summary>
    public class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("鸭子引用")]
        public DuckPlayerController duckController;

        private bool isPressed = false;

        void Start()
        {
            // 自动查找鸭子控制器
            if (duckController == null)
            {
                duckController = FindObjectOfType<DuckPlayerController>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;

            // 触发攻击
            if (duckController != null)
            {
                TriggerAttack();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
        }

        /// <summary>
        /// 触发攻击
        /// </summary>
        void TriggerAttack()
        {
            // 使用 SendMessage 触发攻击
            if (duckController != null)
            {
                duckController.SendMessage("TriggerAttack", SendMessageOptions.DontRequireReceiver);
            }
        }

        public bool IsPressed()
        {
            return isPressed;
        }

        void OnDisable()
        {
            // 禁用时重置状态
            isPressed = false;
        }
    }
}