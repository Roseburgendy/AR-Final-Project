using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LX_Game
{
    /// <summary>
    /// 游戏管理器 - 整合剧情流程
    /// </summary>
    public class LX_GameManager : MonoBehaviour
    {
        [Header("游戏对象")]
        public DogAIController dog;
        public DuckPlayerController duck;
        public GameObject hunter; // 猎人对象

        [Header("剧情管理")]
        public LX_DialogueManager dialogueManager;

        [Header("UI")]
        public GameObject gameUI; // 游戏UI（摇杆和攻击按钮）

        [Header("剧情时间设置")]
        public float introDelay = 1f; // 开场延迟
        [Tooltip("（已弃用）对话时间现在在DialogueManager中的每个DialogueEntry设置")]
        public float dialogueDuration = 2.5f; // 保留用于兼容，但不再使用
        [Tooltip("（已弃用）对话时间现在在DialogueManager中的每个DialogueEntry设置")]
        public float narrationDuration = 3f; // 保留用于兼容，但不再使用

        private bool gameStarted = false;
        private bool gameOver = false;
        private bool dogWasHit = false;
        private bool introStarted = false; // 标记是否已经开始过开场剧情
        private Vector3 duckStartPosition;

        void Start()
        {
            // 自动查找组件
            if (dog == null) dog = FindObjectOfType<DogAIController>();
            if (duck == null) duck = FindObjectOfType<DuckPlayerController>();
            if (dialogueManager == null) dialogueManager = FindObjectOfType<LX_DialogueManager>();

            if (duck != null)
            {
                duckStartPosition = duck.transform.position;
            }

            // 初始状态：隐藏UI，禁用控制
            if (gameUI != null)
            {
                gameUI.SetActive(false);
            }

            if (duck != null)
            {
                duck.DisableControl();
            }

            // 默认等待AR触发
            Debug.Log("等待AR目标识别...");
        }

        /// <summary>
        /// 触发游戏开始（由外部调用，如AR识别触发）
        /// </summary>
        public void TriggerGameStart()
        {
            if (introStarted)
            {
                Debug.Log("开场剧情已经开始过了，不重复触发");
                return;
            }

            introStarted = true;
            Debug.Log("AR目标识别成功！开始游戏剧情");
            StartCoroutine(StartGameSequence());
        }

        /// <summary>
        /// 游戏开场剧情序列
        /// </summary>
        IEnumerator StartGameSequence()
        {
            yield return new WaitForSeconds(introDelay);

            // 1. 猎人："Pochita，让它知道厉害！"
            Debug.Log("剧情: 猎人说话");
            if (dialogueManager != null)
            {
                dialogueManager.PlayHunterIntro();
                yield return new WaitForSeconds(dialogueManager.GetHunterIntroDuration());
            }
            else
            {
                yield return new WaitForSeconds(dialogueDuration);
            }

            // 2. Pochita汪汪叫
            Debug.Log("剧情: Pochita汪汪叫");
            if (dialogueManager != null)
            {
                dialogueManager.PlayPochitaBark();
                yield return new WaitForSeconds(dialogueManager.GetPochitaBarkDuration());
            }
            else
            {
                yield return new WaitForSeconds(1.5f);
            }

            // 3. 旁白："操作丑小鸭赶走猎犬吧"
            Debug.Log("剧情: 旁白");
            if (dialogueManager != null)
            {
                dialogueManager.PlayNarration();
                yield return new WaitForSeconds(dialogueManager.GetNarrationDuration());
            }
            else
            {
                yield return new WaitForSeconds(narrationDuration);
            }

            // 4. 显示游戏UI，开始游戏
            Debug.Log("游戏开始！显示UI");
            if (gameUI != null)
            {
                gameUI.SetActive(true);
            }

            // 启用玩家控制
            if (duck != null)
            {
                duck.EnableControl();
            }

            // 让狗开始移动（GameUI激活后）
            if (dog != null)
            {
                dog.StartMoving();
            }

            // 隐藏对话框
            if (dialogueManager != null)
            {
                dialogueManager.HideDialogue();
            }

            gameStarted = true;
        }

        /// <summary>
        /// 狗被击中时调用（用于变红效果）
        /// </summary>
        public void OnDogHit()
        {
            if (!dogWasHit && gameStarted)
            {
                dogWasHit = true;
                Debug.Log("狗被击中，变红！");
                // 变红效果已在DogAIController中实现
            }
        }

        /// <summary>
        /// 狗死亡时调用 - 触发终结剧情
        /// </summary>
        public void OnDogDied()
        {
            if (gameOver) return;
            gameOver = true;

            Debug.Log("狗被击败，开始终结剧情");
            StartCoroutine(EndGameSequence());
        }

        /// <summary>
        /// 游戏结束剧情序列
        /// </summary>
        IEnumerator EndGameSequence()
        {
            // 2. 隐藏游戏UI
            Debug.Log("剧情: 隐藏游戏UI");
            if (gameUI != null)
            {
                gameUI.SetActive(false);
            }

            // 禁用玩家控制
            if (duck != null)
            {
                duck.DisableControl();
            }

            yield return new WaitForSeconds(1f);

            // 1. 猎人："你还有点实力，看来，得我亲自出马了。"
            Debug.Log("剧情: 猎人挑战");
            if (dialogueManager != null)
            {
                dialogueManager.PlayHunterChallenge();
                yield return new WaitForSeconds(dialogueManager.GetHunterChallengeDuration());
            }
            else
            {
                yield return new WaitForSeconds(6f);
            }

            yield return new WaitForSeconds(0.5f);

            // 3. 丑小鸭："休想！强风吹拂。" + 跳跃动画
            Debug.Log("剧情: 丑小鸭使用大招");
            if (dialogueManager != null)
            {
                dialogueManager.PlayDuckUltimate();
            }

            if (duck != null)
            {
                duck.PlayJumpAnimation();
            }

            if (dialogueManager != null)
            {
                yield return new WaitForSeconds(dialogueManager.GetDuckUltimateDuration());
            }
            else
            {
                yield return new WaitForSeconds(1.5f);
            }

            // 4. 猎人和狗旋转飞出
            Debug.Log("剧情: 吹飞敌人");
            BlowAwayEnemies();

            yield return new WaitForSeconds(1f);

            // 5. 猎人："呜呜呜，我一定会回来的！"
            Debug.Log("剧情: 猎人逃跑台词");
            if (dialogueManager != null)
            {
                dialogueManager.PlayHunterRetreat();
                yield return new WaitForSeconds(dialogueManager.GetHunterRetreatDuration());
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

            // 游戏胜利
            Debug.Log("=== 游戏胜利！完整剧情结束 ===");

            // 可以在这里添加胜利UI或返回主菜单
        }

        /// <summary>
        /// 吹飞敌人效果
        /// </summary>
        void BlowAwayEnemies()
        {
            // 吹飞狗
            if (dog != null && duck != null)
            {
                Vector3 blowDirection = (dog.transform.position - duck.transform.position).normalized;
                blowDirection.y = 0.5f; // 添加向上的分量
                dog.GetBlownAway(blowDirection, 0.2f);
            }

            // 吹飞猎人
            if (hunter != null && duck != null)
            {
                Vector3 blowDirection = (hunter.transform.position - duck.transform.position).normalized;
                blowDirection.y = 0.5f; // 添加向上的分量
                StartCoroutine(BlowAwayHunter(blowDirection, 0.2f));
            }
        }

        /// <summary>
        /// 猎人被吹飞效果协程
        /// </summary>
        IEnumerator BlowAwayHunter(Vector3 direction, float force)
        {
            float duration = 2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // 旋转
                hunter.transform.Rotate(Vector3.up, 720f * Time.deltaTime);
                hunter.transform.Rotate(Vector3.right, 360f * Time.deltaTime);
                
                // 移动
                hunter.transform.position += direction * force * Time.deltaTime;

                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.Log("猎人被吹飞了！");
        }

        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        public bool IsGameOver()
        {
            return gameOver;
        }
    }
}
