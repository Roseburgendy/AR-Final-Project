using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LX_Game
{
    /// <summary>
    /// 对话和剧情管理器
    /// 管理所有语音和文字提示（支持Inspector编辑）
    /// </summary>
    public class LX_DialogueManager : MonoBehaviour
    {
        // 定义一个结构体，把文字和音频绑在一起，方便在面板编辑
        [System.Serializable]
        public struct DialogueEntry
        {
            [TextArea(2, 5)] // 在Inspector显示为多行文本框
            public string dialogueText;
            public AudioClip voiceClip;
            [Tooltip("对话显示的持续时间（秒）")]
            public float duration; // 每个对话的持续时间
        }

        [Header("UI组件")]
        public Text dialogueUIText; // 改个名避免跟结构体字段冲突
        public GameObject dialoguePanel;
        public float typingSpeed = 0.05f;

        [Header("剧情对话配置")]
        public DialogueEntry hunterIntro;      // 猎人开场
        public DialogueEntry pochitaBark;      // 狗狗叫（可选文字）
        public DialogueEntry narration;        // 旁白
        public DialogueEntry hunterChallenge;  // 猎人挑战
        public DialogueEntry duckUltimate;     // 鸭子大招
        public DialogueEntry hunterRetreat;    // 猎人逃跑

        [Header("音频组件")]
        public AudioSource audioSource;

        private bool isTyping = false;
        private Coroutine currentDialogue = null;

        void Start()
        {
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }

        /// <summary>
        /// 核心显示方法：现在直接接收 DialogueEntry 结构体
        /// </summary>
        public void ShowDialogueEntry(DialogueEntry entry, bool useTypewriter = true)
        {
            ShowDialogue(entry.dialogueText, entry.voiceClip, useTypewriter);
        }

        // 保留原有的灵活接口，方便临时调用
        public void ShowDialogue(string text, AudioClip voiceClip = null, bool useTypewriter = true)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(true);

            if (voiceClip != null && audioSource != null)
                audioSource.PlayOneShot(voiceClip);

            if (currentDialogue != null) StopCoroutine(currentDialogue);

            if (useTypewriter)
                currentDialogue = StartCoroutine(TypeText(text));
            else if (dialogueUIText != null)
                dialogueUIText.text = text;
        }

        public void HideDialogue()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (currentDialogue != null) StopCoroutine(currentDialogue);
            isTyping = false;
        }

        IEnumerator TypeText(string text)
        {
            isTyping = true;
            if (dialogueUIText != null)
            {
                dialogueUIText.text = "";
                foreach (char c in text)
                {
                    dialogueUIText.text += c;
                    yield return new WaitForSeconds(typingSpeed);
                }
            }
            isTyping = false;
            currentDialogue = null;
        }

        // ============================================
        // 调用方法：现在逻辑非常清晰，直接从面板读取
        // ============================================

        public void PlayHunterIntro() => ShowDialogueEntry(hunterIntro);
        public float GetHunterIntroDuration() => hunterIntro.duration > 0 ? hunterIntro.duration : 2.5f;
        
        public void PlayNarration() => ShowDialogueEntry(narration);
        public float GetNarrationDuration() => narration.duration > 0 ? narration.duration : 3f;
        
        public void PlayHunterChallenge() => ShowDialogueEntry(hunterChallenge);
        public float GetHunterChallengeDuration() => hunterChallenge.duration > 0 ? hunterChallenge.duration : 6f;
        
        public void PlayDuckUltimate() => ShowDialogueEntry(duckUltimate);
        public float GetDuckUltimateDuration() => duckUltimate.duration > 0 ? duckUltimate.duration : 2f;
        
        public void PlayHunterRetreat() => ShowDialogueEntry(hunterRetreat);
        public float GetHunterRetreatDuration() => hunterRetreat.duration > 0 ? hunterRetreat.duration : 3f;

        public void PlayPochitaBark()
        {
            // 如果你在面板里写了狗狗叫的文字，就显示；否则只播放声音
            if (!string.IsNullOrEmpty(pochitaBark.dialogueText))
                ShowDialogueEntry(pochitaBark);
            else if (pochitaBark.voiceClip != null)
                audioSource.PlayOneShot(pochitaBark.voiceClip);
            
            Debug.Log("Pochita is barking!");
        }
        public float GetPochitaBarkDuration() => pochitaBark.duration > 0 ? pochitaBark.duration : 1.5f;
    }
}