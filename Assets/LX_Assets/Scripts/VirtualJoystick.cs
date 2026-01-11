using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LX_Game
{
    /// <summary>
    /// 虚拟摇杆控制器
    /// 用于手机触摸控制
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("UI组件")]
        public RectTransform joystickBackground; // 摇杆背景
        public RectTransform joystickHandle; // 摇杆手柄

        [Header("设置")]
        public float handleRange = 50f; // 手柄移动范围
        public float deadZone = 0.1f; // 死区

        private Vector2 inputDirection = Vector2.zero;
        private bool isTouching = false;

        void Start()
        {
            // 如果没有设置组件，尝试自动获取
            if (joystickBackground == null)
                joystickBackground = GetComponent<RectTransform>();

            if (joystickHandle == null && transform.childCount > 0)
                joystickHandle = transform.GetChild(0).GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isTouching = true;
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isTouching = false;
            inputDirection = Vector2.zero;

            // 重置手柄位置
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (joystickBackground == null) return;

            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground,
                eventData.position,
                eventData.pressEventCamera,
                out position
            );

            // 计算方向
            position = position / joystickBackground.sizeDelta * 2;
            inputDirection = position.magnitude > 1.0f ? position.normalized : position;

            // 应用死区
            if (inputDirection.magnitude < deadZone)
            {
                inputDirection = Vector2.zero;
            }

            // 移动手柄
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = inputDirection * handleRange;
            }
        }

        /// <summary>
        /// 获取输入方向
        /// </summary>
        public Vector2 GetInputDirection()
        {
            return inputDirection;
        }

        /// <summary>
        /// 是否正在触摸
        /// </summary>
        public bool IsTouching()
        {
            return isTouching;
        }

        void OnDisable()
        {
            // 禁用时重置状态
            inputDirection = Vector2.zero;
            isTouching = false;
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
        }
    }
}

