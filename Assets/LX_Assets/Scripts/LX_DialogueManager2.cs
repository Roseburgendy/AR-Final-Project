using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LX_Game
{
    /// <summary>
    /// 对话管理器2 - 结局剧情版本
    /// 管理Duck、Chicken、Chick的对话和旁白
    /// </summary>
    public class LX_DialogueManager2 : MonoBehaviour
    {
        [Header("UI组件")]
        [Tooltip("对话框面板")]
        public GameObject dialoguePanel;

        [Tooltip("显示对话文本的Text组件")]
        public Text dialogueUIText;

        [Header("打字机效果")]
        [Tooltip("启用打字机效果")]
        public bool useTypewriterEffect = true;

        [Tooltip("打字机速度（秒/字符）")]
        public float typingSpeed = 0.05f;

        [Header("音效播放器")]
        [Tooltip("用于播放语音的AudioSource")]
        public AudioSource voiceAudioSource;

        [Header("=== Duck对话 ===")]
        [TextArea(2, 5)]
        public string duckDialogue = "Whoa! You've become so strong! That last move was absolutely epic!";
        public AudioClip duckVoiceClip;
        [Tooltip("Duck对话显示时长（秒）")]
        public float duckDuration = 5f;

        [Header("=== Chicken对话 ===")]
        [TextArea(2, 5)]
        public string chickenDialogue = "We're so sorry... for the way we treated you before. I truly, deeply apologize.";
        public AudioClip chickenVoiceClip;
        [Tooltip("Chicken对话显示时长（秒）")]
        public float chickenDuration = 6f;

        [Header("=== Chick对话 ===")]
        [TextArea(2, 5)]
        public string chickDialogue = "You're our hero!";
        public AudioClip chickVoiceClip;
        [Tooltip("Chick对话显示时长（秒）")]
        public float chickDuration = 3f;

        [Header("=== 旁白1 ===")]
        [TextArea(3, 6)]
        public string narration1 = "True transformation is not about becoming a beautiful swan in the eyes of others, but about finding your own strength in the face of adversity and becoming a stronger version of yourself.";
        public AudioClip narration1Clip;
        [Tooltip("旁白1显示时长（秒）")]
        public float narration1Duration = 10f;

        [Header("=== 旁白2 ===")]
        [TextArea(3, 6)]
        public string narration2 = "The Ugly Duckling didn't change the color of his feathers, yet he earned everyone's respect. He understood that courage and kindness are the most precious qualities of all.";
        public AudioClip narration2Clip;
        [Tooltip("旁白2显示时长（秒）")]
        public float narration2Duration = 10f;

        private Coroutine currentDialogue;
        private bool isTyping = false;

        void Start()
        {
            // 初始状态：隐藏对话框
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            // 自动查找AudioSource
            if (voiceAudioSource == null)
            {
                voiceAudioSource = GetComponent<AudioSource>();
                if (voiceAudioSource == null)
                {
                    voiceAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        #region 对话播放方法

        /// <summary>
        /// 播放Duck的对话
        /// </summary>
        public void PlayDuckDialogue()
        {
            ShowDialogue("Duck", duckDialogue, duckVoiceClip, useTypewriterEffect);
        }

        /// <summary>
        /// 播放Chicken的对话
        /// </summary>
        public void PlayChickenDialogue()
        {
            ShowDialogue("Chicken", chickenDialogue, chickenVoiceClip, useTypewriterEffect);
        }

        /// <summary>
        /// 播放Chick的对话
        /// </summary>
        public void PlayChickDialogue()
        {
            ShowDialogue("Chick", chickDialogue, chickVoiceClip, useTypewriterEffect);
        }

        /// <summary>
        /// 播放旁白1
        /// </summary>
        public void PlayNarration1()
        {
            ShowDialogue("Narrator", narration1, narration1Clip, useTypewriterEffect);
        }

        /// <summary>
        /// 播放旁白2
        /// </summary>
        public void PlayNarration2()
        {
            ShowDialogue("Narrator", narration2, narration2Clip, useTypewriterEffect);
        }

        #endregion

        #region 获取时长方法

        public float GetDuckDuration() => duckDuration;
        public float GetChickenDuration() => chickenDuration;
        public float GetChickDuration() => chickDuration;
        public float GetNarration1Duration() => narration1Duration;
        public float GetNarration2Duration() => narration2Duration;

        #endregion

        #region 核心显示方法

        /// <summary>
        /// 显示对话（通用方法）
        /// </summary>
        void ShowDialogue(string characterName, string text, AudioClip voiceClip, bool useTypewriter)
        {
            // 激活对话框面板
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            // 播放语音
            if (voiceClip != null && voiceAudioSource != null)
            {
                voiceAudioSource.clip = voiceClip;
                voiceAudioSource.Play();
            }

            // 停止之前的对话
            if (currentDialogue != null)
            {
                StopCoroutine(currentDialogue);
            }

            // 直接显示文本，不添加任何前缀
            string fullText = text;

            // 显示文本
            if (useTypewriter)
            {
                currentDialogue = StartCoroutine(TypeText(fullText));
            }
            else
            {
                if (dialogueUIText != null)
                {
                    dialogueUIText.text = fullText;
                }
            }
        }

        /// <summary>
        /// 打字机效果协程
        /// </summary>
        IEnumerator TypeText(string text)
        {
            isTyping = true;
            if (dialogueUIText != null)
            {
                dialogueUIText.text = "";
                foreach (char c in text)
                {
                    // 检查是否是停顿标记（用 | 表示1秒停顿）
                    if (c == '|')
                    {
                        yield return new WaitForSeconds(1f);
                        continue;
                    }
                    dialogueUIText.text += c;
                    yield return new WaitForSeconds(typingSpeed);
                }
            }
            isTyping = false;
            currentDialogue = null;
        }

        /// <summary>
        /// 隐藏对话框
        /// </summary>
        public void HideDialogue()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (voiceAudioSource != null)
            {
                voiceAudioSource.Stop();
            }

            if (currentDialogue != null)
            {
                StopCoroutine(currentDialogue);
                currentDialogue = null;
            }
        }

        /// <summary>
        /// 立即完成打字机效果（跳过动画）
        /// </summary>
        public void SkipTypewriter()
        {
            if (isTyping && currentDialogue != null)
            {
                StopCoroutine(currentDialogue);
                // 这里可以显示完整文本
                isTyping = false;
            }
        }

        #endregion
    }
}

