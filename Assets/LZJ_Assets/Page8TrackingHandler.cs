using UnityEngine;
using Vuforia;
using _Scripts.WY.DialogueSystem;

public class Page8TrackingHandler : DefaultObserverEventHandler
{
    [Header("对话设置")]
    public string page8DialogueKey = "Scene8";

    [Header("春天场景")]
    public GameObject springScene;

    private bool hasTriggered = false;

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();

        Debug.Log("=== 识别到第8页 ===");

        if (!hasTriggered)
        {
            InitializePage8();
            hasTriggered = true;
        }
    }

    void InitializePage8()
    {
        Debug.Log("初始化第8页场景");

        // 显示春天场景
        if (springScene != null)
        {
            springScene.SetActive(true);
        }

        // 播放春天音乐
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play("Spring");
            Debug.Log("播放春天音乐");
        }

        // 播放第8页对话
        if (DialogueController.instance != null)
        {
            DialogueController.instance.PlayDialogue(page8DialogueKey);
            Debug.Log($"播放对话：{page8DialogueKey}");
        }
        else
        {
            Debug.LogError("DialogueController 不存在！");
        }
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        Debug.Log("第8页图片丢失");
    }
}