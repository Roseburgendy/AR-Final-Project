using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LX_Game
{
    /// <summary>
    /// 游戏管理器2 - 结局剧情管理
    /// 管理Duck、Chicken、Chick的跳跃动画和对话序列
    /// </summary>
    public class LX_GameManager2 : MonoBehaviour
    {
        [Header("主要角色引用（用于对话）")]
        [Tooltip("Duck角色")]
        public GameObject duck;

        [Tooltip("Chicken角色")]
        public GameObject chicken;

        [Tooltip("Chick角色")]
        public GameObject chick;

        [Header("参与随机跳跃的所有角色")]
        [Tooltip("所有会跳跃的角色（包括主角、Duck、Chicken、Chick等所有角色）")]
        public List<GameObject> jumpingCharacters = new List<GameObject>();

        [Header("剧情管理")]
        [Tooltip("对话管理器")]
        public LX_DialogueManager2 dialogueManager;

        [Tooltip("剧情开始前的延迟（秒）")]
        public float startDelay = 1f;

        [Tooltip("单次跳跃动画持续时间（秒）")]
        public float jumpDuration = 1f;

        [Tooltip("跳跃高度（向上移动的距离）")]
        public float jumpHeight = 0.01f;

        [Header("随机跳跃设置")]
        [Tooltip("随机跳跃阶段持续时间（秒）")]
        public float randomJumpDuration = 8f;

        [Tooltip("每个角色跳跃间隔的最小值（秒）")]
        public float jumpIntervalMin = 0.5f;

        [Tooltip("每个角色跳跃间隔的最大值（秒）")]
        public float jumpIntervalMax = 2f;

        [Header("调试")]
        [Tooltip("显示详细日志")]
        public bool showDebugLog = true;

        private Dictionary<GameObject, Animator> characterAnimators = new Dictionary<GameObject, Animator>();
        private Dictionary<GameObject, Vector3> characterOriginalPositions = new Dictionary<GameObject, Vector3>();
        private bool isSequencePlaying = false;
        private bool gameStarted = false;

        void Start()
        {
            // 初始化角色列表
            InitializeCharacters();

            // 默认等待AR触发
            DebugLog("等待AR触发...");
        }

        /// <summary>
        /// 初始化角色列表和Animator
        /// </summary>
        void InitializeCharacters()
        {
            characterAnimators.Clear();
            characterOriginalPositions.Clear();

            // 为所有跳跃角色缓存Animator和原始位置
            foreach (GameObject character in jumpingCharacters)
            {
                if (character != null)
                {
                    Animator anim = character.GetComponent<Animator>();
                    if (anim != null)
                    {
                        characterAnimators[character] = anim;
                    }
                    else
                    {
                        Debug.LogWarning($"{character.name} 没有Animator组件，将无法播放动画");
                    }
                    
                    // 保存原始位置
                    characterOriginalPositions[character] = character.transform.position;
                }
            }

            DebugLog($"初始化完成，共 {jumpingCharacters.Count} 个跳跃角色");
        }

        /// <summary>
        /// 由AR触发器调用（公共接口）
        /// </summary>
        public void TriggerGameStart()
        {
            if (gameStarted)
            {
                Debug.LogWarning("结局剧情已经开始过了！");
                return;
            }

            gameStarted = true;
            DebugLog("收到AR触发信号，开始结局剧情");
            StartEndingSequence();
        }

        /// <summary>
        /// 开始结局剧情序列（可被外部调用）
        /// </summary>
        public void StartEndingSequence()
        {
            if (isSequencePlaying)
            {
                Debug.LogWarning("剧情已经在播放中！");
                return;
            }

            StartCoroutine(EndingSequenceCoroutine());
        }

        /// <summary>
        /// 结局剧情协程
        /// </summary>
        IEnumerator EndingSequenceCoroutine()
        {
            isSequencePlaying = true;
            DebugLog("========== 结局剧情开始 ==========");

            yield return new WaitForSeconds(startDelay);

            // 1. Duck跳跃 + 说话
            yield return new WaitForSeconds(jumpDuration);
            if (dialogueManager != null)
            {
                dialogueManager.PlayDuckDialogue();
                PlayJumpAnimation(duck);
            }
            yield return new WaitForSeconds(dialogueManager != null ? dialogueManager.GetDuckDuration() : 5f);

            // 2. Chicken跳跃 + 说话
            yield return new WaitForSeconds(jumpDuration);
            if (dialogueManager != null)
            {
                dialogueManager.PlayChickenDialogue();
                PlayJumpAnimation(chicken);
            }
            yield return new WaitForSeconds(dialogueManager != null ? dialogueManager.GetChickenDuration() : 6f);

            // 3. Chick跳跃 + 说话
            yield return new WaitForSeconds(jumpDuration);

            if (dialogueManager != null)
            {
                dialogueManager.PlayChickDialogue();
                PlayJumpAnimation(chick);
            }
            yield return new WaitForSeconds(dialogueManager != null ? dialogueManager.GetChickDuration() : 3f);

            // 4. 所有角色开始随机跳跃
            DebugLog("4. 所有角色开始随机、不间断跳跃");
            StartRandomJumping();

            // 在随机跳跃期间等待一小段时间
            yield return new WaitForSeconds(2f);

            // 5. 旁白1
            DebugLog("5. 播放旁白1");
            if (dialogueManager != null)
            {
                dialogueManager.PlayNarration1();
            }
            yield return new WaitForSeconds(dialogueManager != null ? dialogueManager.GetNarration1Duration() : 10f);

            // 6. 旁白2
            DebugLog("6. 播放旁白2");
            if (dialogueManager != null)
            {
                dialogueManager.PlayNarration2();
            }
            yield return new WaitForSeconds(dialogueManager != null ? dialogueManager.GetNarration2Duration() : 10f);

            // 7. 停止随机跳跃
            DebugLog("7. 停止随机跳跃");
            StopRandomJumping();

            // 隐藏对话框
            if (dialogueManager != null)
            {
                dialogueManager.HideDialogue();
            }

            DebugLog("========== 结局剧情结束 ==========");
            isSequencePlaying = false;
        }

        #region 跳跃动画控制

        /// <summary>
        /// 播放单个角色的跳跃（仅物理跳跃，不设置动画参数）
        /// </summary>
        void PlayJumpAnimation(GameObject character)
        {
            if (character == null)
            {
                Debug.LogWarning("角色为空，无法播放跳跃动画");
                return;
            }

            // 只启动物理跳跃，不设置Animator参数
            StartCoroutine(JumpPhysics(character));
            DebugLog($"{character.name} 开始跳跃（物理跳跃）");
        }
        
        /// <summary>
        /// 物理跳跃协程（向上再向下）
        /// </summary>
        IEnumerator JumpPhysics(GameObject character)
        {
            if (character == null) yield break;
            
            Vector3 originalPos = character.transform.position;
            Vector3 targetPos = originalPos + Vector3.up * jumpHeight;
            
            // 向上跳
            float halfDuration = jumpDuration / 2f;
            float elapsed = 0f;
            
            while (elapsed < halfDuration)
            {
                character.transform.position = Vector3.Lerp(originalPos, targetPos, elapsed / halfDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            character.transform.position = targetPos;
            
            // 向下落
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                character.transform.position = Vector3.Lerp(targetPos, originalPos, elapsed / halfDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            character.transform.position = originalPos;
        }

        /// <summary>
        /// 重置角色动画为待机（现在不做任何操作）
        /// </summary>
        void ResetAnimation(GameObject character)
        {
            // 不再设置Animator参数
            // 角色保持原有动画状态
        }

        #endregion

        #region 随机跳跃控制

        private List<Coroutine> randomJumpCoroutines = new List<Coroutine>();
        private bool isRandomJumping = false;

        /// <summary>
        /// 开始所有角色的随机跳跃
        /// </summary>
        void StartRandomJumping()
        {
            if (isRandomJumping)
            {
                Debug.LogWarning("随机跳跃已经在进行中");
                return;
            }

            isRandomJumping = true;
            randomJumpCoroutines.Clear();

            // 为每个角色启动一个随机跳跃协程
            foreach (GameObject character in jumpingCharacters)
            {
                if (character != null)
                {
                    Coroutine jumpCoroutine = StartCoroutine(RandomJumpCoroutine(character));
                    randomJumpCoroutines.Add(jumpCoroutine);
                }
            }

            DebugLog($"开始随机跳跃，共 {randomJumpCoroutines.Count} 个角色");
        }

        /// <summary>
        /// 停止所有角色的随机跳跃
        /// </summary>
        void StopRandomJumping()
        {
            isRandomJumping = false;

            // 停止所有随机跳跃协程
            foreach (Coroutine coroutine in randomJumpCoroutines)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            randomJumpCoroutines.Clear();

            // 重置所有角色动画为待机
            foreach (GameObject character in jumpingCharacters)
            {
                ResetAnimation(character);
            }

            DebugLog("停止随机跳跃");
        }

        /// <summary>
        /// 单个角色的随机跳跃协程
        /// </summary>
        IEnumerator RandomJumpCoroutine(GameObject character)
        {
            while (isRandomJumping)
            {
                // 随机等待一段时间
                float waitTime = Random.Range(jumpIntervalMin, jumpIntervalMax);
                yield return new WaitForSeconds(waitTime);

                // 播放跳跃动画
                PlayJumpAnimation(character);

                // 等待跳跃动画完成
                yield return new WaitForSeconds(jumpDuration);

                // 重置为待机动画
                ResetAnimation(character);
            }
        }

        #endregion

        #region 调试方法

        void DebugLog(string message)
        {
            if (showDebugLog)
            {
                Debug.Log($"[LX_GameManager2] {message}");
            }
        }

        #endregion

        #region 测试方法（在Inspector中可调用）

        /// <summary>
        /// 测试：播放Duck跳跃
        /// </summary>
        [ContextMenu("测试/Duck跳跃")]
        public void TestDuckJump()
        {
            PlayJumpAnimation(duck);
        }

        /// <summary>
        /// 测试：播放Chicken跳跃
        /// </summary>
        [ContextMenu("测试/Chicken跳跃")]
        public void TestChickenJump()
        {
            PlayJumpAnimation(chicken);
        }

        /// <summary>
        /// 测试：播放Chick跳跃
        /// </summary>
        [ContextMenu("测试/Chick跳跃")]
        public void TestChickJump()
        {
            PlayJumpAnimation(chick);
        }

        /// <summary>
        /// 测试：开始随机跳跃
        /// </summary>
        [ContextMenu("测试/开始随机跳跃")]
        public void TestStartRandomJump()
        {
            StartRandomJumping();
        }

        /// <summary>
        /// 测试：停止随机跳跃
        /// </summary>
        [ContextMenu("测试/停止随机跳跃")]
        public void TestStopRandomJump()
        {
            StopRandomJumping();
        }

        #endregion
    }
}

