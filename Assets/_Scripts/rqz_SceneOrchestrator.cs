using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class rqz_SceneOrchestrator : MonoBehaviour
{
    // --- 新增: 父物体引用 ---
    [Header("Scene Management")]
    [Tooltip("一个空的父物体，包含了所有AR场景内容(模型、动画等)")]
    public GameObject arContentRoot; // 把你在编辑器里创建的 ARContentRoot 拖到这里！

    [Header("Global UI")]
    public TextMeshProUGUI globalSubtitleText;

    [Header("Narration")]
    public AudioSource narrationAudioSource;
    public AudioClip narrationClip;
    [TextArea(3, 5)]
    public string narrationSubtitleContent;

    [Header("Main Character (Hunter)")]
    public Animator hunterAnimator;
    public AudioSource hunterAudioSource;
    [TextArea(3, 5)]
    public string hunterSpeechContent;
    [TextArea(3, 5)]
    public string hunterLaughContent;
    public AudioSource hunterWalkAudioSource;

    [Header("Hunter Clips")]
    public AudioClip speechClip;
    public AudioClip laughClip;

    [Header("Speaking Animal")]
    public AudioSource animalAudioSource;
    public AudioClip animalClip;
    [TextArea(3, 5)]
    public string animalDialogueContent;

    [Header("Scene Timing")]
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

        // 2. 重置所有动画状态，确保每次都是从头开始
        ResetAllAnimatorStates();

        // 3. 停止任何可能残留的旧流程，并开始新的流程
        StopAllCoroutines();
        StartCoroutine(HunterFullSequence());
    }

  
    public void OnTargetLost()
    {
        // 1. 停止所有正在运行的动画序列和延迟事件
        StopAllCoroutines();

        // 2. 立即停止所有声音
        if (narrationAudioSource != null) narrationAudioSource.Stop();
        if (hunterAudioSource != null) hunterAudioSource.Stop();
        if (animalAudioSource != null) animalAudioSource.Stop();

        // 3. 隐藏所有AR内容，这是最关键的一步！
        if (arContentRoot != null)
        {
            arContentRoot.SetActive(false);
        }
    }


    private void ResetAllAnimatorStates()
    {
        if (hunterAnimator != null) hunterAnimator.Play("IdleLookAround", -1, 0f);

        foreach (Animator animal in allFleeingAnimals)
        {
            if (animal != null) animal.Play(0, -1, 0f); // 播放默认状态
        }

        foreach (Animator path in allPathControllers)
        {
            if (path != null) path.Play(0, -1, 0f); // 播放默认状态
        }
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

        yield return new WaitForSeconds(0.7f);


        // --- 阶段1: 猎人站立说话 ---
        globalSubtitleText.text = hunterSpeechContent;
        globalSubtitleText.transform.parent.gameObject.SetActive(true);
        hunterAudioSource.clip = speechClip;
        hunterAudioSource.Play();
        yield return new WaitForSeconds(speechClip.length);
        globalSubtitleText.transform.parent.gameObject.SetActive(false);

        // --- 阶段2: 猎人开始走路 ---
        hunterAnimator.SetTrigger("StartWalking");
        if (hunterWalkAudioSource != null)
        {
            hunterWalkAudioSource.Play();
        }
        yield return new WaitForSeconds(walkDuration);

        if (hunterWalkAudioSource != null)
        {
            hunterWalkAudioSource.Stop();
        }
        // --- 阶段3: 猎人停下大笑 ---
        hunterAnimator.SetTrigger("StopAndLaugh");
        hunterAudioSource.clip = laughClip;
        hunterAudioSource.Play();
        globalSubtitleText.text = hunterLaughContent;
        globalSubtitleText.transform.parent.gameObject.SetActive(true);

        yield return new WaitForSeconds(laughClip.length);

        // --- 阶段4: 动物反应 ---
        if (animalAudioSource != null && animalClip != null)
        {
            globalSubtitleText.text = animalDialogueContent;
            globalSubtitleText.transform.parent.gameObject.SetActive(true);
            animalAudioSource.clip = animalClip;
            animalAudioSource.Play();
        }

        foreach (Animator animal in allFleeingAnimals)
        {
            if (animal != null) animal.SetTrigger("Flee");
        }

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
        hunterAnimator.SetTrigger("ReturnToIdle");
    }

  
}