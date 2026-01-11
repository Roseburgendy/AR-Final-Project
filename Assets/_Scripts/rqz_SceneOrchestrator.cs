using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; 

public class rqz_SceneOrchestrator : MonoBehaviour
{
    [Header("Global UI")]
    public TextMeshProUGUI globalSubtitleText;

    [Header("Narration")]
    [Tooltip("用于播放旁白的AudioSource")]
    public AudioSource narrationAudioSource;
    public AudioClip narrationClip;
    [TextArea(3, 5)]
    [Tooltip("开场旁白的台词内容")]
    public string narrationSubtitleContent;

    [Header("Main Character (Hunter)")]
    public Animator hunterAnimator;
    public AudioSource hunterAudioSource;
    [TextArea(3, 5)]
    [Tooltip("猎人说话的台词内容")]
    public string hunterSpeechContent;
    [TextArea(3, 5)]
    [Tooltip("猎人大笑的台词内容")]
    public string hunterLaughContent;

    [Header("Hunter Clips")]
    public AudioClip speechClip;
    public AudioClip laughClip;

    [Header("Speaking Animal")]
    [Tooltip("动物的AudioSource")]
    public AudioSource animalAudioSource;
    public AudioClip animalClip;
    [TextArea(3, 5)]
    [Tooltip("动物的台词内容")]
    public string animalDialogueContent;

    [Header("Scene Timing")]
    [Tooltip("猎人走路的时长（秒）")]
    public float walkDuration = 5.0f;

    [Header("All Fleeing Animals")]

    public List<Animator> allFleeingAnimals;

    public List<Animator> allPathControllers;



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

        StartCoroutine(HunterFullSequence());
    }


    IEnumerator HunterFullSequence()
    {
        // --- 阶段0: 播放开场旁白 ---
        if (narrationAudioSource != null && narrationClip != null)
        {
            globalSubtitleText.text = narrationSubtitleContent;
            globalSubtitleText.transform.parent.gameObject.SetActive(true);

            narrationAudioSource.clip = narrationClip;
            narrationAudioSource.Play();
            yield return new WaitForSeconds(narrationClip.length);

            globalSubtitleText.transform.parent.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        // --- 阶段1: 猎人站立说话 ---
        globalSubtitleText.text = hunterSpeechContent;
        globalSubtitleText.transform.parent.gameObject.SetActive(true);
        hunterAudioSource.clip = speechClip;
        hunterAudioSource.Play();
        yield return new WaitForSeconds(speechClip.length);
        globalSubtitleText.transform.parent.gameObject.SetActive(false);

        // --- 阶段2: 猎人开始走路 ---
       // Debug.Log("Stage 2: Hunter starts walking.");
        hunterAnimator.SetTrigger("StartWalking");
        yield return new WaitForSeconds(walkDuration);

        // --- 阶段3: 猎人停下大笑 ---
      //  Debug.Log("Stage 3: Hunter laughs...");
        hunterAnimator.SetTrigger("StopAndLaugh");
        hunterAudioSource.clip = laughClip;
        hunterAudioSource.Play();
        globalSubtitleText.text = hunterLaughContent;
        globalSubtitleText.transform.parent.gameObject.SetActive(true);

        // 等待完整的大笑音频播放完毕
       // Debug.Log("...waiting for the entire laugh to finish...");
        yield return new WaitForSeconds(laughClip.length);

        // --- 阶段4: 动物反应 ---
      //  Debug.Log("Laugh finished! NOW animals flee!");

        // 播放动物的声音和字幕
        if (animalAudioSource != null && animalClip != null)
        {
            globalSubtitleText.text = animalDialogueContent;
            globalSubtitleText.transform.parent.gameObject.SetActive(true);
            animalAudioSource.clip = animalClip;
            animalAudioSource.Play();
        }

        // 触发所有动物的逃跑动画
        foreach (Animator animal in allFleeingAnimals)
        {
            if (animal != null) animal.SetTrigger("Flee");
        }

        // 触发所有动物的移动路径
        foreach (Animator pathController in allPathControllers)
        {
            if (pathController != null) pathController.SetTrigger("StartPath");
        }

        if (animalClip != null)
        {
            yield return new WaitForSeconds(animalClip.length);
        }

        if (globalSubtitleText != null)
        {
            globalSubtitleText.transform.parent.gameObject.SetActive(false);
        }

        // --- 阶段5: 猎人回归站立 ---
      //  Debug.Log("Stage 5: Hunter returns to idle.");
        hunterAnimator.SetTrigger("ReturnToIdle");

     //   Debug.Log("Hunter sequence complete.");
    }


    public void ResetScene()
    {
        // 停止所有正在运行的协程
        StopAllCoroutines();

        // 停止所有可能正在播放的音频
        if (narrationAudioSource != null) narrationAudioSource.Stop();
        if (hunterAudioSource != null) hunterAudioSource.Stop();
        if (animalAudioSource != null) animalAudioSource.Stop();

        // 隐藏全局字幕
        if (globalSubtitleText != null)
            globalSubtitleText.transform.parent.gameObject.SetActive(false);

        // 重置所有动画控制器的状态到默认
        if (hunterAnimator != null) hunterAnimator.Play("IdleLookAround", -1, 0f);

        foreach (Animator animal in allFleeingAnimals)
        {
            if (animal != null) animal.Play(0, -1, 0f);
        }

        foreach (Animator path in allPathControllers)
        {
            if (path != null) path.Play(0, -1, 0f);
        }
    }
}