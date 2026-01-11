using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class rqz_SceneOrchestrator2 : MonoBehaviour
{

    [Header("Scene Management")]
    [Tooltip("一个空的父物体，包含了此场景所有AR内容")]
    public GameObject arContentRoot; 

    [Header("Global UI")]
    public TextMeshProUGUI globalSubtitleText;

    [Header("Characters & Animators")]
    public Animator hunterAnimator;
    public Animator uglyDucklingAnimator;

    [Header("Panicked Duck Dialogue")]
    public AudioSource panickedDuckAudioSource;
    public AudioClip panickedDuckClip;
    [TextArea(3, 5)]
    public string panickedDuckDialogue;

    [Header("Narration (Ugly Duckling's Thoughts)")]
    public AudioSource narrationAudioSource;
    public AudioClip narrationClip;
    [TextArea(5, 7)]
    public string narrationContent;

    [Header("Ugly Duckling's Dialogue")]
    public AudioSource uglyDucklingAudioSource;
    public AudioClip dialogueClip1;
    [TextArea(3, 5)]
    public string dialogueContent1;

    [Header("Scene Timing")]
    public float heroActionDelay = 0.5f;


    void Start()
    {
        // 初始时，隐藏字幕和所有AR内容
        if (globalSubtitleText != null)
        {
            globalSubtitleText.transform.parent.gameObject.SetActive(false);
        }
        if (arContentRoot != null)
        {
            arContentRoot.SetActive(false);
        }
    }

 
    public void OnTargetFound()
    {
        // 1. 显示所有AR内容
        if (arContentRoot != null)
        {
            arContentRoot.SetActive(true);
        }

        // 2. 重置所有动画状态，确保每次都从头开始
        ResetAllAnimatorStates();

        // 3. 停止任何旧的流程，并开始新的故事序列
        StopAllCoroutines();
        StartCoroutine(HeroicStandSequence());
    }


    public void OnTargetLost()
    {
        // 1. 停止所有正在运行的序列和延迟事件
        StopAllCoroutines();

        // 2. 立即停止所有声音
        if (panickedDuckAudioSource != null) panickedDuckAudioSource.Stop();
        if (narrationAudioSource != null) narrationAudioSource.Stop();
        if (uglyDucklingAudioSource != null) uglyDucklingAudioSource.Stop();

        // 3. 隐藏所有AR内容和UI
        if (globalSubtitleText != null)
            globalSubtitleText.transform.parent.gameObject.SetActive(false);

        if (arContentRoot != null)
        {
            arContentRoot.SetActive(false);
        }
    }


    private void ResetAllAnimatorStates()
    {
        if (hunterAnimator != null) hunterAnimator.Play("Idle", -1, 0f);
        if (uglyDucklingAnimator != null) uglyDucklingAnimator.Play("Idle", -1, 0f);
    }


    IEnumerator HeroicStandSequence()
    {
        // --- 阶段 1: 一只鸭子惊慌失措 ---
        Debug.Log("Stage 1: A duck panics.");

        globalSubtitleText.text = panickedDuckDialogue;
        globalSubtitleText.transform.parent.gameObject.SetActive(true);
        panickedDuckAudioSource.clip = panickedDuckClip;
        panickedDuckAudioSource.Play();
        yield return new WaitForSeconds(panickedDuckClip.length);
        globalSubtitleText.transform.parent.gameObject.SetActive(false);


        // --- 阶段 2: 猎人前进 & 旁白同时进行  ---
        Debug.Log("Stage 2: Hunter advances while narration plays.");

        if (narrationAudioSource == null || narrationClip == null)
        {
            Debug.LogError("旁白音频或AudioSource未设置！无法进行同步。");
            yield break;
        }

        hunterAnimator.SetTrigger("StartWalking");

        globalSubtitleText.text = narrationContent;
        globalSubtitleText.transform.parent.gameObject.SetActive(true);
        narrationAudioSource.clip = narrationClip;
        narrationAudioSource.Play();

        yield return new WaitForSeconds(narrationClip.length);

        hunterAnimator.SetTrigger("StopWalking");
        globalSubtitleText.transform.parent.gameObject.SetActive(false);

        // --- 阶段 3: 丑大鸭挺身而出！ ---
        Debug.Log("Stage 3: Ugly Duckling turns and confronts the hunter.");
        uglyDucklingAnimator.SetTrigger("TurnAndAttack");
        yield return new WaitForSeconds(heroActionDelay);

        // --- 阶段 4: 丑大鸭说出台词 ---
        globalSubtitleText.text = dialogueContent1;
        globalSubtitleText.transform.parent.gameObject.SetActive(true);
        uglyDucklingAudioSource.clip = dialogueClip1;
        uglyDucklingAudioSource.Play();
        yield return new WaitForSeconds(dialogueClip1.length);


        // --- 阶段 5: 场景结束 ---
        Debug.Log("Stage 5: Sequence complete.");
        globalSubtitleText.transform.parent.gameObject.SetActive(false);

        hunterAnimator.SetTrigger("Surprised");
    }

  
}