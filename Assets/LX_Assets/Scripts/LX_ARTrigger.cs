using UnityEngine;
using Vuforia;

namespace LX_Game
{
    /// <summary>
    /// AR触发器 - 监听Vuforia的目标识别事件
    /// 当AR目标被识别时，触发游戏剧情开始
    /// </summary>
    public class LX_ARTrigger : MonoBehaviour
    {
        [Header("游戏管理器")]
        [Tooltip("第一关游戏管理器")]
        public LX_GameManager gameManager;
        
        [Tooltip("结局剧情管理器")]
        public LX_GameManager2 gameManager2;

        [Header("触发设置")]
        public bool triggerOnce = true; // 是否只触发一次
        public float triggerDelay = 0.5f; // 识别后延迟触发的时间

        private bool hasTriggered = false;
        private ObserverBehaviour observerBehaviour;

        void Start()
        {
            // 自动查找GameManager（如果都没设置）
            if (gameManager == null && gameManager2 == null)
            {
                gameManager = FindObjectOfType<LX_GameManager>();
                if (gameManager == null)
                {
                    gameManager2 = FindObjectOfType<LX_GameManager2>();
                }
            }

            // 获取Vuforia的ObserverBehaviour组件
            observerBehaviour = GetComponent<ObserverBehaviour>();

            if (observerBehaviour == null)
            {
                Debug.LogError("LX_ARTrigger: 找不到ObserverBehaviour组件！请将此脚本添加到Image Target对象上。");
                return;
            }

            // 注册Vuforia事件
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
            
            string managerType = gameManager != null ? "LX_GameManager" : 
                                gameManager2 != null ? "LX_GameManager2" : "未设置";
            Debug.Log($"LX_ARTrigger: AR触发器已初始化，将触发 {managerType}，等待目标识别...");
        }

        void OnDestroy()
        {
            // 取消注册事件
            if (observerBehaviour != null)
            {
                observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
            }
        }

        /// <summary>
        /// Vuforia目标状态变化回调
        /// </summary>
        private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
        {
            // 检查目标是否被追踪到
            if (targetStatus.Status == Status.TRACKED || 
                targetStatus.Status == Status.EXTENDED_TRACKED)
            {
                OnTargetFound();
            }
            else if (targetStatus.Status == Status.NO_POSE)
            {
                OnTargetLost();
            }
        }

        /// <summary>
        /// 目标被识别到
        /// </summary>
        void OnTargetFound()
        {
            // 如果设置为只触发一次且已触发过，则返回
            if (triggerOnce && hasTriggered)
                return;

            hasTriggered = true;

            Debug.Log($"AR目标已识别！{triggerDelay}秒后触发游戏开始...");

            // 延迟触发游戏开始
            Invoke("TriggerGame", triggerDelay);
        }

        /// <summary>
        /// 目标丢失
        /// </summary>
        void OnTargetLost()
        {
            // 可以在这里添加目标丢失时的处理逻辑
            // 例如暂停游戏、显示提示等
            Debug.Log("AR目标丢失");
        }

        /// <summary>
        /// 触发游戏开始
        /// </summary>
        void TriggerGame()
        {
            if (gameManager != null)
            {
                gameManager.TriggerGameStart();
            }
            else if (gameManager2 != null)
            {
                gameManager2.TriggerGameStart();
            }
            else
            {
                Debug.LogError("LX_ARTrigger: 未找到任何GameManager！请拖入 LX_GameManager 或 LX_GameManager2");
            }
        }

        /// <summary>
        /// 手动触发（用于测试）
        /// </summary>
        [ContextMenu("手动触发游戏开始")]
        public void ManualTrigger()
        {
            Debug.Log("手动触发游戏开始");
            TriggerGame();
        }
    }
}

