using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class rqz_SceneOrchestrator2 : MonoBehaviour
{
    [Header("Global UI")]
    public TextMeshProUGUI globalSubtitleText;

    [Header("Characters & Animators")]
    public Animator hunterAnimator;
    public Animator uglyDucklingAnimator;

    [Header("Panicked Duck Dialogue")]
    public AudioSource panickedDuckAudioSource;
    public AudioClip panickedDuckClip;
    [TextArea(3, 5)]
    public string panickedDuckDialogue ;

    [Header("Narration (Ugly Duckling's Thoughts)")]
    public AudioSource narrationAudioSource;
    public AudioClip narrationClip; 
    [TextArea(5, 7)]
    public string narrationContent ;

    [Header("Ugly Duckling's Dialogue")]
    public AudioSource uglyDucklingAudioSource;
    public AudioClip dialogueClip1;
    [TextArea(3, 5)]
    public string dialogueContent1 ;
  

    [Header("Scene Timing")]
    [Tooltip("丑大鸭转身和攻击动画开始后，播放第一句台词前的短暂延迟")]
    public float heroActionDelay = 0.5f;


    void Start()
    {
        if (globalSubtitleText != null)
        {
            globalSubtitleText.transform.parent.gameObject.SetActive(false);
        }
    }

    public void StartScene()
    {
        StopAllCoroutines();
        StartCoroutine(HeroicStandSequence());
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

        // 检查旁白音频是否存在
        if (narrationAudioSource == null || narrationClip == null)
        {
            Debug.LogError("旁白音频或AudioSource未设置！无法进行同步。");
            yield break; // 终止协程
        }

        // 触发猎人走路
        hunterAnimator.SetTrigger("StartWalking");

        // 同时播放旁白
        globalSubtitleText.text = narrationContent;
        globalSubtitleText.transform.parent.gameObject.SetActive(true);
        narrationAudioSource.clip = narrationClip;
        narrationAudioSource.Play();

       
        yield return new WaitForSeconds(narrationClip.length);

        // 猎人停下
        hunterAnimator.SetTrigger("StopWalking");
        globalSubtitleText.transform.parent.gameObject.SetActive(false);
        // 此处不再需要手动停止旁白，因为它刚好播放完毕


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

    public void ResetScene()
    {
        StopAllCoroutines();

        if (panickedDuckAudioSource != null) panickedDuckAudioSource.Stop();
        if (narrationAudioSource != null) narrationAudioSource.Stop();
        if (uglyDucklingAudioSource != null) uglyDucklingAudioSource.Stop();

        if (globalSubtitleText != null)
            globalSubtitleText.transform.parent.gameObject.SetActive(false);

        if (hunterAnimator != null) hunterAnimator.Play("Idle", -1, 0f);
        if (uglyDucklingAnimator != null) uglyDucklingAnimator.Play("Idle", -1, 0f);
    }
}
